using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public interface IApiHealthCheckService
    {
        void SetResponseData(Platform platform, ApiResponseData apiResponseData);
        Task<List<ApiHealthInfo>> SetAllHealthInfo();
        Task<List<ApiHealthInfo>> GetAllHealthInfo();
        Task<bool> DeleteAllHealthInfo();
        Task<ApiHealthInfo> SetPlatformHealthInfo(PutApiHealthReq req);
        Task<ApiHealthInfo> GetPlatformHealthInfo(Platform platform);
    }
    public class ApiHealthCheckService : IApiHealthCheckService
    {

        private readonly ILogger<ApiHealthCheckService> _logger;
        private readonly ICacheDataService _cacheDataService;

        private int _cacheSeconds = 60 * 12;
        private int _timeoffset = -10;
        private int _delayLimit = 3000;
        private int _TimeoutLimit = 15000;
        private int _delayCountLimit = 10;
        private int _TimeoutCountLimit = 10;

        public ApiHealthCheckService(ILogger<ApiHealthCheckService> logger, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _cacheDataService = cacheDataService;
        }
        public async void SetResponseData(Platform platform, ApiResponseData apiResponseData)
        {
            try
            {
                var secondsSummary = await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{platform}/{apiResponseData.reqDateTime}",
                    () => Task.FromResult(new ApiResponseSecondSummary() { SummaryTime = apiResponseData.reqDateTime })
                    , _cacheSeconds);
                secondsSummary.TotalCount++;
                secondsSummary.TotalElapsedMilliseconds += apiResponseData.ElapsedMilliseconds;
                secondsSummary.MaxElapsedMilliseconds = Math.Max(secondsSummary.MaxElapsedMilliseconds, apiResponseData.ElapsedMilliseconds);
                if (apiResponseData.ElapsedMilliseconds > _TimeoutLimit) secondsSummary.TotalTimeOutCount++;
                if (apiResponseData.ElapsedMilliseconds > _delayLimit) secondsSummary.TotalDelayCount++;
                await _cacheDataService.StringSetAsync($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{platform}/{apiResponseData.reqDateTime}", secondsSummary, _cacheSeconds);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "SetResponseData exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
        public async Task<List<ApiHealthInfo>> SetAllHealthInfo()
        {

            List<ApiHealthInfo> apiHealthInfoList = new List<ApiHealthInfo>();
            var dtnow = DateTime.Now;
            var oldInfo = await _cacheDataService.StringGetAsync<List<ApiHealthInfo>>($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL");

            List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));
            foreach (string r in openGame)
            {

                ApiHealthInfo lastData = null;
                if (oldInfo != null)
                {
                    var search = oldInfo.Where(x => x.Platform.ToLower() == r.ToLower());
                    if (search.Any())
                    {
                        lastData = search.Single();
                    }
                }
                var nowTimeOffset = DateTimeOffset.Now;
                var HealthData = await GetHealthData((Platform)Enum.Parse(typeof(Platform), r.ToUpper()), nowTimeOffset.AddMinutes(_timeoffset).ToUnixTimeSeconds(), nowTimeOffset.ToUnixTimeSeconds());
                if (HealthData != null && HealthData.Any())
                {

                    //else if (lastData is { Status: > Status.DELAY })
                    //{
                    //    //若狀態是維護或Timeout則保留舊資料
                    //    apiHealthInfoData.Status = lastData.Status;
                    //    apiHealthInfoData.Operator = lastData.Operator;
                    //    apiHealthInfoData.SuspendTime = lastData.SuspendTime;
                    //}

                    ApiHealthInfo apiHealthInfoData = new ApiHealthInfo();
                    apiHealthInfoData.Count = HealthData.Sum(s => s.TotalCount);
                    apiHealthInfoData.Platform = r;
                    apiHealthInfoData.TimeOutCount = HealthData.Sum(s => s.TotalTimeOutCount);
                    apiHealthInfoData.MaxElapsedMilliseconds = HealthData.Max(x => x.MaxElapsedMilliseconds);
                    apiHealthInfoData.AvgMaxElapsedMilliseconds = HealthData.Sum(s => s.TotalElapsedMilliseconds) / HealthData.Sum(s => s.TotalCount);
                    apiHealthInfoData.UpdateTime = dtnow;
                    var DelayCount = HealthData.Sum(s => s.TotalDelayCount);
                    if (lastData != null && lastData.Operator != null)
                    {
                        apiHealthInfoData.Status = lastData.Status;
                        apiHealthInfoData.Operator = lastData.Operator;
                        apiHealthInfoData.SuspendTime = lastData.SuspendTime;
                    }
                    else
                    {
                        apiHealthInfoData.Status = Status.NORMAL;
                        if (DelayCount >= _delayCountLimit)
                        {
                            apiHealthInfoData.Status = Status.DELAY;
                        }
                        if (apiHealthInfoData.TimeOutCount >= _TimeoutCountLimit)
                        {
                            apiHealthInfoData.Status = Status.TIMEOUT;
                            apiHealthInfoData.SuspendTime = dtnow;
                        }


                        //當上次狀態為正常，切換為狀態是維護或Timeout 發送Slack通知
                        if (lastData != null && lastData.Status == Status.NORMAL && apiHealthInfoData.Status == Status.TIMEOUT && Config.OneWalletAPI.Prefix_Key == "prd")
                        {
                            string url = "";
                            string requestData = "";

                            var GetHealthCheckUrl = await _cacheDataService.StringGetAsync<dynamic>($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{apiHealthInfoData.Platform}/LOG");
                            if (GetHealthCheckUrl != null)
                            {
                                url = GetHealthCheckUrl.url;
                                requestData = GetHealthCheckUrl.requestData;
                            }
                            // _logger.LogInformation($"SlackWebHook SendMessage Platform:{apiHealthInfoData.Platform},lastStatus:{lastData.Status},newStatus:{apiHealthInfoData.Status}");
                            SlackWebHook _slack = new SlackWebHook();
                            await _slack.SendMessageAsync(apiHealthInfoData.Platform, apiHealthInfoData.Status.ToString(), apiHealthInfoData.SuspendTime, url, requestData, "", "W1Api健康狀態資訊");

                        }


                    }
                    apiHealthInfoList.Add(apiHealthInfoData);
                }
                else
                {
                    //10分鐘內若無API資料判斷舊狀態非正常要保留狀態
                    if (lastData != null && lastData.Status > Status.DELAY)
                    {
                        apiHealthInfoList.Add(lastData);
                    }
                    else
                    {
                        ApiHealthInfo apiData = new ApiHealthInfo();
                        apiData.Platform = r.ToUpper();
                        apiData.UpdateTime = dtnow;
                        apiHealthInfoList.Add(apiData);
                    }
                }
            }
            return apiHealthInfoList;
        }
        public async Task<List<ApiHealthInfo>> GetAllHealthInfo()
        {
            var result = await _cacheDataService.StringGetAsync<List<ApiHealthInfo>>($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL");
            if (result == null)
            {
                return new List<ApiHealthInfo>();
            }
            else
            {
                return result;
            }
        }
        public async Task<bool> DeleteAllHealthInfo()
        {
            return await _cacheDataService.KeyDelete($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL");
        }
        private Task<List<ApiResponseSecondSummary>> GetHealthData(Platform platform, long startTime, long endTime)
        {
            var keys = new List<string>();
            for (var time = startTime; time <= endTime; time++)
                keys.Add($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{platform}/{time}");

            return _cacheDataService.BatchStringGetAsync<ApiResponseSecondSummary>(keys);
        }
        public async Task<ApiHealthInfo> SetPlatformHealthInfo(PutApiHealthReq req)
        {
            var dtNow = DateTime.Now;
            var oldInfo = await _cacheDataService.StringGetAsync<List<ApiHealthInfo>>($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL");
            if (oldInfo != null)
            {
                var model = oldInfo.Where(x => x.Platform.ToLower() == req.Platform.ToLower()).FirstOrDefault();
                if (model != null)
                {
                    if (req.Status == Status.TIMEOUT || req.Status == Status.MAINTAIN)
                    {
                        model.SuspendTime = dtNow;
                        model.Operator = req.Operator;
                    }
                    else if (req.Status == Status.NORMAL)
                    {
                        await _cacheDataService.KeyDelete($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{req.Platform.ToUpper()}");
                        model.Operator = null;

                        //if (model.Status == Status.TIMEOUT)
                        //{
                        //    //手動由TIMEOUT 切換回NORMAL 發送SLACK通知
                        //    SlackWebHook.SendMessage(req.Platform, Status.NORMAL.ToString(), DateTime.MinValue, $"{req.Operator} 啟用 遊戲館:{req.Platform} TIMEOUT => NORMAL", "W1Api健康狀態資訊");
                        //}
                    }
                    else
                    {
                        model.Operator = null;
                    }
                    model.UpdateTime = dtNow;
                    model.Status = req.Status;
                    await _cacheDataService.StringSetAsync($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL", oldInfo, 6000);
                    return model;
                }
                else
                {
                    ApiHealthInfo apiData = new ApiHealthInfo();
                    apiData.Platform = req.Platform;
                    apiData.SuspendTime = dtNow;
                    if (req.Status == Status.TIMEOUT || req.Status == Status.MAINTAIN)
                    {
                        apiData.SuspendTime = dtNow;
                        apiData.Operator = req.Operator;
                    }
                    apiData.Status = req.Status;
                    apiData.UpdateTime = dtNow;
                    oldInfo.Add(apiData);
                    await _cacheDataService.StringSetAsync($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL", oldInfo, 6000);
                    return apiData;
                }

            }
            else
            {
                return null;
            }
        }
        public async Task<ApiHealthInfo> GetPlatformHealthInfo(Platform platform)
        {
            try
            {
                var allInfo = await _cacheDataService.StringGetAsync<List<ApiHealthInfo>>($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/ALL");
                if (allInfo != null)
                {
                    var paltformInfo = allInfo.Where(x => x.Platform.ToUpper() == platform.ToString().ToUpper()).FirstOrDefault();
                    return paltformInfo;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("GetPlatformHealthInfo exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return null;
            }
        }
    }

    public class ApiResponseData
    {
        public long reqDateTime { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public ApiResponseData()
        {
            reqDateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }

    public class ApiResponseSecondSummary
    {
        public long SummaryTime { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
        public int TotalCount { get; set; } = 0;
        public long TotalElapsedMilliseconds { get; set; } = 0L;
        public int TotalTimeOutCount { get; set; } = 0;
        public int TotalDelayCount { get; set; } = 0;
        public long MaxElapsedMilliseconds { get; set; } = long.MinValue;
    }
}
