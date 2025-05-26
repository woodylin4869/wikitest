using System;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.EGSlot;
using Microsoft.AspNetCore.Mvc;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Response;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Request;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.EGSlot
{
    [Route("api/[controller]")]
    [ApiController]
    public class EGSlotController : ControllerBase
    {
        private readonly IEGSlotApiService _iEGSlotApiService;

        public EGSlotController(IEGSlotApiService iEGSlotApiService)
        {
            _iEGSlotApiService = iEGSlotApiService;
        }

        [HttpPost]
        [Route("Createplayer")]
        public async Task<PlayersResponse> CreateplayerAsync(PlayersRequest source)
        {
            source.AgentName = "royal_thb";
            return await _iEGSlotApiService.PlayersAsync(source);
        }
        [HttpPost]
        [Route("PlayerStatus")]
        public async Task<StatusResponse> PlayerStatusAsync(StatusRequest source)
        {
            source.AgentName = "royal_thb";
            return await _iEGSlotApiService.StatusAsync(source);
        }
        [HttpPost]
        [Route("Transferin")]
        public async Task<TransferoutResponse> TransferoutAsync(TransferoutRequest source)
        {
            source.AgentName = "royal_thb";
            source.TakeAll = false;
            return await _iEGSlotApiService.TransferoutAsync(source);
        }
        [HttpPost]
        [Route("Transferout")]
        public async Task<TransferinResponse> TransferAsync(TransferinRequest source)
        {
            source.AgentName = "royal_thb";
            return await _iEGSlotApiService.TransferinAsync(source);
        }
        [HttpPost]
        [Route("TransferHistory")]
        public async Task<TransferHistoryResponse> TransferHistoryAsync(TransferHistoryRequest source)
        {
            source.AgentName = "royal_thb";
            return await _iEGSlotApiService.TransferHistoryAsync(source);
        }
        [HttpPost]
        [Route("Login")]
        public async Task<LoginResponse> LoginAsync(LoginRequest source)
        {
            source.AgentName = "royal_thb";
            source.HomeURL = "http://ts.bacctest.com/home/index";
            return await _iEGSlotApiService.LoginAsync(source);
        }
        [HttpPost]
        [Route("Logout")]
        public async Task<LogoutResponse> LogoutAsync(LogoutRequest source)
        {
            return await _iEGSlotApiService.LogoutAsync(source);
        }
        [HttpPost]
        [Route("LogoutAll")]
        public async Task<LogoutAllResponse> LogoutAllAsync(LogoutAllRequest source)
        {
            source.AgentName = "royal_thb";
            source.GameID = "";
            return await _iEGSlotApiService.LogoutAllAsync(source);
        }
        [HttpPost]
        [Route("Transaction")]
        public async Task<TransactionResponse> TransactionAsync(TransactionRequest source)
        {
            source.AgentName = "royal_thb";
            return await _iEGSlotApiService.TransactionAsync(source);
        }

        [HttpPost]
        [Route("Getagents")]
        public async Task<GetagentsResponse> GetagentsAsync()
        {
            return await _iEGSlotApiService.GetagentsAsync();
        }
    }
}
