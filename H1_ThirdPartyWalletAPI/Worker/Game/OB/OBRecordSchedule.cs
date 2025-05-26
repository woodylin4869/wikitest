using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.OB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class OBRecordSchedule : IInvocable
    {
        private readonly ILogger<OBRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IOBInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "OBRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public OBRecordSchedule(ILogger<OBRecordSchedule> logger, IOBInterfaceService apiInterfaceService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke OBRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "OB取得注單排程",
                    description = "OB記錄end_time"
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
                _logger.LogInformation("OB record stop time: {time}", parameter.value);
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
                var req = new BetHistoryRecordReqserver
                {
                    startTime = lastEndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = nextTime.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                    pageIndex = Page,
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                };

                if ((nextTime - lastEndTime).TotalMinutes > 30)
                {
                    req.endTime = lastEndTime.AddMinutes(30).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                    parameter.value = lastEndTime.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss");
                }

                var res = new List<BetHistoryRecordResponse.Record>();
                while (true)
                {
                    req.pageIndex = Page;
                    var betLogs = await _gameApiService._OBApi.BetHistoryRecordAsync(req);

                    if (betLogs.data.pageSize == 0|| betLogs.code=="92222")//訪問受限
                    {
                        break;
                    }
                    res.AddRange(betLogs.data.record);

                    Page++;
                    if (Page > betLogs.data.totalPage)
                        break;
                    //api建議4秒爬一次
                    await Task.Delay(4000);
                }
                if (!res.Any())
                {
                    await _systemParameterDbService.PutSystemParameter(parameter);
                    return;
                }
                await _apiInterfaceService.PostOBRecord(res);

                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run OB record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
