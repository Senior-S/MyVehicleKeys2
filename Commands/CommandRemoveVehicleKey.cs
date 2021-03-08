using Microsoft.Extensions.Localization;
using MyVehicleKeys2.Providers;
using MyVehicleKeys2.Helper;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;
using OpenMod.Core.Commands;

namespace MyVehicleKeys2.Commands
{
	[Command("removevehiclekey")]
    [CommandDescription("Remove a key from a vehicle!")]
    public class CommandRemoveVehicleKey : Command
    {
        private readonly IKeysManager m_KeysManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandRemoveVehicleKey(IServiceProvider serviceProvider, IKeysManager keysManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_KeysManager = keysManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            InteractableVehicle? vehicle = RaycastHelper.Raycast(user.Player.Player, 6f);
            if (vehicle != null)
            {
                string ownerId = await m_KeysManager.CheckVehicleOwner(vehicle.instanceID);
                if (ownerId != "")
                {
                    await m_KeysManager.TryRemoveKeyFromBunch(ownerId, vehicle.instanceID);
                    await user.PrintMessageAsync(m_StringLocalizer["translations:vehicle_removedkey", new { vehicleID = vehicle.instanceID }]);
                    var player = PlayerTool.getPlayer(new Steamworks.CSteamID(ulong.Parse(ownerId)));
                    if (player != null)
                    {
                        ChatManager.say(player.channel.owner.playerID.steamID, m_StringLocalizer["translations:vehicle_lockpicked", new { vehicle = vehicle.asset.name, vehicleID = vehicle.instanceID }], UnityEngine.Color.red, EChatMode.SAY, true);
                    }
                }
            }
            else
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:vehicle_notfound"], System.Drawing.Color.Red, false, "");
            }
        }
    }
}
