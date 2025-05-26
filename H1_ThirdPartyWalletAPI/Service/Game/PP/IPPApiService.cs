using H1_ThirdPartyWalletAPI.Model.Game.PP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.PP.Responses.PP_Responses;

namespace H1_ThirdPartyWalletAPI.Service.Game.PP
{
    public interface IPPApiService
    {
        Task<PP_Responses> CreateMemberAsync(CreatePlayerRequest source);
        Task<StartGameResponses> StartGameAsync(StartGameRequest source);

        Task<TransferResponses> TransferAsync(TransferRequest source);

        Task<GetBalanceResponses> GetBalanceAsync(GetBalanceRequest source);

        Task<GetTransferStatusResponses> GetTransferStatusAsync(GetTransferStatusRequest source);

        Task<TerminateSessionResponses> TerminateSessionAsync(TerminateSessionRequest source);

        Task<OpenHistoryResponses> OpenHistoryAsync(OpenHistoryRequest source);

        Task<List<GetRecordResponses>> GetRecordAsync(GetRecordRequest source);
        Task<HealthCheckResponse> HealthCheckAsync();

        Task<EnvironmentResponses> EnvironmentAsync(EnvironmentRequest source);
    }
}
