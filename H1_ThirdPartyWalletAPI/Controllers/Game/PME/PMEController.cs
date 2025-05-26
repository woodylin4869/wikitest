using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Game.PME;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.PME
{
    [Route("api/[controller]")]
    [ApiController]
    public class PMEController : ControllerBase
    {
        private readonly IPMEApiService _apiService;

        public PMEController(IPMEApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpPost("Register")]
        public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            return _apiService.RegisterAsync(request);
        }

        [HttpPost("GetBalance")]
        public Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request)
        {
            return _apiService.GetBalanceAsync(request);
        }

        [HttpPost("Login")]
        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return _apiService.LoginAsync(request);
        }

        [HttpPost("QueryScroll")]
        public Task<QueryScrollResponse> QueryScrollAsync(QueryScrollRequest request)
        {
            return _apiService.QueryScrollAsync(request);
        }

        [HttpPost("Transfer")]
        public Task<TransferResponse> TransferAsync(TransferRequest request)
        {
            return _apiService.TransferAsync(request);
        }

        [HttpPost("TransferQuery")]
        public Task<TransferQueryResponse> TransferQueryAsync(TransferQueryRequest request)
        {
            return _apiService.TransferQueryAsync(request);
        }



        [HttpPost("ModifyPassword")]
        public Task<ModifyResponse> ModifyPasswordAsync(GetMemberBalanceReq request)
        {

            var modifyReq = new ModifyRequest()
            {
                username = request.Club_id,
                password = PME_InterfaceService.MakePassword(request.Club_id)
            };

            return _apiService.ModifyAsync(modifyReq);
        }


    }
}
