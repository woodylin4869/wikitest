using H1_ThirdPartyWalletAPI.Service.Game.IDN;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.IDN;

namespace ThirdPartyWallet.GameAPI.Service.Game.IDN
{
    public static class IDNApiServiceExtension
    {
        public static IServiceCollection AddIDNApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<IDNConfig>
            services.Configure<IDNConfig>(config.GetSection(IDNConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊IDNApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IIDNApiService, IDNApiService>((client) =>
                {
                    //Timeout 14 秒
                    client.Timeout = TimeSpan.FromSeconds(14);
                })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, IDNApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddIDNApiService(this IServiceCollection services, Action<IDNConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IIDNApiService, IDNApiService>();
            return services;
        }
    }
}