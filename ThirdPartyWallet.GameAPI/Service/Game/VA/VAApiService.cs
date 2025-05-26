using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.VA;
using ThirdPartyWallet.Share.Model.Game.VA.Request;
using ThirdPartyWallet.Share.Model.Game.VA.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.VA
{
    public class VAApiService : IVAApiService
    {
        public const string PlatformName = "VA";

        private readonly LogHelper<VAApiService> _logger;
        private readonly IOptions<VAConfig> _options;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _channelId;
        private readonly string _authorization;
        private readonly string _apiUrl;
        private readonly JsonSerializerSettings _serializerSettings;
        public VAApiService(LogHelper<VAApiService> logger, IOptions<VAConfig> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
            _apiKey = _options.Value.VA_KEY ?? string.Empty;
            _channelId = _options.Value.VA_channelId ?? string.Empty;
            _authorization = _options.Value.VA_Authorization ?? string.Empty;
            _apiUrl = _options.Value.VA_URL?.TrimEnd('/') ?? string.Empty;

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
        public async Task<BaseResponse<CreateResponse>> CreateAsync(CreateRequest source)
        {
            var url = _apiUrl + "/api/v1.0/member/create";

            source.Currency = "THB";
            source.ChannelId = _channelId;

            return await PostAsync<CreateRequest, CreateResponse>(url, source);
        }

        public async Task<BaseResponse<GetBalanceResponse>> GetBalanceAsync(GetBalanceRequest source)
        {
            var url = _apiUrl + "/api/v1.0/member/balance";

            source.Currency = "THB";
            source.ChannelId = _channelId;

            return await GetAsync<GetBalanceRequest, GetBalanceResponse>(url, source);
        }

        public async Task<BaseResponse<KickUserResponse>> KickUserAsync(KickUserRequest source)
        {
            var url = _apiUrl + "/api/v1.0/member/kick";
            source.ChannelId = _channelId;
            return await PostAsync<KickUserRequest, KickUserResponse>(url, source);
        }

        public async Task<BaseResponse<GetGameListResponse>> GetGameListAsync(GetGameListRequest source)
        {
            var url = _apiUrl + "/api/v1.0/info/gameList";
            return await GetAsync<GetGameListRequest, GetGameListResponse>(url, source);
        }


        public async Task<BaseResponse<GameLinkResponse>> GameLinkAsync(GameLinkRequest source)
        {
            var url = _apiUrl + "/api/v2.0/game/gameLink";
            source.ChannelId = _channelId;
            source.Currency = "THB";
            source.App = "N";

            try
            {
                // 將請求數據轉換為字典形式
                var queryParams = GetDictionary(source);
                queryParams.Add("sign", GenerateSign(queryParams, _apiKey)); // 計算簽名
                queryParams.Add("url", source.Url); // URL
                // 將字典轉換為 URL 查詢參數
                var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));

                // 拼接完整 URL
                var fullUrl = $"{url}?{queryString}";

                // 創建 GET 請求
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl);

                // 設置授權標頭
                requestMessage.Headers.TryAddWithoutValidation("Authorization", _authorization);

                // 發送請求
                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                // 檢查是否成功
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Call VAApi Failed! URL: {fullUrl}, Status: {response.StatusCode}");
                }

                // 讀取響應正文
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(body))
                {
                    throw new Exception("Empty response body from API");
                }

                // 反序列化響應數據
                var result = JsonConvert.DeserializeObject<BaseResponse<GameLinkResponse>>(body);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize response body");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                // 捕獲網絡請求錯誤
                throw new Exception($"Request to {url} failed with error: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                // 捕獲反序列化錯誤
                throw new Exception("Failed to deserialize response", ex);
            }
            catch (Exception ex)
            {
                // 捕獲所有其他異常
                throw new Exception("An error occurred during the API request", ex);
            }
        }


        public async Task<BaseResponse<DepositResponse>> DepositAsync(DepositRequest source)
        {
            var url = _apiUrl + "/api/v1.0/transaction/deposit";
            source.Currency = "THB";
            source.ChannelId = _channelId;
            return await PostAsync<DepositRequest, DepositResponse>(url, source);
        }

        public async Task<BaseResponse<WithdrawResponse>> WithdrawAsync(WithdrawRequest source)
        {
            var url = _apiUrl + "/api/v1.0/transaction/withdraw";
            source.Currency = "THB";
            source.ChannelId = _channelId;
            return await PostAsync<WithdrawRequest, WithdrawResponse>(url, source);
        }

        public async Task<BaseResponse<TransactionDetailResponse>> TransactionDetailAsync(TransactionDetailRequest source)
        {
            var url = _apiUrl + "/api/v1.0/info/transactionDetail";

            return await GetAsync<TransactionDetailRequest, TransactionDetailResponse>(url, source);
        }

        /// <summary>
        /// 注單最大保留時間為2小時。若同一注單單號有一張注單以上的注單 (Ex:改單狀況)，則回傳最晚生成的那張注單。
        /// 此服務有連線次數限制，限制為每秒限制呼叫 2 次 (2/s)。
        /// 按照searchMode帶入參數回傳下注時間或結單時間介於搜尋區間的注單 (包含搜尋起訖時間)。
        /// 單次搜尋的起迄時間區間最大為15分鐘。
        /// 搜尋結束時間應小於當前時間前2分鐘。
        /// 此功能不提供單錢包介接商使用。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BaseResponse<BetlogListByTimeResponse>> BetlogListByTimeAsync(BetlogListByTimeRequest source)
        {
            var url = _apiUrl + "/api/v1.0/betlog/listByTime";
            source.ChannelId = _channelId;
            source.SearchMode = 0;
            return await GetAsync<BetlogListByTimeRequest, BetlogListByTimeResponse>(url, source);
        }

        public async Task<BaseResponse<BetlogDetailResponse>> BetlogDetailAsync(BetlogDetailRequest source)
        {
            var url = _apiUrl + "/api/v1.0/betlog/detail";
            return await GetAsync<BetlogDetailRequest, BetlogDetailResponse>(url, source);
        }

        /// <summary>
        /// 歷史注單為1小時前注單，最大保留時間為3個月。
        /// 若同一注單單號有一張注單以上的注單 (Ex:改單狀況)，則回傳最晚生成的那張注單。
        /// 此服務有連線次數限制，限制為每秒限制呼叫 2 次 (2/s)。
        /// 按照searchMode帶入參數回傳下注時間或結單時間介於搜尋區間的注單 (包含搜尋起訖時間)。
        /// 單次搜尋的起迄時間區間最大為15分鐘。
        /// 此功能不提供單錢包介接商使用。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BaseResponse<BetlogHistoryListByTimeResponse>> BetlogHistoryListByTimeAsync(BetlogHistoryListByTimeRequest source)
        {
            var url = _apiUrl + "/api/v1.0/betlog/history/listByTime";
            source.ChannelId = _channelId;
            source.SearchMode = 0;
            return await GetAsync<BetlogHistoryListByTimeRequest, BetlogHistoryListByTimeResponse>(url, source);
        }

        /// <summary>
        /// 5.1 取得幣別報表
        /// 注單最大保留時間為3個月。
        /// 此服務有連線次數限制，限制為每天限制呼叫 80 次 (80/d)
        /// 按照searchMode帶入參數對下注時間或結單時間介於搜尋區間的注單(包含搜尋起訖時間)進行加總。
        /// 若按照下注時間搜尋，會回傳下注期間的所有注單；若按照結算時間搜尋，僅會回傳已結算的注單。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BaseResponse<ReportCurrencyResponse>> ReportCurrencyAsync(ReportCurrencyRequest source)
        {
            var url = _apiUrl + "/api/v1.0/report/currency";
            source.ChannelId = _channelId;
            source.SearchMode = 0;
            return await GetAsync<ReportCurrencyRequest, ReportCurrencyResponse>(url, source);
        }

        /// <summary>
        /// 8.1 API服務檢查
        /// 應用說明
        /// 此服務供檢查API狀態是否正常。
        /// 此服務有連線次數限制，限制為每 5 秒限制呼叫 １ 次(1/5s)。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<healthcheckResponse> healthcheckAsync(healthcheckRequest source)
        {

            var url = _apiUrl + "/health-check";
            try
            {
                // 將請求數據轉換為字典形式
                var queryParams = GetDictionary(source);
                queryParams.Add("sign", GenerateSign(queryParams, _apiKey)); // 計算簽名

                // 將字典轉換為 URL 查詢參數
                var queryString = string.Join("&", queryParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // 拼接完整 URL
                var fullUrl = $"{url}?{queryString}";

                // 創建 GET 請求
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl);

                // 設置授權標頭
                requestMessage.Headers.TryAddWithoutValidation("Authorization", _authorization);

                // 發送請求
                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                // 檢查是否成功
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Call VAApi Failed! URL: {fullUrl}, Status: {response.StatusCode}");
                }

                // 讀取響應正文
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(body))
                {
                    throw new Exception("Empty response body from API");
                }

                // 反序列化響應數據
                var result = JsonConvert.DeserializeObject<healthcheckResponse>(body);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize response body");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                // 捕獲網絡請求錯誤
                throw new Exception($"Request to {url} failed with error: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                // 捕獲反序列化錯誤
                throw new Exception("Failed to deserialize response", ex);
            }
            catch (Exception ex)
            {
                // 捕獲所有其他異常
                throw new Exception("An error occurred during the API request", ex);
            }
        }



        // 生成 sign 的方法
        public static string GenerateSign(Dictionary<string, string> data, string apiKey)
        {
            // 1. 按照 key 進行升序排序
            var sortedData = data.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // 2. 將每個參數轉換為 URL-encoded 並合併成字串
            var queryString = string.Join("&", sortedData.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

            // 3. 加上 apiKey
            queryString += apiKey;

            // 4. 計算 MD5 編碼
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }

                return hashStringBuilder.ToString().ToLower(); // 返回小寫的 md5 結果
            }
        }
        /// <summary>
        /// 時間格式轉換
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="KeepNullField"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            List<string> ignoreList = new List<string> { "Url" };
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, string>();

            foreach (var prop in props)
            {
                // Skip property if it is in the ignoreList
                if (ignoreList.Contains(prop.Name))
                    continue;

                // 轉換為小駝峰命名規則
                var propName = ToCamelCase(prop.Name);
                string propValue = prop.PropertyType == typeof(DateTime) ? ((DateTime)prop.GetValue(request)).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz") : prop.GetValue(request)?.ToString();

                if (KeepNullField || propValue is not null)
                    param.Add(propName, propValue);
            }

            return param;
        }

        // 將屬性名稱轉換為小駝峰格式
        private static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2)
                return str.ToLower();

            return char.ToLower(str[0]) + str.Substring(1);
        }

        protected async Task<BaseResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            try
            {
                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                // 設置授權標頭
                requestMessage.Headers.TryAddWithoutValidation("Authorization", _authorization);

                // 轉換請求數據為字典形式
                var formData = GetDictionary(request);
                formData.Add("sign", GenerateSign(formData, _apiKey)); // 計算簽名

                // 使用 MultipartFormDataContent 來處理表單數據
                using var formContent = new MultipartFormDataContent();

                // 添加每一個表單字段
                foreach (var field in formData)
                {
                    formContent.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                }

                // 設置請求內容
                requestMessage.Content = formContent;

                // 發送請求
                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                // 檢查是否成功
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Call VAApi Failed! URL: {url}, Status: {response.StatusCode}");
                }

                // 讀取響應正文
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(body))
                {
                    throw new Exception("Empty response body from API");
                }

                // 反序列化響應數據
                var result = JsonConvert.DeserializeObject<BaseResponse<TResponse>>(body);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize response body");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                // 捕獲網絡請求錯誤
                throw new Exception($"Request to {url} failed with error: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                // 捕獲反序列化錯誤
                throw new Exception("Failed to deserialize response", ex);
            }
            catch (Exception ex)
            {
                // 捕獲所有其他異常
                throw new Exception("An error occurred during the API request", ex);
            }
        }

        protected async Task<BaseResponse<TResponse>> GetAsync<TRequest, TResponse>(string url, TRequest request)
        {
            try
            {
                // 將請求數據轉換為字典形式
                var queryParams = GetDictionary(request);
                queryParams.Add("sign", GenerateSign(queryParams, _apiKey)); // 計算簽名

                // 將字典轉換為 URL 查詢參數
                var queryString = string.Join("&", queryParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

                // 拼接完整 URL
                var fullUrl = $"{url}?{queryString}";

                // 創建 GET 請求
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fullUrl);

                // 設置授權標頭
                requestMessage.Headers.TryAddWithoutValidation("Authorization", _authorization);

                // 發送請求
                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                // 檢查是否成功
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Call VAApi Failed! URL: {fullUrl}, Status: {response.StatusCode}");
                }

                // 讀取響應正文
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(body))
                {
                    throw new Exception("Empty response body from API");
                }

                // 反序列化響應數據
                var result = JsonConvert.DeserializeObject<BaseResponse<TResponse>>(body);
                if (result == null)
                {
                    throw new Exception("Failed to deserialize response body");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                // 捕獲網絡請求錯誤
                throw new Exception($"Request to {url} failed with error: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                // 捕獲反序列化錯誤
                throw new Exception("Failed to deserialize response", ex);
            }
            catch (Exception ex)
            {
                // 捕獲所有其他異常
                throw new Exception("An error occurred during the API request", ex);
            }
        }
    }
}
