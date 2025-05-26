using H1_ThirdPartyWalletAPI.Service.Game.RGRICH;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.RGRICH;

namespace ThirdPartyWallet.GameAPI.Service.Game.RGRICH
{
    public static class RGRICHApiServiceExtension
    {
        public static IServiceCollection AddRGRICHApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<RGRICHConfig>
            services.Configure<RGRICHConfig>(config.GetSection(RGRICHConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊RGRICHApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IRGRICHApiService, RGRICHApiService>((client) =>
                {
                    //Timeout 14 秒
                    client.Timeout = TimeSpan.FromSeconds(14);
                })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, RGRICHApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddRGRICHApiService(this IServiceCollection services, Action<RGRICHConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IRGRICHApiService, RGRICHApiService>();
            return services;
        }
    }
}