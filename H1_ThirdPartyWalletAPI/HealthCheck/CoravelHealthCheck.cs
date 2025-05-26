using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.HealthCheck
{
    public class CoravelHealthCheck : IHealthCheck
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CoravelHealthCheck> _logger;

        public CoravelHealthCheck(IMemoryCache memoryCache, ILogger<CoravelHealthCheck> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var time = _memoryCache.Get<DateTime>("CoravelHealthCheck");
            if (time != default && DateTime.Now - time < TimeSpan.FromMinutes(5))
                return Task.FromResult(HealthCheckResult.Healthy("Coravel still running."));
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                    "Coravel healthCheck Fail"));
        }
    }
}
