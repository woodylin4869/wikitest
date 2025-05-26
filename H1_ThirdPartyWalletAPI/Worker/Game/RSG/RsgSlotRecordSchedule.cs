using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ThirdPartyWallet.Common;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RSG
{
    public class RsgSlotRecordSchedule : IInvocable
    {
        private ConcurrentQueue<Exception> _exceptions = new ConcurrentQueue<Exception>();
        private readonly ILogger<RsgSlotRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IRsgH1InterfaceService _rsgInterfaceService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly LogHelper<RsgSlotRecordSchedule> _logHelper;
        private static DateTime last_endtime;
        private const int defaultPastTime = 5;
        private const int retry = 2;
        private readonly IMemoryCache _memoryCache;
        private const int memory_cache_min = 30; //分鐘
        private const int pageLimit = 20000;
        private const string memory_cache_key = "RSG_System_Web_Code";

        public const string PARAMETER_KEY = "RSGSlotSchedule";

        public RsgSlotRecordSchedule(ILogger<RsgSlotRecordSchedule> logger,
            IGameApiService gameaApiService,
            IRsgH1InterfaceService rsgInterfaceService,
            ICommonService commonService,
            IMemoryCache memoryCache,
            ISystemParameterDbService systemParameterDbService,
            LogHelper<RsgSlotRecordSchedule> logHelper)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _rsgInterfaceService = rsgInterfaceService;
            _commonService = commonService;
            last_endtime = DateTime.Now.AddMinutes(-defaultPastTime);
            last_endtime = new DateTime(last_endtime.Year, last_endtime.Month, last_endtime.Day, last_endtime.Hour, last_endtime.Minute, 0);
            _memoryCache = memoryCache;
            _systemParameterDbService = systemParameterDbService;
            _logHelper = logHelper;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var totalCount = 0;

            try
            {
                // 清空 Exception
                _exceptions.Clear();

                t_system_parameter parameter = null;

                // 取得RSG老虎機拉單排程時間
                parameter = await _systemParameterDbService.GetSystemParameter(PARAMETER_KEY);

                // 取得當前時間，計算下一個拉單的時間
                var dt = DateTime.Now.ToLocalTime().AddMinutes(-defaultPastTime);
                var nextTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = PARAMETER_KEY,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "RSG老虎機拉單排程時間",
                        description = "RSG老虎機拉單排程時間時間基準點"
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
                    if (Convert.ToDateTime(parameter.value) < Convert.ToDateTime(nextTime))
                    {
                        var lastReportTime = Convert.ToDateTime(parameter.value);
                        parameter.value = lastReportTime.AddMinutes(1).ToString("yyyy-MM-dd HH:mm");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }

                // 排程開關
                if (int.Parse(parameter.min_value) == 0)
                {
                    _logHelper.ScheduleLog(GetType().Name, totalCount.ToString(), "Stop", sw.ElapsedMilliseconds);
                    await Task.CompletedTask;
                    return;
                }

                // 取得SystemCode、WebId
                var AgentList = await _memoryCache.GetOrCreateAsync(memory_cache_key, async entry =>
                {
                    var AgentList = await _commonService._serviceDB.GetRSgSystemWebCode();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(memory_cache_min));
                    _memoryCache.Set(memory_cache_key, AgentList, cacheEntryOptions);
                    return AgentList;
                });

                // 查詢時間寫回 DB
                await _systemParameterDbService.PutSystemParameterValue(parameter);

                // 取老虎機資料
                foreach (var agent in AgentList)
                {
                    // Task 集合
                    var taskList = new List<Task<int>>();

                    //先取得第一頁資料
                    var responseData = await GetRsgRecordDetailWithRetryAsync(new GetPagedGameDetailRequest()
                    {
                        SystemCode = Config.CompanyToken.RSG_SystemCode,
                        WebId = agent.web_id,
                        GameType = 1,
                        TimeStart = parameter.value,
                        TimeEnd = parameter.value,
                        Page = 1,
                        Rows = pageLimit
                    }, retry);

                    // 取得總頁數
                    var pageCount = responseData.Data.PageCount;
                    //var pageCount = (responseData.Data.DataCount / pageLimit) + 1; //原方法整除後會有多出空白頁

                    // 第一頁寫單非同步作業
                    taskList.Add(Task.Run(async () =>
                    {
                        try
                        {
                            return await PostRsgRecordDetailWithRetryAsync(responseData.Data.GameDetail, retry);
                        }
                        catch (Exception ex)
                        {
                            _exceptions.Enqueue(ex);
                            return 0;
                        }
                    }));

                    // 第二頁以後的拉單/寫單非同步作業
                    for (var i = 2; i <= pageCount; i++)
                    {
                        taskList.Add(GetAndPostRsgRecordDetailAsync(new GetPagedGameDetailRequest()
                        {
                            SystemCode = Config.CompanyToken.RSG_SystemCode,
                            WebId = agent.web_id,
                            GameType = 1,
                            TimeStart = parameter.value,
                            TimeEnd = parameter.value,
                            Page = i,
                            Rows = pageLimit
                        }));
                    }

                    // 本次處理的資料比數
                    var result = await Task.WhenAll(taskList);
                    Interlocked.Add(ref totalCount, result.Sum());
                }

                await Task.CompletedTask;

                if (!_exceptions.IsEmpty)
                {
                    throw new AggregateException(_exceptions);
                }

                _logHelper.ScheduleLog(GetType().Name, totalCount.ToString(), "Success", sw.ElapsedMilliseconds);
            }
            catch (AggregateException error)
            {
                foreach (Exception item in error.InnerExceptions)
                {
                    _logHelper.ScheduleErrorLog(item, GetType().Name, totalCount.ToString(), "Fail", sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                _logHelper.ScheduleErrorLog(ex, GetType().Name, totalCount.ToString(), "Fail", sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// 拉單後寫單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<int> GetAndPostRsgRecordDetailAsync(GetPagedGameDetailRequest request)
        {
            try
            {
                var response = await GetRsgRecordDetailWithRetryAsync(request, retry);
                return await PostRsgRecordDetailWithRetryAsync(response.Data.GameDetail, retry);
            }
            catch (Exception ex)
            {
                _exceptions.Enqueue(ex);
            }

            return 0;
        }

        /// <summary>
        /// 拉單，包含重試
        /// </summary>
        /// <param name="request"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<GetPagedGameDetailResponse> GetRsgRecordDetailWithRetryAsync(GetPagedGameDetailRequest request, int retry)
        {
            try
            {
                var response = await _gameApiService._RsgAPI.GetPagedGameDetailAsync(request);
                if (response.ErrorCode != (int)ErrorCodeEnum.OK)
                {
                    throw new Exception(response.ErrorMessage);
                }

                return response;
            }
            catch (Exception ex)
            {
                if (retry > 0)
                {
                    _logHelper.ScheduleErrorLog(ex, GetType().Name, string.Empty, "Retry API");

                    retry--;
                    var random = new Random();
                    var randomDelay = random.Next(1, 1500);
                    await Task.Delay(1000 + randomDelay);
                    return await GetRsgRecordDetailWithRetryAsync(request, retry);
                }

                throw;
            }
        }

        /// <summary>
        /// 寫單，包含重試
        /// </summary>
        /// <param name="request"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<int> PostRsgRecordDetailWithRetryAsync(List<GameDetail> request, int retry)
        {
            try
            {
                return await _rsgInterfaceService.PostRsgRecordDetail(request);
            }
            catch (Exception ex)
            {
                if (retry > 0)
                {
                    _logHelper.ScheduleErrorLog(ex, GetType().Name, string.Empty, "Retry DB");

                    retry--;
                    var random = new Random();
                    var randomDelay = random.Next(1, 1500);
                    await Task.Delay(1000 + randomDelay);
                    return await PostRsgRecordDetailWithRetryAsync(request, retry);
                }

                throw;
            }
        }
    }
}