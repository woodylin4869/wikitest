using System;
using System.Threading;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Controllers.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace H1_ThirdPartyWalletAPI.HealthCheck
{
    public class RedisHealthCheck: IHealthCheck
    {
        private readonly ICacheDataService _cacheDataService;
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(ICacheDataService cacheDataService, ILogger<RedisHealthCheck> logger)
        {
            _cacheDataService = cacheDataService;
            _logger = logger;
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = Guid.NewGuid().ToString();
            var value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");

            try
            {
                await _cacheDataService.StringSetAsync(key, value, 5);
                var cacheData = await _cacheDataService.StringGetAsync<string>(key);
                _cacheDataService.KeyDelete(key);
                if (cacheData == value)
                {
                    return await Task.FromResult(HealthCheckResult.Healthy("redis healthCheck success"));
                }

                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                    "redis healthCheck Fail"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{message}", ex.Message);
                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                    "redis healthCheck Fail"));
            }
        }
    }
}