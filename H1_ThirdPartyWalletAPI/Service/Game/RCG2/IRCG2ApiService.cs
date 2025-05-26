using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.RCG2
{
    public interface IRCG2ApiService
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
