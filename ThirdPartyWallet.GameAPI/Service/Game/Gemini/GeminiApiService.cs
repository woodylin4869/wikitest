using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.Gemini;
using ThirdPartyWallet.Share.Model.Game.Gemini.Request;
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;



namespace H1_ThirdPartyWalletAPI.Service.Game.Gemini
{
    public class GeminiApiService : IGeminiApiService
    {
        public const string PlatformName = "GEMINI";

        private readonly LogHelper<GeminiApiService> _logger;
        private readonly IOptions<GeminiConfig> _options;
        private readonly HttpClient _httpClient;
        public GeminiApiService(LogHelper<GeminiApiService> logger, IOptions<GeminiConfig> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }

        public async Task<CreateplayerResponse> CreateplayerAsync(CreateplayerRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/player/create";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = "1";

            return await ApiHandle<CreateplayerRequest, CreateplayerResponse>(url, source);
        }

        public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/player/balance";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = "3";

            return await ApiHandle<GetBalanceRequest, GetBalanceResponse>(url, source);
        }

        public async Task<TransferinResponse> TransferinAsync(TransferinRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/transaction/transfer_in";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = "4";

            return await ApiHandle<TransferinRequest, TransferinResponse>(url, source);
        }
        public async Task<TransferoutResponse> TransferoutAsync(TransferoutRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/transaction/transfer_out";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = "5";

            return await ApiHandle<TransferoutRequest, TransferoutResponse>(url, source);
        }

        public async Task<QueryorderResponse> QueryorderAsync(QueryorderRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/transaction/query_order";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = "6";

            return await ApiHandle<QueryorderRequest, QueryorderResponse>(url, source);
        }

        public async Task<LaunchResponse> LaunchAsync(LaunchRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/game/launch";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = "1";

            return await ApiHandle<LaunchRequest, LaunchResponse>(url, source);
        }

        public async Task<BetlistResponse> BetlistAsync(BetlistRequest source)
        {
            var url = _options.Value.Gemini_URL + "v2/operator/record/bet_list";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = Guid.NewGuid().ToString();
            source.supplier = "Gemini";

            return await ApiHandle<BetlistRequest, BetlistResponse>(url, source);
        }


        public async Task<GamedetailResponse> GamedetailAsync(GamedetailRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/url/game_detail";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = Guid.NewGuid().ToString();
            return await ApiHandle<GamedetailRequest, GamedetailResponse>(url, source);
        }

        public async Task<GameListResponse> GameListAsync(GameListRequest source)
        {
            var url = _options.Value.Gemini_URL + "v1/operator/game/list";
            source.product_id = _options.Value.Gemini_MerchantCode;
            source.seq = Guid.NewGuid().ToString();
            return await ApiHandle<GameListRequest, GameListResponse>(url, source);
        }

        public async Task<string> healthcheckAsync()
        {
            var url = _options.Value.Gemini_URL + "v1/health_check";
            return await GetAsync(url);
        }


        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            //var Dic = Helper.GetDictionary(source);

            var accesskey = Helper.MD5encryption(_options.Value.Gemini_secrect, JsonConvert.SerializeObject(source), "").ToUpper();

            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" },
                {"els-access-key",accesskey}
            };

            var responseData = await Post(url, source, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }

        private async Task<string> Post<TRequest>(string url, TRequest source, Dictionary<string, string> headers = null)
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
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return body;
        }

        public async Task<string> GetAsync(string url)
        {

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
