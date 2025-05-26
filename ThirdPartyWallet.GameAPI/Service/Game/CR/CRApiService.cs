using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.CR;
using ThirdPartyWallet.Share.Model.Game.CR.Request;
using ThirdPartyWallet.Share.Model.Game.CR.Response;
using static H1_ThirdPartyWalletAPI.Service.Game.CR.Helper;

namespace H1_ThirdPartyWalletAPI.Service.Game.CR
{
    public class CRApiService : ICRApiService
    {
        public const string PlatformName = "CR";
        private const string _w1password = "aa8888";
        private readonly LogHelper<CRApiService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<CRConfig> _options;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;

        public CRApiService(LogHelper<CRApiService> logger, IOptions<CRConfig> options, HttpClient httpClient, IMemoryCache memoryCache)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _serializerSettings = new JsonSerializerSettings()
            {
                // 小駝峰
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                // 序列和反序列對於時區統一使用local
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                //// 廠商回復null可以不需要加入加解密驗證
                //NullValueHandling = NullValueHandling.Ignore
            };

            _serializerSettings.Converters.Add(new LongToStringConverter());
            _serializerSettings.Converters.Add(new IntToStringConverter());
            _serializerSettings.Converters.Add(new DecimalToStringConverter());
        }


        public async Task<string> healthcheckAsync()
        {
            PingRequestRequest source = new PingRequestRequest();
            string url = _options.Value.CR_URL;
            string method = "PingRequest";
            source.method = method;
            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);

            var responseData = await Post(url, method, encryptedRequest, headers);

            return responseData;
        }

        /// <summary>
        /// 3.1 登入系統AGLogin
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<AGLoginResponse> AGLoginAsync(AGLoginRequest source)
        {
            source.method = "AGLogin";
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.username = _options.Value.CR_Username;
            source.password = _options.Value.CR_Password;
            source.remoteip = "127.0.0.1";

            string url = _options.Value.CR_URL;
            string method = "AGLogin";

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);

            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<AGLoginResponse>(tempResponse);

        }


        /// <summary>
        /// 3.2 建立新的會員CreateMember 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateMemberResponse> CreateMemberAsync(CreateMemberRequest source)
        {
            string method = "CreateMember";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;
            source.password = _w1password;

            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<CreateMemberResponse>(tempResponse);

        }

        /// <summary>
        /// 3.3 存款Deposit
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<DepositResponse> DepositAsync(DepositRequest source)
        {
            string method = "Deposit";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<DepositResponse>(tempResponse);

        }

        /// <summary>
        /// 3.4 提款Withdraw 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source)
        {
            string method = "Withdraw";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<WithdrawResponse>(tempResponse);

        }


        /// <summary>
        /// 3.5 會員登入 MemLogin
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<MemLoginResponse> MemLoginAsync(MemLoginRequest source)
        {
            string method = "MemLogin";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;
            source.password = _w1password;

            //LOGIN沒帶參數 固定寫THB
            source.currency = "THB";
            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<MemLoginResponse>(tempResponse);

        }

        /// <summary>
        /// 3.6 登入遊戲LaunchGame
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LaunchGameResponse> LaunchGameAsync(LaunchGameRequest source)
        {
            string method = "LaunchGame";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;
            source.password = _w1password;
            string url = _options.Value.CR_URL;

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);

            var responseData = await Post(url, method, encryptedRequest, headers);
            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<LaunchGameResponse>(tempResponse);

        }

        /// <summary>
        /// 3.8 確認會員目前餘額chkMemberBalance
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<chkMemberBalanceResponse> chkMemberBalanceAsync(chkMemberBalanceRequest source)
        {
            string method = "chkMemberBalance";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);
            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<chkMemberBalanceResponse>(tempResponse);

        }


        /// <summary>
        /// 3.12 全部會員的注單資料ALLWager
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ALLWagerResponse> ALLWagerAsync(ALLWagerRequest source, int retry = -1)
        {
            string method = "ALLWager";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);

            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);

            DecryptLog(method, DecRequest, tempResponse);


            if (tempResponse.Contains("token驗證錯誤") && retry > 0)
            {
                retry--;
                var random = new Random();
                var randomDelay = random.Next(1, 1500);
                await Task.Delay(1000 + randomDelay);
                await ALLWagerAsync(source, retry);
            }

            return JsonConvert.DeserializeObject<ALLWagerResponse>(tempResponse);

        }

        /// <summary>
        ///  3.22 檢查報表統計資料 CheckAGReport
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckAGReportResponse> CheckAGReportAsync(CheckAGReportRequest source)
        {
            string method = "CheckAGReport";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis().ToString();
            //source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);

            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<CheckAGReportResponse>(tempResponse);

        }

        private void DecryptLog(string method, string DecRequest, string tempResponse)
        {
            var responselog = "";
            if (tempResponse.Length > 10000)
            {
                responselog = tempResponse.Substring(0, 9999);
            }
            else
            {
                responselog = tempResponse;
            }

            _logger.APILog(
                    PlatformName
                   , method
                   , "POST"
                   , DecRequest
                   , responselog
                   , 200
                   , 0);
        }


        /// <summary>
        /// 3.18  將單一會員登出 KickOutMem
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KickOutMemResponse> KickOutMemAsync(KickOutMemRequest source)
        {
            string method = "KickOutMem";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            //source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<KickOutMemResponse>(tempResponse);

        }

        /// <summary>
        /// 3.16 查詢存提款記錄 ChkTransInfo
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<ChkTransInfoResponse> ChkTransInfoAsync(ChkTransInfoRequest source)
        {
            string method = "ChkTransInfo";
            string token = await GetAPITOKEN();
            source.timestamp = Helper.GetCurrentUnixTimestampMillis();
            //source.method = method;
            source.token = token;

            string url = _options.Value.CR_URL;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

            string secretKey = _options.Value.CR_SecretKey;
            string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
            string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);
            var responseData = await Post(url, method, encryptedRequest, headers);

            string tempResponse = responseData;
            tempResponse = await HandlerResponse(secretKey, responseData, tempResponse);
            DecryptLog(method, DecRequest, tempResponse);
            return JsonConvert.DeserializeObject<ChkTransInfoResponse>(tempResponse);

        }

        private async Task<string> GetAPITOKEN()
        {
            try
            {
                var cachekey = "CR_API_Token";
                var token = await _memoryCache.GetOrCreateAsync<string>(cachekey, async entry =>
                {
                    entry.SetOptions(new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30) });// 有效時間 60 分鐘
                    AGLoginRequest source = new AGLoginRequest();
                    source.method = "AGLogin";
                    source.timestamp = Helper.GetCurrentUnixTimestampMillis();
                    source.username = _options.Value.CR_Username;
                    source.password = _options.Value.CR_Password;
                    source.remoteip = "127.0.0.1";

                    string url = _options.Value.CR_URL;
                    string method = "AGLogin";

                    var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" }
            };

                    string secretKey = _options.Value.CR_SecretKey;
                    string DecRequest = JsonConvert.SerializeObject(source, _serializerSettings);
                    string encryptedRequest = Helper.AES_Encrypt(DecRequest, secretKey);

                    using var request = new HttpRequestMessage(HttpMethod.Post, url);
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }

                    var obj = new
                    {
                        Method = method,
                        AGID = _options.Value.CR_AGID,
                        Request = encryptedRequest
                    };

                    request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                    using var response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.GetLogger.LogError("ServiceName: {ServiceType} | Get AccessToken Failed", "CRService");

                        throw new Exception("GetAccessTokenError");
                    }
                    else
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        string tempResponse = responseData;
                        if (!Helper.IsValidJson(responseData))
                        {
                            tempResponse = Helper.AES_Decrypt(responseData, secretKey);
                        }
                        else
                        {
                            throw new Exception("GetAccessTokenError2:DecryptError:" + tempResponse);
                        }
                        DecryptLog(method, DecRequest, tempResponse);

                        AGLoginResponse AGLoginResponse = JsonConvert.DeserializeObject<AGLoginResponse>(tempResponse);

                        return AGLoginResponse.token;
                    }
                });
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private async Task<string> HandlerResponse(string secretKey, string responseData, string tempResponse)
        {
            if (!Helper.IsValidJson(responseData))
            {
                tempResponse = Helper.AES_Decrypt(responseData, secretKey);
            }


            if (tempResponse.Contains("token驗證錯誤"))
            {
                var cachekey = "CR_API_Token";
                AGLoginRequest source = new AGLoginRequest();
                AGLoginResponse aGLoginResponse = await AGLoginAsync(source);
                _memoryCache.Set<string>(cachekey, aGLoginResponse.token);
            }
            return tempResponse;
        }


        private async Task<string> Post(string url, string Method, string encryptedRequest, Dictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }

            var obj = new
            {
                Method = Method,
                AGID = _options.Value.CR_AGID,
                Request = encryptedRequest
            };

            request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return body;
        }
    }
}