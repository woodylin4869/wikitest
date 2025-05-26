using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.SEXY
{
    public class SEXYApiService : ISEXYApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SEXYApiService> _logger;
        public SEXYApiService(IHttpClientFactory httpClientFactory, ILogger<SEXYApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            IsoDateTimeConverter converter = new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz"
            };
            var json = JsonConvert.SerializeObject(source, converter);
            try
            {

                var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
            };
                var postData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                //Dictionary<string, string> keyValuePairs = new Dictionary<string, string>()
                //{
                //    {"cert", "RrqUwBase6I9QKH8bho"},
                //    {"agentId", "h1sexytest"},
                //    {"timeFrom", "2023-02-22T12:00:00+08:00"},
                //    {"platform", "SEXYBCRT"},
                //    {"currency", "THB"},
                //    {"delayTime", "0"},
                //};



                var responseJson = await Post(url, postData, json, headers);
                //var responseJson = Helper.DESDecrypt(responseData, Config.CompanyToken.SEXY_Key, Config.CompanyToken.SEXY_IV);
                return JsonConvert.DeserializeObject<TResponse>(responseJson);
            }
            catch (Exception ex)
            {
                _logger.LogDebug("ApiHandle: {RequestPath} | json:{json}", url, json);
                throw;
            }

        }
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> Post(string url, Dictionary<string, string> postData, string reqJson, Dictionary<string, string> headers = null, int retry = 0)
        {
            HttpResponseMessage response = null;
            var apiResInfo = new ApiResponseData();
            try
            {
                //HttpClientHandler handler = new HttpClientHandler()
                //{
                //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                //};
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
                    var content = new FormUrlEncodedContent(postData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.SEXY, url, content);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
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
                    try
                    {
                        var responselog = "";
                        if (body.Length > 10000)
                        {
                            responselog = body.Substring(0, 9999);
                        }
                        else
                        {
                            responselog = body;
                        }
                        var dics = new Dictionary<string, object>
                        {
                            { "request", reqJson },
                            { "response", responselog }
                        };
                        using (var scope = _logger.BeginScope(dics))
                        {
                            _logger.LogInformation("Get RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                        }


                        try
                        {
                            var _SEXYBaseStatusRespones = JsonConvert.DeserializeObject<SEXYBaseStatusRespones>(body);
                            if (_SEXYBaseStatusRespones.status == (int)ErrorCodeEnum.Unable_to_proceed_please_try_again_later)
                            {
                                //api建議20~30秒爬一次
                                await Task.Delay(20000);
                                return await Post(url, postData, reqJson, headers, 0);
                            }
                        }
                        catch
                        {
                            _logger.LogError("Post SEXYBaseStatusResponesError: {url} | body:{body} | reqJson:{reqJson}", url, body, reqJson);
                        }
                    }
                    catch
                    {

                    }
                    return body;
                }

            }
            catch (HttpRequestException ex)
            {
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call SEXYApi Failed:{0} | Msg:{1}| reqJson:{1}", url, ex.Message, reqJson));
                }
                return await Post(url, postData, reqJson, headers, retry - 1);
            }
        }

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateMemberResponse> CreateMember(CreateMemberRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/createMember";
            var resultResponse = await ApiHandle<CreateMemberRequest, CreateMemberResponse>(url, source);
            return resultResponse;
        }

        public async Task<GetBalanceResponse> GetBalance(GetBalanceRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/getBalance";
            var resultResponse = await ApiHandle<GetBalanceRequest, GetBalanceResponse>(url, source);
            return resultResponse;
        }

        public async Task<GameLoginResponse> GameLogin(GameLoginRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/login";
            var resultResponse = await ApiHandle<GameLoginRequest, GameLoginResponse>(url, source);
            return resultResponse;
        }

        public async Task<DoLoginAndLaunchGameResponse> DoLoginAndLaunchGame(DoLoginAndLaunchGameRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/doLoginAndLaunchGame";
            var resultResponse = await ApiHandle<DoLoginAndLaunchGameRequest, DoLoginAndLaunchGameResponse>(url, source);
            return resultResponse;
        }

        public async Task<UpdateBetLimitResponse> UpdateBetLimit(UpdateBetLimitRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/updateBetLimit";
            var resultResponse = await ApiHandle<UpdateBetLimitRequest, UpdateBetLimitResponse>(url, source);
            return resultResponse;
        }

        public async Task<GameLogoutResponse> GameLogout(GameLogoutRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/logout";
            var resultResponse = await ApiHandle<GameLogoutRequest, GameLogoutResponse>(url, source);
            return resultResponse;
        }

        public async Task<DepositResponse> Deposit(DepositRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/deposit";
            var resultResponse = await ApiHandle<DepositRequest, DepositResponse>(url, source);
            return resultResponse;
        }

        public async Task<WithdrawResponse> Withdraw(WithdrawRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/withdraw";
            var resultResponse = await ApiHandle<WithdrawRequest, WithdrawResponse>(url, source);
            return resultResponse;
        }

        public async Task<CheckTransferOperationResponse> CheckTransferOperation(CheckTransferOperationRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/checkTransferOperation";
            var resultResponse = await ApiHandle<CheckTransferOperationRequest, CheckTransferOperationResponse>(url, source);
            return resultResponse;
        }

        public async Task<GetTransactionByUpdateDateResponse> GetTransactionByUpdateDate(GetTransactionByUpdateDateRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "fetch/gzip/getTransactionByUpdateDate";
            var resultResponse = await ApiHandle<GetTransactionByUpdateDateRequest, GetTransactionByUpdateDateResponse>(url, source);
            return resultResponse;
        }

        /// <summary>
        /// 每次最大拉取区间仅可设置为 1 小时
        /// 仅可拉 7 天内的帐
        /// 每次最多可拉 20,000 笔资料
        /// 必须带入 platform 参数。API 最快支持每 20 秒进行一次拉帐
        /// 捞取资料依照注单交易时间排序
        /// 我方回应格式使用 Content-Encoding: gzip
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetTransactionByTxTimeResponse> GetTransactionByTxTime(GetTransactionByTxTimeRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "fetch/gzip/getTransactionByTxTime";
            var resultResponse = await ApiHandle<GetTransactionByTxTimeRequest, GetTransactionByTxTimeResponse>(url, source);
            return resultResponse;
        }

        public async Task<GetSummaryByTxTimeHourResponse> GetSummaryByTxTimeHour(GetSummaryByTxTimeHourRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "fetch/getSummaryByTxTimeHour";
            GetSummaryByTxTimeHourResponse resultResponse = await ApiHandle<GetSummaryByTxTimeHourRequest, GetSummaryByTxTimeHourResponse>(url, source);
            if (resultResponse.transactions == null)
            {
                resultResponse.transactions = new List<GetSummaryByTxTimeHourResponse.Transaction>();
            }

            return resultResponse;
        }

        public async Task<GetTransactionHistoryResultResponse> GetTransactionHistoryResult(GetTransactionHistoryResultRequest source)
        {
            var url = Config.GameAPI.SEXY_URL + "wallet/getTransactionHistoryResult";
            var resultResponse = await ApiHandle<GetTransactionHistoryResultRequest, GetTransactionHistoryResultResponse>(url, source);
            return resultResponse;
        }
    }
}
