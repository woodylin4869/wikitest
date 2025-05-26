using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace H1_ThirdPartyWalletAPI.Service.Game.OB
{
    public class OBApiService : IOBApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OBApiService> _logger;
        public OBApiService(IHttpClientFactory httpClientFactory, ILogger<OBApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<CreateMemberResponse> CreateMemberAsync(Model.Game.OB.Reqserver.CreateMemberRequest source)
        {
            var url = Config.GameAPI.OB_API_URL + "api/merchant/create/v1";
            return await ApiHandle<Model.Game.OB.Reqserver.CreateMemberRequest, CreateMemberResponse>(url, source); ;
        }

        public async Task<FastGameResponse> FastGameAsync(FastGameReqserver source)
        {
            var url = Config.GameAPI.OB_API_URL + "api/merchant/forwardGame/v2";
            return await ApiHandle<FastGameReqserver, FastGameResponse>(url, source); ;
        }

        public async Task<GetbalanceResponse> GetbalanceAsync(GetbalanceReqserver source)
        {
            var url = Config.GameAPI.OB_API_URL + "api/merchant/balance/v1";
            return await ApiHandle<GetbalanceReqserver, GetbalanceResponse>(url, source); ;
        }
        public async Task<DepositResponse> DepositAsync(depositReqserver source)
        {
            var url = Config.GameAPI.OB_API_URL + "api/merchant/deposit/v1";
            return await ApiHandle<depositReqserver, DepositResponse>(url, source);
        }
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawReqserver source)
        {
            var url = Config.GameAPI.OB_API_URL + "api/merchant/withdraw/v1";
            return await ApiHandle<WithdrawReqserver, WithdrawResponse>(url, source); ;
        }
        public async Task<TransferResponse> TransferAsync(TransferReqserver source)
        {
            var url = Config.GameAPI.OB_API_URL + "api/merchant/transfer/v1";
            return await ApiHandle<TransferReqserver, TransferResponse>(url, source); ;
        }

        public async Task<BetHistoryRecordResponse> BetHistoryRecordAsync(BetHistoryRecordReqserver source)
        {
            var url = Config.GameAPI.OB_DATA_URL + "data/merchant/betHistoryRecord/v1";
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "merchantCode",Config.CompanyToken.OB_MerchantCode},
                { "pageIndex",source.pageIndex.ToString()},
            };
            return await DataHandle<BetHistoryRecordReqserver, BetHistoryRecordResponse>(url, source, headers);
        }

        public async Task<ReportAgentResponse> ReportAgentAsync(ReportAgentReqserver source)
        {
            var url = Config.GameAPI.OB_DATA_URL + "data/merchant/reportAgent/v1";
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "merchantCode",Config.CompanyToken.OB_MerchantCode},
                { "pageIndex",source.pageIndex.ToString()},
            };
            return await DataHandle<ReportAgentReqserver, ReportAgentResponse>(url, source, headers);
        }
        public async Task<ReportAgentResponse> OnlineUsersAsync(OnlineUsersReqserver source)
        {
            var url = Config.GameAPI.OB_DATA_URL + "data/merchant/onlineUsers/v1";
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "merchantCode",Config.CompanyToken.OB_MerchantCode},
                { "pageIndex",source.pageIndex.ToString()},
            };
            return await DataHandle<OnlineUsersReqserver, ReportAgentResponse>(url, source, headers);
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {

            var headers = new Dictionary<string, string>
            {
            };

            var json = JsonConvert.SerializeObject(source);
            var signature = Helper.MD5encryption(json + Config.CompanyToken.OB_MD5KEY).ToUpper();
            var param = Helper.Encrypt(json, Config.CompanyToken.OB_Aeskey);

            var postData = new Dictionary<string, string>
            {
                { "merchantCode",Config.CompanyToken.OB_MerchantCode},
                { "signature",signature},
                { "params",param}
            };
            _logger.LogInformation("OB parameter RequestPath: {RequestPath}|{Requestdata}", url, json);
            var responseData = await Post(url, postData, headers);

            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }


        private async Task<TResponse> DataHandle<TRequest, TResponse>(string url, TRequest source, Dictionary<string, string> headers)
        {

            var json = JsonConvert.SerializeObject(source);
            var signature = Helper.MD5encryption(json + Config.CompanyToken.OB_MD5KEY).ToUpper();
            var param = Helper.Encrypt(json, Config.CompanyToken.OB_Aeskey);

            var postData = new Dictionary<string, string>
            {
                { "merchantCode",Config.CompanyToken.OB_MerchantCode},
                { "signature",signature},
                { "params",param}
            };

            _logger.LogInformation("OB parameter RequestPath: {RequestPath}|{Requestdata}", url, json);
            var responseData = await Post(url, postData, headers);
            return JsonConvert.DeserializeObject<TResponse>(responseData);

        }


        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null, int retry = 3)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
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

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.OB, url, new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json"));
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
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
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call OBApi Failed:{0}", url));
                }

                return await Post(url, postData, headers, retry - 1);
            }
        }

    }
}
