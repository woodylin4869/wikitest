using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.SPLUS;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Request;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.SPLUS
{
    public class SPLUSApiService : ISPLUSApiService
    {
        public const string PlatformName = "SPLUS";

        private readonly ILogger<SPLUSApiService> _logger;
        private readonly IOptions<SPLUSConfig> _options;
        private readonly HttpClient _httpClient;
        private readonly string API_KEY;
        private readonly List<string> mockUserList;

        private string API_TOKEN { get; set; }
        public SPLUSApiService(ILogger<SPLUSApiService> logger, IOptions<SPLUSConfig> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;

            API_KEY = _options.Value.secretkey;
            API_TOKEN = _options.Value.SPLUS_token;
            

        }

        /// <summary>
        /// 建立 Player
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<CreateResponse>> Player(CreateRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/player/create";
            return await PostAsync<CreateRequest, ResponseBase<CreateResponse>>(url, request);
        }
        /// <summary>
        /// 玩家登出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<LogoutResponse>> Logout(LogoutRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/player/logout";
            return await PostAsync<LogoutRequest, ResponseBase<LogoutResponse>>(url, request);
        }

        /// <summary>
        /// Player 取得遊戲連結
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<GetlinkResponse>> GameLink(GetlinkRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/game/gamelink";
            request.betlimit = "2";
            return await PostAsync<GetlinkRequest, ResponseBase<GetlinkResponse>>(url, request);
        }

        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<WithdrawResponse>> Withdraw(WithdrawRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/player/withdraw";
            return await PostAsync<WithdrawRequest, ResponseBase<WithdrawResponse>>(url, request);
        }

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<DepositResponse>> Deposit(DepositRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/player/deposit";
            return await PostAsync<DepositRequest, ResponseBase<DepositResponse>>(url, request);
        }

        /// <summary>
        /// 玩家遊戲錢包查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<WalletResponse>> Wallet(WalletRequest request)       
        {
            var url = _options.Value.SPLUS_URL + "api/player/wallet";
            return await PostAsync<WalletRequest, ResponseBase<WalletResponse>>(url, request);
        }

        /// <summary>
        /// 單筆交易紀錄查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<TransferResponse>> Transaction(TransferRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/player/transfer";
            return await PostAsync<TransferRequest, ResponseBase<TransferResponse>>(url, request);
        }


        /// <summary>
        /// 查詢注單詳細資訊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<BetlogResponse>> Betlog(BetlogRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/betlog/betlog_time";
            //要多GZIP條件
            return await PostAsync<BetlogRequest, ResponseBase<BetlogResponse>>(url, request, null, true);
        }

        /// <summary>
        ///查詢注單詳細資訊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<TotalBetlogResponse>> Betlog_total(TotalBetlogRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/betlog/betlog_total";
            return await GetAsync<TotalBetlogRequest, ResponseBase<TotalBetlogResponse>>(url, request);
        }

        /// <summary>
        /// 第三層明細
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<PlaycheckResponse>> Playcheck(PlaycheckRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/betlog/playcheck";
            return await GetAsync<PlaycheckRequest, ResponseBase<PlaycheckResponse>>(url, request);
        }
      
        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<HealthcheckResponse>> Healthcheck(HealthcheckRequest request)
        {
            var url = _options.Value.SPLUS_URL + "api/healthcheck";
            return await PostAsync<HealthcheckRequest, ResponseBase<HealthcheckResponse>>(url, request);
        }


        /// <summary>
        /// 設定Header
        /// </summary>
        /// <param name="httpClient"></param>
        private void SetHeader(ref HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", API_TOKEN);
        }
        private static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            return typeof(TRequest).GetProperties()
            .Where(prop => KeepNullField || prop.GetValue(request) != null)
            .ToDictionary(
                prop => prop.Name,
                prop => prop.PropertyType switch
                {
                    Type t when t == typeof(DateTime) => ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-dd HH:mm:ss"),
                    Type t when t == typeof(decimal) => ((decimal)prop.GetValue(request)).ToString("0.##"),
                    _ => prop.GetValue(request)?.ToString()
                });
        }
        /// <summary>
        /// 每次呼叫都必須在網址加上一個sign參數，而sign參數是以傳遞資料及API KEY產生:
        /// 先將參數陣列照key值進行升序排序
        /// => 組成query string
        /// => 後面串上api key後md5加密
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private string ComputeSign<TRequest>(TRequest request)
        {
            Dictionary<string, string> props = request as Dictionary<string, string>;
            // 1. 按照 key 進行升序排序
            var sortedData = props.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // 2. 將每個參數轉換為 URL-encoded 並合併成字串
            var queryString = string.Join("&", sortedData.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            // 3. 加上 apiKey
            queryString += API_KEY;

            // 4. 計算 MD5 編碼
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }

                return hashStringBuilder.ToString(); // 返回小寫的 md5 結果
            }
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest req, Func<string, string> responseLogFormat = null, bool useGzip = false)
        {
            try
            {
              
                    using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Content = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");


                var formdata = GetDictionary(req);  // 获取请求字典
                var sign = ComputeSign(formdata);
                if (useGzip == true)
                    request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip");
                request.Headers.TryAddWithoutValidation("Authorization", API_TOKEN);
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                request.Headers.Add("sign", sign);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request);
                sw.Stop();
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call SPLUSApi Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));

                string body = "";
                IEnumerable<string> values;
                if (response.Content.Headers.TryGetValues("Content-Encoding", out values) && values.Contains("gzip"))
                {
                    var gzip = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
                    var sr = new StreamReader(gzip, Encoding.UTF8);

                    body = await sr.ReadToEndAsync();
                }
                else
                {
                    body = await response.Content.ReadAsStringAsync();
                }

                var dics = new Dictionary<string, object>();

                dics.Add("request", JsonConvert.SerializeObject(formdata));
                dics.Add("response", responseLogFormat == null ? body : responseLogFormat(body));
                dics.Add("API_KEY", API_KEY);
                dics.Add("SignValue", sign);
                using (var scope = _logger.BeginScope(dics))
                {
                    _logger.LogInformation("SPLUS Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                }

                var result = JsonConvert.DeserializeObject<TResponse>(body);
                return result;
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
        }
        protected async Task<TResponse> GetAsync<TRequest, TResponse>(string url, TRequest req, Func<string, string> responseLogFormat = null, bool useGzip = false)
        {
            try
            {
                var queryParams = GetDictionary(req);
                var query = (string.Join("&", queryParams.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))));
                url = $"{url}?{query}";
                // 3. 加上 apiKey
                query += API_KEY;
                // 4. 計算 MD5 編碼
                string singstring ;
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query));
                    StringBuilder hashStringBuilder = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        hashStringBuilder.Append(b.ToString("x2"));
                    }
                    singstring = hashStringBuilder.ToString();
                }
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Authorization", API_TOKEN);
                request.Headers.Add("sign", singstring);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request);
                sw.Stop();
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call SPLUSApi Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));
                string body = "";

                body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TResponse>(body);
                return result;
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
        }
    }
}
