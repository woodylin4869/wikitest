using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.RSG;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JILI
{
    public class JILIApiService : IJILIApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JILIApiService> _logger;
        public JILIApiService(IHttpClientFactory httpClientFactory, ILogger<JILIApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateMemberResponse> CreateMemberAsync(CreateMemberRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "CreateMember";
            return await ApiHandle<CreateMemberRequest, CreateMemberResponse>(url, source);
        }
        /// <summary>
        /// 登入遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LoginWithoutRedirectResponse> LoginWithoutRedirectAsync(LoginWithoutRedirectRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "LoginWithoutRedirect";
            return await ApiHandle<LoginWithoutRedirectRequest, LoginWithoutRedirectResponse>(url, source);
        }
        /// <summary>
        /// 遊戲清單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetGameListResponse> GetGameListAsync()
        {
            var url = Config.GameAPI.JILI_URL + "GetGameList";
            return await ApiHandle<object, GetGameListResponse>(url, null);
        }
        /// <summary>
        /// 查詢會員狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetMemberInfoResponse> GetMemberInfoAsync(GetMemberInfoRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "GetMemberInfo";
            return await ApiHandle<GetMemberInfoRequest, GetMemberInfoResponse>(url, source);
        }
        /// <summary>
        /// 額度轉移
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ExchangeTransferByAgentIdResponse> ExchangeTransferByAgentIdAsync(ExchangeTransferByAgentIdRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "ExchangeTransferByAgentId";
            return await ApiHandle<ExchangeTransferByAgentIdRequest, ExchangeTransferByAgentIdResponse>(url, source);
        }
        /// <summary>
        /// 單獨會員踢線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KickMemberResponses> KickMemberAsync(KickMemberRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "KickMember";
            return await ApiHandle<KickMemberRequest, KickMemberResponses>(url, source);
        }
        /// <summary>
        /// 取得遊戲注單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetBetRecordByTimeResponse> GetBetRecordByTimeAsync(GetBetRecordByTimeRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "GetBetRecordByTime";
            return await ApiHandle<GetBetRecordByTimeRequest, GetBetRecordByTimeResponse>(url, source);
        }
        /// <summary>
        /// 取得住單統計
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetBetRecordSummaryResponse> GetBetRecordSummaryAsync(GetBetRecordSummaryRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "GetBetRecordSummary";
            return await ApiHandle<GetBetRecordSummaryRequest, GetBetRecordSummaryResponse>(url, source);
        }
        /// <summary>
        /// 取得住單-URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetGameDetailUrlResponse> GetGameDetailUrlAsync(GetGameDetailUrlRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "GetGameDetailUrl";
            return await ApiHandle<GetGameDetailUrlRequest, GetGameDetailUrlResponse>(url, source);
        }
        /// <summary>
        /// TransactionId查詢交易紀錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckTransferByTransactionIdResponse> CheckTransferByTransactionIdAsync(CheckTransferByTransactionIdRequest source)
        {
            var url = Config.GameAPI.JILI_URL + "CheckTransferByTransactionId";
            return await ApiHandle<CheckTransferByTransactionIdRequest, CheckTransferByTransactionIdResponse>(url, source);
        }
        /// <summary>
        /// 取得所有在縣人數
        /// </summary>
        /// <returns></returns>
        public async Task<GetOnlineMemberResponse> GetOnlineMemberAsync()
        {
            var url = Config.GameAPI.JILI_URL + "GetOnlineMember";
            return await ApiHandle<object, GetOnlineMemberResponse>(url, null);
        }
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var nowDateTime = DateTime.UtcNow;
            var timestamp = nowDateTime.AddHours(-4).ToString("yyMMd");
            //組成KeyG
            var KeyG = Helper.MD5encryption(timestamp, Config.CompanyToken.JILI_AgentId, Config.CompanyToken.JILI_AgentKey).ToLower();
            //組成參數字串
            Dictionary<string, string> MD5Dictionary = new Dictionary<string, string>();
            Dictionary<string, string> URLDictionary = new Dictionary<string, string>();
            if (source != null)
            {
                MD5Dictionary = Helper.GetMD5Dictionary(source);
            }
            if (source != null)
            {
                URLDictionary = Helper.GetURLDictionary(source);
            }
            MD5Dictionary.Add("AgentId", Config.CompanyToken.JILI_AgentId);
            URLDictionary.Add("AgentId", Config.CompanyToken.JILI_AgentId);
            string querystring = string.Join("&", MD5Dictionary.Select(q => $"{q.Key}={q.Value}"));
            string URLstring = string.Join("&", URLDictionary.Select(q => $"{q.Key}={q.Value}"));
            //組成MD5
            string md5string = Helper.MD5encryption(querystring, KeyG, "");
            //組成KEY
            var Key = "123456" + md5string + "abcdef";

            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
            };

            var postData = new Dictionary<string, string>
            {

            };
            _logger.LogInformation("Jili Get RequestPath: {RequestPath}", url);
            var responseData = await Post(url + "?" + URLstring + "&" + "Key=" + Key, postData, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }

        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 3)
        {

            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                _logger.LogInformation("Jili Post RequestPath: {RequestPath}", url);
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
                    var content = new FormUrlEncodedContent(postData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.JILI, url, content);
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
                    throw new Exception(string.Format("Call JILIApi Failed:{0}", url));
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }
    }
}
