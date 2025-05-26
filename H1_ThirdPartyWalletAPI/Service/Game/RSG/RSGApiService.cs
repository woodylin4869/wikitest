using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace H1_ThirdPartyWalletAPI.Service.Game.RSG
{
    public class RSGApiService : IRSGApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RSGApiService> _logger;

        public RSGApiService(IHttpClientFactory httpClientFactory, ILogger<RSGApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 建立會員(幣別帳戶) 
        /// </summary>
        public async Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/CreatePlayer";
            return await ApiHandle<CreatePlayerRequest, CreatePlayerResponse>(url, source);
        }

        /// <summary>
        /// 存入點數 
        /// </summary>
        public async Task<DepositResponse> DepositAsync(DepositRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/Deposit";
            return await ApiHandle<DepositRequest, DepositResponse>(url, source);
        }

        /// <summary>
        /// 取出點數
        /// </summary>
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/Withdraw";
            return await ApiHandle<WithdrawRequest, WithdrawResponse>(url, source);
        }

        /// <summary>
        /// 查詢點數
        /// </summary>
        public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetBalance";
            return await ApiHandle<GetBalanceRequest, GetBalanceResponse>(url, source);
        }

        /// <summary>
        /// 查詢點數交易結果
        /// </summary>
        public async Task<GetTransactionResultResponse> GetTransactionResultAsync(GetTransactionResultRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetTransactionResult";
            return await ApiHandle<GetTransactionResultRequest, GetTransactionResultResponse>(url, source);
        }

        /// <summary>
        /// 查詢點數交易歷程
        /// 1. 查詢日期為 2020-04-24，取得的數據範圍為 2020-04-24 12:00:00 至 2020-04-25 11:59:59
        /// 2. 可以查詢的範圍為 180 天內。
        /// </summary>
        public async Task<GetTransactionHistoryResponse> GetTransactionHistoryAsync(GetTransactionHistoryRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetTransactionHistory";
            return await ApiHandle<GetTransactionHistoryRequest, GetTransactionHistoryResponse>(url, source);
        }

        /// <summary>
        /// 取得遊戲網址(進入遊戲) 
        /// ExitAction 帶空字串 ( ExitAction=”” ) 時，離開遊戲時將關閉視窗
        /// </summary>
        public async Task<GetURLTokenResponse> GetURLTokenAsync(GetURLTokenRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetURLToken";
            return await ApiHandle<GetURLTokenRequest, GetURLTokenResponse>(url, source);
        }

        /// <summary>
        /// 取得遊戲中的會員
        /// </summary>
        public async Task<PlayerOnlineListResponse> PlayerOnlineListAsync(PlayerOnlineListRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/PlayerOnlineList";
            return await ApiHandle<PlayerOnlineListRequest, PlayerOnlineListResponse>(url, source);
        }
        /// <summary>
        /// 剔除遊戲中的會員
        /// KickType = 1，會剔除系統下所有人，WebId、UserId 請填空字串，GameId 請填 0
        /// KickType = 2，會剔除站台下所有人，UserId 請填空字串，GameId 請填 0
        /// KickType = 3，會剔除正在該遊戲的所有人，WebId、UserId 請填空字串
        /// KickType = 4，會剔除特定會員，GameId 請填 0
        /// 此 API 會回傳符合剔除條件的會員數量，符合條件者將於數秒內被剔除系統
        /// </summary>
        public async Task<KickoutResponse> KickoutAsync(KickoutRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/Kickout";
            return await ApiHandle<KickoutRequest, KickoutResponse>(url, source);
        }

        /// <summary>
        /// 取得點數不為 0 的會員帳戶資訊(已離開遊戲)
        /// </summary>
        public async Task<GetUnwithdrawnResponse> GetUnwithdrawnAsync(GetUnwithdrawnRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetUnwithdrawn";
            return await ApiHandle<GetUnwithdrawnRequest, GetUnwithdrawnResponse>(url, source);
        }

        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        public async Task<GameListResponse> GameListAsync(GameListRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Game/GameList";
            return await ApiHandle<GameListRequest, GameListResponse>(url, source);
        }

        /// <summary>
        /// 取得遊戲詳細資訊
        /// 1. 可以查詢的範圍為目前時間的３分鐘前，最多可以查詢到目前時間往前 72 小時內，譬如目前是 2020-04-24 16:30，只能查詢 2020-04-24 16:26 ～ 2020-04-21 16:31
        /// 2. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 3. 每次查詢最多 3 分鐘
        /// </summary>
        public async Task<GetGameDetailResponse> GetGameDetailAsync(GetGameDetailRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/History/GetGameDetail";
            return await ApiHandle<GetGameDetailRequest, GetGameDetailResponse>(url, source);
        }
        /// <summary>
        /// 取得遊戲詳細資訊 (有分頁)
        /// </summary>
        public async Task<GetPagedGameDetailResponse> GetPagedGameDetailAsync(GetPagedGameDetailRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/History/GetPagedGameDetail";
            return await ApiHandle<GetPagedGameDetailRequest, GetPagedGameDetailResponse>(url, source);
        }
        /// <summary>
        /// 取得遊戲每分鐘統計資訊
        /// 1. 可以查詢的範圍為目前時間的15分鐘前，最多可以查詢到目前時間往前 72 小時內，譬如目前是 2020-04-24 16:30，只能查詢 2020-04-24 16:26 ～ 2020-04-21 16:31
        /// 2. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 3. 每次查詢最多 15 分鐘
        /// </summary>
        public async Task<GetGameMinReportResponse> GetGameMinReportAsync(GetGameMinReportRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Report/GetGameMinReport";
            return await ApiHandle<GetGameMinReportRequest, GetGameMinReportResponse>(url, source);
        }
        /// <summary>
        /// 取得某帳戶某分鐘內的遊戲歷程網址
        /// </summary>
        public async Task<GetGameMinDetailURLTokenResponse> GetGameMinDetailURLTokenAsync(GetGameMinDetailURLTokenRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetGameMinDetailURLToken";
            return await ApiHandle<GetGameMinDetailURLTokenRequest, GetGameMinDetailURLTokenResponse>(url, source);
        }
        /// <summary>
        /// 取得遊戲每日統計資訊
        /// 1. 查詢日期為 2020-04-24，取得的數據範圍為 2020 - 04 - 24 12:00:00 至 2020 - 04 - 25 11:59:59
        /// 2. 可以查詢的開始範圍為一天前，最多可以查詢到 60 天內，譬如目前是 2020 - 04 - 24 10:30，可查詢的開始範圍為 2020 - 04 - 22
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// </summary>
        public async Task<GetGameDailyReportResponse> GetGameDailyReportAsync(GetGameDailyReportRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Report/GetGameDailyReport";
            return await ApiHandle<GetGameDailyReportRequest, GetGameDailyReportResponse>(url, source);
        }

        /// <summary>
        /// 查詢會員線上狀態
        /// </summary>
        public async Task<GetPlayerOnlineStatusResponse> GetPlayerOnlineStatusAsync(GetPlayerOnlineStatusRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetPlayerOnlineStatus";
            return await ApiHandle<GetPlayerOnlineStatusRequest, GetPlayerOnlineStatusResponse>(url, source);
        }

        /// <summary>
        /// 取得遊戲每日統計資訊(全部遊戲類型) 
        /// </summary>
        public async Task<GetGameDailyReportAllGameTypeResponse> GetGameDailyReportAllGameTypeAsync(GetGameDailyReportAllGameTypeRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Report/GetGameDailyReportAllGameType";
            return await ApiHandle<GetGameDailyReportAllGameTypeRequest, GetGameDailyReportAllGameTypeResponse>(url, source);
        }

        /// <summary>
        /// 取得 Jackpot 中獎紀錄 
        /// 1. 查詢日期為 2021-11-22 ~ 2021-11-24，取得的數據範圍為 2021 - 11 - 22 12:00:00 至 2021 - 11 - 25 11:59:59
        /// 2. 可以查詢最多 60 天內，譬如目前是 2021-11-24 19:12:28，可查詢範圍為 2021 - 09 - 25 ～ 2021 - 11 - 24
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 4. Jackpot Type: 0: GRAND, 1: MAJOR, 2: MINOR, 3: MINI
        /// </summary>
        public async Task<GetJackpotHitRecResponse> GetJackpotHitRecAsync(GetJackpotHitRecRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Jackpot/GetJackpotHitRec";
            return await ApiHandle<GetJackpotHitRecRequest, GetJackpotHitRecResponse>(url, source);
        }

        /// <summary>
        /// 取得 捕魚機 Jackpot 中獎紀錄 
        /// 1. 查詢日期為 2021-11-22 ~ 2021-11-24，取得的數據範圍為 2021 - 11 - 22 12:00:00 至 2021 - 11 - 25 11:59:59
        /// 2. 可以查詢最多 60 天內，譬如目前是 2021-11-24 19:12:28，可查詢範圍為 2021 - 09 - 25 ～ 2021 - 11 - 24
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 4. Jackpot Type: 0: GRAND, 1: MAJOR
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetFishJackpotHitRecResponse> GetFishJackpotHitRec(GetFishJackpotHitRecRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Jackpot/GetFishJackpotHitRec";
            return await ApiHandle<GetFishJackpotHitRecRequest, GetFishJackpotHitRecResponse>(url, source);
        }

        /// <summary>
        /// API Handle
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var timestamp = Helper.GetTimestamp();
            var json = JsonConvert.SerializeObject(source);
            var encodeData = Helper.DESEncryption(json, Config.CompanyToken.RSG_Key, Config.CompanyToken.RSG_IV);
            var signature = Helper.CreateSignature(Config.CompanyToken.RSG_Client_ID, Config.CompanyToken.RSG_Secret, timestamp.ToString(), encodeData);

            var headers = new Dictionary<string, string>
            {
                {"Content-Type", "application/x-www-form-urlencoded"},
                {"X-API-ClientID", Config.CompanyToken.RSG_Client_ID},
                {"X-API-Signature", signature},
                {"X-API-Timestamp", timestamp.ToString()}
            };

            var postData = new Dictionary<string, string>
            {
                {"Msg", encodeData}
            };

            var responseJson = await Post(url, postData, json, headers);
            //var responseJson = Helper.DESDecrypt(responseData, Config.CompanyToken.RSG_Key, Config.CompanyToken.RSG_IV);
            return JsonConvert.DeserializeObject<TResponse>(responseJson);
        }

        /// <summary>
        /// 語系列表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetLanguageDictionary()
        {
            return new Dictionary<string, string>
            {
                { "en-US", "英文" },
                { "zh-TW", "繁體中文" },
                { "zh-CN", "簡體中文" },
                { "th-TH", "泰文" },
                { "ko-KR", "韓文" },
                { "ja-JP", "日文" },
                { "my-MM", "緬甸文" },
                { "id-ID", "印尼文" },
                { "vi-VN", "越南文" },
            };
        }

        /// <summary>
        /// 幣別列表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetCurrencyDictionary()
        {
            return new Dictionary<string, string>
            {
                {"NT", "新台幣"},
                {"HK", "港元"},
                {"IDR", "印尼盾"},
                {"JPY", "日圓"},
                {"KRW", "韓圓"},
                {"MYR", "馬幣"},
                {"RMB", "人民幣"},
                {"SGD", "新加坡元"},
                {"THB", "泰銖"},
                {"USA", "美元"},
                {"MMK", "緬甸緬元"},
                {"VND", "越南盾"},
                {"INR", "印度盧比"},
                {"PHP", "披索"},
                {"EUR", "歐元"},
                {"GBP", "英鎊"},
                {"USDT", "泰達幣"},
                {"MYR2", "馬幣"},
            };
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

                    if (url.Contains("GetPlayerGameHistory"))
                    {
                        request.Timeout = TimeSpan.FromSeconds(30);
                    }
                    else
                    {
                        request.Timeout = TimeSpan.FromSeconds(60);
                    }
                    var content = new FormUrlEncodedContent(postData);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.RSG, url, content);
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    body = Helper.DESDecrypt(body, Config.CompanyToken.RSG_Key, Config.CompanyToken.RSG_IV);
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

        /// <summary>
        /// 取得某帳戶 slot 遊戲歷程的遊戲盤面
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetSlotGameRecordURLTokenResponse> GetSlotGameRecordURLTokenAsync(GetSlotGameRecordURLTokenRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Player/GetSlotGameRecordURLToken";
            return await ApiHandle<GetSlotGameRecordURLTokenRequest, GetSlotGameRecordURLTokenResponse>(url, source);
        }

        /// <summary>
        /// H1取得遊戲中的會員
        /// </summary>
        public async Task<GetOnlineMemberListResponse> H1GetOnlineMemberList(GetOnlineMemberListRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetOnlineMemberList";
            return await ApiHandle<GetOnlineMemberListRequest, GetOnlineMemberListResponse>(url, source);
        }
        /// <summary>
        /// H1取得維護狀態
        /// </summary>
        public async Task<GetMaintainStatusResponse> H1GetMaintainStatus(GetMaintainStatusRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetMaintainStatus";
            return await ApiHandle<GetMaintainStatusRequest, GetMaintainStatusResponse>(url, source);
        }
        /// <summary>
        /// H1設定維護狀態
        /// </summary>
        public async Task<SetMaintainStatusResponse> H1SetMaintainStatus(SetMaintainStatusRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/SetMaintainStatus";
            return await ApiHandle<SetMaintainStatusRequest, SetMaintainStatusResponse>(url, source);
        }
        /// <summary>
        /// 以 Id 來取得 Session 彙總注單
        /// </summary>
        public async Task<GetAPILogByIdResponse> H1GetAPILogById(GetAPILogByIdRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetAPILogById";
            return await ApiHandle<GetAPILogByIdRequest, GetAPILogByIdResponse>(url, source);
        }
        /// <summary>
        /// 以時間區間來取得 Session 彙總注單
        /// </summary>
        public async Task<GetAPILogByTimeResponse> H1GetAPILogByTime(GetAPILogByTimeRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetAPILogByTime";
            return await ApiHandle<GetAPILogByTimeRequest, GetAPILogByTimeResponse>(url, source);
        }
        /// <summary>
        ///  ( 老虎機專用 ) 取得特定 Session 內的遊戲歷程
        /// </summary>
        public async Task<GetPlayerGameHistoryResponse> H1GetPlayerGameHistory(GetPlayerGameHistoryRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetPlayerGameHistory";
            return await ApiHandle<GetPlayerGameHistoryRequest, GetPlayerGameHistoryResponse>(url, source);
        }
        /// <summary>
        /// ( 老虎機專用 ) 取得特定遊戲紀錄的遊戲盤面網址
        /// </summary>
        public async Task<GetGameRecordURLResponse> H1GetGameRecordURL(GetGameRecordURLRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetGameRecordURL";
            return await ApiHandle<GetGameRecordURLRequest, GetGameRecordURLResponse>(url, source);
        }
        /// <summary>
        /// ( 捕魚機專用 ) 取得特定 Session 內的每分鐘遊戲歷程彙總
        /// </summary>
        public async Task<GetPlayerFishGameMinuteSummaryResponse> H1GetPlayerFishGameMinuteSummary(GetPlayerFishGameMinuteSummaryRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetPlayerFishGameMinuteSummary";
            return await ApiHandle<GetPlayerFishGameMinuteSummaryRequest, GetPlayerFishGameMinuteSummaryResponse>(url, source);
        }
        /// <summary>
        /// ( 捕魚機專用 ) 取得特定分鐘的遊戲歷程網址 
        /// </summary>
        public async Task<GetFishGameRecordURLResponse> H1GetFishGameRecordURL(GetFishGameRecordURLRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetFishGameRecordURL";
            return await ApiHandle<GetFishGameRecordURLRequest, GetFishGameRecordURLResponse>(url, source);
        }
        /// <summary>
        /// 取得 Session 彙總注單每小時統計資訊
        /// </summary>
        public async Task<GetGameHourReportResponse> H1GetGameHourReport(GetGameHourReportRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/H1Spec/GetGameHourReport";
            return await ApiHandle<GetGameHourReportRequest, GetGameHourReportResponse>(url, source);
        }

        /// <summary>
        /// 取得 Jackpot 目前 Pool 值
        /// </summary>
        public async Task<GetJackpotPoolValueResponse> GetJackpotPoolValueAsync(GetJackpotPoolValueRequest source)
        {
            var url = Config.GameAPI.RSG_URL + "WithBalance/Jackpot/GetJackpotPoolValue";
            return await ApiHandle<GetJackpotPoolValueRequest, GetJackpotPoolValueResponse>(url, source);
        }

        /// <summary>
        /// RSG HealthCheck
        /// </summary>
        /// <returns></returns>
        public async Task HealthCheck()
        {
            var url = Config.GameAPI.RSG_URL + "Home/Index";
            using var request = _httpClientFactory.CreateClient("log");
            await request.GetAsync(Platform.RSG, url).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError("RSG HealthCheck failed: {error}", t.Exception?.Message);
                }
                else
                {
                    _logger.LogInformation("RSG HealthCheck succeeded");
                }
            });
        }
    }
}