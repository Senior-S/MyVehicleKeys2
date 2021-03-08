using Microsoft.Extensions.Localization;
using MyVehicleKeys2.Models;
using MyVehicleKeys2.Providers;
using OpenMod.API.Commands;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using OpenMod.Core.Users;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Linq;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace MyVehicleKeys2.Commands
{
    [Command("transfervehicle")]
    [CommandSyntax("<player> <vehicle id>")]
    [CommandDescription("Transfer a vehicle to other player.")]
    public class CommandTransferVehicle : Command
    {
        private readonly IKeysManager m_KeysManager;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IUserManager m_UserManager;

        public CommandTransferVehicle(IServiceProvider serviceProvider, IKeysManager keysManager, IStringLocalizer stringLocalizer, IUserManager userManager) : base(serviceProvider)
        {
            m_KeysManager = keysManager;
            m_StringLocalizer = stringLocalizer;
            m_UserManager = userManager;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(1);
            string vic = await Context.Parameters.GetAsync<string>(0);
            Player player = PlayerTool.getPlayer(vic);
            if (player == null)
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:player_notfound"], System.Drawing.Color.Red, false, "");
            }
#pragma warning disable CS8602
            UnturnedUser? victim = (UnturnedUser?)await m_UserManager.FindUserAsync(KnownActorTypes.Player, player.channel.owner.playerID.steamID.ToString(), UserSearchMode.FindById);
#pragma warning restore CS8602
            if (victim == null)
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:player_notfound"], System.Drawing.Color.Red, false, "");
            }
            else if (victim == user)
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:yourself_error"], System.Drawing.Color.Red, false, "");
            }
            else if (victim != null && user != null)
            {
                PlayerBunch? userBunch = await m_KeysManager.TryGetPlayerBunch(user.Id);
                PlayerBunch? victimBunch = await m_KeysManager.TryGetPlayerBunch(victim.Id);

                if (userBunch != null && victimBunch != null)
                {
                    if (!userBunch.VehiclesKey.Any(v => v.InstanceId == id))
                    {
                        if (victimBunch.VehiclesKey.Count >= (victimBunch.MaxKeys - 1))
                        {
                            await user.PrintMessageAsync(m_StringLocalizer["translations:victim_maxkeys"]);
                        }
                        else
                        {

                            var vehicleKey = await m_KeysManager.TransferVehicle(user.Id, victim.Id, id);
                            await user.PrintMessageAsync(m_StringLocalizer["translations:user_vehicle_transfer", new { vehicle = vehicleKey.VehicleName, victim = victim.DisplayName }], System.Drawing.Color.Green);
                            await victim.PrintMessageAsync(m_StringLocalizer["translations:victim_vehicle_transfer", new { vehicle = vehicleKey.VehicleName, user = user.DisplayName }], System.Drawing.Color.Green);
                        }
                    }
                    else
                    {
                        await user.PrintMessageAsync(m_StringLocalizer["translations:no_vehicle", new { vehicleID = id }]);
                    }
                }
            }
            else
            {
                throw new UserFriendlyException("Internal error!");
            }
        }
    }
}
