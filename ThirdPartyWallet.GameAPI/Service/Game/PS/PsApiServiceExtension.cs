using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.PS;

namespace ThirdPartyWallet.GameAPI.Service.Game.PS
{
    public static class PsApiServiceExtension
    {
        public static IServiceCollection AddPsApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<GeminiConfig>
            services.Configure<PsConfig>(config.GetSection(PsConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊GeminiApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IPsApiService, PsApiService>((client) =>
            {
                //Timeout 14 秒
                client.Timeout = TimeSpan.FromSeconds(14);
            })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, PsApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddPsApiService(this IServiceCollection services, Action<PsConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IPsApiService, PsApiService>();
            return services;
        }
    }
}
