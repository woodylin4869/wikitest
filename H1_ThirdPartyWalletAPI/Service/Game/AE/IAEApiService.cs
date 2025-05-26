using System;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.AE
{
    public interface IAEApiService
    {
        Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request);
        Task<DepositResponse> DepositAsync(DepositRequest request);
        Task<GetBalanceResponse> GetBalanceAsync(GetBalanceRequest request);
        /// <summary>
        /// 这个方法会返回玩家的下注记录 to_time - from_time 必须小於等於 15mins
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GetBetHistoriesResponse> GetBetHistories(GetBetHistoriesRequest request);
        Task<GetGameHistoryUrlResponse> GetGameHistoryUrl(GetGameHistoryUrlRequest request);
        Task<GetGameListResponse> GetGameListAsync(GetGameListRequest request);
        string GetJWTTokenFromRequest<TRequest>(TRequest request);
        Task<GetLoginUrlResponse> GetLoginUrlAsync(GetLoginUrlRequest request);
        Task<GetTransactionResponse> GetTransactionInfo(GetTransactionRequest request);
        Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request);
        Task<FreezePlayerResponse> FreezePlayerAsync(FreezePlayerRequest request);
        Task<GetReportResponse> GetReorts(GetReportRequest request);
    }

    public class FreezePlayerRequest: AERequestBase
    {
        public override string action => "freeze_player";
        public string account_name { get; set; }
        public int period { get; set; }
    }

    public class FreezePlayerResponse: AERequestBase
    {
        public string error_code { get; set; }
        public string freeze_end_at { get; set; }
    }
}