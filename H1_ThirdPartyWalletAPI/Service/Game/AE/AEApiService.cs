using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Service.Common;

namespace H1_ThirdPartyWalletAPI.Service.Game.AE
{
    public class AEApiService : AEApiServiceBase, IAEApiService
    {
        public AEApiService(IHttpClientFactory httpClientFactory, ILogger<AEApiServiceBase> logger) : base(httpClientFactory, logger)
        {
        }
        #region Games
        public async Task<GetGameListResponse> GetGameListAsync(GetGameListRequest request)
        {
            SetApiTarget("dms/api");
            var response = await PostAsync<GetGameListRequest, GetGameListResponse>(request);
            return response;
        }
        public async Task<GetGameHistoryUrlResponse> GetGameHistoryUrl(GetGameHistoryUrlRequest request)
        {
            SetApiTarget("dms/api");
            var response = await PostAsync<GetGameHistoryUrlRequest, GetGameHistoryUrlResponse>(request);
            return response;
        }
        /// <summary>
        /// 这个方法会返回玩家的下注记录 to_time - from_time 必须小於等於 15mins
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetBetHistoriesResponse> GetBetHistories(GetBetHistoriesRequest request)
        {
            SetApiTarget("dms/api");
            var response = await PostAsyncWithGzip<GetBetHistoriesRequest, GetBetHistoriesResponse>(request);
            return response;
        }
        public async Task<GetReportResponse> GetReorts(GetReportRequest request)
        {
            SetApiTarget("dms/api");
            var response = await PostAsyncWithGzip<GetReportRequest, GetReportResponse>(request);
            return response;
        }
        public async Task<FreezePlayerResponse> FreezePlayerAsync(FreezePlayerRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<FreezePlayerRequest, FreezePlayerResponse>(request);
            return response;
        }
        #endregion
        #region Player
        public async Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<CreateAccountRequest, CreateAccountResponse>(request);
            return response;
        }
        public async Task<GetLoginUrlResponse> GetLoginUrlAsync(GetLoginUrlRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<GetLoginUrlRequest, GetLoginUrlResponse>(request);
            return response;
        }
        public async Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<GetBalanceRequest, GetBalanceResponse>(request);
            return response;
        }
        #endregion
        #region Transaction
        public async Task<GetTransactionResponse> GetTransactionInfo(GetTransactionRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<GetTransactionRequest, GetTransactionResponse>(request);
            return response;
        }
        public async Task<DepositResponse> DepositAsync(DepositRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<DepositRequest, DepositResponse>(request);
            return response;
        }
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request)
        {
            SetApiTarget("ams/api");
            var response = await PostAsync<WithdrawRequest, WithdrawResponse>(request);
            return response;
        }
        #endregion



    }
}
