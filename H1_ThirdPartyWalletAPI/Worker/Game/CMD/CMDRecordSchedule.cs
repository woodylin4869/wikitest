using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.CMD368;

public class CMDRecordSchedule : IInvocable
{
    private readonly ILogger<CMDRecordSchedule> _logger;
    private readonly ICMDInterfaceService _apiInterfaceService;
    private readonly IDBService _dbService;
    private readonly ICacheDataService _cacheDataService;
    private readonly ISystemParameterDbService _systemParameterDbService;

    private const string SYSTEM_PARAMETERS_KEY = "CMDRecordSchedule";

    private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-6);
    private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

    public CMDRecordSchedule(ILogger<CMDRecordSchedule> logger, ICMDInterfaceService apiInterfaceService, IDBService dbService, ICacheDataService cacheDataService, ISystemParameterDbService systemParameterDbService)
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

        _logger.LogInformation("{platform} Invoke CMDRecordSchedule on time : {time}", Platform.CMD368, DateTime.Now.ToLocalTime());


        // 取得當前時間，計算下一個匯總的時間
        var now = DateTime.Now.ToLocalTime();
        now = now.Add(GAP_TIME);
        var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

        // 取得上次結束時間
        var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

        // 檢查有無資料，沒資料的話新增預設值
        if (parameter == null)
        {
            var model = new t_system_parameter()
            {
                key = SYSTEM_PARAMETERS_KEY,
                value = "0",
                min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                name = "CMD取得注單排程",
                description = "CMD記錄MaxVersion"
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
            _logger.LogInformation("{platform} record stop time: {time}", Platform.CMD368   , parameter.value);
            await Task.CompletedTask;
            return;
        }

        var version = long.Parse(parameter.value);

        try
        {
            var betInfos = await _apiInterfaceService.GetCMDRecord(version);

            if (!betInfos.Any())
                return;

            foreach (var group in betInfos.GroupBy(b => b.TransDate / (long)TimeSpan.FromHours(3).Ticks))
            {
                await _apiInterfaceService.PostCMDRecord(group);
            }

            parameter.value = betInfos.Max(b => b.Id).ToString();

            await _systemParameterDbService.PutSystemParameter(parameter);
        }
        catch (Exception ex)
        {
            TriggerFailOver(parameter);
            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
            _logger.LogError("Run {platform} record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", Platform.CMD368, ex.GetType().FullName, ex.Message, errorFile, errorLine);
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
            platform = Platform.CMD368,
            repairParameter = parameter.value,
            delay = TimeSpan.FromMinutes(5)
        };

        await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform.CMD368}", failoverReq);
    }
}
