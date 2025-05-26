using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.SPLUS;

namespace ThirdPartyWallet.GameAPI.Service.Game.SPLUS
{
    public static class SPLUSApiServiceExtension
    {
        //註冊IOptions<GeminiConfig>
        public static IServiceCollection AddSPLUSApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<GeminiConfig>
            services.Configure<SPLUSConfig>(config.GetSection(SPLUSConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊GeminiApiService並設定NamedHttpLogHandler
            services.AddHttpClient<ISPLUSApiService, SPLUSApiService>((client) =>
            {
                //Timeout 14 秒
                client.Timeout = TimeSpan.FromSeconds(14);
            })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, SPLUSApiService.PlatformName));

            return services;
        }
        public static IServiceCollection AddSPLUSApiService(this IServiceCollection services, Action<SPLUSConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<ISPLUSApiService, SPLUSApiService>();
            return services;
        }
    }
}
