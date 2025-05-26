using System.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.RGRICH;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Request;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Response;
using Newtonsoft.Json.Converters;

namespace H1_ThirdPartyWalletAPI.Service.Game.RGRICH
{
    public class RGRICHApiService : IRGRICHApiService
    {
        public const string PlatformName = "RGRICH";

        private readonly LogHelper<RGRICHApiService> _logger;
        private readonly IOptions<RGRICHConfig> _options;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;

        public RGRICHApiService(LogHelper<RGRICHApiService> logger, IOptions<RGRICHConfig> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
            _serializerSettings = new JsonSerializerSettings()
            {
                // 小駝峰
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                // 序列和反序列對於時區統一使用local
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                // 廠商回復null可以不需要加入加解密驗證
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// 創建玩家帳號
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public async Task<ResponseBase<CreateUserResponse>> CreateUserAsync(CreateUserRequest source, string lang = "en")
        {
            lang = string.IsNullOrEmpty(lang) ? ThirdPartyWallet.Share.Model.Game.RGRICH.RGRICH.Lang["en-US"] : lang;
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + $"/{lang}/createUser";
            return await ApiHandle<CreateUserRequest, ResponseBase<CreateUserResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 檢查玩家帳號是否
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<UserIsExistResponse>> UserIsExistAsync(UserIsExistDataRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/userIsExist";
            return await ApiHandle<UserIsExistDataRequest, ResponseBase<UserIsExistResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 查詢玩家餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<BalanceResponse>> BalanceAsync(BalanceRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/balance";
            return await ApiHandle<BalanceRequest, ResponseBase<BalanceResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 玩家充值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public async Task<ResponseBase<RechargeResponse>> RechargeAsync(RechargeRequest source, string? lang = "en")
        {
            lang = string.IsNullOrEmpty(lang) ? ThirdPartyWallet.Share.Model.Game.RGRICH.RGRICH.Lang["en-US"] : lang;
            source.FlowNumber = string.IsNullOrEmpty(source.FlowNumber) ? Guid.NewGuid().ToString() : source.FlowNumber;
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + $"/{lang}/recharge";
            return await ApiHandle<RechargeRequest, ResponseBase<RechargeResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 玩家提現
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public async Task<ResponseBase<WithdrawResponse>> WithdrawAsync(WithdrawRequest source, string lang = "en")
        {
            lang = string.IsNullOrEmpty(lang) ? ThirdPartyWallet.Share.Model.Game.RGRICH.RGRICH.Lang["en-US"] : lang;
            source.FlowNumber = string.IsNullOrEmpty(source.FlowNumber) ? Guid.NewGuid().ToString() : source.FlowNumber;
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + $"/{lang}/withdraw";

            return await ApiHandle<WithdrawRequest, ResponseBase<WithdrawResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 根據商家產生交易流水號查詢玩家充值或提現記錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<RechargeOrWithdrawRecordResponse>> RechargeOrWithdrawRecordAsync(RechargeOrWithdrawRecordRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/rechargeOrWithdrawRecord";
            return await ApiHandle<RechargeOrWithdrawRecordRequest, ResponseBase<RechargeOrWithdrawRecordResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 獲取單一遊戲地址
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public async Task<ResponseBase<GameUrlResponse>> GameUrlAsync(GameUrlRequest source, string lang = "en")
        {
            lang = string.IsNullOrEmpty(lang) ? ThirdPartyWallet.Share.Model.Game.RGRICH.RGRICH.Lang["en-US"] : lang;
            source.PlatId = 2;
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + $"/{lang}/gameUrl";

            return await ApiHandle<GameUrlRequest, ResponseBase<GameUrlResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 查詢玩家下注記錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBaseWithMeta<List<BetRecordResponse>>> BetRecordAsync(BetRecordRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/betRecord";

            return await ApiHandle<BetRecordRequest, ResponseBaseWithMeta<List<BetRecordResponse>>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 獲取細單連結地址
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public async Task<ResponseBase<BetDetailUrlResponse>> BetDetailUrlAsync(BetDetailUrlRequest source, string lang = "en")
        {
            lang = string.IsNullOrEmpty(lang) ? ThirdPartyWallet.Share.Model.Game.RGRICH.RGRICH.Lang["en-US"] : lang;
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + $"/{lang}/betDetailUrl";
            return await ApiHandle<BetDetailUrlRequest, ResponseBase<BetDetailUrlResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 獲取游戲清單
        /// e.g. {"3001":"魔獸世界","3002":"狂野海盜","3003":"魔龍傳奇"}
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<Dictionary<string, string>>> GameListAsync(GameListRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/gameList";
            return await ApiHandle<GameListRequest, ResponseBase<Dictionary<string, string>>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 查詢每小時注單統計紀錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<ReportHourResponse>> ReportHourAsync(ReportHourRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/reportHour";
            return await ApiHandle<ReportHourRequest, ResponseBase<ReportHourResponse>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 剔除在線會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<object>> KickUserAsync(KickUserRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey;
            var url = _options.Value.RGRICH_URL + "/kickUser";
            return await ApiHandle<KickUserRequest, ResponseBase<object>, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// 健康度檢查
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<HealthCheckResponse> HealthCheckAsync(HealthCheckRequest source, string lang = "en")
        {
            source.AppKey = _options.Value.RGRICH_AppKey; 
            var url = _options.Value.RGRICH_URL + "/healthCheck";
            return await ApiHandle<HealthCheckRequest, HealthCheckResponse, ResponseBase<object>>(url, source);
        }

        /// <summary>
        /// API處理器
        /// </summary>
        /// <typeparam name="TRequest">資料Model</typeparam>
        /// <typeparam name="TResponse">RespModel</typeparam>
        /// <typeparam name="TRespBase">RespBase基底模型</typeparam>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<TResponse> ApiHandle<TRequest, TResponse, TRespBase>(string url, TRequest source)
            where TResponse : class, new()
            where TRespBase : ResponseBase<object>, new()
        {
            var appKey = _options.Value.RGRICH_AppKey;
            var appSecret = _options.Value.RGRICH_AppSecret;

            var headers = new Dictionary<string, string>();

            var postData = GetPostData(source, appKey, appSecret);

            var responseData = await Post(url, postData, headers);

            var resultResponse = JsonConvert.DeserializeObject<TRespBase>(responseData.RespContent);
            if (resultResponse is IMessage msg)
            {
                // 如果有錯誤資訊
                if (string.IsNullOrEmpty(msg.Message) == false)
                {
                    // Handle error messages
                    return HandleErrorMessage<TResponse>(msg);
                }
            }

            return JsonConvert.DeserializeObject<TResponse>(responseData.RespContent);
        }

        private TResponse HandleErrorMessage<TResponse>(IMessage message)
            where TResponse : class, new()
        {
            if (!string.IsNullOrEmpty(message.Message))
            {
                TResponse response = new TResponse();
                ((IMessage)response).Message = message.Message;
                Console.WriteLine($"ApiHandle Resp Message: {message.Message}");
                return response;
            }
            return default(TResponse);
        }

        /// <summary>
        /// 取得廠商API Request Model
        /// </summary>
        /// <typeparam name="TDataRequest">資料Model</typeparam>
        /// <param name="source">每個API的資料model</param>
        /// <param name="appKey">key</param>
        /// <param name="appSecret">secret</param>
        /// <returns></returns>
        private RequestBase GetPostData<TDataRequest>(TDataRequest source, string appKey, string appSecret)
        {
            Dictionary<string, string> paramsDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(source, _serializerSettings));
            var sortedParams = new SortedDictionary<string, string>(paramsDict);

            string paramsString = "";
            foreach (var pair in sortedParams)
            {
                paramsString += $"{pair.Key}={pair.Value}&";
            }

            string verifyStr = Helper.CalculateMD5Hash(paramsString + appSecret).ToLower();
            paramsString += $"verifyStr={verifyStr}";

            string encryptedParams = Helper.Encrypt(paramsString, appSecret);

            return new RequestBase()
            {
                p = encryptedParams,
                ak = appKey
            };
        }

        /// <summary>
        /// 呼叫廠商Post方法
        /// </summary>
        /// <param name="url">APIUrl</param>
        /// <param name="postData">廠商API Request Model</param>
        /// <param name="headers">headers</param>
        /// <returns></returns>
        private async Task<(HttpStatusCode HttpStatusCode, string RespContent)> Post(string url, RequestBase postData, Dictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }
            request.Content = new FormUrlEncodedContent(JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(postData, _serializerSettings)));

            // request.Content = new StringContent(JsonConvert.SerializeObject(postData, _serializerSettings), Encoding.UTF8, "application/x-www-form-urlencoded");

            using var response = await _httpClient.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            // 廠商只有成功才給200，所以先關閉Ensure200
            // response.EnsureSuccessStatusCode();

            var statusCode = response.StatusCode;

            return (statusCode, body);
        }
    }
}