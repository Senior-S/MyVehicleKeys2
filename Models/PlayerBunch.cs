using System.Collections.Generic;

namespace MyVehicleKeys2.Models
{
    public class PlayerBunch
    {
        public string? OwnerId { get; set; }

        public byte MaxKeys { get; set; } = 3;

        public List<Key> VehiclesKey { get; set; } = new List<Key>();
    }
}
