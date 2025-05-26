using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.RSG;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.RLG
{
    public class RLGApiService : IRLGApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RLGApiService> _logger;

        public RLGApiService(IHttpClientFactory httpClientFactory, ILogger<RLGApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        /// <summary>
        /// 取得 URL Token
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetURLTokenResponse> GetURLTokenAsync(GetURLTokenRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/GetURLToken";
            return await ApiHandle<GetURLTokenRequest, GetURLTokenResponse>(url, source);
        }
        /// <summary>
        /// 建立與更新會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreateOrSetUserResponse> CreateOrSetUserAsync(CreateOrSetUserRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/CreateOrSetUser";
            return await ApiHandle<CreateOrSetUserRequest, CreateOrSetUserResponse>(url, source);


        }

        /// <summary>
        /// 注單資訊
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BetInfoResponse> BetInfoJsonnAsync(BetInfoRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Record/BetInfo";
            return await ApiHandle<BetInfoRequest, BetInfoResponse>(url, source);
        }


        public async Task<BetInfourlResponse> BetInfourlAsync(BetInfoRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Record/BetInfo";
            return await ApiHandle<BetInfoRequest, BetInfourlResponse>(url, source);
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<DepositResponse> DepositAsync(DepositRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/Deposit";
            return await ApiHandle<DepositRequest, DepositResponse>(url, source);
        }
        /// <summary>
        /// 查詢玩家在線列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<PlayerOnlineListResponse> PlayerOnlineListAsync(PlayerOnlineListRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/PlayerOnlineList";
            return await ApiHandle<PlayerOnlineListRequest, PlayerOnlineListResponse>(url, source);
        }
        /// <summary>
        /// 剔除玩家
        /// </summary>
        public async Task<KickoutResponse> KickoutAsync(KickoutRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/Kickout";
            return await ApiHandle<KickoutRequest, KickoutResponse>(url, source);
        }
        /// <summary>
        /// 會員點數交易分頁列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<TransferRecordResponse> TransferRecordAsync(TransferRecordRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Record/TransferRecord";
            return await ApiHandle<object, TransferRecordResponse>(url, new
            {
                source.SystemCode,
                source.WebId,
                source.UserId,
                source.TransferNo,
                StartTime = source.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = source.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                source.PageIndex,
                source.PageSize,
                source.Language

            });
        }
        /// <summary>
        /// 會員投注紀錄分頁列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetBetRecordResponse> GetBetRecordAsync(GetBetRecordRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Record/GetBetRecord";
            return await ApiHandle<object, GetBetRecordResponse>(url, new
            {
                source.SystemCode,
                source.WebId,
                source.UserId,
                source.GameId,
                StartTime = source.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = source.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                source.PageIndex,
                source.PageSize,
                source.SetOption,
                source.Language
            });
        }
        /// <summary>
        /// 批次查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BatchBalanceResponse> BatchBalanceAsync(BatchBalancepostdata source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/BatchBalance";
            return await ApiHandle<object, BatchBalanceResponse>(url, source);
        }
        /// <summary>
        /// 批次會員提款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<BatchWithdrawalResponse> BatchWithdrawalAsync(BatchWithdrawalpostdata source)
        {
            var url = Config.GameAPI.RLG_URL + "Player/BatchWithdrawal";
            return await ApiHandle<object, BatchWithdrawalResponse>(url, source);
        }
        /// <summary>
        /// 會員投注總計列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetBetTotalListResponse> GetBetTotalListAsync(GetBetTotalListserverRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Record/GetBetTotalList";
            return await ApiHandle<GetBetTotalListserverRequest, GetBetTotalListResponse>(url, source);
        }

        /// <summary>
        /// 目前開放遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetOpenGameResponse> GetOpenGameAsync(GetOpenGameRequest source)
        {
            var url = Config.GameAPI.RLG_URL + "Record/GetOpenGame";
            return await ApiHandle<GetOpenGameRequest, GetOpenGameResponse>(url, source);
        }

        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var timestamp = Helper.GetTimestamp();
            var json = JsonConvert.SerializeObject(source);
            var encodeData = Helper.DESEncryption(json, Config.CompanyToken.RLG_Key, Config.CompanyToken.RLG_IV);
            var signature = Helper.CreateSignature(Config.CompanyToken.RLG_Client_ID, Config.CompanyToken.RLG_Secret, timestamp.ToString(), encodeData).ToLower();

            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
                {"X-API-ClientID", Config.CompanyToken.RLG_Client_ID},
                {"X-API-Signature", signature},
                {"X-API-Timestamp", timestamp.ToString()}
            };

            var postData = new Dictionary<string, string>
            {
                {"Msg", encodeData}
            };

            var responseData = await Post(url, postData, headers);
            return JsonConvert.DeserializeObject<TResponse>(responseData);
        }

        private async Task<string> Post(string url, Dictionary<string, string> postData, Dictionary<string, string> headers = null)
        {
            using var request = _httpClientFactory.CreateClient("log");
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
            var response = await request.PostAsync(Platform.RLG, url, content);
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
}
