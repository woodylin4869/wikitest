using H1_ThirdPartyWalletAPI.Service.Game.EGSlot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.EGSlot;

namespace ThirdPartyWallet.GameAPI.Service.Game.EGSlot
{
    public static class EGSlotApiServiceExtension
    {
        public static IServiceCollection AddEGSlotApiService(this IServiceCollection services, IConfiguration config)
        {
            //註冊IOptions<EGSlotConfig>
            services.Configure<EGSlotConfig>(config.GetSection(EGSlotConfig.ConfigKey));

            //註冊LogHelper<>
            services.TryAddSingleton(typeof(LogHelper<>));

            //註冊EGSlotApiService並設定NamedHttpLogHandler
            services.AddHttpClient<IEGSlotApiService, EGSlotApiService>((client) =>
                {
                    //Timeout 14 秒
                    client.Timeout = TimeSpan.FromSeconds(14);
                })
                .AddHttpMessageHandler((sp) => NamedHttpLogHandler.Build(sp, EGSlotApiService.PlatformName));

            return services;
        }

        public static IServiceCollection AddEGSlotApiService(this IServiceCollection services, Action<EGSlotConfig> optionAction)
        {
            services.Configure(optionAction);
            services.TryAddSingleton(typeof(LogHelper<>));
            services.AddTransient<IEGSlotApiService, EGSlotApiService>();
            return services;
        }
    }
}
