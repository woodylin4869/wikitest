using H1_ThirdPartyWalletAPI.Model.Game.MP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Response;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.MP
{
    public interface IMPApiService
    {
        Task<LoginToPlatformResponse> LoginToPlatformAsync(LoginToPlatformParam source);
        Task<KickPlayerOfflineResponse> KickPlayerOfflineAsync(KickPlayerOfflineParam source);

        Task<LnquiryScoreStatusResponse> LnquiryScoreStatusAsync(LnquiryScoreStatusParam source);

        Task<FundInResponse> FundInAsync(KFundInParam source);
        Task<FundOutResponse> FundOutAsync( KFundOutParam source);

        Task<InquiryaboutOrderStatusResponse> InquiryaboutOrderStatusAsync(InquiryaboutOrderparam source);

        Task<List<MPData>> PullGameBettingSlipAsync( PullGameBettingSlipParam source);

        Task<CheckSummaryResponse> CheckSummaryAsync(CheckSummaryparam source);

        /// <summary>
        /// 拉匯總
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<GetPlatformStatusResponse> GetPlatformStatus(GetPlatformStatusRequest source);
    }
}
