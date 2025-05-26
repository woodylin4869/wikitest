using H1_ThirdPartyWalletAPI.Service.Game.CR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.CR;

namespace ThirdPartyWallet.GameAPI.Service.Game.CR
{
    public static class CRApiServiceExtension
    {
        public static IServiceCollection AddCRApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<CRConfig>
            services.Configure<CRConfig>(config.GetSection(CRConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊CRApiService並設定NamedHttpLogHandler
            services.AddHttpClient<ICRApiService, CRApiService>((client) =>
                {
                    //Timeout 14 秒
                    client.Timeout = TimeSpan.FromSeconds(14);
                })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, CRApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddCRApiService(this IServiceCollection services, Action<CRConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<ICRApiService, CRApiService>();
            return services;
        }
    }
}