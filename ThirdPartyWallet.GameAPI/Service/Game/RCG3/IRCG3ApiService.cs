using ThirdPartyWallet.Share.Model.Game.RCG3.Request;
using ThirdPartyWallet.Share.Model.Game.RCG3.Response;
using static ThirdPartyWallet.Share.Model.Game.RCG3.RCG3;
namespace ThirdPartyWallet.GameAPI.Service.Game.RCG3
{
    public interface IRCG3ApiService
    {
        Task<RCG_ResBase<RCG_CreateOrSetUser_Res>> CreateOrSetUser(RCG_CreateOrSetUser request);
        Task<RCG_ResBase<RCG_Login_Res>> Login(RCG_Login request);
        Task<RCG_ResBase<RCG_GetBalance_Res>> GetBalance(RCG_GetBalance request);
        Task<RCG_ResBase<RCG_Deposit_Res>> Deposit(RCG_Deposit request);
        Task<RCG_ResBase<RCG_Withdraw_Res>> Withdraw(RCG_Withdraw request);
        Task<RCG_ResBase<RCG_KickOut_Res>> KickOut(RCG_KickOut request);
        Task<RCG_ResBase<RCG_GetTransactionLog_Res>> GetTransactionLog(RCG_GetTransactionLog request);
        Task<BaseResponse<GetBetRecordListResponse>> GetBetRecordList(GetBetRecordListRequest request);
        Task<BaseResponse<GetOpenListResponse>> GetOpenList(GetOpenListRequest request);
        Task<SingleRecordWithGameResultResponse> SingleRecordWithGameResult(SingleRecordWithGameResultRequest request);

        Task<SetCompanyGameBetLimitResponse> SetCompanyGameBetLimitResult(SetCompanyGameBetLimitRequset request);

        Task<BaseResponse<GetBetRecordListByDateRangeResponse>> GetBetRecordListByDateRange(GetBetRecordListByDateRangeRequest request);
        Task<string> HelloWorld();
    }
}
