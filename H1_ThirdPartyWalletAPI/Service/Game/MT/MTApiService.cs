using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.MT
{
    public class MTApiService : IMTApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MTApiService> _logger;

        public MTApiService(IHttpClientFactory httpClientFactory, ILogger<MTApiService> logger, IApiHealthCheckService apiHealthCheckService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<playerCreate2Response> playerCreateAsync(PlayerCreateRequest source)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/playerCreate2/";

            PlayerCreaterawData rawData = new PlayerCreaterawData()
            {
                nickname = source.playerName,
                playerLevel = 0
            };
            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.pwd = Helper.MD5encryption(source.pwd, "", "").ToLower();
            source.code = Helper.MD5encryption(Config.CompanyToken.MT_key, rawDataJson, "").ToLower();
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<PlayerCreateRequest, playerCreate2Response>(url, source);

        }
        /// <summary>
        /// 取得會員餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<getPlayerBalanceResponse> getPlayerBalanceAsync(getPlayerBalanceRequest source)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/getPlayerBalance/";

            getPlayerBalancerawData rawData = new getPlayerBalancerawData()
            {
                currency = "THB1"
            };
            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<getPlayerBalanceRequest, getPlayerBalanceResponse>(url, source);
        }
        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<deposit2Response> deposit2Async(deposit2Request source)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/deposit2/";

            deposit2rawData rawData = new deposit2rawData()
            {
                merchantId = Config.CompanyToken.MT_merchantId,
                playerName = source.playerName,
                extTransId = source.extTransId,
                coins = source.coins,
                currency = "THB1"
            };
            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.code = Helper.MD5encryption(Config.CompanyToken.MT_key, rawDataJson, "").ToLower();
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<deposit2Request, deposit2Response>(url, source);
        }

        /// <summary>
        /// 轉出
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<withdraw2Response> withdraw2Async(withdraw2Request source)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/withdraw2/";

            withdraw2rawData rawData = new withdraw2rawData()
            {
                merchantId = Config.CompanyToken.MT_merchantId,
                playerName = source.playerName,
                extTransId = source.extTransId,
                coins = source.coins,
                currency = "THB1"
            };
            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.code = Helper.MD5encryption(Config.CompanyToken.MT_key, rawDataJson, "").ToLower();
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<withdraw2Request, withdraw2Response>(url, source);

        }
        /// <summary>
        ///  登入
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<playerPlatformUrlResponse> playerPlatformUrlAsync(PlayerPlatformUrlRequest source, PlayerPlatformUrlrawData rawData)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/playerPlatformUrl/";

            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.pwd = Helper.MD5encryption(source.pwd, "", "").ToLower();
            source.code = Helper.MD5encryption(Config.CompanyToken.MT_key, rawDataJson, "").ToLower();
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<PlayerPlatformUrlRequest, playerPlatformUrlResponse>(url, source);
        }
        /// <summary>
        /// 查詢交易狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<QueryTransbyIdResponse> queryTransbyIdAsync(QueryTransbyIdRequest source)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/queryTransbyId/";

            QueryTransbyIdrawData rawData = new QueryTransbyIdrawData()
            {
                currency = "THB1"
            };
            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<QueryTransbyIdRequest, QueryTransbyIdResponse>(url, source);
        }
        /// <summary>
        /// 取得住單
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public async Task<queryMerchantGameRecord2Response> queryMerchantGameRecord2Async(QueryMerchantGameRecord2rawData rawData)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/queryMerchantGameRecord2/";
            QueryMerchantGameRecord2Request source = new QueryMerchantGameRecord2Request();

            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<QueryMerchantGameRecord2Request, queryMerchantGameRecord2Response>(url, source);
        }

        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<logOutGameResponse> logOutGameAsync(logOutGameRequest source)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/logOutGame/";
            source.merchantId = Config.CompanyToken.MT_merchantId;
            return await ApiHandle<logOutGameRequest, logOutGameResponse>(url, source);
        }
        /// <summary>
        /// 明細轉跳
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public async Task<playCheckUrlResponse> playCheckUrlAsync(playCheckUrlrawData rawData)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/playCheckUrl/";
            playCheckUrlRequest source = new playCheckUrlRequest();

            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.code = Helper.MD5encryption(Config.CompanyToken.MT_key, rawDataJson, "").ToLower();
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<playCheckUrlRequest, playCheckUrlResponse>(url, source);
        }

        /// <summary>
        /// 每日匯總 日曆日
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public async Task<queryMerchantGameDataResponse> queryMerchantGameDataAsync(queryMerchantGameDatarawData rawData)
        {
            var url = Config.GameAPI.MT_URL + "services/dg/player/queryMerchantGameData/";
            queryMerchantGameDataRequest source = new queryMerchantGameDataRequest();
            var rawDataJson = JsonConvert.SerializeObject(rawData);
            source.merchantId = Config.CompanyToken.MT_merchantId;
            source.data = Helper.Bast64(rawDataJson);
            return await ApiHandle<queryMerchantGameDataRequest, queryMerchantGameDataResponse>(url, source);
        }
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var Dic = Helper.GetDictionary(source);

            var DataString = String.Join("/", Dic.Select(x => x.Value));

            url = url + DataString;


            var headers = new Dictionary<string, string>
            {
                { "Content-Type","application/json" },
            };

            var postData = new Dictionary<string, string>
            {
            };
            var responseData = await Post(url, Dic, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }

        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 3)
        {
            try
            {
                _logger.LogInformation("MT Post RequestPath: {RequestPath}", url);
                using (var request = _httpClientFactory.CreateClient("log"))
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            request.DefaultRequestHeaders.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    request.Timeout = TimeSpan.FromSeconds(14);
                    if (url.Contains("services/dg/player/queryMerchantGameRecord2"))
                        request.Timeout = TimeSpan.FromSeconds(60);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var response = await request.PostAsync(Platform.MT, url, new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));
                    sw.Stop();
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    var dics = new Dictionary<string, object>
                {
                    { "request", postData },
                    { "response", body }
                };

                    using (var scope = _logger.BeginScope(dics))
                    {
                        _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                    }

                    return body;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "{platform} {action} {status} {statuscode}", Platform.MT, "POST", LogLevel.Error, (int)ex.StatusCode);
                throw;
            }
        }
    }
}
