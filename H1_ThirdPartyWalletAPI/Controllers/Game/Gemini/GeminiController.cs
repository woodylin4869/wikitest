using System;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using Microsoft.AspNetCore.Mvc;
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;
using ThirdPartyWallet.Share.Model.Game.Gemini.Request;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.Gemini
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiApiService _iGeminiApiService;

        public GeminiController(IGeminiApiService iGeminiApiService)
        {
            _iGeminiApiService = iGeminiApiService;
        }

        [HttpPost]
        [Route("Createplayer")]
        public async Task<CreateplayerResponse> CreateplayerAsync(CreateplayerRequest source)
        {
            return await _iGeminiApiService.CreateplayerAsync(source);
        }

        [HttpPost]
        [Route("GetBalance")]
        public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest source)
        {
            return await _iGeminiApiService.GetBalanceAsync(source);
        }
        [HttpPost]
        [Route("Transferin")]
        public async Task<TransferinResponse> TransferinAsync(TransferinRequest source)
        {
            source.transfer_id = Guid.NewGuid().ToString();
            return await _iGeminiApiService.TransferinAsync(source);
        }
        [HttpPost]
        [Route("Transferout")]
        public async Task<TransferoutResponse> TransferoutAsync(TransferoutRequest source)
        {
            source.transfer_id = Guid.NewGuid().ToString();
            return await _iGeminiApiService.TransferoutAsync(source);
        }

        [HttpPost]
        [Route("Queryorder")]
        public async Task<QueryorderResponse> QueryorderAsync(QueryorderRequest source)
        {

            return await _iGeminiApiService.QueryorderAsync(source);
        }
        [HttpPost]
        [Route("Launch")]
        public async Task<LaunchResponse> LaunchAsync(LaunchRequest source)
        {

            return await _iGeminiApiService.LaunchAsync(source);
        }

        [HttpPost]
        [Route("Betlist")]
        public async Task<BetlistResponse> BetlistAsync(BetlistRequest source)
        {
            var begintime = new DateTime(2024, 01, 22, 11, 00, 00);
            var endtime = new DateTime(2024, 01, 22, 12, 00, 00);
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            //source.status= new string[] { "Settled", "Cancelled", "Unsettlement", "Abnormal", "Rollback" };
            source.begintime = (long)(begintime - unixEpoch).TotalMilliseconds;
            source.endtime = (long)(endtime - unixEpoch).TotalMilliseconds;
            return await _iGeminiApiService.BetlistAsync(source);
        }


        [HttpPost]
        [Route("Gamedetail")]
        public async Task<GamedetailResponse> GamedetailAsync(GamedetailRequest source)
        {
            return await _iGeminiApiService.GamedetailAsync(source);
        }

        [HttpPost]
        [Route("GameList")]
        public async Task<GameListResponse> GetListAsync(GameListRequest source)
        {
            return await _iGeminiApiService.GameListAsync(source);
        }

        [HttpPost]
        [Route("healthcheck")]
        public async Task<string> healthcheckAsync()
        {
            return await _iGeminiApiService.healthcheckAsync();
        }
    }
}
