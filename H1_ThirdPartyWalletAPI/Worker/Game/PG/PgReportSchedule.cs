using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 同步 PG 每小時遊戲匯總報表排程
    /// </summary>
    public class PgReportSchedule : IInvocable
    {
        private readonly ILogger<PgReportSchedule> _logger;
        private readonly GameRecordService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public PgReportSchedule(ILogger<PgReportSchedule> logger, GameRecordService gameRecordService, ICommonService commonService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameRecordService = gameRecordService;
            _commonService = commonService;
            _systemParameterDbService = systemParameterDbService;
        }

        /// <summary>
        /// 流程大綱
        /// 1. 取得匯總的時間
        /// 2. 根據匯總時間向遊戲商查詢匯總報表
        /// 3. 儲存遊戲商匯總報表到 DB
        /// 4. 將遊戲商的匯總報表轉換成轉帳中心格式儲存到 DB
        /// 5. 轉帳中心的注單明細匯總後儲存到 DB
        /// </summary>
        /// <returns></returns>
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke PgReportSchedule on time : {time}", DateTime.Now);
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個匯總的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddHours(-3);
                var nextReportTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                var key = "PgReportSchedule";

                // 取得同步 PG 每小時遊戲匯總報表的時間基準
                parameter = await _systemParameterDbService.GetSystemParameter(key);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextReportTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = string.Format("{0}", 1),
                        name = "PG 每小時遊戲匯總報表排程",
                        description = "PG 紀錄排程時間基準點"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                    }
                    else
                    {
                        _logger.LogInformation("return pg report schedule current Time : {now} report time : {report time} ", now, parameter.value);
                        return; // 新增失敗就結束排程
                    }
                }
                else
                {
                    if (int.Parse(parameter.min_value) == 0)
                    {
                        _logger.LogInformation("PG report schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextReportTime)
                    {
                        var lastReportTime = Convert.ToDateTime(parameter.value);
                        parameter.value = lastReportTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }
                // 產生W1 PG 每小時報表
                await _gameRecordService._pgInterfaceService.SummaryW1Report(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value).AddHours(1));
                // 產生Game PG 每小時報表
                await _gameRecordService._pgInterfaceService.SummaryGameProviderReport(DateTime.Parse(parameter.value), DateTime.Parse(parameter.value).AddHours(1));
                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }

            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run pg report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            finally
            {
            }
        }
    }
}
