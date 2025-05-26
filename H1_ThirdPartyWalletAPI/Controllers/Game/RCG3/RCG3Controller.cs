using H1_ThirdPartyWalletAPI.Code;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using RCG3setup = ThirdPartyWallet.Share.Model.Game.RCG3.RCG3;
using ThirdPartyWallet.GameAPI.Service.Game.RCG3;
using ThirdPartyWallet.Share.Model.Game.RCG3.Response;
using ThirdPartyWallet.Share.Model.Game.RCG3.Request;
using static ThirdPartyWallet.Share.Model.Game.RCG3.RCG3;
namespace H1_ThirdPartyWalletAPI.Controllers.Game.RCG3
{
    [Route("api/[controller]")]
    [ApiController]
    public class RCG3Controller : ControllerBase
    {
        private readonly IRCG3ApiService _ircg3ApiService;

        public RCG3Controller(IRCG3ApiService IRCG3ApiService)
        {
            _ircg3ApiService = IRCG3ApiService;
        }

        [HttpPost]
        [Route("CreateOrSetUser")]
        public async Task<RCG_ResBase<RCG_CreateOrSetUser_Res>> CreateOrSetUser([FromBody] RCG_CreateOrSetUser request)
        {
            return await _ircg3ApiService.CreateOrSetUser(request);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<RCG_ResBase<RCG_Login_Res>> Login([FromBody] RCG_Login request)
        {
            return await _ircg3ApiService.Login(request);
        }

        [HttpPost]
        [Route("GetBalance")]
        public async Task<RCG_ResBase<RCG_GetBalance_Res>> GetBalance([FromBody] RCG_GetBalance request)
        {
            return await _ircg3ApiService.GetBalance(request);
        }

        [HttpPost]
        [Route("Deposit")]
        public async Task<RCG_ResBase<RCG_Deposit_Res>> Deposit([FromBody] RCG_Deposit request)
        {
            return await _ircg3ApiService.Deposit(request);
        }

        [HttpPost]
        [Route("Withdraw")]
        public async Task<RCG_ResBase<RCG_Withdraw_Res>> Withdraw([FromBody] RCG_Withdraw request)
        {
            return await _ircg3ApiService.Withdraw(request);

        }

        [HttpPost]
        [Route("KickOut")]
        public async Task<RCG_ResBase<RCG_KickOut_Res>> KickOut([FromBody] RCG_KickOut request)
        {
            return await _ircg3ApiService.KickOut(request);

        }

        [HttpPost]
        [Route("GetBetRecordList")]
        public async Task<BaseResponse<GetBetRecordListResponse>> GetBetRecordList([FromBody] GetBetRecordListRequest request)
        {
            return await _ircg3ApiService.GetBetRecordList(request);
        }

        [HttpPost]
        [Route("GetOpenList")]
        public async Task<BaseResponse<GetOpenListResponse>> GetOpenList([FromBody] GetOpenListRequest request)
        {
            return await _ircg3ApiService.GetOpenList(request);
        }

        [HttpPost]
        [Route("SingleRecordWithGameResult")]
        public async Task<SingleRecordWithGameResultResponse> SingleRecordWithGameResult([FromBody] SingleRecordWithGameResultRequest request)
        {
            return await _ircg3ApiService.SingleRecordWithGameResult(request);
        }

        [HttpPost]
        [Route("SetCompanyGameBetLimitResult")]
        public async Task<SetCompanyGameBetLimitResponse> SetCompanyGameBetLimitResult([FromBody] SetCompanyGameBetLimitRequset request)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            if (RCG3setup.Currency.TryGetValue(request.currency, out string mappingCurrency) == false)
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }

            request.currency = mappingCurrency;
            return await _ircg3ApiService.SetCompanyGameBetLimitResult(request);
        }
        /// <summary>
        /// 拉單-時間區間
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBetRecordListByDateRange")]
        public async Task<BaseResponse<GetBetRecordListByDateRangeResponse>> GetBetRecordListByDateRange([FromBody] GetBetRecordListByDateRangeRequest request)
        {
            return await _ircg3ApiService.GetBetRecordListByDateRange(request);
        }


        [HttpPost]
        [Route("HelloWorld")]
        public async Task<string> HelloWorld()
        {
            return await _ircg3ApiService.HelloWorld();
        }
    }
}
