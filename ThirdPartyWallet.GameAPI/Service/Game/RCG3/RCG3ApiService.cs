
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.RCG3;
using ThirdPartyWallet.Share.Model.Game.RCG3.Request;
using ThirdPartyWallet.Share.Model.Game.RCG3.Response;
using static ThirdPartyWallet.Share.Model.Game.RCG3.RCG3;

namespace ThirdPartyWallet.GameAPI.Service.Game.RCG3
{
    public class RCG3ApiService : IRCG3ApiService
    {
        public const string PlatformName = "RCG3";


        private readonly LogHelper<RCG3ApiService> _logger;
        private readonly IOptions<RCG3Config> _options;
        private readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim recordLock = new(1);
        public RCG3ApiService(LogHelper<RCG3ApiService> logger, IOptions<RCG3Config> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_CreateOrSetUser_Res>> CreateOrSetUser(RCG_CreateOrSetUser request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Player/CreateOrSetUser";
            return await ApiHandle<RCG_CreateOrSetUser, RCG_ResBase<RCG_CreateOrSetUser_Res>>(url, request);
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_Login_Res>> Login(RCG_Login request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Player/Login";
            return await ApiHandle<RCG_Login, RCG_ResBase<RCG_Login_Res>>(url, request);
        }

        /// <summary>
        /// 取得玩家餘額
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_GetBalance_Res>> GetBalance(RCG_GetBalance request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Player/GetBalance";
            return await ApiHandle<RCG_GetBalance, RCG_ResBase<RCG_GetBalance_Res>>(url, request);
        }

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_Deposit_Res>> Deposit(RCG_Deposit request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Player/Deposit";
            return await ApiHandle<RCG_Deposit, RCG_ResBase<RCG_Deposit_Res>>(url, request);
        }

        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_Withdraw_Res>> Withdraw(RCG_Withdraw request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Player/Withdraw";
            return await ApiHandle<RCG_Withdraw, RCG_ResBase<RCG_Withdraw_Res>>(url, request);
        }

        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_KickOut_Res>> KickOut(RCG_KickOut request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Player/KickOut";
            return await ApiHandle<RCG_KickOut, RCG_ResBase<RCG_KickOut_Res>>(url, request);
        }

        /// <summary>
        /// 交易紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RCG_ResBase<RCG_GetTransactionLog_Res>> GetTransactionLog(RCG_GetTransactionLog request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Record/GetTransactionLog";
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

            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Record/GetBetRecordList";
            return await ApiHandle<GetBetRecordListRequest, BaseResponse<GetBetRecordListResponse>>(url, request);
        }

        /// <summary>
        /// 開牌紀錄
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<GetOpenListResponse>> GetOpenList(GetOpenListRequest request)
        {
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Record/GetOpenList";
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
            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/W3/Bet/SingleRecord/WithGameResult";
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

            request.systemCode = _options.Value.RCG3_SystemCode;
            request.webId = _options.Value.RCG3_WebId;
            var url = _options.Value.RCG3_URL + "api/Record/GetBetRecordListByDateRange";
            return await ApiHandle<GetBetRecordListByDateRangeRequest, BaseResponse<GetBetRecordListByDateRangeResponse>>(url, request);
        }


        public async Task<SetCompanyGameBetLimitResponse> SetCompanyGameBetLimitResult(SetCompanyGameBetLimitRequset request)
        {
            var url = _options.Value.RCG3_URL + "api/H1/SetCompanyGameBetLimit";
            return await ApiHandle<SetCompanyGameBetLimitRequset, SetCompanyGameBetLimitResponse>(url, request);
        }

        public async Task<string> HelloWorld()
        {
            var url = _options.Value.RCG3_URL + "api/Player/HelloWorld";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);
      
            if ((int)response.StatusCode != 400 && (int)response.StatusCode != 200)
                throw new Exception(string.Format("Call RCG3 Failed! url:{url} status:{status}", url, response.StatusCode.ToString()));

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var Dic = Helper.GetDictionary(source);
            var json = JsonConvert.SerializeObject(Dic);

            string Key = _options.Value.RCG3_DesKey;
            string IV = _options.Value.RCG3_DesIV;
            string Clinet_id = _options.Value.RCG3_ClientID;
            string Secret = _options.Value.RCG3_ClientSecret;
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


                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    }
                }


                var timeout = new CancellationTokenSource(14000);
                if (url.EndsWith("api/Record/GetBetRecordListByDateRange"))
                {
                    timeout = new CancellationTokenSource(60000);
                }


                var queryString = Helper.desEncryptBase64(JsonConvert.SerializeObject(postData), _options.Value.RCG3_DesKey, _options.Value.RCG3_DesIV);
                var UrlEncode = HttpUtility.UrlEncode(queryString);
                request.Content = new StringContent(UrlEncode, Encoding.UTF8, "application/json");

                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var response = await _httpClient.SendAsync(request, timeout.Token);

                sw.Stop();

                if ((int)response.StatusCode != 400 && (int)response.StatusCode != 200)
                    throw new Exception(string.Format("Call RCG3 Failed! url:{0} Postdata:{1} status:{2}", url, JsonConvert.SerializeObject(postData), response.StatusCode.ToString()));
                var result = await response.Content.ReadAsStringAsync();
                var body = Helper.desDecryptBase64(result, _options.Value.RCG3_DesKey, _options.Value.RCG3_DesIV);


                return body;
            }
            catch (Exception ex) 
            {
                throw;
            }

        }
    }
}
