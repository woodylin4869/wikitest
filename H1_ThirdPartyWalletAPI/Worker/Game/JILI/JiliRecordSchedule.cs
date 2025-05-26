using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses.GetBetRecordByTimeResponse;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class JiliRecordSchedule : IInvocable
    {
        private readonly ILogger<JiliRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "JiliRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public JiliRecordSchedule(ILogger<JiliRecordSchedule> logger, GameRecordService gameRecordService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _gameRecordService = gameRecordService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke JiliRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "JILI取得注單排程",
                    description = "JILI記錄end_time"
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
                _logger.LogInformation("jili record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                var Page = 1;
                var req = new GetBetRecordByTimeRequest
                {
                    StartTime = lastEndTime.AddHours(-12),
                    EndTime = nextTime.AddHours(-12).AddSeconds(-1),
                    Page = Page,
                    PageLimit = 10000,
                    FilterAgent = 1
                };

                if ((nextTime.AddHours(-12) - lastEndTime.AddHours(-12)).TotalMinutes > 60)
                {
                    req.EndTime = req.StartTime.AddHours(1).AddSeconds(-1);
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                }

                GetBetRecordByTimeData res = new GetBetRecordByTimeData()
                {
                    Result = new List<Result>()
                };
                while (true)
                {
                    req.Page = Page;
                    var betLogs = await _gameApiService._JiliApi.GetBetRecordByTimeAsync(req);

                    if (betLogs.Data.Result.Count == 0)
                    {
                        break;
                    }
                    res.Result.AddRange(betLogs.Data.Result);

                    Page++;
                    if (Page > betLogs.Data.Pagination.TotalPages)
                        break;
                    //api建議20~30秒爬一次
                    await Task.Delay(1000);
                }


                if (!res.Result.Any())
                {
                    await _systemParameterDbService.PutSystemParameter(parameter);
                    return;
                }

                await _gameRecordService._jiliInterfaceService.PostJiliRecord(res);

                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run jili record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
