using ThirdPartyWallet.Share.Model.Game.SPLUS.Request;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Response;

namespace ThirdPartyWallet.GameAPI.Service.Game.SPLUS
{
    public interface ISPLUSApiService
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
        /// 健康度
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ResponseBase<HealthcheckResponse>> Healthcheck(HealthcheckRequest request);
    }
}
