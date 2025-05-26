using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.SPLUS;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Request;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Response;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.SPLUS
{
    [Route("/[controller]")]
    [ApiController]
    public class SPLUSController : ControllerBase
    {
        private readonly ISPLUSApiService _isplusApiService;

        public SPLUSController(ISPLUSApiService imsmtApiService)
        {
            _isplusApiService = imsmtApiService;
        }
        /// <summary>
        /// Deposit
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Deposit")]
        public async Task<ResponseBase<DepositResponse>> Deposit(DepositRequest request)
        {
            return await _isplusApiService.Deposit(request);
        }
        /// <summary>
        /// Withdraw
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Withdraw")]
        public async Task<ResponseBase<WithdrawResponse>> Withdraw(WithdrawRequest request)
        {
            return await _isplusApiService.Withdraw(request);
        }
        
    }
}
