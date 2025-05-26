using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.RSG
{
    /// <summary>
    /// RSG API
    /// </summary>
    public interface IRSGApiService
    {
        /// <summary>
        /// 建立會員(幣別帳戶) 
        /// </summary>
        Task<CreatePlayerResponse> CreatePlayerAsync(CreatePlayerRequest source);
        /// <summary>
        /// 存入點數 
        /// </summary>
        Task<DepositResponse> DepositAsync(DepositRequest source);
        /// <summary>
        /// 取出點數
        /// </summary>
        Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source);
        /// <summary>
        /// 查詢點數
        /// </summary>
        Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest source);
        /// <summary>
        /// 查詢點數交易結果
        /// </summary>
        Task<GetTransactionResultResponse> GetTransactionResultAsync(GetTransactionResultRequest source);
        /// <summary>
        /// 查詢點數交易歷程
        /// 1. 查詢日期為 2020-04-24，取得的數據範圍為 2020-04-24 12:00:00 至 2020-04-25 11:59:59
        /// 2. 可以查詢的範圍為 180 天內。
        /// </summary>
        Task<GetTransactionHistoryResponse> GetTransactionHistoryAsync(GetTransactionHistoryRequest source);
        /// <summary>
        /// 取得遊戲網址(進入遊戲) 
        /// ExitAction 帶空字串 ( ExitAction=”” ) 時，離開遊戲時將關閉視窗
        /// </summary>
        Task<GetURLTokenResponse> GetURLTokenAsync(GetURLTokenRequest source);
        /// <summary>
        /// 取得遊戲中的會員
        /// </summary>
        Task<PlayerOnlineListResponse> PlayerOnlineListAsync(PlayerOnlineListRequest source);
        /// <summary>
        /// 剔除遊戲中的會員
        /// KickType = 1，會剔除系統下所有人，WebId、UserId 請填空字串，GameId 請填 0
        /// KickType = 2，會剔除站台下所有人，UserId 請填空字串，GameId 請填 0
        /// KickType = 3，會剔除正在該遊戲的所有人，WebId、UserId 請填空字串
        /// KickType = 4，會剔除特定會員，GameId 請填 0
        /// 此 API 會回傳符合剔除條件的會員數量，符合條件者將於數秒內被剔除系統
        /// </summary>
        Task<KickoutResponse> KickoutAsync(KickoutRequest source);
        /// <summary>
        /// 取得點數不為 0 的會員帳戶資訊(已離開遊戲)
        /// </summary>
        Task<GetUnwithdrawnResponse> GetUnwithdrawnAsync(GetUnwithdrawnRequest source);
        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        Task<GameListResponse> GameListAsync(GameListRequest source);
        /// <summary>
        /// 取得遊戲詳細資訊
        /// 1. 可以查詢的範圍為目前時間的３分鐘前，最多可以查詢到目前時間往前 72 小時內，譬如目前是 2020-04-24 16:30，只能查詢 2020-04-24 16:26 ～ 2020-04-21 16:31
        /// 2. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 3. 每次查詢最多 3 分鐘
        /// </summary>
        Task<GetGameDetailResponse> GetGameDetailAsync(GetGameDetailRequest source);
        /// <summary>
        /// 取得遊戲詳細資訊(有分頁)
        /// </summary>
        Task<GetPagedGameDetailResponse> GetPagedGameDetailAsync(GetPagedGameDetailRequest source);
        /// <summary>
        /// 取得遊戲每分鐘統計資訊
        /// 1. 可以查詢的範圍為目前時間的15分鐘前，最多可以查詢到目前時間往前 72 小時內，譬如目前是 2020-04-24 16:30，只能查詢 2020-04-24 16:26 ～ 2020-04-21 16:31
        /// 2. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 3. 每次查詢最多 15 分鐘
        /// </summary>
        Task<GetGameMinReportResponse> GetGameMinReportAsync(GetGameMinReportRequest source);
        /// <summary>
        /// 取得某帳戶某分鐘內的遊戲歷程網址
        /// </summary>
        Task<GetGameMinDetailURLTokenResponse> GetGameMinDetailURLTokenAsync(GetGameMinDetailURLTokenRequest source);
        /// <summary>
        /// 取得遊戲每日統計資訊
        /// 1. 查詢日期為 2020-04-24，取得的數據範圍為 2020 - 04 - 24 12:00:00 至 2020 - 04 - 25 11:59:59
        /// 2. 可以查詢的開始範圍為一天前，最多可以查詢到 60 天內，譬如目前是 2020 - 04 - 24 10:30，可查詢的開始範圍為 2020 - 04 - 22
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// </summary>
        Task<GetGameDailyReportResponse> GetGameDailyReportAsync(GetGameDailyReportRequest source);
        /// <summary>
        /// 查詢會員線上狀態
        /// </summary>
        Task<GetPlayerOnlineStatusResponse> GetPlayerOnlineStatusAsync(GetPlayerOnlineStatusRequest source);
        /// <summary>
        /// 取得遊戲每日統計資訊(全部遊戲類型) 
        /// </summary>
        Task<GetGameDailyReportAllGameTypeResponse> GetGameDailyReportAllGameTypeAsync(GetGameDailyReportAllGameTypeRequest source);
        /// <summary>
        /// 取得 Jackpot 中獎紀錄 
        /// 1. 查詢日期為 2021-11-22 ~ 2021-11-24，取得的數據範圍為 2021 - 11 - 22 12:00:00 至 2021 - 11 - 25 11:59:59
        /// 2. 可以查詢最多 60 天內，譬如目前是 2021-11-24 19:12:28，可查詢範圍為 2021 - 09 - 25 ～ 2021 - 11 - 24
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 4. Jackpot Type: 0: GRAND, 1: MAJOR, 2: MINOR, 3: MINI
        /// </summary>
        Task<GetJackpotHitRecResponse> GetJackpotHitRecAsync(GetJackpotHitRecRequest source);

        /// <summary>
        /// 語系列表
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetLanguageDictionary();

        /// <summary>
        /// 幣別列表
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetCurrencyDictionary();

        /// <summary>
        /// 取得某帳戶 slot 遊戲歷程的遊戲盤面
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetSlotGameRecordURLTokenResponse> GetSlotGameRecordURLTokenAsync(GetSlotGameRecordURLTokenRequest source);

        Task<GetOnlineMemberListResponse> H1GetOnlineMemberList(GetOnlineMemberListRequest source);
        Task<GetMaintainStatusResponse> H1GetMaintainStatus(GetMaintainStatusRequest source);
        Task<SetMaintainStatusResponse> H1SetMaintainStatus(SetMaintainStatusRequest source);
        Task<GetAPILogByIdResponse> H1GetAPILogById(GetAPILogByIdRequest source);
        /// <summary>
        /// 以時間區間來取得 Session 彙總注單
        /// </summary>
        Task<GetAPILogByTimeResponse> H1GetAPILogByTime(GetAPILogByTimeRequest source);
        /// <summary>
        /// ( 老虎機專用 ) 取得特定 Session 內的遊戲歷程
        /// </summary>
        Task<GetPlayerGameHistoryResponse> H1GetPlayerGameHistory(GetPlayerGameHistoryRequest source);
        /// <summary>
        /// ( 老虎機專用 ) 取得特定遊戲紀錄的遊戲盤面網址
        /// </summary>
        Task<GetGameRecordURLResponse> H1GetGameRecordURL(GetGameRecordURLRequest source);
        /// <summary>
        /// ( 捕魚機專用 ) 取得特定 Session 內的每分鐘遊戲歷程彙總
        /// </summary>
        Task<GetPlayerFishGameMinuteSummaryResponse> H1GetPlayerFishGameMinuteSummary(GetPlayerFishGameMinuteSummaryRequest source);
        /// <summary>
        /// ( 捕魚機專用 ) 取得特定分鐘的遊戲歷程網址 
        /// </summary>
        Task<GetFishGameRecordURLResponse> H1GetFishGameRecordURL(GetFishGameRecordURLRequest source);
        /// <summary>
        /// 取得 Session 彙總注單每小時統計資訊 
        /// </summary>
        Task<GetGameHourReportResponse> H1GetGameHourReport(GetGameHourReportRequest source);
        Task<GetFishJackpotHitRecResponse> GetFishJackpotHitRec(GetFishJackpotHitRecRequest source);

        /// <summary>
        /// 取得 Jackpot 目前 Pool 值
        /// </summary>
        Task<GetJackpotPoolValueResponse> GetJackpotPoolValueAsync(GetJackpotPoolValueRequest source);

        /// <summary>
        /// RSG HealthCheck
        /// </summary>
        /// <returns></returns>
        Task HealthCheck();
    }
}