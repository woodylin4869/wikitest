using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Core;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.STANDARDS;
using ThirdPartyWallet.Share.Model.Game.STANDARDS.Request;
using ThirdPartyWallet.Share.Model.Game.STANDARDS.Response;
using static ThirdPartyWallet.Share.Model.Game.STANDARDS.Response.BetlogResponse;

namespace ThirdPartyWallet.GameAPI.Service.Game.STANDARDS
{
    public class STANDARDSApiService : ISTANDARDSApiService
    {
        public const string PlatformName = "STANDARDS";

        private readonly ILogger<STANDARDSApiService> _logger;
        private readonly IOptions<STANDARDSConfig> _options;
        private readonly HttpClient _httpClient;

        private readonly string API_KEY;
        private readonly List<string> mockUserList;

        private string API_TOKEN { get; set; }
        public STANDARDSApiService(ILogger<STANDARDSApiService> logger, IOptions<STANDARDSConfig> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;

            API_KEY = _options.Value.secretkey;
            API_TOKEN = _options.Value.STANDARDS_token;
            

        }

        /// <summary>
        /// 建立 Player
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<CreateResponse>> Player(CreateRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/player/create";
            //測試用
            //return new()
            //{
            //    status = new()
            //    {
            //        code = "1"
            //    }
            //};

            return await PostAsync<CreateRequest, ResponseBase<CreateResponse>>(url, request);
        }
        /// <summary>
        /// 玩家登出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<LogoutResponse>> Logout(LogoutRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/player/logout";
            ////測試用
            //return new()
            //{
            //    status = new()
            //    {
            //        code = "1"
            //    }
            //};
            return await PostAsync<LogoutRequest, ResponseBase<LogoutResponse>>(url, request);
        }

        /// <summary>
        /// Player 取得遊戲連結
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<GetlinkResponse>> GameLink(GetlinkRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/game/gamelink";
            ////測試用
            //return new()
            //{
            //    data = new()
            //    {
            //        URL = "https://ts.bacctest.com/"
            //    },
            //    status = new()
            //    {
            //        code = "1"
            //    }
            //};
            return await PostAsync<GetlinkRequest, ResponseBase<GetlinkResponse>>(url, request);
        }

        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<WithdrawResponse>> Withdraw(WithdrawRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/player/withdraw";
            ///測試用
            //return new()
            //{
            //    data = new()
            //    {
            //        account = request.account,
            //        amount = 1000
            //    },
            //    status = new()
            //    {
            //        code = "1"
            //    }
            //};
            return await PostAsync<WithdrawRequest, ResponseBase<WithdrawResponse>>(url, request);
        }

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<DepositResponse>> Deposit(DepositRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/player/deposit";
            ///測試用
            //return new()
            //{
            //    data = new()
            //    {
            //        account = request.account,
            //        amount = 1000
            //    },
            //    status = new()
            //    {
            //        code = "1"
            //    }
            //};
            return await PostAsync<DepositRequest, ResponseBase<DepositResponse>>(url, request);
        }

        /// <summary>
        /// 玩家遊戲錢包查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<WalletResponse>> Wallet(WalletRequest request)       
        {
            var url = _options.Value.STANDARDS_URL + "api/player/wallet";
            //測試用
            //return new()
            //{
            //    data = new()
            //    {
            //        account = request.account,
            //        balance = 1000
            //    },
            //    status = new()
            //    {
            //        code = "1"
            //    }
            //};
            return await PostAsync<WalletRequest, ResponseBase<WalletResponse>>(url, request);
        }

        /// <summary>
        /// 單筆交易紀錄查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<TransferResponse>> Transaction(TransferRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/player/transfer";
            return await PostAsync<TransferRequest, ResponseBase<TransferResponse>>(url, request);
        }


        /// <summary>
        /// 查詢注單詳細資訊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<BetlogResponse>> Betlog(BetlogRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/betlog/betlog_time";
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
            var url = _options.Value.STANDARDS_URL + "api/betlog/betlog_total";
            return await PostAsync<TotalBetlogRequest, ResponseBase<TotalBetlogResponse>>(url, request);
        }

        /// <summary>
        /// 第三層明細
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<PlaycheckResponse>> Playcheck(PlaycheckRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/betlog/playcheck";
            return await PostAsync<PlaycheckRequest, ResponseBase<PlaycheckResponse>>(url, request);
        }
        /// <summary>
        /// 查詢玩家是否在線
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<OnlineResponse>> Online(OnlineRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/player/online";
            return await PostAsync<OnlineRequest, ResponseBase<OnlineResponse>>(url, request);
        }
        /// <summary>
        /// 取得每小時總結
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<HourbetlogResponse>> Betlog_hour(HourbetlogRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/betlog/betlog_hour";
            return await PostAsync<HourbetlogRequest, ResponseBase<HourbetlogResponse>>(url, request);
        }
        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ResponseBase<HealthcheckResponse>> Healthcheck(HealthcheckRequest request)
        {
            var url = _options.Value.STANDARDS_URL + "api/healthcheck";
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
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                var propName = prop.Name;
                string propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-dd HH:mm:ss") : prop.GetValue(request)?.ToString();

                if (KeepNullField || propValue is not null)
                    param.Add(propName, propValue);
            }

            return param;
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
        private string BuildQueryString<TRequest>(TRequest request)
        {
            Dictionary<string, string> props = request as Dictionary<string, string>;

            if (request is null)
                props = GetDictionary(request);

            return string.Join('&', props.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => $"{p.Key}={p.Value ?? string.Empty}"));
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

                _logger.LogInformation("STANDARDS Post RequestPath: {RequestPath} Body:{body}", url, JsonConvert.SerializeObject(formdata));

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.SendAsync(request);

                sw.Stop();
                if (!response.IsSuccessStatusCode)
                    throw new Exception(string.Format("Call STANDARDSApi Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));

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
                dics.Add("sign", sign);
                using (var scope = _logger.BeginScope(dics))
                {
                    _logger.LogInformation("STANDARDS Post RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
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
                string singstring;
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

        /// <summary>
        /// 測試用 假住單資料
        /// </summary>
        public async Task<ResponseBase<BetlogResponse>> GetPagedGameDetailAsync(BetlogRequest source)
        {
            int DataCount = 4500;  //資料總筆數
            int PageSize = source.page_size;
            int page = source.page;
            DateTime playtime = Convert.ToDateTime(source.start_time);

            List<string> mockUserList = new List<string>();
            mockUserList.Add("devC240000000088");
            mockUserList.Add("dev2200011773");
            mockUserList.Add("dev2200011770");
            mockUserList.Add("dev2200011768");
            mockUserList.Add("dev2200011767");
            mockUserList.Add("dev2200011765");
            mockUserList.Add("dev2200011764");
            mockUserList.Add("dev2200011763");

            int fakeNum = PageSize;
            ResponseBase<BetlogResponse> fake = new ResponseBase<BetlogResponse>()
            {
                data = new BetlogResponse()
                {
                    page_info = new List<Page_Info>(), // 空陣列
                    current_page = page,
                    from = 0,
                    to = 0,
                    per_page = 2000,
                    last_page = DataCount / PageSize,
                    total = DataCount
                },
                status = new ()
                {
                    code="1",
                    message = "",
                    
                }
            };

            



            if (PageSize * page > DataCount)
            {
                int tempSize = DataCount - PageSize * (page-1)  ;
                if (tempSize >= PageSize)
                {
                    fakeNum = 0;
                }
                else
                {
                    fakeNum = tempSize;

                }
            }

            var random = new Random();


            for (int i = 0; i < fakeNum; i++)
            {
                string seqNumString = playtime.ToString("MMddHHmmss") + $"{((page ) * fakeNum + (i + 1)):D5}";
                string fakeUserId = mockUserList[random.Next(mockUserList.Count) % mockUserList.Count];
                fake.data.page_info.Add(new Page_Info()
                {
                    bet_id = seqNumString,
                    round="0",
                    gamecode = "Game01",
                    account = fakeUserId,
                    currency = "THB",
                    bet_amount = 10,
                    bet_valid_amount = 10,
                    pay_off_amount = 10+ i,
                    jp_win = 5,
                    freegame = 0,
                    bet_time = playtime,
                    pay_off_time = playtime,
                    status = "Win"
                });
            }

            await Task.Delay(random.Next(2000));

            return await Task.FromResult(fake);
        }


    }
}
