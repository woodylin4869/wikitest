using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.XG;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class XgRecordSchedule : IInvocable
    {
        private readonly ILogger<XgRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IDBService _dbService;
        private readonly IXGInterfaceService _xgInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const string SYSTEM_PARAMETERS_KEY = "XgRecordSchedule";
        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-6);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);
        private readonly TimeSpan XG_TIME = TimeSpan.FromHours(-12); // UTC-4
        private readonly string _xgAgentId;

        public XgRecordSchedule(ILogger<XgRecordSchedule> logger, IXGInterfaceService xgInterfaceService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _xgInterfaceService = xgInterfaceService;
            _xgAgentId = Config.CompanyToken.XG_AgentID;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke XgRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

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
                    value = nextTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "XG取得注單排程",
                    description = "XG記錄end_time"
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
                _logger.LogInformation("XgRecordSchedule stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-ddTHH:mm:ss");

            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                // 頁數1
                var Page = 1;
                var req = new GetBetRecordByTimeRequest
                {
                    // 請求 XG 起始時間 <= 回傳資料時間 <= 結束時間
                    // XG 請求時間以及回傳時間的欄位 皆是(UTC-4) 最後改在請求參數時先轉換
                    // XG 有限制 請求的起訖時間要小於1小時
                    AgentId = _xgAgentId,
                    StartTime = lastEndTime.AddHours(0).AddSeconds(1).Add(XG_TIME),
                    EndTime = nextTime.AddHours(0).AddSeconds(0).Add(XG_TIME),
                    Page = Page,
                    PageLimit = 10000
                };

                // 共用注單model
                GetBetRecordByTimeResponse.DataInfo res = new GetBetRecordByTimeResponse.DataInfo()
                {
                    Result = new List<GetBetRecordByTimeResponse.Result>()
                };

                // XG 有限制 請求的起訖時間要小於1小時
                if (req.EndTime.Subtract(req.StartTime).TotalMinutes > 59)
                {
                    req.EndTime = req.StartTime.AddMinutes(59).AddSeconds(59);
                    parameter.value = (req.EndTime.AddHours(12)).ToString("yyyy-MM-ddTHH:mm:ss");
                }

                await _systemParameterDbService.PutSystemParameter(parameter);

                // 拉取注單
                while (true)
                {
                    req.Page = Page;
                    // todo: 是否每 loop 回傳抓到資料 就先更新 parameter & page_index
                    var betData = await _gameApiService._XgAPI.GetBetRecordByTime(req);

                    // 拉帳回應錯誤 15	參數是必須的 或 數據格式錯誤 或 參數驗證錯誤	422
                    if (betData.Data.Pagination.TotalNumber == 0)
                    {
                        break;
                    }
                    res.Result.AddRange(betData.Data.Result);

                    Page++;
                    if (Page > betData.Data.Pagination.TotalPages)
                        break;

                    //api建議 ? 秒爬一次
                    //await Task.Delay(1000);
                }

                if (res.Result.Any())
                    await _xgInterfaceService.PostXGRecord(res.Result);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run XgRecordSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
