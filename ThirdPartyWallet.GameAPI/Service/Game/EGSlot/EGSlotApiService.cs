using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.GameAPI.Service.Game.EGSlot;
using ThirdPartyWallet.Share.Model.Game.EGSlot;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Response;



namespace H1_ThirdPartyWalletAPI.Service.Game.EGSlot
{
    public class EGSlotApiService : IEGSlotApiService
    {
        public const string PlatformName = "EGSlot";
        private readonly LogHelper<EGSlotApiService> _logger;
        private readonly IOptions<EGSlotConfig> _options;
        private readonly HttpClient _httpClient;

        public EGSlotApiService(LogHelper<EGSlotApiService> logger, IOptions<EGSlotConfig> options,
            HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }

        /// <summary>
        /// 進線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LoginResponse> LoginAsync(LoginRequest source)
        {
            source.HomeURL = HttpUtility.UrlEncode(source.HomeURL);
            var response = await GetHandlerHash("login", source);
            return JsonConvert.DeserializeObject<LoginResponse>(response);
        }
        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LogoutResponse> LogoutAsync(LogoutRequest source)
        {
            var response = await PostHandlerHash("logout", source);
            return JsonConvert.DeserializeObject<LogoutResponse>(response);
        }
        /// <summary>
        /// 登出全部玩家
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<LogoutAllResponse> LogoutAllAsync(LogoutAllRequest source)
        {
            var response = await PostHandlerHash("logout/all", source);
            return JsonConvert.DeserializeObject<LogoutAllResponse>(response);
        }
        /// <summary>
        /// 創建會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<PlayersResponse> PlayersAsync(PlayersRequest source)
        {
            var response = await PostHandlerHash("players", source);
            return JsonConvert.DeserializeObject<PlayersResponse>(response);

        }
        /// <summary>
        /// 會員取餘額(查詢狀態)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<StatusResponse> StatusAsync(StatusRequest source)
        {
            var response = await GetHandlerHash("players/status", source);
            return JsonConvert.DeserializeObject<StatusResponse>(response);
        }
        /// <summary>
        /// 會員轉帳正數存款負數取款 存款 TakeAll 要為False
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferinResponse> TransferinAsync(TransferinRequest source)
        {
            var response = await PostHandlerHash("transfer", source);
            return JsonConvert.DeserializeObject<TransferinResponse>(response);

        }
        /// <summary>
        /// 會員轉帳正數存款負數取款 存款 TakeAll 要為False
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferoutResponse> TransferoutAsync(TransferoutRequest source)
        {
            var response = await PostHandlerHash("transfer", source);
            return JsonConvert.DeserializeObject<TransferoutResponse>(response);

        }
        /// <summary>
        /// 注單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransactionResponse> TransactionAsync(TransactionRequest source)
        {
            source.AgentName = _options.Value.EGSlot_MerchantCode;
            var response = await GetHandlerHash("history/transaction", source);
            return JsonConvert.DeserializeObject<TransactionResponse>(response);
        }
        public async Task<TransferHistoryResponse> TransferHistoryAsync(TransferHistoryRequest source)
        {
            var response = await GetHandlerHash("history/transfer", source);
            return JsonConvert.DeserializeObject<TransferHistoryResponse>(response);
        }

        public async Task<GetdetailurlResponse> GetdetailurlAsync(GetdetailurlRequest source)
        {
            var response = await GetHandlerHash("history/detail/url", source);
            return JsonConvert.DeserializeObject<GetdetailurlResponse>(response);
        }

        public async Task<GethourdataResponse> GethourdataAsync(GethourdataRequest source)
        {
            var response = await GetHandlerHash("history/summary", source);
            return JsonConvert.DeserializeObject<GethourdataResponse>(response);
        }

        public async Task<GetagentsResponse> GetagentsAsync()
        {
            var response = await GetHandlerHash("agents", new object());
            return JsonConvert.DeserializeObject<GetagentsResponse>(response);
        }

        private async Task<string> GetHandlerHash(string url, object source)
        {
            //&Username=<string>&GameID=<string>&LoginHall=<boolean>&AgentName=<string>&Lang=<string>&HomeURL=<string>
            var Hashpasswords = Helper.ConvertToKeyValue(source, "HomeURL");

            var AddUrl = Helper.ConvertToKeyValue(source, "");
            //Hash加密 &Username=<string>&GameID=<string>&LoginHall=<boolean>&AgentName=<string>&Lang=<string>&HomeURL=<string> 成為字串
            var Hashdo = Helper.CreateHMACSHA256(Hashpasswords, _options.Value.EGSlot_HashKey);

            //{{baseUrl}}/:Platform/login?Hash=<string>&Token=<string>&Username=<string>&GameID=<string>&LoginHall=<boolean>&AgentName=<string>&Lang=<string>&HomeURL=<string>
            var apiUrl = $"{_options.Value.EGSlot_URL}/{_options.Value.EGSlot_Platform}/{url}?Hash={Hashdo}&{AddUrl}";
            // Append each parameter only if it's not null or empty
            var response = await Get(apiUrl, source);
            // Response回傳為空值就會直接回傳
            if (string.IsNullOrEmpty(response))
            {
                response = JsonConvert.SerializeObject(new ErrorCodeResponse());
            }
            return response;
        }

        private async Task<string> PostHandlerHash(string url, object source)
        {
            //將帶入的參數轉換成JSON格式={"test":"hello world"}
            var HashJson = JsonConvert.SerializeObject(source);

            //Hash加密 將範例{"test":"hello world"} 轉換成為字串
            var Hashdo = Helper.CreateHMACSHA256(HashJson, _options.Value.EGSlot_HashKey);

            //{{baseUrl}}/:Platform/login?Hash=<string>&Token=<string>&Username=<string>&GameID=<string>&LoginHall=<boolean>&AgentName=<string>&Lang=<string>&HomeURL=<string>
            var apiUrl = $"{_options.Value.EGSlot_URL}/{_options.Value.EGSlot_Platform}/{url}?Hash={Hashdo}";
            // Append each parameter only if it's not null or empty
            var response = await Post(apiUrl, source);
            // 如果Response為null就會直接回傳空值
            if (string.IsNullOrEmpty(response))
            {
                response = JsonConvert.SerializeObject(new ErrorCodeResponse());
            }
            return response;
        }

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private async Task<string> Post<TRequest>(string url, TRequest source,
            Dictionary<string, string> headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            var body = "";
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != null && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                body = await response.Content.ReadAsStringAsync();
            }
            else if (contentType == null && response.IsSuccessStatusCode)
            {
                body = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new InvalidOperationException($"Expected application/json response, got {contentType}");
            }
            //var body = await response.Content.ReadAsStringAsync();

            return body;
        }
        /// <summary>
        /// GET
        /// </summary>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<string> Get(string url, object source)
        {

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);
            var body = "";
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType != null && contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                body = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new InvalidOperationException($"Expected application/json response, got {contentType}");
            }
            //var body = await response.Content.ReadAsStringAsync();

            return body;

        }
    }
}
