using H1_ThirdPartyWalletAPI.Service.Game.WE;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.WE;

namespace ThirdPartyWallet.GameAPI.Service.Game.WE
{
    public static class WEApiServiceExtension
    {
        public static IServiceCollection AddWEApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<WEConfig>
            services.Configure<WEConfig>(config.GetSection(WEConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊WEApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IWEApiService, WEApiService>((client) =>
                {
                    //Timeout 14 秒
                    client.Timeout = TimeSpan.FromSeconds(14);
                })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, WEApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddWEApiService(this IServiceCollection services, Action<WEConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IWEApiService, WEApiService>();
            return services;
        }
    }
}