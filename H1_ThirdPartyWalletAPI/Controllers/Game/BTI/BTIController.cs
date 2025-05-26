using H1_ThirdPartyWalletAPI.Model.Game.BTI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Response;
using H1_ThirdPartyWalletAPI.Service.Game.BTI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.BTI
{
    [Route("api/[controller]")]
    [ApiController]
    public class BTIController : ControllerBase
    {
        private readonly IBTIApiService _apiService;

        public BTIController(IBTIApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// CreateUserNew 创新会员 （新）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CreateUserNew")]
        public Task<BaseWalletResponse> CreateUserNewAsync(CreateUserNewRequest request)
        {
            return _apiService.CreateUserNewAsync(request);
        }

        /// <summary>
        /// GetCustomerAuthToken 获取令牌参数值
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetCustomerAuthToken")]
        public Task<BaseWalletResponse> GetCustomerAuthTokenAsync(GetCustomerAuthTokenRequest request)
        {
            return _apiService.GetCustomerAuthTokenAsync(request);
        }

        /// <summary>
        /// GetBalance 获取余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetBalance")]
        public Task<BaseWalletResponse> GetBalanceAsync(GetBalanceRequest request)
        {
            return _apiService.GetBalanceAsync(request);
        }

        /// <summary>
        /// TransferToWHL 转入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("TransferToWHL")]
        public Task<BaseWalletResponse> TransferToWHLAsync(TransferToWHLRequest request)
        {
            return _apiService.TransferToWHLAsync(request);
        }

        /// <summary>
        /// TransferFromWHL 转出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("TransferFromWHL")]
        public Task<BaseWalletResponse> TransferFromWHLAsync(TransferFromWHLRequest request)
        {
            return _apiService.TransferFromWHLAsync(request);
        }

        /// <summary>
        /// CheckTransaction 检查转账记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CheckTransaction")]
        public Task<BaseWalletResponse> CheckTransactionAsync(CheckTransactionRequest request)
        {
            return _apiService.CheckTransactionAsync(request);
        }

        /// <summary>
        /// 获取接口验证令牌
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("AuthorizeV2")]
        public Task<AuthorizeV2Response> AuthorizeV2Async(AuthorizeV2Request request)
        {
            return _apiService.AuthorizeV2Async(request);
        }

        /// <summary>
        /// 获取接口验证令牌 取預設設定檔
        /// </summary>
        /// <returns></returns>
        [HttpPost("AuthorizeV2Default")]
        public Task<AuthorizeV2Response> AuthorizeV2DefaultAsync()
        {
            return _apiService.AuthorizeV2DefaultAsync();
        }

        /// <summary>
        /// 投注历史分页
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetHistoryBetsPaging")]
        public Task<GetHistoryBetsPagingResponse> GetHistoryBetsPagingAsync(GetHistoryBetsPagingRequest request)
        {
            /*var authReq = new AuthorizeV2Request();
            authReq.AgentUserName = request.AgentUserName;
            authReq.AgentPassword = request.AgentPassword;
            var authResp = _apiService.AuthorizeV2Async(authReq);*/
            //string token = authResp.token;
            string token = "3NoSgmSQaTS7PQw6ATCgQo2O5lI";
            return _apiService.GetHistoryBetsPagingAsync(request, token);
        }

        /// <summary>
        /// 未结算新的注单分页
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("GetOpenBetsPaging")]
        public Task<GetOpenBetsPagingResponse> GetOpenBetsPagingAsync(GetOpenBetsPagingRequest request)
        {
            /*var authReq = new AuthorizeV2Request();
            authReq.AgentUserName = request.AgentUserName;
            authReq.AgentPassword = request.AgentPassword;
            var authResp = _apiService.AuthorizeV2Async(authReq);*/
            //string token = authResp.token;
            string token = "3NoSgmSQaTS7PQw6ATCgQo2O5lI";
            return _apiService.GetOpenBetsPagingAsync(request, token);
        }
    }
}
