using ThirdPartyWallet.Share.Model.Game.STANDARDS.Request;
using ThirdPartyWallet.Share.Model.Game.STANDARDS.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.STANDARDS
{
    public interface ISTANDARDSApiService
    {
        Task<ResponseBase<CreateResponse>> Player(CreateRequest request);
        /// <summary>
        /// 玩家登出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<LogoutResponse>> Logout(LogoutRequest request);

        /// <summary>
        /// Player 取得遊戲連結
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<GetlinkResponse>> GameLink(GetlinkRequest request);

        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<WithdrawResponse>> Withdraw(WithdrawRequest request);

        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<DepositResponse>> Deposit(DepositRequest request);

        /// <summary>
        /// 玩家遊戲錢包查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<WalletResponse>> Wallet(WalletRequest request);

        /// <summary>
        /// 單筆交易紀錄查詢
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<TransferResponse>> Transaction(TransferRequest request);
        /// <summary>
        /// 查詢注單詳細資訊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<BetlogResponse>> Betlog(BetlogRequest request);
        /// <summary>
        ///查詢注單詳細資訊
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<TotalBetlogResponse>> Betlog_total(TotalBetlogRequest request);
        /// <summary>
        /// 第三層明細
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<PlaycheckResponse>> Playcheck(PlaycheckRequest request);
        /// <summary>
        /// 查詢玩家是否在線
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<OnlineResponse>> Online(OnlineRequest request);
        /// <summary>
        /// 取得每小時總結
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<HourbetlogResponse>> Betlog_hour(HourbetlogRequest request);
        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<HealthcheckResponse>> Healthcheck(HealthcheckRequest request);
        /// <summary>
        /// 測試用 填假住單資料
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<ResponseBase<BetlogResponse>> GetPagedGameDetailAsync(BetlogRequest source);
    }
}
