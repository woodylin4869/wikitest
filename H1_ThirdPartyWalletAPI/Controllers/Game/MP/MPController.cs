using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Response;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using H1_ThirdPartyWalletAPI.Service.Game.MP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.MP
{
    [Route("/[controller]")]
    [ApiController]
    public class MPController : ControllerBase
    {
        private readonly IMPApiService _IMPApiService;
        public MPController(IMPApiService iMPApiService)
        {
            _IMPApiService = iMPApiService;
        }
        /// <summary>
        /// 登入遊戲創建帳號
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LoginToPlatform")]
        public async Task<LoginToPlatformResponse> LoginToPlatformAsync([FromBody] LoginToPlatformParam source)
        {
            return await _IMPApiService.LoginToPlatformAsync(source);
        }
        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("KickPlayerOffline")]
        public async Task<KickPlayerOfflineResponse> KickPlayerOfflineAsync([FromBody] KickPlayerOfflineParam source)
        {
            return await _IMPApiService.KickPlayerOfflineAsync(source);
        }
        /// <summary>
        /// 查詢會員金額
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LnquiryScoreStatus")]
        public async Task<LnquiryScoreStatusResponse> LnquiryScoreStatusAsync([FromBody] LnquiryScoreStatusParam source)
        {
            return await _IMPApiService.LnquiryScoreStatusAsync(source);
        }
        /// <summary>
        /// 上分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("FundIn")]
        public async Task<FundInResponse> FundInAsync([FromBody] KFundInParam source)
        {
            return await _IMPApiService.FundInAsync(source);
        }
        /// <summary>
        /// 下分
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("FundOut")]
        public async Task<FundOutResponse> FundOutAsync([FromBody] KFundOutParam source)
        {
            return await _IMPApiService.FundOutAsync(source);
        }
        /// <summary>
        /// 查詢交易狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("InquiryaboutOrderStatus")]
        public async Task<InquiryaboutOrderStatusResponse> InquiryaboutOrderStatusAsync([FromBody] InquiryaboutOrderparam source)
        {
            return await _IMPApiService.InquiryaboutOrderStatusAsync(source);
        }

        [HttpPost]
        [Route("PullGameBettingSlip")]
        public async Task<List<MPData>> PullGameBettingSlipAsync([FromBody] PullGameBettingSlipParam source)
        {
            var datetime = DateTimeOffset.UtcNow;

            source.startTime = datetime.AddMinutes(-5).ToUnixTimeMilliseconds().ToString();
            source.endTime = datetime.ToUnixTimeMilliseconds().ToString();

            return await _IMPApiService.PullGameBettingSlipAsync(source);
        }

        [HttpPost]
        [Route("CheckSummary")]
        public async Task<CheckSummaryResponse> CheckSummaryAsync([FromBody] CheckSummaryparam source)
        {
           return await _IMPApiService.CheckSummaryAsync(source);
        }

        [HttpPost]
        [Route("GetPlatformStatus")]
        public async Task<GetPlatformStatusResponse> GetPlatformStatusAsync([FromBody] GetPlatformStatusRequest source)
        {
            return await _IMPApiService.GetPlatformStatus(source);
        }
    }
}
