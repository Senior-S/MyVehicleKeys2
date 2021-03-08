using System;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.API.Persistence;
using SDG.Unturned;
using OpenMod.Core.Helpers;
using MyVehicleKeys2.Providers;
using Microsoft.Extensions.Configuration;
using MyVehicleKeys2.Models;
using UnityEngine;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

[assembly: PluginMetadata("SS.MyVehicleKeys2", DisplayName = "MyVehicleKeys2")]
namespace MyVehicleKeys2
{
    public class MyVehicleKeys2 : OpenModUnturnedPlugin
    {
        private readonly ILogger<MyVehicleKeys2> m_Logger;
        private readonly IDataStore m_DataStore;
        private readonly IKeysManager m_KeysManager;
        private readonly IConfiguration m_Configuration;
        private readonly IStringLocalizer m_StringLocalizer;

        public MyVehicleKeys2(
            ILogger<MyVehicleKeys2> logger,
            IDataStore dataStore,
            IKeysManager keysManager,
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_DataStore = dataStore;
            m_KeysManager = keysManager;
            m_Configuration = configuration;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async UniTask OnLoadAsync()
        {
            // await UniTask.SwitchToMainThread();
            m_Logger.LogInformation(" Plugin loaded correctly!");

            this.Harmony.PatchAll(GetType().Assembly);

            if (!await m_DataStore.ExistsAsync(KeysManager.MVKKey))
            {
                await m_DataStore.SaveAsync(KeysManager.MVKKey, new List<PlayerBunch>());
            }

            Patch.OnPlayerLockVehicle += Patch_OnPlayerLockVehicle;
        }

        private void Patch_OnPlayerLockVehicle(Steamworks.CSteamID steamId, uint instanceId, bool locked)
        {
            if (!locked || Provider.server == steamId)
            {
                return;
            }
            AsyncHelper.RunSync(async () => 
            {
                var pBunch = await m_KeysManager.TryGetPlayerBunch(steamId.ToString());
                var user = PlayerTool.getPlayer(steamId);
                InteractableVehicle vehicle = VehicleManager.findVehicleByNetInstanceID(instanceId);
                if(pBunch != null)
                {
                    if (locked)
                    {
                        if (pBunch.VehiclesKey.Count >= m_Configuration.GetSection("configuration:max_keys").Get<int>())
                        {
                            await UniTask.SwitchToMainThread();
                            VehicleManager.instance.channel.send("tellVehicleLock", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, new object[]
                            {
                                instanceId,
                                steamId,
                                user.quests.groupID,
                                false
                            });

                            ChatManager.say(steamId, m_StringLocalizer["translations:max_keys_reach"], Color.red, EChatMode.SAY, false);
                        }
                        else
                        {
                            await m_KeysManager.TryAddKeyToBunch(steamId.ToString(), new Key { InstanceId = instanceId, VehicleName = vehicle.asset.name });
                            await UniTask.SwitchToMainThread();
                            ChatManager.say(steamId, m_StringLocalizer["translations:vehicle_added", new { vehicleId = instanceId, vehicle = vehicle.asset.name }], Color.red, EChatMode.SAY, false);
                        }
                    }
                }
            });
            
        }

        protected override async UniTask OnUnloadAsync()
        {
            // await UniTask.SwitchToMainThread();
            Patch.OnPlayerLockVehicle -= Patch_OnPlayerLockVehicle;
            m_Logger.LogInformation(" Plugin unloaded correctly!");
        }
    }
}
