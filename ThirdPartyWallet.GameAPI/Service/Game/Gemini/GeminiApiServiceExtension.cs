using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.Gemini;

namespace ThirdPartyWallet.GameAPI.Service.Game.Gemini
{
    public static class GeminiApiServiceExtension
    {
        public static IServiceCollection AddGeminiApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<GeminiConfig>
            services.Configure<GeminiConfig>(config.GetSection(GeminiConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊GeminiApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IGeminiApiService, GeminiApiService>((client) =>
                {
                    //Timeout 14 秒
                    client.Timeout = TimeSpan.FromSeconds(14);
                })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, GeminiApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddGeminiApiService(this IServiceCollection services, Action<GeminiConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IGeminiApiService, GeminiApiService>();
            return services;
        }
    }
}
