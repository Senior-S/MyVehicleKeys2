using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using MyVehicleKeys2.Providers;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;
using OpenMod.Core.Commands;

namespace MyVehicleKeys2.Commands
{
	[Command("deletevehicle")]
	[CommandSyntax("<vehicle id>")]
	[CommandDescription("Delete a vehicle from your keys.")]
    public class CommandDeleteVehicle : Command
    {
        private readonly IKeysManager m_KeysManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDeleteVehicle(IServiceProvider service, IKeysManager keysManager, IStringLocalizer stringLocalizer) : base(service)
        {
            m_KeysManager = keysManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(0);
            if (!await m_KeysManager.TryRemoveKeyFromBunch(user.Id, id))
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:no_vehicle", new { vehicleID = id }], System.Drawing.Color.Red, false, "");
            }
            else
            {
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(id);
                await UniTask.SwitchToMainThread();
                VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                {
                            vehicle.instanceID,
                            user.SteamId,
                            user.Player.Player.quests.groupID,
                            false
                });
                await user.PrintMessageAsync(m_StringLocalizer["translations:vehicle_delete", new { vehicleID = id }], System.Drawing.Color.Red, false, "");
            }
        }
    }
}
