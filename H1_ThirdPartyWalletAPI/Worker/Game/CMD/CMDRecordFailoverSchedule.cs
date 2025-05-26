using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service;

namespace H1_ThirdPartyWalletAPI.Worker.Game.CMD
{
    public class CMDRecordFailoverSchedule : IInvocable
    {
        private readonly ILogger<CMDRecordFailoverSchedule> _logger;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ICacheDataService _cacheDataService;

        public CMDRecordFailoverSchedule(ILogger<CMDRecordFailoverSchedule> logger, IRepairBetRecordService repairBetRecordService, ICacheDataService cacheDataService)
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
                var req = await _cacheDataService.ListPopAsync<PullRecordFailoverRequest>($"{RedisCacheKeys.PullRecordFailOver}:{Platform.CMD368}");

                if (req is null) return;

                if (req.requestTime.Add(req.delay) >= DateTime.Now)
                {
                    await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.CMD368}", req);
                    return;
                }

                _logger.LogInformation("Execute CMD PullRecordFailOver {value}", req.repairParameter);

                var repairTime = DateTime.Parse(req.repairParameter);

                await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                {
                    game_id = Platform.CMD368.ToString(),
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
