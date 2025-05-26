using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.IDN;
using ThirdPartyWallet.Share.Model.Game.IDN.Request;
using ThirdPartyWallet.Share.Model.Game.IDN.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.IDN
{
    public class IDNApiService : IIDNApiService
    {
        public string CLIENT_CREDENTIAL_TOKEN = "";
        public const string PlatformName = "IDN";
        private const string _w1password = "aa8888";
        private readonly LogHelper<IDNApiService> _logger;
        private readonly IOptions<IDNConfig> _options;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly IMemoryCache _memoryCache;
        private const string ACCEPT_404_HEADER = "X-Accept-404";

        public IDNApiService(LogHelper<IDNApiService> logger, IOptions<IDNConfig> options, HttpClient httpClient, IMemoryCache memoryCache)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
            _memoryCache = memoryCache;

            _serializerSettings = new JsonSerializerSettings()
            {
                //// 小駝峰
                //ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                // 序列和反序列對於時區統一使用local
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                //// 廠商回復null可以不需要加入加解密驗證
                //NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// 取得憑證
        /// Limit for access is 1 minute / 1 request
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<AuthResponse> AuthAsync(AuthRequest source)
        {
            source.grant_type = "client_credentials";
            source.client_id = _options.Value.IDN_ClientID;
            source.client_secret = _options.Value.IDN_ClientSecret;
            var url = _options.Value.IDN_URL + "/oauth/token";
            return await ApiAuthHandle<AuthRequest, AuthResponse>(url, source);
        }

        /// <summary>
        /// 創建玩家帳號
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public async Task<ResponseBase<RegistrationResponse>> RegistrationAsync(RegistrationRequest source)
        {

            source.username = (source.username).ToLower();
            source.password = _w1password;
            source.password_confirmation = source.password;
            //THB
            int currency_id = 0;
            int.TryParse(_options.Value.IDN_currency_id, out currency_id);
            source.currency = currency_id;
            source.fullname = source.username;
            source.signup_ip = "127.0.0.1";
            int whitelabel_id = 0;
            int.TryParse(_options.Value.IDN_whitelabel_id, out whitelabel_id);
            source.whitelabel_id = whitelabel_id;

            var whitelabelCode = _options.Value.IDN_whitelabel_code;
            var url = _options.Value.IDN_URL + $"/pubs/v1/users/registration/{whitelabelCode}";

            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }

            return await ApiHandle<RegistrationRequest, ResponseBase<RegistrationResponse>, ResponseBase<object>>(HttpMethod.Post, url, source, CLIENT_CREDENTIAL_TOKEN);
        }

        public async Task<ResponseBase<UserIsExistResponse>> UserIsExistAsync(UserIsExistDataRequest source)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 會員身分登入
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<UserAuthResponse>> UserAuthAsync(UserAuthRequest source)
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;
            source.username = (_options.Value.IDN_whitelabel_code + source.username).ToLower();
            source.password = _w1password;
            var url = _options.Value.IDN_URL + $"/pubs/v1/users/auth/{whitelabelCode}";

            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }
            return await ApiHandle<UserAuthRequest, ResponseBase<UserAuthResponse>, ResponseBase<object>>(HttpMethod.Post, url, source, CLIENT_CREDENTIAL_TOKEN);
        }


        /// <summary>
        /// 取得餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ResponseBase<BalanceResponse>> BalanceAsync(BalanceRequest source)
        {
            var url = _options.Value.IDN_URL + $"/pubs/v1/users/wallet";

            string USER_ACCESS_TOKEN = await GetUserAccessToken(source.UserName);
            return await ApiHandle<BalanceRequest, ResponseBase<BalanceResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, USER_ACCESS_TOKEN);
        }

        /// <summary>
        /// 校準餘額，將餘額轉回主錢包
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<object>> CalibrateAsync(CalibrateRequest source)
        {
            var url = _options.Value.IDN_URL + $"/pubs/v1/users/wallet/calibrate";

            string USER_ACCESS_TOKEN = await GetUserAccessToken(source.UserName);
            return await ApiGetHandle<ResponseBase<object>, ResponseBase<object>>(url, USER_ACCESS_TOKEN);
        }

        /// <summary>
        /// 會員登入TOKEN
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        private async Task<string> GetUserAccessToken(string UserName)
        {
            UserAuthRequest authRequest = new UserAuthRequest();
            authRequest.username = UserName;
            ResponseBase<UserAuthResponse> authResponse = await UserAuthAsync(authRequest);
            string USER_ACCESS_TOKEN = authResponse.data.access_token;
            return USER_ACCESS_TOKEN;
        }


        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<WithdrawResponse>> WithdrawAsync(string UserName, WithdrawRequest source)
        {
            int payment_id = 0;
            int.TryParse(_options.Value.IDN_payment_id, out payment_id);
            source.payment_id = payment_id;
            source.domain = "bacc6666.com";
            source.platform = "OSX";
            source.user_agent = "Google Chrome";
            source.device = "Mac";
            source.is_mobile = 0;

            var url = _options.Value.IDN_URL + $"/pubs/v1/users/withdraw";
            string USER_ACCESS_TOKEN = await GetUserAccessToken(UserName);
            return await ApiHandle<WithdrawRequest, ResponseBase<WithdrawResponse>, ResponseBase<object>>(HttpMethod.Post, url, source, USER_ACCESS_TOKEN);
        }

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<DepositResponse>> DepositAsync(string UserName, DepositRequest source)
        {
            int payment_id = 0;
            int.TryParse(_options.Value.IDN_payment_id, out payment_id);
            source.payment_id = payment_id;
            source.destination_bank = "-";
            source.is_mobile = 0;

            var url = _options.Value.IDN_URL + $"/pubs/v1/users/deposit";
            string USER_ACCESS_TOKEN = await GetUserAccessToken(UserName);
            return await ApiHandle<DepositRequest, ResponseBase<DepositResponse>, ResponseBase<object>>(HttpMethod.Post, url, source, USER_ACCESS_TOKEN);
        }

        /// <summary>
        /// 檢查存款清單
        /// </summary>
        /// <param name="page"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<CheckDepositListResponse>> CheckDepositListAsync(int page, CheckDepositListRequest source)
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;

            var url = _options.Value.IDN_URL + $"/pubs/v1/whitelabels/{whitelabelCode}/deposit/list?page={page}";
            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }
            return await ApiHandle<CheckDepositListRequest, ResponseBase<CheckDepositListResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, CLIENT_CREDENTIAL_TOKEN);
        }

        /// <summary>
        /// 檢查提款清單
        /// </summary>
        /// <param name="page"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<CheckWithdrawListResponse>> CheckWithdrawListAsync(int page, CheckWithdrawListRequest source)
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;

            var url = _options.Value.IDN_URL + $"/pubs/v1/whitelabels/{whitelabelCode}/withdraw/list?page={page}";
            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }
            return await ApiHandle<CheckWithdrawListRequest, ResponseBase<CheckWithdrawListResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, CLIENT_CREDENTIAL_TOKEN);
        }



        /// <summary>
        /// 進入遊戲
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="GameID"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<LaunchResponse>> LaunchAsync(string UserName, string GameID, LaunchRequest source)
        {
            if (string.IsNullOrEmpty(GameID))
            {
                GameID = "lobby";
            }
            var url = _options.Value.IDN_URL + $"/pubs/v1/game/idnlive/launch/idnseamless/{GameID}?" + Helper.ConvertToKeyValue(source);
            string USER_ACCESS_TOKEN = await GetUserAccessToken(UserName);


            ResponseBase<LaunchResponse> responseBase = await ApiHandle<LaunchRequest, ResponseBase<LaunchResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, USER_ACCESS_TOKEN);
            if (!responseBase.success && responseBase.Message == "Please wait we are preparing the game user")
            {
                await Task.Delay(3000);
                responseBase = await ApiHandle<LaunchRequest, ResponseBase<LaunchResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, USER_ACCESS_TOKEN);
            }
            return responseBase;
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LogoutResponse> LogoutAsync(string UserName, LogoutRequest source)
        {
            var url = _options.Value.IDN_URL + $"/pubs/v1/users/logout";
            string USER_ACCESS_TOKEN = await GetUserAccessToken(UserName);
            return await ApiHandle<LogoutRequest, LogoutResponse, ResponseBase<object>>(HttpMethod.Post, url, source, USER_ACCESS_TOKEN);
        }

        /// <summary>
        /// 取得下注紀錄
        /// 每分鐘限量30個請求
        /// 日期時間使用GMT+7
        /// 數據僅保留最近2週
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<bethistoryResponse>> bethistoryAsync(bethistoryRequest source)
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;
            var url = _options.Value.IDN_URL + $"/pubs/v1/whitelabels/{whitelabelCode}/bethistory/idnlive";
            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }
            return await ApiHandle<bethistoryRequest, ResponseBase<bethistoryResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, CLIENT_CREDENTIAL_TOKEN);
        }

        /// <summary>
        /// 取得每日報表
        /// 每分鐘限量2個請求
        /// 日期時間使用GMT+7
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<dailyreportResponse>> dailyreportAsync(dailyreportRequest source)
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;
            var url = _options.Value.IDN_URL + $"/pubs/v1/whitelabels/{whitelabelCode}/dailyreport";
            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }

            return await ApiHandle<dailyreportRequest, ResponseBase<dailyreportResponse>, ResponseBase<object>>(HttpMethod.Get, url, source, CLIENT_CREDENTIAL_TOKEN);
        }

        private async Task<string> GetAuthToken()
        {
            _logger.APILog(
                    PlatformName
                   , "GetAuthToken"
                   , "POST"
                   , ""
                   , "IDN GetAuthToken Error"
                   , 200
                   , 0);
            return "";
            //try
            //{
            //    var cachekey = "IDN_API_Token";
            //    var token = await _memoryCache.GetOrCreateAsync<string>(cachekey, async entry =>
            //    {
            //        entry.SetOptions(new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60) });
            //        AuthRequest authRequest = new AuthRequest();
            //        authRequest.scope = "";
            //        AuthResponse authResponse = await AuthAsync(authRequest);


            //        if (string.IsNullOrEmpty(authResponse.access_token))
            //        {
            //            _logger.GetLogger.LogError("ServiceName: {ServiceType} | Get AccessToken Failed", "IDNService");

            //            throw new Exception("GetAccessTokenError");
            //        }
            //        else
            //        {
            //            CLIENT_CREDENTIAL_TOKEN = authResponse.access_token;
            //            return authResponse.access_token;
            //        }
            //    });
            //    return token;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.ToString());
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void SetAuthToken(string access_token)
        {
            this.CLIENT_CREDENTIAL_TOKEN = access_token;
            //try
            //{
            //    var cachekey = "IDN_API_Token";
            //    var token = _memoryCache.Set<string>(cachekey, access_token);
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.ToString());
            //}
        }

        /// <summary>
        /// 健康度檢查
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ResponseBase<WhitelabelInfoResponse>> HealthCheckAsync()
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;
            var url = _options.Value.IDN_URL + $"/pubs/v1/whitelabels/{whitelabelCode}";
            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }
            return await ApiHandle<dailyreportRequest, ResponseBase<WhitelabelInfoResponse>, ResponseBase<object>>(HttpMethod.Get, url, null, CLIENT_CREDENTIAL_TOKEN);
        }


        /// <summary>
        /// 取得餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<GetGameResultResponse> GetGameResultAsync(GetGameResultRequest source)
        {
            var whitelabelCode = _options.Value.IDN_whitelabel_code;
            var matchId = source.matchId;
            var date = source.date;
            var gameId = source.gameId;
            var url = _options.Value.IDN_URL + $"/pubs/v1/games/{whitelabelCode}/idnlive/result/{gameId}/{matchId}/{date}";
            if (string.IsNullOrEmpty(CLIENT_CREDENTIAL_TOKEN))
            {
                CLIENT_CREDENTIAL_TOKEN = await GetAuthToken();
            }
            return await ApiHandle<GetGameResultRequest, GetGameResultResponse, ResponseBase<object>>(HttpMethod.Get, url, source, CLIENT_CREDENTIAL_TOKEN);
        }


        /// <summary>
        /// API Auth處理
        /// </summary>
        /// <typeparam name="TRequest">資料Model</typeparam>
        /// <typeparam name="TResponse">RespModel</typeparam>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<TResponse> ApiAuthHandle<TRequest, TResponse>(string url, TRequest source, string Authorization = "")
            where TResponse : class, new()
        {
            var headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" }
            };
            if (!string.IsNullOrEmpty(Authorization))
            {
                headers.Add("Authorization", String.Format("Bearer {0}", Authorization));
            }


            var responseData = await Post(HttpMethod.Post, url, source, headers);
            if (responseData.HttpStatusCode == HttpStatusCode.OK)
            {
                var resultResponse = JsonConvert.DeserializeObject<TResponse>(responseData.RespContent);
                return resultResponse;
            }
            else
            {
                var resultResponse = JsonConvert.DeserializeObject<AuthResponseBase>(responseData.RespContent);
                if (resultResponse is IAuthErrorMessage msg)
                {
                    // 如果有錯誤資訊
                    if (string.IsNullOrEmpty(msg.message) == false)
                    {
                        TResponse response = new TResponse();
                        ((IAuthErrorMessage)response).message = msg.message;
                        Console.WriteLine($"ApiHandle Resp Message: {msg.message}");
                        return response;
                    }
                }
            }




            return JsonConvert.DeserializeObject<TResponse>(responseData.RespContent);
        }


        /// <summary>
        /// API處理器
        /// </summary>
        /// <typeparam name="TRequest">資料Model</typeparam>
        /// <typeparam name="TResponse">RespModel</typeparam>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<TResponse> ApiHandle<TRequest, TResponse, TRespBase>(HttpMethod method, string url, TRequest source, string Authorization = "")
            where TResponse : class, new()
                 where TRespBase : ResponseBase<object>, new()
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Authorization))
            {
                headers.Add("Authorization", String.Format("Bearer {0}", Authorization));
            }

            //using (LogContext.PushProperty("X_Authorization", Authorization))
            //using (LogContext.PushProperty("GameId", PlatformName))
            //{
            //    _logger.GetLogger.LogInformation("IDN_Authorization");
            //}

            var responseData = await Post(method, url, source, headers);
            var resultResponse = JsonConvert.DeserializeObject<TRespBase>(responseData.RespContent);
            if (resultResponse is IMessage msg)
            {

                // 如果有錯誤資訊
                if (!msg.success && string.IsNullOrEmpty(msg.Message) == false)
                {
                    // Handle error messages
                    return HandleErrorMessage<TResponse>(msg);
                }
            }
            return JsonConvert.DeserializeObject<TResponse>(responseData.RespContent);
        }

        /// <summary>
        /// API處理器
        /// </summary>
        /// <typeparam name="TRequest">資料Model</typeparam>
        /// <typeparam name="TResponse">RespModel</typeparam>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<TResponse> ApiGetHandle<TResponse, TRespBase>(string url, string Authorization = "")
            where TResponse : class, new()
                 where TRespBase : ResponseBase<object>, new()
        {
            var headers = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Authorization))
            {
                headers.Add("Authorization", String.Format("Bearer {0}", Authorization));
            }

            var responseData = await Get(url, headers);
            var resultResponse = JsonConvert.DeserializeObject<TRespBase>(responseData.RespContent);
            if (resultResponse is IMessage msg)
            {
                // 如果有錯誤資訊
                if (!msg.success && string.IsNullOrEmpty(msg.Message) == false)
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
            TResponse response = new TResponse();
            ((IMessage)response).response_code = message.response_code;

            if (!string.IsNullOrEmpty(message.Message))
            {
                string ErrorMsg = "";

                ErrorMsg = message.Message;

                Console.WriteLine($"ApiHandle Resp Message: {message.Message}");
                if (message.Errors != null)
                {
                    foreach (var error in message.Errors)
                    {
                        ErrorMsg += $" | {error.Key}: {string.Join(", ", error.Value)}";
                    }
                }
               ((IMessage)response).Message = ErrorMsg;
                return response;
            }
            return default(TResponse);
        }

        /// <summary>
        /// 呼叫廠商Post方法
        /// </summary>
        /// <param name="url">APIUrl</param>
        /// <param name="postData">廠商API Request Model</param>
        /// <param name="headers">headers</param>
        /// <returns></returns>
        private async Task<(HttpStatusCode HttpStatusCode, string RespContent)> Post<TRequest>(HttpMethod Method, string url, TRequest postData, Dictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(Method, url);
            request.Headers.TryAddWithoutValidation(ACCEPT_404_HEADER, "true");
            
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }
            request.Content = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            // 廠商只有成功才給200，所以先關閉Ensure200
            // response.EnsureSuccessStatusCode();
            var statusCode = response.StatusCode;

            return (statusCode, body);
        }

        /// <summary>
        /// 呼叫廠商Get方法
        /// </summary>
        /// <param name="url">APIUrl</param>
        /// <param name="postData">廠商API Request Model</param>
        /// <param name="headers">headers</param>
        /// <returns></returns>
        private async Task<(HttpStatusCode HttpStatusCode, string RespContent)> Get(string url, Dictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation(ACCEPT_404_HEADER, "true");
            
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }
            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // 廠商只有成功才給200，所以先關閉Ensure200
            // response.EnsureSuccessStatusCode();

            var statusCode = response.StatusCode;

            return (statusCode, body);
        }
    }
}