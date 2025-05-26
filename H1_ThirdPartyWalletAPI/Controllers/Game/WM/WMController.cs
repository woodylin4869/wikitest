
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Response;
using H1_ThirdPartyWalletAPI.Service.Game.WM;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.WM
{
    [Route("/[controller]")]
    [ApiController]
    public class WMController : ControllerBase
    {
        private readonly IWMApiService _iwmapiservice;

        public WMController(IWMApiService iwmapiservice)
        {
            _iwmapiservice = iwmapiservice;
        }
        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MemberRegister")]
        public async Task<WMResponse> MemberRegisterAsync([FromBody] MemberRegisterRequest source)
        {
            source.cmd = "MemberRegister";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await _iwmapiservice.MemberRegisterAsync(source);
        }
        /// <summary>
        /// 進入遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SigninGame")]
        public async Task<WMResponse> SigninGameAsync([FromBody] SigninGameRequest source)
        {
            source.cmd = "SigninGame";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await _iwmapiservice.SigninGameAsync(source);
        }
        /// <summary>
        /// 登出遊戲
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("LogoutGame")]
        public async Task<WMResponse> LogoutGameAsync([FromBody] LogoutGameRequest source)
        {
            source.cmd = "LogoutGame";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await _iwmapiservice.LogoutGameAsync(source);
        }

        /// <summary>
        /// 查詢會員額度
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetBalance")]
        public async Task<WMResponse> GetBalanceAsync([FromBody] GetBalanceRequest source)
        {
            source.cmd = "GetBalance";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await _iwmapiservice.GetBalanceAsync(source);
        }
        /// <summary>
        /// 存款/取款
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ChangeBalance")]
        public async Task<WMBalanceResponse> ChangeBalanceAsync([FromBody] ChangeBalanceRequest source)
        {
            source.cmd = "ChangeBalance";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            source.order = Guid.NewGuid().ToString().Replace("-","");
            return await _iwmapiservice.ChangeBalanceAsync(source);
        }
        /// <summary>
        /// 交易纪录
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetMemberTradeReport")]
        public async Task<WMTradeResponse> GetMemberTradeReportAsync([FromBody] GetMemberTradeReportRequest source)
        {
            source.cmd = "GetMemberTradeReport";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await _iwmapiservice.GetMemberTradeReportAsync(source);
        }
        /// <summary>
        /// 取住單
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDateTimeReport")]
        public async Task<WMDataReportResponse> GetDateTimeReportAsync([FromBody] GetDateTimeReportRequest source)
        {
            source.cmd = "GetDateTimeReport";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            return await _iwmapiservice.GetDateTimeReportAsync(source);
        }
        /// <summary>
        /// 修改限住
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("EditLimit")]
        public async Task<WMResponse> EditLimitAsync([FromBody] EditLimitRequest source)
        {
            source.cmd = "EditLimit";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            source.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            return await _iwmapiservice.EditLimitAsync(source);
        }

        /// <summary>
        /// 測試連線
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Hello")]
        public async Task<HelloResponse> HelloAsync([FromBody] HelloRequest source)
        {
            source.cmd = "Hello";
            source.vendorId = Config.CompanyToken.WM_THB_vendorId;
            source.signature = Config.CompanyToken.WM_THB_signature;
            return await _iwmapiservice.HelloAsync(source);
        }
    }
}
