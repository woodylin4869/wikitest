using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.VA;
using ThirdPartyWallet.Share.Model.Game.VA.Request;
using ThirdPartyWallet.Share.Model.Game.VA.Response;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.VA
{
    [Route("api/[controller]")]
    [ApiController]
    public class VAController : ControllerBase
    {
        private readonly IVAApiService _iVAApiService;
        private readonly ICacheDataService _cacheService;

        public VAController(IVAApiService iVAApiService, ICacheDataService cacheService)
        {
            _iVAApiService = iVAApiService;
            _cacheService = cacheService;
        }

        [HttpPost]
        [Route("Create")]
        public async Task<BaseResponse<CreateResponse>> CreateAsync(CreateRequest source)
        {
            return await _iVAApiService.CreateAsync(source);
        }
        [HttpPost]
        [Route("GetBalance")]
        public async Task<BaseResponse<GetBalanceResponse>> GetBalanceAsync(GetBalanceRequest source)
        {
            return await _iVAApiService.GetBalanceAsync(source);
        }

        [HttpPost]
        [Route("kickout")]
        public async Task<BaseResponse<KickUserResponse>> kickoutAsync(KickUserRequest source)
        {
            return await _iVAApiService.KickUserAsync(source);
        }

        [HttpPost]
        [Route("GetGameList")]
        public async Task<BaseResponse<GetGameListResponse>> GetGameListAsync(GetGameListRequest source)
        {
            return await _iVAApiService.GetGameListAsync(source);
        }

        [HttpPost]
        [Route("GameLink")]
        public async Task<BaseResponse<GameLinkResponse>> GameLinkAsync(GameLinkRequest source)
        {
            return await _iVAApiService.GameLinkAsync(source);
        }

        [HttpPost]
        [Route("Deposit")]
        public async Task<BaseResponse<DepositResponse>> DepositAsync(DepositRequest source)
        {
            return await _iVAApiService.DepositAsync(source);
        }
        [HttpPost]
        [Route("Withdraw")]
        public async Task<BaseResponse<WithdrawResponse>> WithdrawAsync(WithdrawRequest source)
        {
            return await _iVAApiService.WithdrawAsync(source);
        }

        [HttpPost]
        [Route("TransactionDetail")]
        public async Task<BaseResponse<TransactionDetailResponse>> TransactionDetailAsync(TransactionDetailRequest source)
        {
            return await _iVAApiService.TransactionDetailAsync(source);
        }

        [HttpPost]
        [Route("BetlogListByTime")]
        public async Task<BaseResponse<BetlogListByTimeResponse>> BetlogListByTimeAsync(BetlogListByTimeRequest source)
        {
            source.Page = 1;
            source.PageSize = 5000;
            return await _iVAApiService.BetlogListByTimeAsync(source);
        }
        [HttpPost]
        [Route("BetlogDetail")]
        public async Task<BaseResponse<BetlogDetailResponse>> BetlogDetailAsync(BetlogDetailRequest source)
        {
            return await _iVAApiService.BetlogDetailAsync(source);
        }

        [HttpPost]
        [Route("BetlogHistoryListByTime")]
        public async Task<BaseResponse<BetlogHistoryListByTimeResponse>> BetlogHistoryListByTimeAsync(BetlogHistoryListByTimeRequest source)
        {
            source.Page = 1;
            source.PageSize = 5000;
            return await _iVAApiService.BetlogHistoryListByTimeAsync(source);
        }

        [HttpPost]
        [Route("healthcheck")]
        public async Task<healthcheckResponse> healthcheckAsync(healthcheckRequest source)
        {
            return await _iVAApiService.healthcheckAsync(source);
        }

    }
}

