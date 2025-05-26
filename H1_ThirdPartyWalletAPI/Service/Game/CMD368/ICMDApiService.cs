using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response;
using System.Threading;
using System.Threading.Tasks;
using DepositResponse = H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response.DepositResponse;
using WithdrawResponse = H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response.WithdrawResponse;

namespace H1_ThirdPartyWalletAPI.Service.Game.CMD368
{
    public interface ICMDApiService
    {
        /// <summary>
        /// 玩家注册
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<GetCreateUserResponse> RegisterAsync(GetCreateUserRequest request, CancellationToken cancellation = default);


        /// <summary>
        /// 玩家资金转入/转出
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<DepositResponse> DepositAsync(DepositRequest request, CancellationToken cancellation = default);
        Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 餘額查詢
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<BalanceResponse> BalanceAsync(BalanceRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 會員踢線與所有會員踢線
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<KickResponse> KickAsync(KickRequest request, CancellationToken cancellation = default);
        Task<KickAllResponse> KickAllAsync(KickAllRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 會員存取款交易狀態
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<GetWDTResponse> GetWDTAsync(GetWDTRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 在線會員列表
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<OnlineUserResponse> OnlineUserAsync(OnlineUserRequest request, CancellationToken cancellation = default);

        /// <summary>
        /// 會員是否在線
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<IfOnlineResponse> IfOnlineAsync(IfOnlineRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 會員是否存在
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<IfUserExistResponse> IfUserExistAsync(IfUserExistRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 注单查询
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<BetRecordResponse> BetRecordByDateAsync(BetRecordByDateRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 注单查询
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<BetRecordResponse> BetRecordAsync(BetRecordRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 限注
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<LimitResponse> LimitAsync(LimitRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 語系
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<LanguageInfoResponse> LanguageInfoAsync(LanguageInfoRequest request, CancellationToken cancellation = default);
        /// <summary>
        /// 串關資訊
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<ParlayBetRecordResponse> ParlayBetRecordAsync(ParlayBetRecordRequest request, CancellationToken cancellation = default);
    }
    
}
