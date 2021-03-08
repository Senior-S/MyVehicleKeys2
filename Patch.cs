using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVehicleKeys2
{
    public class Patch
    {
        public delegate void PlayerLockVehicle(CSteamID steamId, uint instanceId, bool locked);
        public static event PlayerLockVehicle? OnPlayerLockVehicle;

        [HarmonyPatch]
        public static class TellVehicleLock_Patch
        {
            [HarmonyPatch(typeof(VehicleManager), "tellVehicleLock")]
            [HarmonyPrefix]
            public static bool VehicleLock(VehicleManager __instance, CSteamID steamID, uint instanceID, CSteamID owner, CSteamID group, bool locked)
            {
                OnPlayerLockVehicle?.Invoke(owner, instanceID, locked);
                return true;
            }
        }
    }
}
