using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Request;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.GR.JsonConverter;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Service.Game.GR
{
    public class GRApiService : IGRApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GRApiService> _logger;

        public GRApiService(IHttpClientFactory httpClientFactory, ILogger<GRApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// 0001 – 平台確認使用者是否在線上 check_user_online
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckUserOnlineResponse> CheckUserOnline(CheckUserOnlineRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/check_user_online";
            return await ApiHandle<CheckUserOnlineRequest, CheckUserOnlineResponse>(url, source);
        }

        /// <summary>
        /// 0002-v3 - 平台使用者轉入點數 credit_balance_v3
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CreditBalanceV3Response> CreditBalanceV3(CreditBalanceV3Request source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/credit_balance_v3";
            return await ApiHandle<CreditBalanceV3Request, CreditBalanceV3Response>(url, source);
        }

        /// <summary>
        /// 0003-v3 - 平台使用者轉出點數 debit_balance_v3
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<DebitBalanceV3Response> DebitBalanceV3(DebitBalanceV3Request source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/debit_balance_v3";
            return await ApiHandle<DebitBalanceV3Request, DebitBalanceV3Response>(url, source);
        }

        /// <summary>
        /// 0018 – 平台檢查是否已有單號存在 check_order_exist_v3
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckOrderExistV3Response> CheckOrderExistV3(CheckOrderExistV3Request source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/check_order_exist_v3";
            return await ApiHandle<CheckOrderExistV3Request, CheckOrderExistV3Response>(url, source);
        }

        /// <summary>
        /// 0004 – 平台註冊使用者 reg_user_info
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<RegUserInfoResponse> RegUserInfo(RegUserInfoRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/reg_user_info";
            return await ApiHandle<RegUserInfoRequest, RegUserInfoResponse>(url, source);
        }

        /// <summary>
        /// 0005 - 平台踢出使用者 kick_user_by_account
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<KickUserByAccountResponse> KickUserByAccount(KickUserByAccountRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/kick_user_by_account";
            return await ApiHandle<KickUserByAccountRequest, KickUserByAccountResponse>(url, source);
        }

        /// <summary>
        /// 0006-2 - 平台取得 Slot 使用者下注歷史資料 get_slot_all_bet_details
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CommBetDetailsResponse> GetSlotAllBetDetails(CommBetDetailsRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_slot_all_bet_details";
            return await ApiHandle<CommBetDetailsRequest, CommBetDetailsResponse>(url, source);
        }

        /// <summary>
        /// 0007-2 - 平台取得 Slot 下注遊戲後詳細資訊的結果 get_slot_game_round_details 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetSlotGameRoundDetailsResponse> GetSlotGameRoundDetails(GetSlotGameRoundDetailsRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_slot_game_round_details";
            return await ApiHandle<GetSlotGameRoundDetailsRequest, GetSlotGameRoundDetailsResponse>(url, source);
        }

        /// <summary>
        /// 0006-3 - 平台取得魚機使用者下注歷史資料 get_fish_all_bet_details
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CommBetDetailsResponse> GetFishAllBetDetails(CommBetDetailsRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_fish_all_bet_details";
            return await ApiHandle<CommBetDetailsRequest, CommBetDetailsResponse>(url, source);
        }

        /// <summary>
        /// 0007-3 - 平台取得魚機遊戲結算後詳細資訊 get_fish_game_round_details
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetFishGameRoundDetailsResponse> GetFishGameRoundDetails(GetFishGameRoundDetailsRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_fish_game_round_details";
            return await ApiHandle<GetFishGameRoundDetailsRequest, GetFishGameRoundDetailsResponse>(url, source);
        }

        /// <summary>
        /// 0008 – 平台取得交易詳細記錄 get_transaction_details
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetTransactionDetailsResponse> GetTransactionDetails(GetTransactionDetailsRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_transaction_details";
            return await ApiHandle<GetTransactionDetailsRequest, GetTransactionDetailsResponse>(url, source);
        }

        /// <summary>
        /// 0009 –平台取得所有在線遊戲有效投注總額 get_user_bet_amount
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetUserBetAmountResponse> GetUserBetAmount(GetUserBetAmountRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_user_bet_amount";
            return await ApiHandle<GetUserBetAmountRequest, GetUserBetAmountResponse>(url, source);
        }

        /// <summary>
        /// 0010 – 平台取得使用者輸贏金額 get_user_win_or_lost
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetUserWinOrLostResponse> GetUserWinOrLost(GetUserWinOrLostRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_user_win_or_lost";
            return await ApiHandle<GetUserWinOrLostRequest, GetUserWinOrLostResponse>(url, source);
        }

        /// <summary>
        /// 0013 – 平台取得使用者登入 (sid) get_sid_by_account
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetSidByAccountResponse> GetSidByAccount(GetSidByAccountRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_sid_by_account";
            return await ApiHandle<GetSidByAccountRequest, GetSidByAccountResponse>(url, source);
        }

        /// <summary>
        /// 0014 – 平台使用者取得餘額 get_balance
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetBalanceResponse> GetBalance(GetBalanceRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_balance";
            return await ApiHandle<GetBalanceRequest, GetBalanceResponse>(url, source);
        }

        /// <summary>
        /// 0017 – 平台確認使用者是否存在 check_user_exist
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<CheckUserExistResponse> CheckUserExist(CheckUserExistRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/check_user_exist";
            return await ApiHandle<CheckUserExistRequest, CheckUserExistResponse>(url, source);
        }

        /// <summary>
        /// 0020 – 平台取得代理額度 get_agent_detail
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetAgentDetailResponse> GetAgentDetail(GetAgentDetailRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_agent_detail";
            return await ApiHandle<GetAgentDetailRequest, GetAgentDetailResponse>(url, source);
        }

        /// <summary>
        /// 0021 – 平台取得代理遊戲列表 get_agent_game_list
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetAgentGameListResponse> GetAgentGameList(GetAgentGameListRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_agent_game_list";
            return await ApiHandle<GetAgentGameListRequest, GetAgentGameListResponse>(url, source);
        }

        /// <summary>
        /// 0023 - 平台取每日報表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetreportResponse> GetReportList(GetReportRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_game_daily_report";
            return await ApiHandle<GetReportRequest, GetreportResponse>(url, source);
        }

        /// <summary>
        /// 0024 - 平台透過注單的 SID 與 Account 取得遊戲詳細 Url
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<GetUrlResponse> GetURLList(GetUrlRequest source)
        {
            var url = Config.GameAPI.GR_URL + "api/platform/get_game_detail_url";
            return await ApiHandle<GetUrlRequest, GetUrlResponse>(url, source);
        }


        #region ApiHandle
        private async Task<TResponse> ApiHandle<TRequest, TResponse>(string url, TRequest source)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(source, new System.Text.Json.JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = {
                new DeserializeDateTimeConverter()
               }
            });

            var headers = new Dictionary<string, string>
            {
               {"ContentType", "application/json"},
               {"Cookie", "secret_key=" + Config.CompanyToken.GR_Secret_Key + ";"}
            };

            var responseJson = await Post(url, json, headers);
            return JsonConvert.DeserializeObject<TResponse>(responseJson);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="reqJson"></param>
        /// <param name="headers"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TaskCanceledException"></exception>
        private async Task<string> Post(string url, string reqJson, Dictionary<string, string> headers = null, int retry = 0)
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
                    //拉單相關API Timeout為60秒
                    if (url.EndsWith("api/platform/get_fish_all_bet_details") || url.EndsWith("api/platform/get_slot_all_bet_details"))
                        request.Timeout = TimeSpan.FromSeconds(60);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    response = await request.PostAsync(Platform.GR, url, new StringContent(reqJson, Encoding.UTF8, "application/json"));
                    sw.Stop();
                    apiResInfo.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                    //if (!response.IsSuccessStatusCode)
                    //    throw new Exception(string.Format("Call GR Api Failed! url:{0} status:{1}", url, response.StatusCode.ToString()));

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
            catch (HttpRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Gr Post exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                if (retry == 0)
                {
                    throw new Exception(string.Format("Call GrApi Failed:{0},reqJson:{1}", url, reqJson));
                }
                return await Post(url, reqJson, headers, retry - 1);
            }
        }
        #endregion

    }
}