using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using MyVehicleKeys2.Models;
using MyVehicleKeys2.Providers;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Threading.Tasks;

namespace MyVehicleKeys2.Commands
{
    [Command("keys")]
    [CommandDescription("Get a list of all of your actual keys")]
    public class CommandKeys : Command
    {
        private readonly IConfiguration m_Configuration;
        private readonly IKeysManager m_KeysManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandKeys(IServiceProvider serviceProvider, IKeysManager keysManager, IConfiguration configuration, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_Configuration = configuration;
            m_KeysManager = keysManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;

            PlayerBunch? pBunch = await m_KeysManager.TryGetPlayerBunch(user.SteamId.ToString());
            if (pBunch == null || pBunch.VehiclesKey.Count == 0) await user.PrintMessageAsync(m_StringLocalizer["translations:no_key"], System.Drawing.Color.Red, false, "");
            else
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:your_keys"]);
                for (int i = 0; i < pBunch.VehiclesKey.Count; i++)
                {
                    await user.PrintMessageAsync($"{pBunch.VehiclesKey[i].VehicleName} - {pBunch.VehiclesKey[i].InstanceId}", System.Drawing.Color.FromName(m_Configuration.GetSection("configuration:keys_message_color").Get<string>()),false, "");
                }   
            }
        }
    }
}
