using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Request;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Dapper;
using H1_ThirdPartyWalletAPI.Extensions;

namespace H1_ThirdPartyWalletAPI.Service.Game.WS168
{
    public class WS168ApiService : IWS168ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WS168ApiService> _logger;

        public WS168ApiService(IHttpClientFactory httpClientFactory, ILogger<WS168ApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/players";
            return await ApiHandle<CreatePlayerRequest, CreatePlayerResponse>(url, source);
        }
        /// <summary>
        /// LOGIN URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<PlayerLoginResponse> PlayerLoginAsync(PlayerLoginRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/player/login";

            return await ApiHandle<PlayerLoginRequest, PlayerLoginResponse>(url, source);
        }
        /// <summary>
        /// LOGOUT URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<PlayerLogoutResponse> PlayerLogoutAsync(PlayerLogoutRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/player/logout";

            return await ApiHandle<PlayerLogoutRequest, PlayerLogoutResponse>(url, source);
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<DepositResponse> DepositAsync([FromBody] DepositRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/player/deposit";

            return await ApiHandle<DepositRequest, DepositResponse>(url, source);
        }
        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/player/withdraw";

            return await ApiHandle<WithdrawRequest, WithdrawResponse>(url, source);
        }
        /// <summary>
        /// 查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<QueryPlayerBalanceResponse> QueryPlayerBalanceAsync(QueryPlayerBalanceRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/player/balance";

            return await ApiGet<QueryPlayerBalanceRequest, QueryPlayerBalanceResponse>(url, source);
        }


        /// <summary>
        /// 查詢交易狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<SearchingOrdersStatusResponse> SearchingOrdersStatusAsync(SearchingOrdersStatusRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/player/check";

            return await ApiGet<SearchingOrdersStatusRequest, SearchingOrdersStatusResponse>(url, source);
        }
        /// <summary>
        /// 取得住單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<SearchingOrdersStatusResponse> BetLogAsync([FromBody] BetLogRequest source)
        {
            var url = Config.GameAPI.WS168_URL + "api/merchant/bets";

            return await ApiGet<BetLogRequest, SearchingOrdersStatusResponse>(url, source);
        }

        private async Task<TResponse> ApiGet<TRequest, TResponse>(string url, TRequest source)
        {

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" },
                { "Accept","application/json" },
                { "Authorization","Bearer "+Config.CompanyToken.WS168_AgentToken}
            };

            var Data = Helper.GetDictionary(source);

            var DataString = String.Join("&", Data.Select(x => x.Key + '=' + x.Value));

            url = url + '?' + DataString;

            var responseData = await Get(url, Data, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }


        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" },
                { "Accept","application/json" },
                { "Authorization","Bearer "+Config.CompanyToken.WS168_AgentToken}
            };

            var postData = Helper.GetDictionary(source);

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

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.WS168, url, new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;


                    if ((int)response.StatusCode != 400 && (int)response.StatusCode != 401 && (int)response.StatusCode != 201 && (int)response.StatusCode != 200)
                        throw new Exception(string.Format("Call WS168Api Failed! url:{0} Postdata:{1} status:{2}", url, JsonConvert.SerializeObject(postData), response.StatusCode.ToString()));


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
                    throw new Exception(string.Format("Call WS168Api Failed:{0}", url), ex);
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }



        private async Task<string> Get(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 3)
        {
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

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    HttpResponseMessage response = await request.GetAsync(Platform.WS168, url);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;


                    if ((int)response.StatusCode != 400 && (int)response.StatusCode != 401 && (int)response.StatusCode != 201 && (int)response.StatusCode != 200)
                        throw new Exception(string.Format("Call WS168Api Failed! url:{0} Postdata:{1} status:{2}", url, JsonConvert.SerializeObject(postData), response.StatusCode.ToString()));


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
                    throw new Exception(string.Format("Call WS168Api Failed:{0}", url), ex);
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }

    }
}
