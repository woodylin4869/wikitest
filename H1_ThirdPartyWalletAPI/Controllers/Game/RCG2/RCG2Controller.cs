using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response;
using H1_ThirdPartyWalletAPI.Service.Game.RCG2;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using RCG2setup = H1_ThirdPartyWalletAPI.Model.Game.RCG2.RCG2;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.RCG2
{
    [Route("api/[controller]")]
    [ApiController]
    public class RCG2Controller : ControllerBase
    {
        private readonly IRCG2ApiService _ircg2ApiService;

        public RCG2Controller(IRCG2ApiService IRCG2ApiService)
        {
            _ircg2ApiService = IRCG2ApiService;
        }

        [HttpPost]
        [Route("CreateOrSetUser")]
        public async Task<RCG_ResBase<RCG_CreateOrSetUser_Res>> CreateOrSetUser([FromBody] RCG_CreateOrSetUser request)
        {
            return await _ircg2ApiService.CreateOrSetUser(request);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<RCG_ResBase<RCG_Login_Res>> Login([FromBody] RCG_Login request)
        {
            return await _ircg2ApiService.Login(request);
        }

        [HttpPost]
        [Route("GetBalance")]
        public async Task<RCG_ResBase<RCG_GetBalance_Res>> GetBalance([FromBody] RCG_GetBalance request)
        {
            return await _ircg2ApiService.GetBalance(request);
        }

        [HttpPost]
        [Route("Deposit")]
        public async Task<RCG_ResBase<RCG_Deposit_Res>> Deposit([FromBody] RCG_Deposit request)
        {
            return await _ircg2ApiService.Deposit(request);
        }

        [HttpPost]
        [Route("Withdraw")]
        public async Task<RCG_ResBase<RCG_Withdraw_Res>> Withdraw([FromBody] RCG_Withdraw request)
        {
            return await _ircg2ApiService.Withdraw(request);

        }

        [HttpPost]
        [Route("KickOut")]
        public async Task<RCG_ResBase<RCG_KickOut_Res>> KickOut([FromBody] RCG_KickOut request)
        {
            return await _ircg2ApiService.KickOut(request);

        }

        [HttpPost]
        [Route("GetBetRecordList")]
        public async Task<BaseResponse<GetBetRecordListResponse>> GetBetRecordList([FromBody] GetBetRecordListRequest request)
        {
            return await _ircg2ApiService.GetBetRecordList(request);
        }

        [HttpPost]
        [Route("GetOpenList")]
        public async Task<BaseResponse<GetOpenListResponse>> GetOpenList([FromBody] GetOpenListRequest request)
        {
            return await _ircg2ApiService.GetOpenList(request);
        }

        [HttpPost]
        [Route("SingleRecordWithGameResult")]
        public async Task<SingleRecordWithGameResultResponse> SingleRecordWithGameResult([FromBody] SingleRecordWithGameResultRequest request)
        {
            return await _ircg2ApiService.SingleRecordWithGameResult(request);
        }

        [HttpPost]
        [Route("SetCompanyGameBetLimitResult")]
        public async Task<SetCompanyGameBetLimitResponse> SetCompanyGameBetLimitResult([FromBody] SetCompanyGameBetLimitRequset request)
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            if (RCG2setup.Currency.TryGetValue(request.currency, out string mappingCurrency) == false)
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }

            request.currency = mappingCurrency;
            return await _ircg2ApiService.SetCompanyGameBetLimitResult(request);
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
            return await _ircg2ApiService.GetBetRecordListByDateRange(request);
        }


        [HttpPost]
        [Route("HelloWorld")]
        public async Task<string> HelloWorld()
        {
            return await _ircg2ApiService.HelloWorld();
        }
    }
}
