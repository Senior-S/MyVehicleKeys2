using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyVehicleKeys2.Providers;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Connections.Events;
using System.Threading.Tasks;

namespace MyVehicleKeys2
{
    public class Events
    {
        public class PlayerJoined : IEventListener<UnturnedPlayerConnectedEvent>
        {
            private readonly IKeysManager m_KeysManager;
            private readonly IConfiguration m_Configuration;
            private readonly ILogger<PlayerJoined> m_Logger;

            public PlayerJoined(IKeysManager keysManager, IConfiguration configuration, ILogger<PlayerJoined> logger)
            {
                m_KeysManager = keysManager;
                m_Configuration = configuration;
                m_Logger = logger;
            }

            public async Task HandleEventAsync(object? sender, UnturnedPlayerConnectedEvent @event)
            {
                if (await m_KeysManager.TryAddPlayerBunch(@event.Player.SteamId.ToString(), m_Configuration.GetSection("configuration:max_keys").Get<byte>()))
                {
                    m_Logger.LogInformation($" Player {@event.Player.SteamId} has generated it own key bunch.");
                }
            }
        }
    }
}
