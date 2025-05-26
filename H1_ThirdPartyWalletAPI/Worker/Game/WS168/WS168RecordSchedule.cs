using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.WS168;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class WS168RecordSchedule : IInvocable
    {
        private readonly ILogger<WS168RecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IWS168InterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ICacheDataService _cacheDataService;

        private const string SYSTEM_PARAMETERS_KEY = "WS168RecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public WS168RecordSchedule(ILogger<WS168RecordSchedule> logger, IWS168InterfaceService apiInterfaceService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
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
            _logger.LogInformation("Invoke WS168RecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "WS168取得注單排程",
                    description = "WS168記錄end_time"
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
                _logger.LogInformation("WS168 record stop time: {time}", parameter.value);
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
                string[] type_array = new string[] { "bet_at", "settled_at" };

                var req = new BetLogRequest
                {
                    time_type = "",
                    start_time = lastEndTime.ToUniversalTime(),
                    end_time = nextTime.AddSeconds(-1).ToUniversalTime(),
                    page = 1,
                    page_size = 10000
                };

                if ((nextTime - lastEndTime).TotalMinutes > 60)
                {
                    req.end_time = lastEndTime.AddHours(1).AddSeconds(-1).ToUniversalTime();
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                }
                await _systemParameterDbService.PutSystemParameter(parameter);

                var res = new List<SearchingOrdersStatusResponse.Datum>();
                var resPK = res.Select(x => new { x.slug, x.status, x.settled_at }).ToHashSet();

                foreach (var time_type in type_array)
                {
                    req.time_type = time_type;
                    var Page = 1;
                    while (true)
                    {
                        req.page = Page;
                        var betLogs = await _gameApiService._Ws168API.BetLogAsync(req);

                        if (betLogs.total_count == 0)
                        {
                            break;
                        }
                        foreach (var itme in betLogs.data)
                        {
                            if (resPK.Add(new { itme.slug, itme.status, itme.settled_at}))
                            {
                                res.Add(itme);
                            }
                        }
                        Page++;
                        if (Page > betLogs.total_page)
                            break;
                        //api建議11秒爬一次
                        await Task.Delay(11000);
                    }
                    await Task.Delay(11000);
                }
                if (!res.Any())
                {
                    return;
                }
                await _apiInterfaceService.PostWS168Record(res);


            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run WS168 record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform PlatformWS168 = Platform.WS168;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformWS168,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformWS168}", failoverReq);
        }
    }
}
