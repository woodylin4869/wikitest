using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.PG.Service;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Game.RSG;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.PG
{
    /// <summary>
    /// RSG API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class RSGController : ControllerBase
    {
        private readonly IRSGApiService _rsgApiService;
        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;

        public RSGController(IRSGApiService rsgApiService, IDBService dbIdbService, ITransferWalletService transferWalletService)
        {
            _rsgApiService = rsgApiService;
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
        }

        /// <summary>
        /// 建立會員(幣別帳戶) 
        /// </summary>
        [HttpPost]
        [Route("CreatePlayer")]
        public async Task<CreatePlayerResponse> CreatePlayerAsync([FromBody] CreatePlayerRequest source)
        {
            return await _rsgApiService.CreatePlayerAsync(source); ;
        }

        /// <summary>
        /// 存入點數 
        /// </summary>
        [HttpPost]
        [Route("Deposit")]
        public async Task<DepositResponse> DepositAsync([FromBody] DepositRequest source)
        {
            return await _rsgApiService.DepositAsync(source);
        }

        /// <summary>
        /// 取出點數
        /// </summary>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync([FromBody] WithdrawRequest source)
        {
            return await _rsgApiService.WithdrawAsync(source);
        }

        /// <summary>
        /// 查詢點數
        /// </summary>
        [HttpPost]
        [Route("GetBalance")]
        public async Task<GetBalanceResponse> GetBalanceAsync([FromBody] GetBalanceRequest source)
        {
            return await _rsgApiService.GetBalanceAsync(source);
        }

        /// <summary>
        /// 查詢點數交易結果
        /// </summary>
        [HttpPost]
        [Route("GetTransactionResult")]
        public async Task<GetTransactionResultResponse> GetTransactionResultAsync([FromBody] GetTransactionResultRequest source)
        {
            return await _rsgApiService.GetTransactionResultAsync(source);
        }

        /// <summary>
        /// 查詢點數交易歷程
        /// 1. 查詢日期為 2020-04-24，取得的數據範圍為 2020-04-24 12:00:00 至 2020-04-25 11:59:59
        /// 2. 可以查詢的範圍為 180 天內。
        /// </summary>
        [HttpPost]
        [Route("GetTransactionHistory")]
        public async Task<GetTransactionHistoryResponse> GetTransactionHistoryAsync([FromBody] GetTransactionHistoryRequest source)
        {
            return await _rsgApiService.GetTransactionHistoryAsync(source);
        }

        /// <summary>
        /// 取得遊戲網址(進入遊戲) 
        /// ExitAction 帶空字串 ( ExitAction=”” ) 時，離開遊戲時將關閉視窗
        /// </summary>
        [HttpPost]
        [Route("GetURLToken")]
        public async Task<GetURLTokenResponse> GetURLTokenAsync([FromBody] GetURLTokenRequest source)
        {
            return await _rsgApiService.GetURLTokenAsync(source);
        }

        /// <summary>
        /// 取得遊戲中的會員
        /// </summary>
        [HttpPost]
        [Route("PlayerOnlineList")]
        public async Task<PlayerOnlineListResponse> PlayerOnlineListAsync([FromBody] PlayerOnlineListRequest source)
        {
            return await _rsgApiService.PlayerOnlineListAsync(source);
        }

        /// <summary>
        /// 剔除遊戲中的會員
        /// KickType = 1，會剔除系統下所有人，WebId、UserId 請填空字串，GameId 請填 0
        /// KickType = 2，會剔除站台下所有人，UserId 請填空字串，GameId 請填 0
        /// KickType = 3，會剔除正在該遊戲的所有人，WebId、UserId 請填空字串
        /// KickType = 4，會剔除特定會員，GameId 請填 0
        /// 此 API 會回傳符合剔除條件的會員數量，符合條件者將於數秒內被剔除系統
        /// </summary>
        [HttpPost]
        [Route("Kickout")]
        public async Task<KickoutResponse> KickoutAsync([FromBody] KickoutRequest source)
        {
            return await _rsgApiService.KickoutAsync(source);
        }

        /// <summary>
        /// 取得點數不為 0 的會員帳戶資訊(已離開遊戲)
        /// </summary>
        [HttpPost]
        [Route("GetUnwithdrawn")]
        public async Task<GetUnwithdrawnResponse> GetUnwithdrawnAsync([FromBody] GetUnwithdrawnRequest source)
        {
            return await _rsgApiService.GetUnwithdrawnAsync(source);
        }

        /// <summary>
        /// 取得遊戲列表
        /// </summary>
        [HttpPost]
        [Route("GameList")]
        public async Task<GameListResponse> GameListAsync([FromBody] GameListRequest source)
        {
            return await _rsgApiService.GameListAsync(source);
        }

        /// <summary>
        /// 取得遊戲詳細資訊
        /// 1. 可以查詢的範圍為目前時間的３分鐘前，最多可以查詢到目前時間往前 72 小時內，譬如目前是 2020-04-24 16:30，只能查詢 2020-04-24 16:26 ～ 2020-04-21 16:31
        /// 2. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 3. 每次查詢最多 3 分鐘
        /// </summary>
        [HttpPost]
        [Route("GetGameDetail")]
        public async Task<GetGameDetailResponse> GetGameDetailAsync([FromBody] GetGameDetailRequest source)
        {
            return await _rsgApiService.GetGameDetailAsync(source);
        }

        /// <summary>
        /// 取得遊戲每分鐘統計資訊
        /// 1. 可以查詢的範圍為目前時間的15分鐘前，最多可以查詢到目前時間往前 72 小時內，譬如目前是 2020-04-24 16:30，只能查詢 2020-04-24 16:26 ～ 2020-04-21 16:31
        /// 2. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 3. 每次查詢最多 15 分鐘
        /// </summary>
        [HttpPost]
        [Route("GetGameMinReport")]
        public async Task<GetGameMinReportResponse> GetGameMinReportAsync([FromBody] GetGameMinReportRequest source)
        {
            return await _rsgApiService.GetGameMinReportAsync(source);
        }

        /// <summary>
        /// 取得遊戲每日統計資訊
        /// 1. 查詢日期為 2020-04-24，取得的數據範圍為 2020 - 04 - 24 12:00:00 至 2020 - 04 - 25 11:59:59
        /// 2. 可以查詢的開始範圍為一天前，最多可以查詢到 60 天內，譬如目前是 2020 - 04 - 24 10:30，可查詢的開始範圍為 2020 - 04 - 22
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// </summary>
        [HttpPost]
        [Route("GetGameDailyReport")]
        public async Task<GetGameDailyReportResponse> GetGameDailyReportAsync([FromBody] GetGameDailyReportRequest source)
        {
            return await _rsgApiService.GetGameDailyReportAsync(source);
        }

        /// <summary>
        /// 查詢會員線上狀態
        /// </summary>
        [HttpPost]
        [Route("GetPlayerOnlineStatus")]
        public async Task<GetPlayerOnlineStatusResponse> GetPlayerOnlineStatusAsync([FromBody] GetPlayerOnlineStatusRequest source)
        {
            return await _rsgApiService.GetPlayerOnlineStatusAsync(source);
        }

        /// <summary>
        /// 取得遊戲每日統計資訊(全部遊戲類型) 
        /// </summary>
        [HttpPost]
        [Route("GetGameDailyReportAllGameType")]
        public async Task<GetGameDailyReportAllGameTypeResponse> GetGameDailyReportAllGameTypeAsync([FromBody] GetGameDailyReportAllGameTypeRequest source)
        {
            return await _rsgApiService.GetGameDailyReportAllGameTypeAsync(source);
        }

        /// <summary>
        /// 取得 Jackpot 中獎紀錄 
        /// 1. 查詢日期為 2021-11-22 ~ 2021-11-24，取得的數據範圍為 2021 - 11 - 22 12:00:00 至 2021 - 11 - 25 11:59:59
        /// 2. 可以查詢最多 60 天內，譬如目前是 2021-11-24 19:12:28，可查詢範圍為 2021 - 09 - 25 ～ 2021 - 11 - 24
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 4. Jackpot Type: 0: GRAND, 1: MAJOR, 2: MINOR, 3: MINI
        /// </summary>
        [HttpPost]
        [Route("GetJackpotHitRec")]
        public async Task<GetJackpotHitRecResponse> GetJackpotHitRecAsync([FromBody] GetJackpotHitRecRequest source)
        {
            return await _rsgApiService.GetJackpotHitRecAsync(source);
        }

        /// <summary>
        /// 取得 捕魚機 Jackpot 中獎紀錄 
        /// 1. 查詢日期為 2021-11-22 ~ 2021-11-24，取得的數據範圍為 2021 - 11 - 22 12:00:00 至 2021 - 11 - 25 11:59:59
        /// 2. 可以查詢最多 60 天內，譬如目前是 2021-11-24 19:12:28，可查詢範圍為 2021 - 09 - 25 ～ 2021 - 11 - 24
        /// 3. WebId 有填值將只回傳該 WebId 底下的資料，WebId 為空字串時，將回傳該系統所有資料
        /// 4. Jackpot Type: 0: GRAND, 1: MAJOR
        /// </summary>
        [HttpPost]
        [Route("GetFishJackpotHitRec")]
        public async Task<GetFishJackpotHitRecResponse> GetFishJackpotHitRec([FromBody] GetFishJackpotHitRecRequest source)
        {
            return await _rsgApiService.GetFishJackpotHitRec(source);
        }

        /// <summary>
        /// </summary>
        [HttpPost]
        [Route("GetOnlineMemberList")]
        public async Task<GetOnlineMemberListResponse> GetOnlineMemberListAsync([FromBody] GetOnlineMemberListRequest source)
        {
            return await _rsgApiService.H1GetOnlineMemberList(source);
        }

        /// <summary>
        /// </summary>
        [HttpPost]
        [Route("GetMaintainStatus")]
        public async Task<GetMaintainStatusResponse> GetMaintainStatusAsync([FromBody] GetMaintainStatusRequest source)
        {
            return await _rsgApiService.H1GetMaintainStatus(source);
        }

        /// <summary>
        /// 取得 Jackpot 目前 Pool 值
        /// </summary>
        [HttpPost]
        [Route("GetJackpotPoolValue")]
        public async Task<GetJackpotPoolValueResponse> GetJackpotPoolValueAsync([FromBody] GetJackpotPoolValueRequest source)
        {
            return await _rsgApiService.GetJackpotPoolValueAsync(source);
        }


        [HttpPost]
        [Route("SetMaintainStatus")]
        public async Task<SetMaintainStatusResponse> SetMaintainStatusAsync([FromBody] SetMaintainStatusRequest source)
        {
            return await _rsgApiService.H1SetMaintainStatus(source);
        }

        /// <summary>
        /// RSG HealthCheck
        /// </summary>
        /// <returns></returns>
        public async Task HealthCheck()
        {
            await _rsgApiService.HealthCheck();
        }
    }
}
