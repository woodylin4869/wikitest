using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.VA
{
    public class VARecordFailoverSchedule : IInvocable
    {
        private readonly ILogger<VARecordFailoverSchedule> _logger;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly ICacheDataService _cacheDataService;

        public VARecordFailoverSchedule(ILogger<VARecordFailoverSchedule> logger, IRepairBetRecordService repairBetRecordService, ICacheDataService cacheDataService)
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
                var req = await _cacheDataService.ListPopAsync<PullRecordFailoverWithTimeOffset>($"{RedisCacheKeys.PullRecordFailOver}:{Platform.VA}");

                if (req is null) return;

                if (req.requestTime.Add(req.delay) >= DateTime.Now)
                {
                    await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.VA}", req);
                    return;
                }

                _logger.LogInformation("Execute VA PullRecordFailOver {value}", req.repairParameter);

                var repairEndTime = DateTime.Parse(req.repairParameter);
                var repairTime = DateTime.Parse(req.repairParameter).Add(req.OffTimeSpan);

                await _repairBetRecordService.RepairGameRecord(new RepairBetSummaryReq()
                {
                    game_id = Platform.VA.ToString(),
                    StartTime = repairTime.AddMinutes(-3),
                    EndTime = repairEndTime.AddMinutes(3).AddMilliseconds(-1)
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