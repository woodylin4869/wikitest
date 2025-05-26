using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.NEXTSPIN;

public class NextSpinRecordSchedule : IInvocable
{
    private readonly ILogger<NextSpinRecordSchedule> _logger;
    private readonly INEXTSPIN_InterfaceService _apiInterfaceService;
    private readonly IDBService _dbService;
    private readonly ISystemParameterDbService _systemParameterDbService;
    private readonly ICacheDataService _cacheDataService;

    private const string SYSTEM_PARAMETERS_KEY = "NextSpinRecordSchedule";

    private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-10);
    private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

    public NextSpinRecordSchedule(ILogger<NextSpinRecordSchedule> logger, INEXTSPIN_InterfaceService apiInterfaceService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
    {
        _logger = logger;
        _apiInterfaceService = apiInterfaceService;
        _dbService = dbService;
        _systemParameterDbService = systemParameterDbService;
        _cacheDataService = cacheDataService;
    }

    public async Task Invoke()
    {
        using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
        _logger.LogInformation("{platform} Invoke NextSpinRecordSchedule on time : {time}", Platform.NEXTSPIN, DateTime.Now.ToLocalTime());


        // 取得當前時間，計算下一個匯總的時間
        var now = DateTime.Now.ToLocalTime();
        now = now.Add(GAP_TIME);
        var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        // 取得上次結束時間
        var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

        // 檢查有無資料，沒資料的話新增預設值
        if (parameter == null)
        {
            var value = DateTime.Now.ToLocalTime();
            var model = new t_system_parameter()
            {
                key = SYSTEM_PARAMETERS_KEY,
                value = value.ToString("yyyy-MM-dd HH:mm:ss"),
                min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                name = "NEXTSPIN取得注單排程",
                description = "NEXTSPIN記錄end_time"
            };

            var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
            if (postSystemParameter)
            {
                parameter = model;
            }
            else
            {
                return; // 新增失敗就結束排程
            }
        }

        if (int.Parse(parameter.min_value) == 0)
        {
            _logger.LogInformation("{platform} record stop time: {time}", Platform.NEXTSPIN, parameter.value);
            await Task.CompletedTask;
            return;
        }

        if (Convert.ToDateTime(parameter.value) >= nextTime)
        {
            _logger.LogInformation("return {platform} record schedule current Time : {now} report time : {reportTime} ", Platform.NEXTSPIN, now, parameter.value);
            return; // 時間不變就結束排程
        }

        var lastEndTime = DateTime.Parse(parameter.value);
        parameter.value = lastEndTime.Add(RANGE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss");
        try
        {
            await _systemParameterDbService.PutSystemParameter(parameter);

            var betInfos = await _apiInterfaceService.GetNextSpinRecord(lastEndTime, lastEndTime.Add(RANGE_OFFSET));

            betInfos = betInfos.DistinctBy(x => new { x.ticketId, x.ticketTime }).ToList();
            if (!betInfos.Any())
                return;

            await _apiInterfaceService.PostNextSpinRecordV2(betInfos.ToList());

        }
        catch (Exception ex)
        {
            TriggerFailOver(parameter);
            _logger.LogError(ex, "{action} {message}", nameof(NextSpinRecordSchedule), ex.Message);
        }
    }

    private async void TriggerFailOver(t_system_parameter parameter)
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        var failoverReq = new PullRecordFailoverRequest()
        {
            platform = Platform.NEXTSPIN,
            repairParameter = parameter.value,
            delay = TimeSpan.FromMinutes(5)
        };

        await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.NEXTSPIN}", failoverReq);
    }
}
