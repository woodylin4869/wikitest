using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.CMD368;

public class CMDReportSchedule : IInvocable
{
    private readonly ILogger<CMDReportSchedule> _logger;
    private readonly ICMDInterfaceService _apiInterfaceService;
    private readonly IDBService _dbService;
    private readonly ISystemParameterDbService _systemParameterDbService;

    public const string SYSTEM_PARAMETERS_KEY = "CMDReportSchedule";
    public CMDReportSchedule(ILogger<CMDReportSchedule> logger, ICMDInterfaceService apiInterfaceService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
    {
        _logger = logger;
        _apiInterfaceService = apiInterfaceService;
        _dbService = dbService;
        _systemParameterDbService = systemParameterDbService;
    }

    /// <summary>
    /// 流程大綱
    /// 1. 取得匯總的時間
    /// 2. 轉帳中心的注單明細匯總後儲存到 DB
    /// </summary>
    /// <returns></returns>
    public async Task Invoke()
    {
        using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
        _logger.LogInformation("{platform} Invoke CMDReportSchedule on time : {time}", Platform.CMD368, DateTime.Now);
        try
        {
            t_system_parameter parameter = null;

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.AddHours(-2);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            // 取得同步 PME 每小時遊戲匯總報表的時間基準
            parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "CMD 每小時遊戲匯總報表排程",
                    description = "CMD 紀錄排程時間基準點"
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
            else
            {
                if (int.Parse(parameter.min_value) == 0)
                {
                    _logger.LogInformation("{platform} report schedule stop time: {time}", Platform.CMD368, parameter.value);
                    await Task.CompletedTask;
                    return;
                }
                if (Convert.ToDateTime(parameter.value) < nextTime)
                {
                    var lastReportTime = Convert.ToDateTime(parameter.value);
                    parameter.value = lastReportTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    _logger.LogInformation("return {platform} report schedule current Time : {now} report time : {reportTime} ", Platform.CMD368, now, parameter.value);
                    return; // 時間不變就結束排程
                }
            }

            // 產生W1 CMD368 每小時報表
            await _apiInterfaceService.SummaryW1Report(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value).AddHours(1));

            // 查詢時間寫回 DB
            await _systemParameterDbService.PutSystemParameter(parameter);
        }
        catch (Exception ex)
        {
            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
            _logger.LogError("Run {platform} report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", Platform.CMD368, ex.GetType().FullName, ex.Message, errorFile, errorLine);
        }
    }
}
