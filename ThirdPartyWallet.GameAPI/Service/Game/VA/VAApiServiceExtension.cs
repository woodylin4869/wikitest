using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.VA;

namespace ThirdPartyWallet.GameAPI.Service.Game.VA
{
    public static class VAApiServiceExtension
    {
        public static IServiceCollection AddVAApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<GeminiConfig>
            services.Configure<VAConfig>(config.GetSection(VAConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊GeminiApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IVAApiService, VAApiService>((client) =>
            {
                //Timeout 14 秒
                client.Timeout = TimeSpan.FromSeconds(14);
            })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, VAApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddVAApiService(this IServiceCollection services, Action<VAConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IVAApiService, VAApiService>();
            return services;
        }
    }
}
