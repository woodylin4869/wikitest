using H1_ThirdPartyWalletAPI.Model.Game.KS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Response;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.KS;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.KS
{
    /// <summary>
    /// KS API
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    public class KSController : ControllerBase
    {

        private readonly IDBService _dbIdbService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly IKSApiService _iKSapiservice;
        public KSController(IDBService dbIdbService, ITransferWalletService transferWalletService, IKSApiService iKSapiservice)
        {
            _dbIdbService = dbIdbService;
            _transferWalletService = transferWalletService;
            _iKSapiservice = iKSapiservice;
        }

        /// <summary>
        /// 建立帳號
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateMember")]
        public async Task<KSBaseRespones<UserRegisterResponse>> CreateMemberAsync([FromBody] UserRegisterRequest source)
        {
            _iKSapiservice.SetContentLanguage(Model.Game.KS.KS.lang["th-TH"]);
            return await _iKSapiservice.UserRegister(source);
        }


        /// <summary>
        /// 查询会员的余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UserBalance")]
        public async Task<KSBaseRespones<UserBalanceResponse>> UserBalanceAsync([FromBody] UserBalanceRequest source)
        {
            return await _iKSapiservice.UserBalance(source);
        }


        /// <summary>
        /// 查询会员的余额
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UserTransferInfo")]
        public async Task<KSBaseRespones<UserTransferInfoResponse>> UserTransferInfoAsync([FromBody] UserTransferInfoRequest source)
        {

            return await _iKSapiservice.UserTransferInfo(source);
        }

        /// <summary>
        /// 3.3 Get / 订单拉取
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LogGet")]
        public async Task<KSBaseRespones<LogGetResponse>> LogGetAsync([FromBody] LogGetRequest source)
        {
            source.OrderType = "All";
            source.Type = "UpdateAt";
            source.PageIndex = 1;
            source.PageSize = 1000;
            source.StartAt = System.DateTime.Now.AddDays(-10);
            source.EndAt = System.DateTime.Now;

            return await _iKSapiservice.LogGet(source);
        }


        /// <summary>
        /// 3.3 Get / 订单拉取
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("kSBaseRespones")]
        public async Task<KSBaseRespones<object>> kSBaseResponesAsync([FromBody] LogGetRequest source)
        {
            KSBaseRespones<object> kSBaseRespones = new KSBaseRespones<object>();

            return kSBaseRespones;
        }
    }
}
