using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using ThirdPartyWallet.Common;
using ThirdPartyWallet.Share.Model.Game.PS.Request;
using ThirdPartyWallet.Share.Model.Game.PS.Response;
using ThirdPartyWallet.Share.Model.Game.PS;
using System.Collections.Generic;

namespace ThirdPartyWallet.GameAPI.Service.Game.PS
{
    public class PsApiService : IPsApiService
    {
        public const string PlatformName = "PS";

        private readonly LogHelper<PsApiService> _logger;
        private readonly IOptions<PsConfig> _options;
        private readonly HttpClient _httpClient;
        public PsApiService(LogHelper<PsApiService> logger, IOptions<PsConfig> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }
        public async Task<CreateuserResponse> CreateplayerAsync(CreateuserRequest source)
        {
            var url = _options.Value.PS_URL + "/funds/createplayer";

            return await ApiHandleGet<CreateuserRequest, CreateuserResponse>(url, source);
        }
        public async Task<GetgameResponse> GetgmaeurlAsync(GetgameRequest source)
        {
            var url = _options.Value.PS_URL + "/launch";

            return await ApiHandleGet<GetgameRequest, GetgameResponse>(url, source);
        }
        public async Task<DepositResponse> MoneyinAsync(DepositRequest source)
        {
            var url = _options.Value.PS_URL + "/funds/deposit/";

            return await ApiHandleGet<DepositRequest, DepositResponse>(url, source);
        }
        public async Task<WithdrawResponse> MoneyoutAsync(WithdrawRequest source)
        {
            var url = _options.Value.PS_URL + "/funds/withdraw";

            return await ApiHandleGet<WithdrawRequest, WithdrawResponse>(url, source);
        }
        public async Task<GetbalanceResponse> GetBalanceAsync(GetbalanceRequest source)
        {
            var url = _options.Value.PS_URL + "/funds/getbalance";

            return await ApiHandleGet<GetbalanceRequest, GetbalanceResponse>(url, source);
        }
        public async Task<kickoutResponse> KickUserAsync(kickoutRequest source)
        {
            var url = _options.Value.PS_URL + "/admin/kickout";

            return await ApiHandleGet<kickoutRequest, kickoutResponse>(url, source);
        }
        public async Task<kickallResponse> KickallAsync(kickallRequest source)
        {
            var url = _options.Value.PS_URL + "/admin/kickout";

            return await ApiHandleGet<kickallRequest, kickallResponse>(url, source);
        }
        public async Task<List<QueryorderResponse>> QueryorderAsync(QueryorderRequest source)
        {
            var url = _options.Value.PS_URL + "/funds/log";

            return await ApiHandleGet2<QueryorderRequest, List<QueryorderResponse>>(url, source);
        }
        public async Task<healthcheckResponse> healthcheckAsync(healthcheckRequest source)
        {
            var url = _options.Value.PS_URL + "/admin/gethostapiinfo";
            return await ApiHandleGet<healthcheckRequest, healthcheckResponse>(url, source);
        }

        public async Task<hoursummaryResponse> hourlysummaryAsync(hoursummaryRequest source)
        {
            var url = _options.Value.PS_URL + "/feed/hourlysummary";
            return await ApiHandleGet<hoursummaryRequest, hoursummaryResponse>(url, source);
        }

        public async Task<Dictionary<string, Dictionary<string, List<GetorderResponse.BetRecord>>>> gamehistoryAsync(GetorderRequest source)
        {
            var url = _options.Value.PS_URL + "/feed/gamehistory";
            return await ApiHandleGet1<GetorderRequest, Dictionary<string, Dictionary<string, List<GetorderResponse.BetRecord>>>>(url, source);
        }

        /// <summary>
        /// 時間格式轉換
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <param name="KeepNullField"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetDictionary<TRequest>(TRequest request, bool KeepNullField = false)
        {
            var props = typeof(TRequest).GetProperties();
            var param = new Dictionary<string, object>();
            foreach (var prop in props)
            {
                var propName = prop.Name;
                object propValue = prop.GetValue(request);
                if (prop.PropertyType == typeof(decimal))
                {
                    decimal money = (decimal)propValue;
                    propValue = Math.Round(money, 0);
                }
                if (prop.PropertyType == typeof(DateTime))
                {
                    DateTime dt = (DateTime)propValue;
                    propValue = dt.ToString("yyyy-MM-dd'T'HH:mm:ss");
                }
                if (KeepNullField || propValue != null)
                    param.Add(Uri.EscapeDataString(propName), propValue);
            }
            return param;
        }
        private async Task<TResponse> ApiHandlePOST<TRequest, TResponse>(string url, TRequest source)
        {
            var responseData = await Post(url, source);
            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }
        private async Task<string> Post<TRequest>(string url, TRequest source)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            var dict = source.GetType()
           .GetProperties(BindingFlags.Instance | BindingFlags.Public)
           .ToDictionary(
               prop => prop.Name,
               prop => prop.GetValue(source, null)?.ToString());
            request.Content = new FormUrlEncodedContent(dict);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            return body;
        }
        private async Task<TResponse> ApiHandleGet<TRequest, TResponse>(string url, TRequest source)
        {
            var responseData = await GET(url, source);

            return JsonConvert.DeserializeObject<TResponse>(responseData);
        }
        private async Task<Dictionary<string, Dictionary<string, List<GetorderResponse.BetRecord>>>> ApiHandleGet1<TRequest, TResponse>(string url, TRequest source)
        {
            var responseData = await GET(url, source);

            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<GetorderResponse.BetRecord>>>>(responseData);

        }
        private async Task<List<QueryorderResponse>> ApiHandleGet2<TRequest, TResponse>(string url, TRequest source)
        {
            var responseData = await GET(url, source);
            responseData.Replace("[", "").Replace("]", "");
            return JsonConvert.DeserializeObject<List<QueryorderResponse>>(responseData);
        }

        private async Task<string> GET<TRequest>(string url, TRequest source)
        {
            var queryParams = GetDictionary(source);
            var query = (string.Join("&", queryParams.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))));
            url = $"{url}?{query}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
    }
}
