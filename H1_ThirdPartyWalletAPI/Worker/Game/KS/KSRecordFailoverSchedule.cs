using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.KS
{
    public class KSRecordFailoverSchedule : IInvocable
    {
        private readonly ILogger<KSRecordFailoverSchedule> _logger;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ICacheDataService _cacheDataService;

        public KSRecordFailoverSchedule(ILogger<KSRecordFailoverSchedule> logger, IRepairBetRecordService repairBetRecordService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _repairBetRecordService = repairBetRecordService;
            _cacheDataService = cacheDataService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });

            _logger.LogInformation("Invoke {name} on time : {time}", this.GetType().Name, DateTime.Now.ToLocalTime());

            try
            {
                var req = await _cacheDataService.ListPopAsync<PullRecordFailoverRequest>($"{RedisCacheKeys.PullRecordFailOver}:{Platform.KS}");

                if (req is null) return;

                if (req.requestTime.Add(req.delay) >= DateTime.Now)
                {
                    await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.KS}", req);
                    return;
                }

                _logger.LogInformation("Execute KS PullRecordFailOver {value}", req.repairParameter);

                var repairTime = DateTime.Parse(req.repairParameter);

                await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                {
                    game_id = Platform.KS.ToString(),
                    StartTime = repairTime.AddMinutes(-3),
                    EndTime = repairTime.AddMinutes(3).AddMilliseconds(-1)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessFailover Error! Message:{error}", ex.Message);
            }

            _logger.LogInformation("End {name} on time : {time}", this.GetType().Name, DateTime.Now.ToLocalTime());
        }
    }
}
