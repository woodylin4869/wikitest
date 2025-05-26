using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Service.Game.RTG
{
    public class RTGApiService : IRTGApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RTGApiService> _logger;

        public RTGApiService(IHttpClientFactory httpClientFactory, ILogger<RTGApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        public async Task<GetGameResponse> GetGame(GetGameRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cr/GetGame";
            return await ApiHandle<GetGameRequest, GetGameResponse>(url, source);
        }
        /// <summary>
        /// 建立與更新會員
        /// </summary>
        public async Task<CreateUpdateMemberResponse> CreateUpdateMember(CreateUpdateMemberRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/CreateUpdateMember";
            return await ApiHandle<CreateUpdateMemberRequest, CreateUpdateMemberResponse>(url, source);
        }
        /// <summary>
        /// 建立與更新會員
        /// </summary>
        public async Task<GetUserResponse> GetUser(GetUserRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cr/GetUser";
            return await ApiHandle<GetUserRequest, GetUserResponse>(url, source);
        }
        /// <summary>
        /// 取得遊戲連結
        /// </summary>
        public async Task<GetGameUrlResponse> GetGameUrl(GetGameUrlRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/GetGameUrl";
            return await ApiHandle<GetGameUrlRequest, GetGameUrlResponse>(url, source);
        }
        /// <summary>
        /// 存款
        /// </summary>
        public async Task<DepositResponse> Deposit(DepositRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/Deposit";
            return await ApiHandle<DepositRequest, DepositResponse>(url, source);
        }
        /// <summary>
        /// 提款
        /// </summary>
        public async Task<WithdrawResponse> Withdraw(WithdrawRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/Withdraw";
            return await ApiHandle<WithdrawRequest, WithdrawResponse>(url, source);
        }
        /// <summary>
        /// 查詢單筆交易單
        /// </summary>
        public async Task<SingleTransactionResponse> SingleTransaction(SingleTransactionRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cr/SingleTransaction";
            return await ApiHandle<SingleTransactionRequest, SingleTransactionResponse>(url, source);
        }
        /// <summary>
        /// 踢人
        /// </summary>
        public async Task<KickUserResponse> KickUser(KickUserRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/KickUser";
            return await ApiHandle<KickUserRequest, KickUserResponse>(url, source);
        }
        /// <summary>
        /// 全踢
        /// </summary>
        public async Task<KickAllResponse> KickAll(KickAllRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/KickAll";
            return await ApiHandle<KickAllRequest, KickAllResponse>(url, source);
        }
        /// <summary>
        /// 取得遊戲中的會員
        /// </summary>
        public async Task<GetOnlineUserResponse> GetOnlineUser(GetOnlineUserRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cr/GetOnlineUser";
            return await ApiHandle<GetOnlineUserRequest, GetOnlineUserResponse>(url, source);
        }
        /// <summary>
        /// 取得調閱連結
        /// </summary>
        public async Task<GetVideoLinkResponse> GetVideoLink(GetVideoLinkRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cw/GetVideoLink";
            return await ApiHandle<GetVideoLinkRequest, GetVideoLinkResponse>(url, source);
        }
        /// <summary>
        ///  取得遊戲帳務
        /// </summary>
        public async Task<GameSettlementRecordResponse> GameSettlementRecord(GameSettlementRecordRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cr/GameSettlementRecord";
            return await ApiHandle<GameSettlementRecordRequest, GameSettlementRecordResponse>(url, source);
        }
        /// <summary>
        ///   取得遊戲每日統計資訊
        /// </summary>
        public async Task<GetGameDailyRecordResponse> GetGameDailyRecord(GetGameDailyRecordRequest source)
        {
            var url = Config.GameAPI.RTG_URL + "cr/GetGameDailyRecord";
            return await ApiHandle<GetGameDailyRecordRequest, GetGameDailyRecordResponse>(url, source);
        }
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var json = System.Text.Json.JsonSerializer.Serialize(source);
            var encodeData = Helper.DESEncryption(json, Config.CompanyToken.RTG_Key, Config.CompanyToken.RTG_IV);
            var signature = Helper.CreateSignature(Config.CompanyToken.RTG_Client_ID, Config.CompanyToken.RTG_Secret, timestamp.ToString(), encodeData);

            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
                {"X-API-ClientID", Config.CompanyToken.RTG_Client_ID},
                {"X-API-Signature", signature},
                {"X-API-Timestamp", timestamp.ToString()}
            };
            //var postData = "request=" + encodeData;
            var postData = new Dictionary<string, string>
            {
                {"request", encodeData}
            };

            var responseJson = await Post(url, postData, json, headers);
            //var responseJson = Helper.DESDecrypt(responseData, Config.CompanyToken.RSG_Key, Config.CompanyToken.RSG_IV);
            return JsonConvert.DeserializeObject<TResponse>(responseJson);
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
                    response = await request.PostAsync(Platform.RTG, url, content);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
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
                    }
                    catch
                    {

                    }
                    return body;
                }

            }
            catch (HttpRequestException)
            {
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call RsgApi Failed:{0}", url));
                }
                return await Post(url, postData, reqJson, headers, retry - 1);
            }
        }
    }
}