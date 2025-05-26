using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Schedule
{
    public class CoravelHealthCheckSchedule : IInvocable
    {
        private readonly ILogger<CoravelHealthCheckSchedule> _logger;
        private readonly IMemoryCache _memoryCache;

        public CoravelHealthCheckSchedule(ILogger<CoravelHealthCheckSchedule> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public Task Invoke()
        {
            _logger.LogDebug("Coravel running");
            _memoryCache.Set("CoravelHealthCheck", DateTime.Now);
            return Task.CompletedTask;
        }
    }
}
