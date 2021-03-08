using SDG.Unturned;
using UnityEngine;

namespace MyVehicleKeys2.Helper
{
    public class RaycastHelper
    {
        public static InteractableVehicle? Raycast(Player player, float distance)
        {
            if (Physics.Raycast(player.look.aim.position, player.look.aim.forward, out RaycastHit hit, distance, RayMasks.BARRICADE | RayMasks.STRUCTURE))
            {
                Transform transform = hit.transform;
                if(transform.TryGetComponent<InteractableVehicle>(out InteractableVehicle vehicle))
                {
                    return vehicle;
                }
            }
            return null;
        }
    }
}
