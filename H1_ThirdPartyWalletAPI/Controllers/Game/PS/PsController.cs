using H1_ThirdPartyWalletAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ThirdPartyWallet.GameAPI.Service.Game.PS;
using ThirdPartyWallet.Share.Model.Game.PS.Request;
using ThirdPartyWallet.Share.Model.Game.PS.Response;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using ThirdPartyWallet.GameAPI.Service.Game.EGSlot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using System.Web;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.PS
{
    [Route("api/[controller]")]
    [ApiController]
    public class PsController : ControllerBase
    {
        private readonly IPsApiService _iPsApiService;
        private readonly ICacheDataService _cacheService;

        public PsController(IPsApiService iPsApiService, ICacheDataService cacheService)
        {
            _iPsApiService = iPsApiService;
            _cacheService = cacheService;
        }

        [HttpPost]
        [Route("Createplayer")]
        public async Task<CreateuserResponse> CreateplayerAsync(CreateuserRequest source)
        {
            return await _iPsApiService.CreateplayerAsync(source);
        }
        [HttpPost]
        [Route("Getgmaeurl")]
        public async Task<GetgameResponse> GetgmaeurlAsync(GetgameRequest source)
        {
            return await _iPsApiService.GetgmaeurlAsync(source);
        }
        [HttpPost]
        [Route("Moneyin")]
        public async Task<DepositResponse> MoneyinAsync(DepositRequest source)
        {
            return await _iPsApiService.MoneyinAsync(source);
        }
        [HttpPost]
        [Route("moneyout")]
        public async Task<WithdrawResponse> MoneyoutAsync(WithdrawRequest source)
        {
            return await _iPsApiService.MoneyoutAsync(source);
        }
        [HttpPost]
        [Route("GetBalance")]
        public async Task<GetbalanceResponse> GetBalanceAsync(GetbalanceRequest source)
        {
            return await _iPsApiService.GetBalanceAsync(source);
        }
        [HttpPost]
        [Route("Logout")]
        public async Task<GetbalanceResponse> LogoutAsync(GetbalanceRequest source)
        {
            return await _iPsApiService.GetBalanceAsync(source);
        }

        [HttpPost]
        [Route("Queryorder")]
        public async Task<List<QueryorderResponse>> QueryorderAsync(QueryorderRequest source)
        {
            return await _iPsApiService.QueryorderAsync(source);
        }


        [HttpGet("Authorize")]
        public async Task<AuthorizeResponse> AuthorizeAsync([FromQuery] AuthorizeRequest request)
        {
            var response = new AuthorizeResponse();
            var tokenCacheKey = $"{RedisCacheKeys.LoginToken}:{Platform.PS}:{HttpUtility.UrlEncode(request.access_token)}";
            var  data= await _cacheService.StringGetAsync<LoginRequest>(tokenCacheKey);
            
            if (data == null)
            {
                response.status_code = 1;
                response.member_id = "";
            }
            else
            {
                string token = data.Token;
                string member_id = data.MemberId;
                if (token == HttpUtility.UrlEncode(request.access_token))
                {
                    response.status_code = 0;
                    response.member_id = member_id;
                    if (request.step == 1)
                        _ = _cacheService.KeyDelete(tokenCacheKey);
                }
                else
                {
                    response.status_code = 1;
                    response.member_id = "";
                }
            }

            using var memory = new MemoryStream();
            memory.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(memory);
            return response;
        }
    }
}

