using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;
using H1_ThirdPartyWalletAPI.Service.Game.AE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.AE
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AEController : ControllerBase
    {
        private readonly IAEApiService iAEApiService;

        public AEController(IAEApiService iAEApiService)
        {
            this.iAEApiService = iAEApiService;
        }
        public async Task<string> CreateJWTToken(TestRequest testRequest)
        {
            var result = iAEApiService.GetJWTTokenFromRequest(testRequest);
            return await Task.FromResult(result);
        }

        public async Task<GetBetHistoriesResponse> GetBetHistories(GetBetHistoriesRequest request)
        {
            return await iAEApiService.GetBetHistories(request);
        }

        public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request)
        {
            return await iAEApiService.GetBalanceAsync(request);
        }
        public async Task<CreateAccountResponse> CreateUser(CreateAccountRequest request)
        {
            return await iAEApiService.CreateAccountAsync(request);
        }
        public async Task<GetGameListResponse> GetGameList(GetGameListRequest request)
        {
            return await iAEApiService.GetGameListAsync(request);
        }
    }

    public class TestRequest
    {
        public string action { get; set; }
        public int site_id { get; set; }

    }
}
