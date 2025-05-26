using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.WE;
using Microsoft.AspNetCore.Mvc;
using ThirdPartyWallet.GameAPI.Service.Game.WE;
using ThirdPartyWallet.Share.Model.Game.WE.Request;
using ThirdPartyWallet.Share.Model.Game.WE.Response;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.WE
{
    [Route("api/[controller]")]
    [ApiController]
    public class WEController : ControllerBase
    {
        private readonly IWEApiService _iWeApiService;

        public WEController(IWEApiService iWEApiService)
        {
            _iWeApiService = iWEApiService;
        }

        [HttpPost]
        [Route("Createplayer")]
        public async Task<CreateUserResponse> CreateplayerAsync(CreateUserRequest source)
        {
            return await _iWeApiService.CreateUserAsync(source);
        }
        [HttpPost]
        [Route("Deposit")]
        public async Task<DepositResponse> DepositAsync(DepositRequest source)
        {
            return await _iWeApiService.DepositAsync(source);
        }
        [HttpPost]
        [Route("Withdraw")]
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest source)
        {
            return await _iWeApiService.WithdrawAsync(source);
        }
        [HttpPost]
        [Route("Balance")]
        public async Task<BalanceResponse> BalanceAsync(BalanceRequest source)
        {
            return await _iWeApiService.BalanceAsync(source);
        }
        [HttpPost]
        [Route("Betlimit")]
        public async Task<BetlimitResponse> BetlimitAsync(BetlimitRequest source)
        {
            return await _iWeApiService.BetlimitAsync(source);
        }
        [HttpPost]
        [Route("GameList")]
        public async Task<List<GameListResponse.Datum>> GameListAsync(GameListRequest source)
        {
            return await _iWeApiService.GameListAsync(source);
        }
        [HttpPost]
        [Route("Login")]
        public async Task<LoginResponse> LoginAsync(LoginRequest source)
        {
            return await _iWeApiService.LoginAsync(source);
        }
        [HttpPost]
        [Route("Logout")]
        public async Task<LogoutResponse> LogoutAsync(LogoutRequest source)
        {
            return await _iWeApiService.LogoutAsync(source);
        }
        [HttpPost]
        [Route("LogoutAll")]
        public async Task<LogoutAllResponse> LogoutAllAsync(LogoutAllRequest source)
        {
            return await _iWeApiService.LogoutAllAsync(source);
        }
        [HttpPost]
        [Route("Transfer")]
        public async Task<TransferResponse> TransferAsync(TransferRequest source)
        {
            return await _iWeApiService.TransferAsync(source);
        }
        [HttpPost]
        [Route("BetRecord")]
        public async Task<BetRecordResponse> BetRecordAsync(BetRecordRequest source)
        {
            return await _iWeApiService.BetRecordAsync(source);
        }
        [HttpPost]
        [Route("ReportHour")]
        public async Task<ReportHourResponse> ReportHourAsync(ReportHourRequest source)
        {
            return await _iWeApiService.ReportHourAsync(source);
        }
        [HttpPost]
        [Route("BetDetailUrl")]
        public async Task<BetDetailUrlResponse> BetDetailUrlAsync(BetDetailUrlRequest source)
        {
            return await _iWeApiService.BetDetailUrlAsync(source);
        }
        [HttpPost]
        [Route("HealthCheck")]
        public async Task<dynamic> HealthCheckAsync(HealthCheckRequest source)
        {
            return await _iWeApiService.HealthCheckAsync(source);
        }
    }
}
