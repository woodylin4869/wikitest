using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// MG 交易記錄
    /// </summary>
    public class MgReportSchedule : IInvocable
    {
        /// <summary>
        /// DI注入提供寫入logger
        /// </summary>
        private readonly ILogger<MgReportSchedule> _logger;
        /// <summary>
        /// DI注入提供方法向客戶請求遠端資訊
        /// </summary>
        private readonly IGameApiService _gameApiService;
        /// <summary>
        /// DI注入提供方法表 t_game_report (日報表)
        /// </summary>
        private readonly GameRecordService _gameRecordService;
        /// <summary>
        /// DI注入提供方法存取資料表 t_system_parameter
        /// </summary>
        private readonly IDBService _dbService;
        /// <summary>
        /// 資料表 t_system_parameter 的 key 值
        /// </summary>
        private const string SYSTEM_PARAMETERS_KEY = "MgReportSchedule";

        private readonly ISystemParameterDbService _systemParameterDbService;

        /// <summary>
        /// 建構時用 DI 注入變數
        /// </summary>
        /// <param name="logger">DI注入提供寫入logger</param>
        /// <param name="gameaApiService">DI注入提供方法向客戶請求遠端資訊</param>
        /// <param name="gameRecordService">DI注入提供方法表 t_game_report (日報表)</param>
        /// <param name="dbService">DI注入提供方法存取資料表 t_system_parameter</param>
        public MgReportSchedule(ILogger<MgReportSchedule> logger
            , IGameApiService gameaApiService
            , GameRecordService gameRecordService
            , IDBService dbService
            , ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _gameRecordService = gameRecordService;
            _dbService = dbService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });

            _logger.LogInformation($"Invoke {SYSTEM_PARAMETERS_KEY} on time : {DateTime.Now.ToLocalTime()}");
            try
            {
                t_system_parameter parameter = null;

                // 取得當前時間，計算下一個匯總的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddHours(-2);// -2 是因為給 1 小時客戶的匯總執行時間 buffer，例如： 4:00 我方抓取 3 點的資料，客戶3:59:59執行匯總，至4:01執行匯總完，我方當下就會抓不到資料
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);// 取整點時間
                var fromDate = nextTime;

                // 取得同步 MG 每小時遊戲匯總報表的時間基準                
                parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var presetParameterNextTime = nextTime.AddHours(-5).ToString("yyyy-MM-dd HH:mm:ss");// 初始化，再 -5 是抓 5 小時前開始追回之前關閉排程的記錄

                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = presetParameterNextTime,
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "MG 每小時遊戲匯總報表排程",
                        description = "MG 紀錄排程時間基準點"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        //從預設時間開始匯總
                        fromDate = Convert.ToDateTime(presetParameterNextTime);
                    }
                    else
                    {
                        _logger.LogInformation("MG report schedule stop time: {time}", parameter.value);
                        return; // 新增失敗就結束排程
                    }
                }
                else
                {
                    if (int.Parse(parameter.min_value) == 0)// 排程開關 0: 關閉，這邊需要暫停執行排程
                    {
                        _logger.LogInformation("MG report schedule stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }

                    // 檢查之前記錄的時間是否追上了目前的時間，如果沒有就繼續執行排程
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        // 由資料庫取出的時間紀錄 (parameter.value) 的下一個小時開始匯總，並更新資料庫的時間紀錄 (parameter.value)
                        var lastReportTimebyDB = Convert.ToDateTime(parameter.value);                        
                        fromDate = lastReportTimebyDB.AddHours(1);
                        parameter.value = fromDate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        _logger.LogInformation("return MG report schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                        return; // 時間不變就結束排程
                    }
                }

                DateTime toDate = fromDate.AddHours(1);

                // 產生W1 MG 每小時報表
                // 【轉帳中心】(report_type = 1)統計注單，加總再寫到 t_game_report
                await _gameRecordService._mgInterfaceService.SummaryW1Report(fromDate);

                // 產生Game MG 每小時報表，加總再寫到 t_game_report
                // 【遊戲商】(report_type = 0)統計交易資料，加總再寫到 t_game_report
                await _gameRecordService._mgInterfaceService.SummaryGameProviderReport(fromDate);


                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run mg report schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
