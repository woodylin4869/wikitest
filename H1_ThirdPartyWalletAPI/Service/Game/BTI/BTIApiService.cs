using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Extensions;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Response;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace H1_ThirdPartyWalletAPI.Service.Game.BTI
{
    public interface IBTIApiService
    {
        /// <summary>
        /// CreateUserNew 创新会员 （新）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BaseWalletResponse> CreateUserNewAsync(CreateUserNewRequest request);

        /// <summary>
        /// GetCustomerAuthToken 获取令牌参数值
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BaseWalletResponse> GetCustomerAuthTokenAsync(GetCustomerAuthTokenRequest request);

        /// <summary>
        /// GetBalance 获取余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BaseWalletResponse> GetBalanceAsync(GetBalanceRequest request);

        /// <summary>
        /// TransferToWHL 转入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BaseWalletResponse> TransferToWHLAsync(TransferToWHLRequest request);

        /// <summary>
        /// TransferFromWHL 转出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BaseWalletResponse> TransferFromWHLAsync(TransferFromWHLRequest request);

        /// <summary>
        /// CheckTransaction 检查转账记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BaseWalletResponse> CheckTransactionAsync(CheckTransactionRequest request);

        /// <summary>
        /// 获取接口验证令牌
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AuthorizeV2Response> AuthorizeV2Async(AuthorizeV2Request request);

        /// <summary>
        /// 获取接口验证令牌 取預設輸入
        /// </summary>
        /// <returns></returns>
        Task<AuthorizeV2Response> AuthorizeV2DefaultAsync();

        /// <summary>
        /// 投注历史分页
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<GetHistoryBetsPagingResponse> GetHistoryBetsPagingAsync(GetHistoryBetsPagingRequest request, string token);

        /// <summary>
        /// 未结算新的注单分页
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<GetOpenBetsPagingResponse> GetOpenBetsPagingAsync(GetOpenBetsPagingRequest request, string token);
    }

    public class BTIApiService : IBTIApiService
    {
        private readonly ILogger<BTIApiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _agentUserName;
        private readonly string _agentPassword;
        private readonly string _urlWallet;
        //private readonly string _urlGame;
        private readonly string _urlDataAuth;
        private readonly string _urlDataOpenBet;
        private readonly string _urlDataHistoryBet;

        public BTIApiService(ILogger<BTIApiService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _agentUserName = Config.CompanyToken.BTI_AgentUserName;
            _agentPassword = Config.CompanyToken.BTI_AgentPassword;
            _urlWallet = Config.GameAPI.BTI_WALLET_URL;
            //_urlGame = Config.GameAPI.BTI_GAME_URL;
            _urlDataAuth = Config.GameAPI.BTI_DATE_AUTH_URL;
            _urlDataOpenBet = Config.GameAPI.BTI_DATA_OPEN_BET_URL;
            _urlDataHistoryBet = Config.GameAPI.BTI_DATA_HISTORY_BET_URL;
        }

        /// <summary>
        /// CreateUserNew 创新会员 （新）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<BaseWalletResponse> CreateUserNewAsync(CreateUserNewRequest request)
        {
            var url = _urlWallet + "/CreateUserNew";
            return PostWalletAsync<CreateUserNewRequest, BaseWalletResponse>(url, request);
        }

        /// <summary>
        /// GetCustomerAuthToken 获取令牌参数值
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<BaseWalletResponse> GetCustomerAuthTokenAsync(GetCustomerAuthTokenRequest request)
        {
            var url = _urlWallet + "/GetCustomerAuthToken";
            return PostWalletAsync<GetCustomerAuthTokenRequest, BaseWalletResponse>(url, request);
        }

        /// <summary>
        /// GetBalance 获取余额
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<BaseWalletResponse> GetBalanceAsync(GetBalanceRequest request)
        {
            var url = _urlWallet + "/GetBalance";
            return PostWalletAsync<GetBalanceRequest, BaseWalletResponse>(url, request);
        }

        /// <summary>
        /// TransferToWHL 转入
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<BaseWalletResponse> TransferToWHLAsync(TransferToWHLRequest request)
        {
            var url = _urlWallet + "/TransferToWHL";
            return PostWalletAsync<TransferToWHLRequest, BaseWalletResponse>(url, request);
        }

        /// <summary>
        /// TransferFromWHL 转出
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<BaseWalletResponse> TransferFromWHLAsync(TransferFromWHLRequest request)
        {
            var url = _urlWallet + "/TransferFromWHL";
            return PostWalletAsync<TransferFromWHLRequest, BaseWalletResponse>(url, request);
        }

        /// <summary>
        /// CheckTransaction 检查转账记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<BaseWalletResponse> CheckTransactionAsync(CheckTransactionRequest request)
        {
            var url = _urlWallet + "/CheckTransaction";
            return PostWalletAsync<CheckTransactionRequest, BaseWalletResponse>(url, request);
        }

        /// <summary>
        /// 获取接口验证令牌
        /// https://asia-east2-bq-data-api.cloudfunctions.net/authorize_v2
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AuthorizeV2Response> AuthorizeV2Async(AuthorizeV2Request request)
        {
            var url = _urlDataAuth;
            return PostDataAsync<AuthorizeV2Request, AuthorizeV2Response>(url, request);
        }

        /// <summary>
        /// 获取接口验证令牌 取預設輸入
        /// https://asia-east2-bq-data-api.cloudfunctions.net/authorize_v2
        /// </summary>
        /// <returns></returns>
        public Task<AuthorizeV2Response> AuthorizeV2DefaultAsync()
        {
            var request = new AuthorizeV2Request()
            {
                AgentUserName = _agentUserName,
                AgentPassword = _agentPassword
            };
            return AuthorizeV2Async(request);
        }

        /// <summary>
        /// 投注历史分页
        /// https://get-history-bets-paging-d5nlg5buca-de.a.run.app
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<GetHistoryBetsPagingResponse> GetHistoryBetsPagingAsync(GetHistoryBetsPagingRequest request, string token)
        {
            var url = _urlDataHistoryBet + "?token=" + token;
            return PostDataAsync<GetHistoryBetsPagingRequest, GetHistoryBetsPagingResponse>(url, request);
        }

        /// <summary>
        /// 未结算新的注单分页
        /// https://get-open-bets-paging-d5nlg5buca-de.a.run.app
        /// </summary>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<GetOpenBetsPagingResponse> GetOpenBetsPagingAsync(GetOpenBetsPagingRequest request, string token)
        {
            var url = _urlDataOpenBet + "?token=" + token;
            return PostDataAsync<GetOpenBetsPagingRequest, GetOpenBetsPagingResponse>(url, request);
        }

        /// <summary>
        /// PostWalletAsync
        /// 帶入json 返回xml
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="responseLogFormat"></param>
        /// <returns></returns>
        protected async Task<TResponse> PostWalletAsync<TRequest, TResponse>(string url, TRequest request, Func<string, string> responseLogFormat = null)
            where TRequest : BaseRequest
            where TResponse : BaseWalletResponse
        {
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                client.Timeout = TimeSpan.FromSeconds(14);

                if (request.AgentUserName == default) request.AgentUserName = _agentUserName;
                if (request.AgentPassword == default) request.AgentPassword = _agentPassword;

                var jsonRequest = JsonSerializer.Serialize(request);
                _logger.LogInformation("BTI PostWallet RequestPath: {RequestPath} Body:{body}", url, jsonRequest);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.BTI, url, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                sw.Stop();

                // 返回是xml 轉string寫log用途
                var bodyLog = await response.Content.ReadAsStringAsync();
                var body = await response.Content.ReadAsStreamAsync();

                var dics = new Dictionary<string, object>();
                dics.Add("request", jsonRequest);
                dics.Add("xmlResponse", responseLogFormat == null ? bodyLog : responseLogFormat(bodyLog));

                using (var scope = _logger.BeginScope(dics))
                {
                    _logger.LogInformation("BTI PostWallet RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                }

                XmlSerializer serializer = new XmlSerializer(typeof(TResponse));
                var result = serializer.Deserialize(body);

                return (TResponse)result;
            }
            catch (HttpRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("BTI PostWallet exception {RequestPath} EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", url, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                throw ex;
            }
        }

        /// <summary>
        /// PostDataAsync
        /// 帶入json 返回json
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="responseLogFormat"></param>
        /// <returns></returns>
        protected async Task<TResponse> PostDataAsync<TRequest, TResponse>(string url, TRequest request, Func<string, string> responseLogFormat = null)
            //where TRequest : BaseRequest
            where TResponse : BaseDataResponse
        {
            try
            {
                var client = _httpClientFactory.CreateClient("log");
                // 都抓注單方法在用 先60秒
                client.Timeout = TimeSpan.FromSeconds(60);

                var jsonRequest = JsonSerializer.Serialize(request);
                _logger.LogInformation("BTI PostData RequestPath: {RequestPath} Body:{body}", url, jsonRequest);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.PostAsync(Platform.BTI, url, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                sw.Stop();

                var body = await response.Content.ReadAsStringAsync();
                var dics = new Dictionary<string, object>();
                dics.Add("request", jsonRequest);
                dics.Add("response", responseLogFormat == null ? body : responseLogFormat(body));

                using (var scope = _logger.BeginScope(dics))
                {
                    _logger.LogInformation("BTI PostData RequestPath: {RequestPath} | ResponseHttpStatus:{Status} | exeTime:{exeTime} ms", url, response.StatusCode, sw.Elapsed.TotalMilliseconds);
                }

                var result = JsonSerializer.Deserialize<TResponse>(body);
                return result;
            }
            catch (HttpRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("BTI PostData exception {RequestPath} EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", url, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                throw ex;
            }
        }
    }
}
