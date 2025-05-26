using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.CMD368;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.CMD368
{
    [Route("api/CMD368")]
    [ApiController]
    public class CMDController : ControllerBase
    {
        private readonly ICMDApiService iCMDApiService;
        private readonly IDBService _dbService;
        private readonly ICacheDataService _cacheService;

        public CMDController(ICMDApiService apiService, IDBService dBService, ICacheDataService cacheService)
        {
            iCMDApiService = apiService;
            _dbService = dBService;
            _cacheService = cacheService;
        }
 
        [HttpPost("CreatUser")]
        public async Task<GetCreateUserResponse> CreateUser(GetCreateUserRequest request)
        {
            return await iCMDApiService.RegisterAsync(request);
        }
        [HttpPost("Deposit")]
        public async Task<DepositResponse> Deposit(DepositRequest request)
        {
            //request.TicketNo = Guid.NewGuid().ToString();
            return await iCMDApiService.DepositAsync(request);
        }
        [HttpPost("Withdraw")]
        public async Task<WithdrawResponse> Withdraw(WithdrawRequest request)
        {
            //request.TicketNo = Guid.NewGuid().ToString();
            return await iCMDApiService.WithdrawAsync(request);
        }
        [HttpPost("Balance")]
        public async Task<BalanceResponse> Balance(BalanceRequest request)
        {
            return await iCMDApiService.BalanceAsync(request);
        }
        [HttpPost("Kick")]
        public async Task<KickResponse> Kick(KickRequest request)
        {
            return await iCMDApiService.KickAsync(request);
        }
        [HttpPost("KickAll")]
        public async Task<KickAllResponse> KickAll(KickAllRequest request)
        {
            return await iCMDApiService.KickAllAsync(request);
        }
        [HttpPost("GetWDT")]
        public async Task<GetWDTResponse> GetWDT(GetWDTRequest request)
        {
            //request.TicketNo = Guid.NewGuid().ToString();
            return await iCMDApiService.GetWDTAsync(request);
        }
        [HttpPost("OnlineUser")]
        public async Task<OnlineUserResponse> OnlineUser(OnlineUserRequest request)
        {
            return await iCMDApiService.OnlineUserAsync(request);
        }
        [HttpPost("IfOnline")]
        public async Task<IfOnlineResponse> IfOnline(IfOnlineRequest request)
        {
            return await iCMDApiService.IfOnlineAsync(request);
        }
        [HttpPost("IfUserExist")]
        public async Task<IfUserExistResponse> IfUserExist(IfUserExistRequest request)
        {
            return await iCMDApiService.IfUserExistAsync(request);
        }
        [HttpPost("Limit")]
        public async Task<LimitResponse> Limit(LimitRequest request)
        {
            return await iCMDApiService.LimitAsync(request);
        }
        [HttpPost("BetRecordByDate")]
        public async Task<BetRecordResponse> BetRecordByDate(BetRecordByDateRequest request)
        {
            return await iCMDApiService.BetRecordByDateAsync(request);
        }
        [HttpPost("BetRecoord")]
        public async Task<BetRecordResponse> NoteQuery(BetRecordRequest request)
        {
            return await iCMDApiService.BetRecordAsync(request);
        }
        [HttpPost("LanguageInfo")]
        public async Task<LanguageInfoResponse> LanguageInfo(LanguageInfoRequest request)
        {
            return await iCMDApiService.LanguageInfoAsync(request);
        }
        //[HttpPost("GetDomain")]
        //public async Task<string> GetDomainList(GetDomainListRequest request)
        //{
        //    var result = await iCMDApiService.GetDomainListAsync(request);
        //    return result.domains.FirstOrDefault();

        //}

        [HttpGet("Authorize")]
        public async Task<string> AuthorizeAsync([FromQuery] AuthorizeRequest request)
        {
            var response = new AuthorizeResponse();
            XmlSerializer x = new XmlSerializer(response.GetType());
            response.status_code = (int)Model.Game.CMD368.CMD368.error_code.successed;
            response.message = Model.Game.CMD368.CMD368.error_code.successed.ToString();
 

            var tokenCacheKey = $"{RedisCacheKeys.LoginToken}:{Platform.CMD368}:{request.token}";
            var memberId = await _cacheService.StringGetAsync<string>(tokenCacheKey);

            if (memberId == null)
            {
                response.status_code = 2;
                response.message = "Failed";
            }
            else
            {
                response.member_id = memberId;
                _ = _cacheService.KeyDelete(tokenCacheKey);
            }

            using var memory = new MemoryStream();
            x.Serialize(memory, response);
            memory.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(memory);
            return await reader.ReadToEndAsync();
        }
    }
}

