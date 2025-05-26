using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Game.XG;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.XG
{
    /// <summary>
    /// XG API
    /// </summary>
    [Route("/api/[controller]")]
    [ApiController]
    public class XGController : ControllerBase
    {
        private readonly IXGApiService _XGApiService;
        private readonly string _XGXAgentID;

        public XGController(IXGApiService XGApiService)
        {
            _XGApiService = XGApiService;
            _XGXAgentID = Config.CompanyToken.XG_AgentID;
        }

        /// <summary>
        /// 會員 創建會員帳號 /api/keno-api/xg-casino/CreateMember
        /// </summary>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<CreateMemberResponse> CreateMember([FromBody] CreateMemberRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.CreateMember(request);
        }

        /// <summary>
        /// 會員 取得登入連結 /api/keno-api/xg-casino/Login
        /// </summary>
        [HttpPost]
        [Route("Login")]
        public async Task<LoginResponse> Login([FromBody] LoginRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.Login(request);
        }

        /// <summary>
        /// 會員 踢線 /api/keno-api/xg-casino/KickMember
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("KickMember")]
        public async Task<KickMemberResponse> KickMember([FromBody] KickMemberRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.KickMember(request);
        }

        /// <summary>
        /// 會員 取得會員資料 /api/keno-api/xg-casino/Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Account")]
        public async Task<AccountResponse> Account([FromBody] AccountRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.Account(request);
        }

        /// <summary>
        /// 轉帳 會員轉帳 /api/keno-api/xg-casino/Transfer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Transfer")]
        public async Task<TransferResponse> Transfer([FromBody] TransferRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.Transfer(request);
        }

        /// <summary>
        /// 會員 限注取得 /api/keno-api/xg-casino/Template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetTemplate")]
        public async Task<GetTemplateResponse> GetTemplate([FromBody] GetTemplateRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.GetTemplate(request);
        }

        /// <summary>
        /// 會員 限注設定 /api/keno-api/xg-casino/Template
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetTemplate")]
        public async Task<SetTemplateResponse> SetTemplate([FromBody] SetTemplateRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.SetTemplate(request);
        }

        /// <summary>
        /// 轉帳 取得單筆轉帳資料 /api/keno-api/xg-casino/CheckTransfer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckTransfer")]
        public async Task<CheckTransferResponse> CheckTransfer([FromBody] CheckTransferRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.CheckTransfer(request);
        }

        /// <summary>
        /// 注單 取得會員下注內容 /api/keno-api/xg-casino/GetBetRecordByTime
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBetRecordByTime")]
        public async Task<GetBetRecordByTimeResponse> GetBetRecordByTime([FromBody] GetBetRecordByTimeRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.GetBetRecordByTime(request);
        }

        /// <summary>
        /// 注單 注單編號查詢會員下注內容  /api/keno-api/xg-casino/GetGameDetailUrl
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameDetailUrl")]
        public async Task<GetGameDetailUrlResponse> GetGameDetailUrl([FromBody] GetGameDetailUrlRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.GetGameDetailUrl(request);
        }

        /// <summary>
        /// 注單 取得會員下注內容統計 /api/keno-api/xg-casino/GetGameDetailUrl
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetApiReportUrl")]
        public async Task<GetApiReportUrlResponse> GetApiReportUrl([FromBody] GetApiReportUrlRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.GetApiReportUrl(request);
        }

        [HttpPost]
        [Route("Health")]
        public async Task<HealthResponse> Health([FromBody] HealthRequest request)
        {
            request.AgentId = _XGXAgentID;
            return await _XGApiService.Health(request);
        }
    }
}
