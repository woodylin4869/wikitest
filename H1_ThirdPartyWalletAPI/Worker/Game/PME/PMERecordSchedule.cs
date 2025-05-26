using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.PME;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.PME;

public class PMERecordSchedule : IInvocable
{
    private readonly ILogger<PMERecordSchedule> _logger;
    private readonly IPMEInterfaceService _apiInterfaceService;
    private readonly IDBService _dbService;
    private readonly ICacheDataService _cacheDataService;
    private readonly ISystemParameterDbService _systemParameterDbService;

    private const string SYSTEM_PARAMETERS_KEY = "PMERecordSchedule";

    private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-6);
    private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

    public PMERecordSchedule(ILogger<PMERecordSchedule> logger, IPMEInterfaceService apiInterfaceService, IDBService dbService, ICacheDataService cacheDataService, ISystemParameterDbService systemParameterDbService)
    {
        _logger = logger;
        _apiInterfaceService = apiInterfaceService;
        _dbService = dbService;
        _cacheDataService = cacheDataService;
        _systemParameterDbService = systemParameterDbService;
    }

    public async Task Invoke()
    {
        using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });

        _logger.LogInformation("{platform} Invoke PMERecordSchedule on time : {time}", Platform.PME, DateTime.Now.ToLocalTime());


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
                name = "PME取得注單排程",
                description = "PME記錄end_time"
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
            _logger.LogInformation("{platform} record stop time: {time}", Platform.PME, parameter.value);
            await Task.CompletedTask;
            return;
        }

        if (Convert.ToDateTime(parameter.value) >= nextTime)
        {
            _logger.LogInformation("return {platform} record schedule current Time : {now} report time : {reportTime} ", Platform.PME, now, parameter.value);
            return; // 時間不變就結束排程
        }

        var lastEndTime = DateTime.Parse(parameter.value);
        parameter.value = lastEndTime.Add(RANGE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss");
        await _systemParameterDbService.PutSystemParameter(parameter);
        try
        {
            var betInfos = await _apiInterfaceService.GetPMERecord(lastEndTime, lastEndTime.Add(RANGE_OFFSET));

            if (!betInfos.Any())
                return;

            foreach (var group in betInfos.GroupBy(b => b.bet_time / (long)TimeSpan.FromHours(3).TotalMilliseconds))
            {
                await _apiInterfaceService.PostPMERecord(group);
            }
        }
        catch (Exception ex)
        {
            TriggerFailOver(parameter);
            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
            _logger.LogError("Run {platform} record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", Platform.PME, ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
            platform = Platform.PME,
            repairParameter = parameter.value,
            delay = TimeSpan.FromMinutes(5)
        };

        await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.PME}", failoverReq);
    }
}
