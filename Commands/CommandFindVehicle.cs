using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using MyVehicleKeys2.Models;
using MyVehicleKeys2.Providers;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Linq;
using System.Threading.Tasks;
using Command = OpenMod.Core.Commands.Command;

namespace MyVehicleKeys2.Commands
{
    [Command("findvehicle")]
    [CommandSyntax("<vehicle id>")]
    [CommandDescription("Set the position of your vehicle in the map.")]
    public class CommandFindVehicle : Command
    {
        private readonly IKeysManager m_KeysManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandFindVehicle(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, IKeysManager keysManager) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_KeysManager = keysManager;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            uint id = await Context.Parameters.GetAsync<uint>(0);

            PlayerBunch? pBunch = await m_KeysManager.TryGetPlayerBunch(user.SteamId.ToString());
            if (pBunch == null || pBunch.VehiclesKey.Count == 0) await user.PrintMessageAsync(m_StringLocalizer["translations:no_key"], System.Drawing.Color.Red, false, "");
            else if(!pBunch.VehiclesKey.Any(v => v.InstanceId == id))
            {
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(id);
                if (vehicle != null && !vehicle.isExploded && !vehicle.isDead)
                {
                    await UniTask.SwitchToMainThread();
                    await user.PrintMessageAsync(m_StringLocalizer["translations:vehicle_position", new { vehicle = pBunch.VehiclesKey.Where(z => z.InstanceId == id).First().VehicleName }], System.Drawing.Color.Green);
                    user.Player.Player.quests.replicateSetMarker(true, vehicle.transform.position, "Your vehicle!");
                }
                else
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:vehicle_explode"]);
                    await m_KeysManager.TryRemoveKeyFromBunch(user.SteamId.ToString(), id);
                }
            }
            else
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:no_vehicle", new { vehicleID = id }]);
            }
        }
    }
}
