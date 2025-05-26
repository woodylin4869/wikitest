using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.NEXTSPIN
{
    [Route("api/[controller]")]
    [ApiController]
    public class NEXTSPINController : ControllerBase
    {
        private readonly INEXTSPINApiService _apiService;
        private readonly IDBService _dbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ICacheDataService _cacheService;
        private readonly IGamePlatformUserService _gamePlatformUserService;

        public NEXTSPINController(INEXTSPINApiService apiService, IDBService dBService, ITransferWalletService transferWalletService, ICacheDataService cacheService, IGamePlatformUserService gamePlatformUserService)
        {
            _apiService = apiService;
            _dbService = dBService;
            _transferWalletService = transferWalletService;
            _cacheService = cacheService;
            _gamePlatformUserService = gamePlatformUserService;
        }

        [HttpPost("CheckTransferResponse")]
        public Task<CheckTransferResponse> CheckTransferResponseAsync([FromBody] CheckTransferRequest request)
        {
            return _apiService.CheckTransferAsync(request);
        }

        [HttpPost("Deposit")]
        public Task<DepositResponse> DepositAsync([FromBody] DepositRequest request)
        {
            return _apiService.DepositAsync(request);
        }

        [HttpPost("GetAcctInfo")]
        public Task<GetAcctInfoResponse> GetAcctInfoAsync([FromBody] GetAcctInfoRequest request)
        {
            return _apiService.GetAcctInfoAsync(request);
        }

        [HttpPost("GetBetHistory")]
        public Task<GetBetHistoryResponse> GetBetHistoryAsync([FromBody] GetBetHistoryRequest request)
        {
            return _apiService.GetBetHistoryAsync(request);
        }

        [HttpPost("GetDomainList")]
        public Task<GetDomainListResponse> GetDomainListAsync([FromBody] GetDomainListRequest request)
        {
            return _apiService.GetDomainListAsync(request);
        }

        [HttpPost("KickAcct")]
        public Task<KickAcctResponse> KickAcctAsync([FromBody] KickAcctRequest request)
        {
            return _apiService.KickAcctAsync(request);
        }

        [HttpPost("Withdraw")]
        public Task<WithdrawResponse> WithdrawAsync([FromBody] WithdrawRequest request)
        {
            return _apiService.WithdrawAsync(request);
        }

        [HttpPost("Authorize")]
        public async Task<AuthorizeResponse> AuthorizeAsync([FromBody] AuthorizeRequest request)
        {
            var response = new AuthorizeResponse();
            response.code = (int)Model.Game.NEXTSPIN.NEXTSPIN.ErrorCode.Success;
            response.msg = Model.Game.NEXTSPIN.NEXTSPIN.ErrorCode.Success.ToString();
            response.merchantCode = Config.CompanyToken.NEXTSPIN_MerchantCode;
            response.serialNo = request.serialNo;

            var gamePlatformUser = await _cacheService.StringGetAsync<GamePlatformUser>($"{RedisCacheKeys.LoginToken}:{Platform.NEXTSPIN}:GamePlatformUser:{request.acctId}");
            if (gamePlatformUser == default)
            {
                response.code = 10103;
                response.msg = "Acct Not Found";
                response.acctInfo = null;
                return response;
            }

            var wallet = await _transferWalletService.GetWalletCache(gamePlatformUser.club_id);
            if (wallet == default)
            {
                response.code = 10103;
                response.msg = "Acct Not Found";
                response.acctInfo = null;
                return response;
            }

            var tokenCacheKey = $"{RedisCacheKeys.LoginToken}:{Platform.NEXTSPIN}:{wallet.Club_id}";
            var token = await _cacheService.StringGetAsync<string>(tokenCacheKey);
            if(token == null || token != request.token)
            {
                response.code = 10103;
                response.msg = "Acct Not Found";
                response.acctInfo = null;
                return response;
            }
            _ = _cacheService.KeyDelete(tokenCacheKey);

            var memberBalance = await _transferWalletService.GetGameCredit(Platform.NEXTSPIN, gamePlatformUser);
            response.acctInfo = new()
            {
                acctId = gamePlatformUser.game_user_id,
                userName = wallet.Club_Ename,
                currency = wallet.Currency,
                balance = memberBalance.Amount,
            };
            return response;
        }

        //[HttpPost("ResetRecordCache")]
        //public async Task<ResCodeBase> ResetRecordCache()
        //{
        //    var response = new ResCodeBase();

        //    var maxEnd = DateTime.Now.ToLocalTime().AddMinutes(-15);
        //    var start = maxEnd.AddDays(-7);

        //    var offset = TimeSpan.FromHours(3);

        //    var totalCount = 0;
        //    while (start < maxEnd)
        //    {
        //        var end = start.Add(offset);
        //        if (end > maxEnd)
        //        {
        //            end = maxEnd;
        //        }

        //        var records = await _dbService.GetNextSpinRecordsByTicketTime(start, end);

        //        await Task.WhenAll(
        //            _cacheService.BatchStringSetAsync(
        //                records.ToDictionary(b => $"{RedisCacheKeys.RecordPrimaryKey}:{Platform.NEXTSPIN}:{b.ticketId}", b => b.ticketId.ToString())
        //                , TimeSpan.FromDays(7))
        //            );

        //        totalCount += records.Count;

        //        start = end;
        //    }

        //    response.Message = $"{start} ~ {maxEnd} 共 {totalCount} 筆資料，已同步至Redis";

        //    return response;
        //}
    }
}
