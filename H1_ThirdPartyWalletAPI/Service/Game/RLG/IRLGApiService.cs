using H1_ThirdPartyWalletAPI.Model.Game.RLG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.RLG
{
    public interface IRLGApiService
    {
        /// <summary>
        ///  取得 URL Token
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetURLTokenResponse> GetURLTokenAsync(GetURLTokenRequest source);
        /// <summary>
        /// 建立與更新會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<CreateOrSetUserResponse> CreateOrSetUserAsync(CreateOrSetUserRequest source);

        /// <summary>
        /// 注單資訊JSON
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<BetInfoResponse> BetInfoJsonnAsync(BetInfoRequest source);
        /// <summary>
        /// 住單資訊URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<BetInfourlResponse> BetInfourlAsync(BetInfoRequest source);
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<DepositResponse> DepositAsync(DepositRequest source);
        /// <summary>
        /// 查詢玩家在線列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<PlayerOnlineListResponse> PlayerOnlineListAsync(PlayerOnlineListRequest source);
        /// <summary>
        /// 剔除玩家
        /// </summary>
        Task<KickoutResponse> KickoutAsync(KickoutRequest source);
        /// <summary>
        /// 會員點數交易分頁列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<TransferRecordResponse> TransferRecordAsync(TransferRecordRequest source);
        /// <summary>
        /// 會員投注紀錄分頁列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetBetRecordResponse> GetBetRecordAsync(GetBetRecordRequest source);
        /// <summary>
        /// 批次查詢餘額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<BatchBalanceResponse> BatchBalanceAsync(BatchBalancepostdata source);
        /// <summary>
        /// 批次會員提款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<BatchWithdrawalResponse> BatchWithdrawalAsync(BatchWithdrawalpostdata source);

        /// <summary>
        /// 會員投注總計列表
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetBetTotalListResponse> GetBetTotalListAsync(GetBetTotalListserverRequest source);

        /// <summary>
        /// 目前開放遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetOpenGameResponse> GetOpenGameAsync(GetOpenGameRequest source);
    }
}
