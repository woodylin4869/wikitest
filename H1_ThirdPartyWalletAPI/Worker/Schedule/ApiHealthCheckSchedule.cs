using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using StackExchange.Redis;
using H1_ThirdPartyWalletAPI.Model.W1API;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class ApiHealthCheckSchedule : IInvocable
    {
        private readonly ILogger<ApiHealthCheckSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly string _redisKey = $"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL";

        public ApiHealthCheckSchedule(ILogger<ApiHealthCheckSchedule> logger
            , ICommonService commonService)
        {
            _logger = logger;
            _commonService = commonService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            try
            {
                _logger.LogDebug("Invoke ApiHealthCheck Schedule");
                var info = await _commonService._apiHealthCheck.SetAllHealthInfo();
                await _commonService._cacheDataService.StringSetAsync(_redisKey, info, 6000);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                await Task.CompletedTask;
                _logger.LogError("ApiHealthCheck Schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
