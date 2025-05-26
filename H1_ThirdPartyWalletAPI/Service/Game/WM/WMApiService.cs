using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.WM
{
    public class WMApiService : IWMApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WMApiService> _logger;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> recordLockStore = new();
        public WMApiService(IHttpClientFactory httpClientFactory, ILogger<WMApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<WMResponse> MemberRegisterAsync(MemberRegisterRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<MemberRegisterRequest, WMResponse>(url, source);
        }
        public async Task<WMResponse> SigninGameAsync(SigninGameRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<SigninGameRequest, WMResponse>(url, source);
        }

        public async Task<WMResponse> LogoutGameAsync(LogoutGameRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<LogoutGameRequest, WMResponse>(url, source);
        }

        public async Task<WMResponse> GetBalanceAsync(GetBalanceRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<GetBalanceRequest, WMResponse>(url, source);
        }

        public async Task<WMBalanceResponse> ChangeBalanceAsync(ChangeBalanceRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<ChangeBalanceRequest, WMBalanceResponse>(url, source);
        }

        public async Task<WMTradeResponse> GetMemberTradeReportAsync(GetMemberTradeReportRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<GetMemberTradeReportRequest, WMTradeResponse>(url, source);
        }

        public async Task<WMDataReportResponse> GetDateTimeReportAsync(GetDateTimeReportRequest source)
        {
            var recordLock = recordLockStore.GetOrAdd(source.vendorId, id => new(1));
            await recordLock.WaitAsync();
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(35));
                recordLock.Release();
            });

            var url = Config.GameAPI.WM_URL;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await ApiHandle<GetDateTimeReportRequest, WMDataReportResponse>(url, source);
        }

        public async Task<WMResponse> EditLimitAsync(EditLimitRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<EditLimitRequest, WMResponse>(url, source);
        }

        public async Task<HelloResponse> HelloAsync(HelloRequest source)
        {
            var url = Config.GameAPI.WM_URL;
            return await ApiHandle<HelloRequest, HelloResponse>(url, source);
        }
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {

            var headers = new Dictionary<string, string>
            { };
            var postData = new Dictionary<string, string>
            { };

            var DataDictionary = Helper.GetDictionary(source);
            var keyValueList = new List<string>();
            foreach (var item in DataDictionary)
            {
                keyValueList.Add(item.Key + "=" + item.Value);
            }
            url += "?" + string.Join("&", keyValueList);
            _logger.LogInformation("WM RequestPath: {RequestPath}", url);

            var responseData = await Post(url, postData, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }

        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 3)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                using (var request = _httpClientFactory.CreateClient("log"))
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    request.Timeout = TimeSpan.FromSeconds(14);
                    if (url.Contains("cmd=GetDateTimeReport")) request.Timeout = TimeSpan.FromSeconds(60);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.WM, url, new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var dics = new Dictionary<string, object>
                    {
                        { "request", postData },
                        { "response", body }
                    };

                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }

                    return body;
                }
            }
            catch (HttpRequestException ex)
            {
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call WMApi Failed:{0}", url), ex);
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }
    }
}
