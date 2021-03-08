using MyVehicleKeys2.Models;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyVehicleKeys2.Providers
{
    [Service]
    public interface IKeysManager
    {
        Task<bool> TryAddPlayerBunch(string steamId, byte MaxKeys);

        Task<bool> TryRemovePlayerBunch(string steamId);

        Task<bool> TryAddKeyToBunch(string steamId, Key key);

        Task<bool> TryRemoveKeyFromBunch(string steamId, uint instanceId);

        Task<PlayerBunch?> TryGetPlayerBunch(string steamId);

        Task<Key> TransferVehicle(string ownerId, string victimId, uint instanceId);

        Task<string> CheckVehicleOwner(uint instanceId);
    }

    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, Priority = OpenMod.API.Prioritization.Priority.Low)]
    public class KeysManager : IKeysManager, IAsyncDisposable
    {
        private List<PlayerBunch>? m_KeysCache;
        internal const string MVKKey = "PlayerBunchs";

        private readonly IPluginAccessor<MyVehicleKeys2> m_PluginAccesor;

        public KeysManager(IPluginAccessor<MyVehicleKeys2> pluginAccessor)
        {
            m_PluginAccesor = pluginAccessor;
        }

        public async Task<PlayerBunch?> TryGetPlayerBunch(string steamId)
        {
            List<PlayerBunch> keys = await GetKeysAsync();
            if (!keys.Any(key => key.OwnerId == steamId))
            {
                if (keys.Where(key => key.OwnerId == steamId).First().VehiclesKey.Count == 0) return null;
                return keys.Where(key => key.OwnerId == steamId).First();
            }

            return null;
        }

        public async Task<string> CheckVehicleOwner(uint instanceId)
        {
            List<PlayerBunch> keys = await GetKeysAsync();
            if (!keys.Any(key => key.VehiclesKey.Where(vk => vk.InstanceId == instanceId).FirstOrDefault() != null))
            {
                var PlayerBunch = keys.Where(key => key.VehiclesKey.Where(vk => vk.InstanceId == instanceId) != null).FirstOrDefault();
                if(PlayerBunch.OwnerId != null) return PlayerBunch.OwnerId;
            }
            return "";
        }

        public async Task<bool> TryAddPlayerBunch(string steamId, byte MaxKeys)
        {
            List<PlayerBunch> keys = await GetKeysAsync();
            if (!keys.Any(key => key.OwnerId == steamId))
            {
                keys.Add(new PlayerBunch
                {
                    OwnerId = steamId,
                    MaxKeys = MaxKeys,
                    VehiclesKey = new List<Key>()
                });

                m_KeysCache = keys;

#pragma warning disable CS8602
                await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKey, m_KeysCache);
#pragma warning restore CS8602
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TryRemovePlayerBunch(string steamId)
        {
            List<PlayerBunch> keys = await GetKeysAsync() ?? new List<PlayerBunch>();
            if (!keys.Any(key => key.OwnerId == steamId))
            {
                PlayerBunch keyToRemove = keys.Where(k => k.OwnerId == steamId).First();
                keys.Remove(keyToRemove);

                m_KeysCache = keys;

#pragma warning disable CS8602
                await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKey, m_KeysCache);
#pragma warning restore CS8602
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Key> TransferVehicle(string ownerId, string victimId, uint instanceId)
        {
            List<PlayerBunch> keys = await GetKeysAsync();
            var pBunch = keys.Where(k => k.OwnerId == ownerId).First();
            var vBunch = keys.Where(k => k.OwnerId == victimId).First();
            var key = pBunch.VehiclesKey.Where(key => key.InstanceId == instanceId).First();
            pBunch.VehiclesKey.Remove(key);
            vBunch.VehiclesKey.Add(key);
            return key;
        }

        public async Task<bool> TryAddKeyToBunch(string steamId, Key key)
        {
            List<PlayerBunch> keys = await GetKeysAsync();
            var pKey = keys.Where(k => k.OwnerId == steamId).First();
            if (pKey.VehiclesKey.Count >= pKey.MaxKeys) return false;
            else
            {
                pKey.VehiclesKey.Add(key);

                m_KeysCache = keys;

#pragma warning disable CS8602
                await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKey, m_KeysCache);
#pragma warning restore CS8602
                return true;
            }
        }

        public async Task<bool> TryRemoveKeyFromBunch(string steamId, uint instanceId)
        {
            List<PlayerBunch> keys = await GetKeysAsync();
            var pKey = keys.Where(k => k.OwnerId == steamId).First();
            var vehicle = pKey.VehiclesKey.Where(v => v.InstanceId == instanceId).FirstOrDefault();
            if (vehicle != null)
            {
                pKey.VehiclesKey.Remove(vehicle);

                m_KeysCache = keys;

#pragma warning disable CS8602
                await m_PluginAccesor.Instance.DataStore.SaveAsync(MVKKey, m_KeysCache);
#pragma warning restore CS8602
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<PlayerBunch>> GetKeysAsync()
        {
            if (m_KeysCache == null)
            {
                await ReadData();
            }

            return m_KeysCache ?? new List<PlayerBunch>();
        }

        private async Task ReadData()
        {
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
            m_KeysCache = await m_PluginAccesor.Instance.DataStore.LoadAsync<List<PlayerBunch>>(MVKKey)
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
                            ?? new List<PlayerBunch>();
        }

        public async ValueTask DisposeAsync()
        {
            m_KeysCache = null;
            await Task.CompletedTask;
        }
    }
}
