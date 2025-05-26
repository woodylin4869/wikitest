using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace H1_ThirdPartyWalletAPI.Service.Game.RCG2
{
    public class RCG2ApiService : IRCG2ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RCG2ApiService> _logger;
        private static readonly SemaphoreSlim recordLock = new(1);
        public RCG2ApiService(IHttpClientFactory httpClientFactory, ILogger<RCG2ApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_CreateOrSetUser_Res>> CreateOrSetUser(RCG_CreateOrSetUser request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Player/CreateOrSetUser";
            return await ApiHandle<RCG_CreateOrSetUser, RCG_ResBase<RCG_CreateOrSetUser_Res>>(url, request);
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_Login_Res>> Login(RCG_Login request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Player/Login";
            return await ApiHandle<RCG_Login, RCG_ResBase<RCG_Login_Res>>(url, request);
        }

        /// <summary>
        /// 取得玩家餘額
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_GetBalance_Res>> GetBalance(RCG_GetBalance request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Player/GetBalance";
            return await ApiHandle<RCG_GetBalance, RCG_ResBase<RCG_GetBalance_Res>>(url, request);
        }

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_Deposit_Res>> Deposit(RCG_Deposit request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Player/Deposit";
            return await ApiHandle<RCG_Deposit, RCG_ResBase<RCG_Deposit_Res>>(url, request);
        }

        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_Withdraw_Res>> Withdraw(RCG_Withdraw request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Player/Withdraw";
            return await ApiHandle<RCG_Withdraw, RCG_ResBase<RCG_Withdraw_Res>>(url, request);
        }

        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_KickOut_Res>> KickOut(RCG_KickOut request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Player/KickOut";
            return await ApiHandle<RCG_KickOut, RCG_ResBase<RCG_KickOut_Res>>(url, request);
        }

        /// <summary>
        /// 交易紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_GetTransactionLog_Res>> GetTransactionLog(RCG_GetTransactionLog request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Record/GetTransactionLog";
            return await ApiHandle<RCG_GetTransactionLog, RCG_ResBase<RCG_GetTransactionLog_Res>>(url, request);

        }

        /// <summary>
        /// 取得下注紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<GetBetRecordListResponse>> GetBetRecordList(GetBetRecordListRequest request)
        {
            // RCG有檔 間隔1秒爬一次 那先用1.5s {\"MsgId\":-34,\"Message\":\"TOO_MANY_REQUESTS\",\"Data\":null,\"Timestamp\":1691551421}
            await recordLock.WaitAsync();
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5));
                recordLock.Release();
            });

            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Record/GetBetRecordList";
            return await ApiHandle<GetBetRecordListRequest, BaseResponse<GetBetRecordListResponse>>(url, request);
        }

        /// <summary>
        /// 開牌紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<GetOpenListResponse>> GetOpenList(GetOpenListRequest request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Record/GetOpenList";
            return await ApiHandle<GetOpenListRequest, BaseResponse<GetOpenListResponse>>(url, request);

        }

        /// <summary>
        /// W3取得注單資訊With開牌(SingleRecord/WithGameResult)
        /// 此方法沒有文件 而是使用以下網址測試
        /// https://api2tool.bacc55.com/W3/GetSingleBetRecordWithGameResult
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SingleRecordWithGameResultResponse> SingleRecordWithGameResult(SingleRecordWithGameResultRequest request)
        {
            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/W3/Bet/SingleRecord/WithGameResult";
            return await ApiHandle<SingleRecordWithGameResultRequest, SingleRecordWithGameResultResponse>(url, request);

        }
        /// <summary>
        /// 補單-使用時間拉取住單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<GetBetRecordListByDateRangeResponse>> GetBetRecordListByDateRange(GetBetRecordListByDateRangeRequest request)
        {
            await recordLock.WaitAsync();
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5));
                recordLock.Release();
            });

            request.systemCode = Config.CompanyToken.RCG2_SystemCode;
            request.webId = Config.CompanyToken.RCG2_WebId;
            var url = Config.GameAPI.RCG2_URL + "api/Record/GetBetRecordListByDateRange";
            return await ApiHandle<GetBetRecordListByDateRangeRequest, BaseResponse<GetBetRecordListByDateRangeResponse>>(url, request);
        }


        public async Task<SetCompanyGameBetLimitResponse> SetCompanyGameBetLimitResult(SetCompanyGameBetLimitRequset request)
        {
            var url = Config.GameAPI.RCG2_URL + "api/H1/SetCompanyGameBetLimit";
            return await ApiHandle<SetCompanyGameBetLimitRequset, SetCompanyGameBetLimitResponse>(url, request);
        }

        public async Task<string> HelloWorld()
        {
            var url = Config.GameAPI.RCG2_URL + "api/Player/HelloWorld";
            using var request = _httpClientFactory.CreateClient("log");
            request.Timeout = TimeSpan.FromSeconds(14);

            var response = await request.GetAsync(Platform.RCG2, url);

            if ((int)response.StatusCode != 400 && (int)response.StatusCode != 200)
                throw new Exception(string.Format("Call RCG2 Failed! url:{url} status:{status}", url , response.StatusCode.ToString()));

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var Dic = Helper.GetDictionary(source);
            var json = JsonConvert.SerializeObject(Dic);

            string Key = Config.CompanyToken.RCG2_DesKey;
            string IV = Config.CompanyToken.RCG2_DesIV;
            string Clinet_id = Config.CompanyToken.RCG2_ClientID;
            string Secret = Config.CompanyToken.RCG2_ClientSecret;
            var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            var queryString = Helper.desEncryptBase64(json, Key, IV);

            string MD5String = Clinet_id + Secret + unixTimestamp.ToString() + queryString;
            string MD5CheckSum = Helper.MD5encryption(MD5String);

            var headers = new Dictionary<string, string>
            {
                {"X-API-ClientID", Clinet_id},
                {"X-API-Signature", MD5CheckSum},
                {"X-API-Timestamp", unixTimestamp },
                {"Content-Type", "application/json"}
            };

            var postData = new Dictionary<string, string>
            {
            };
            var responseData = await Post(url, Dic, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);
        }

        private async Task<string> Post(string url, Dictionary<string, object> postData, Dictionary<string, string> headers = null, int retry = 3)
        {
            try
            {
                _logger.LogInformation("RCG2 Post RequestPath: {RequestPath}", url);
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
                    if (url.EndsWith("api/Record/GetBetRecordListByDateRange"))
                        request.Timeout = TimeSpan.FromSeconds(60);

                    var queryString = Helper.desEncryptBase64(JsonConvert.SerializeObject(postData), Config.CompanyToken.RCG2_DesKey, Config.CompanyToken.RCG2_DesIV);
                    var UrlEncode = HttpUtility.UrlEncode(queryString);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var response = await request.PostAsync(Platform.RCG2, url, new StringContent(UrlEncode, Encoding.UTF8, "application/json"));
                    sw.Stop();


                    if ((int)response.StatusCode != 400  && (int)response.StatusCode != 200)
                        throw new Exception(string.Format("Call RCG2 Failed! url:{0} Postdata:{1} status:{2}", url, JsonConvert.SerializeObject(postData), response.StatusCode.ToString()));
                    var result = await response.Content.ReadAsStringAsync();
                    var body = Helper.desDecryptBase64(result, Config.CompanyToken.RCG2_DesKey, Config.CompanyToken.RCG2_DesIV);

                    var dics = new Dictionary<string, object>
                     {
                        { "request", postData },
                        { "response", body }
                     };

                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get RCG2 RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }

                    return body;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "{platform} {action} {status} {statuscode}", Platform.RCG2, "POST", LogLevel.Error, (int)ex.StatusCode);
                throw;
            }
        }
    }
}
