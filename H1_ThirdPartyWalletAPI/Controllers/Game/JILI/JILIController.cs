using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.JILI;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.JILI
{
    /// <summary>
    /// JILI API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class JILIController : ControllerBase
    {

        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly IJILIApiService _ijiliapiservice;
        public JILIController(IDBService dbIdbService, ITransferWalletService transferWalletService, IJILIApiService ijiliapiservice)
        {
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
            _ijiliapiservice = ijiliapiservice;
        }
        /// <summary>
        /// 建立帳號
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<CreateMemberResponse> CreateMemberAsync([FromBody] CreateMemberRequest source)
        {
            return await _ijiliapiservice.CreateMemberAsync(source);
        }

        /// <summary>
        /// 登入遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LoginWithoutRedirect")]
        public async Task<LoginWithoutRedirectResponse> LoginWithoutRedirectAsync([FromBody] LoginWithoutRedirectRequest source)
        {
            return await _ijiliapiservice.LoginWithoutRedirectAsync(source);
        }
        /// <summary>
        /// 遊戲清單
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameList")]
        public async Task<GetGameListResponse> GetGameListAsync()
        {
            return await _ijiliapiservice.GetGameListAsync();
        }

        /// <summary>
        /// 查詢會員狀態
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMemberInfo")]
        public async Task<GetMemberInfoResponse> GetMemberInfoAsync([FromBody] GetMemberInfoRequest source)
        {
            return await _ijiliapiservice.GetMemberInfoAsync(source);
        }
        /// <summary>
        /// 額度轉移
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExchangeTransferByAgentId")]
        public async Task<ExchangeTransferByAgentIdResponse> ExchangeTransferByAgentIdAsync([FromBody] ExchangeTransferByAgentIdRequest source)
        {
            return await _ijiliapiservice.ExchangeTransferByAgentIdAsync(source);
        }

        /// <summary>
        /// 單獨會員踢線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("KickMember")]
        public async Task<KickMemberResponses> KickMemberAsync([FromBody] KickMemberRequest source)
        {
            return await _ijiliapiservice.KickMemberAsync(source);
        }
        /// <summary>
        /// 取得遊戲注單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBetRecordByTime")]
        public async Task<GetBetRecordByTimeResponse> GetBetRecordByTimeAsync([FromBody] GetBetRecordByTimeRequest source)
        {
            return await _ijiliapiservice.GetBetRecordByTimeAsync(source);
        }
        /// <summary>
        /// 取得住單統計
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBetRecordSummary")]
        public async Task<GetBetRecordSummaryResponse> GetBetRecordSummaryAsync([FromBody] GetBetRecordSummaryRequest source)
        {
            return await _ijiliapiservice.GetBetRecordSummaryAsync(source);
        }
        /// <summary>
        /// 取得住單URL
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetGameDetailUrl")]
        public async Task<GetGameDetailUrlResponse> GetGameDetailUrlAsync([FromBody] GetGameDetailUrlRequest source)
        {
            return await _ijiliapiservice.GetGameDetailUrlAsync(source);
        }
        /// <summary>
        /// TransactionId查詢交易紀錄
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckTransferByTransactionId")]
        public async Task<CheckTransferByTransactionIdResponse> CheckTransferByTransactionIdAsync([FromBody] CheckTransferByTransactionIdRequest source)
        {
            return await _ijiliapiservice.CheckTransferByTransactionIdAsync(source);
        }
        /// <summary>
        /// 取得所有在縣人數
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetOnlineMember")]
        public async Task<GetOnlineMemberResponse> GetOnlineMemberAsync()
        {
            return await _ijiliapiservice.GetOnlineMemberAsync();
        }
    }
}
