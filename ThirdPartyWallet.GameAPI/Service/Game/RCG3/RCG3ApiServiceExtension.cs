using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.Gemini;
using ThirdPartyWallet.Share.Model.Game.RCG3;

namespace ThirdPartyWallet.GameAPI.Service.Game.RCG3
{
    public static class RCG3ApiServiceExtension
    {
        public static IServiceCollection AddRCG3ApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<GeminiConfig>
            services.Configure<RCG3Config>(config.GetSection(RCG3Config.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊GeminiApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IRCG3ApiService, RCG3ApiService>((client) =>
            {
                //Timeout 14 秒
                client.Timeout = TimeSpan.FromSeconds(14);
            })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, RCG3ApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddGeminiApiService(this IServiceCollection services, Action<RCG3Config> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IRCG3ApiService, RCG3ApiService>();
            return services;
        }
    }
}
