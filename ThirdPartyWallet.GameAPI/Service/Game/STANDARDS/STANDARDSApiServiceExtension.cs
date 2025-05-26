using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.STANDARDS;

namespace ThirdPartyWallet.GameAPI.Service.Game.STANDARDS
{
    public static class STANDARDSApiServiceExtension
    {
        //註冊IOptions<GeminiConfig>
        public static IServiceCollection AddSTANDARDSApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<GeminiConfig>
            services.Configure<STANDARDSConfig>(config.GetSection(STANDARDSConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊GeminiApiService並設定NamedHttpLogHandler
            services.AddHttpClient<ISTANDARDSApiService, STANDARDSApiService>((client) =>
            {
                //Timeout 14 秒
                client.Timeout = TimeSpan.FromSeconds(14);
            })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, STANDARDSApiService.PlatformName));

            return services;
        }
        public static IServiceCollection AddSTANDARDSApiService(this IServiceCollection services, Action<STANDARDSConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<ISTANDARDSApiService, STANDARDSApiService>();
            return services;
        }
    }
}
