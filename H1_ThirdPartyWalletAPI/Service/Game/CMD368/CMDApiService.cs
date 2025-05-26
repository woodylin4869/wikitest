using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.CMD368.CMD368;

namespace H1_ThirdPartyWalletAPI.Service.Game.CMD368
{
    public class CMDApiService : CMDApiServiceBase, ICMDApiService
    {
        private readonly ILogger<CMDApiServiceBase> _logger;
        private static readonly SemaphoreSlim recordLock = new(1);
        private static readonly SemaphoreSlim recordByDateLock = new(1);

        public CMDApiService(ILogger<CMDApiServiceBase> logger, IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            _logger = logger;
        }

        public async Task<GetCreateUserResponse> RegisterAsync(GetCreateUserRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=createmember&PartnerKey={request.Partnerkey}&UserName={request.Username}&Currency={request.CurrencyCode}";
            var result = await GetAsync< GetCreateUserResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed && result.Code != (int)error_code.Useralraedyexist)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }

            return result;
        }
        public async Task<DepositResponse> DepositAsync(DepositRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=balancetransfer&PartnerKey={request.PartnerKey}&UserName={request.UserName}&PaymentType={request.PaymentType}&Money={request.Money}&TicketNo={request.TicketNo}";
            var result = await GetAsync<DepositResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }

            return result;
        }
        public async Task<WithdrawResponse> WithdrawAsync(WithdrawRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=balancetransfer&PartnerKey={request.PartnerKey}&UserName={request.UserName}&PaymentType={request.PaymentType}&Money={request.Money}&TicketNo={request.TicketNo}";
            var result = await GetAsync<WithdrawResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }

            return result;
        }

        public async Task<BalanceResponse> BalanceAsync(BalanceRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=getbalance&PartnerKey={request.PartnerKey}&UserName={request.UserName}";
            var result = await GetAsync<BalanceResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
        public async Task<KickResponse> KickAsync(KickRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=kickuser&PartnerKey={request.PartnerKey}&UserName={request.UserName}";
            var result = await GetAsync<KickResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
        public async Task<KickAllResponse> KickAllAsync(KickAllRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=kickuser&PartnerKey={request.PartnerKey}&UserName={request.UserName}";
            var result = await GetAsync<KickAllResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
        public async Task<GetWDTResponse> GetWDTAsync(GetWDTRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=checkfundtransferstatus&PartnerKey={request.PartnerKey}&UserName={request.UserName}&TicketNo={request.TicketNo}";
            var result = await GetAsync<GetWDTResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
        public async Task<OnlineUserResponse> OnlineUserAsync(OnlineUserRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=onlineusers&PartnerKey={request.PartnerKey}";
            var result = await GetAsync<OnlineUserResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }

        public async Task<IfOnlineResponse> IfOnlineAsync(IfOnlineRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=isonline&PartnerKey={request.PartnerKey}&UserName={request.UserName}";
            var result = await GetAsync<IfOnlineResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
        public async Task<IfUserExistResponse> IfUserExistAsync(IfUserExistRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=exist&PartnerKey={request.PartnerKey}&UserName={request.UserName}";
            var result = await GetAsync<IfUserExistResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
        public async Task<BetRecordResponse> BetRecordByDateAsync(BetRecordByDateRequest request, CancellationToken cancellation = default)
        {

            await recordByDateLock.WaitAsync(cancellation);
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(60));
                recordByDateLock.Release();
            }, CancellationToken.None);

            var RequestPath = $"?Method=betrecordbydate&PartnerKey={request.PartnerKey}&TimeType={request.TimeType}&StartDate={request.StartDate:yyyy-MM-ddTHH:mm:ss}&EndDate={request.EndDate:yyyy-MM-ddTHH:mm:ss}&Version={request.Version}";
            var result = await GetAsync<BetRecordResponse>(RequestPath, cancellation);

            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }

            return result;
        }
        public async Task<BetRecordResponse> BetRecordAsync(BetRecordRequest request, CancellationToken cancellation = default)
        {

            await recordLock.WaitAsync(cancellation);
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                recordLock.Release();
            }, CancellationToken.None);

            var RequestPath = $"?Method=betrecord&PartnerKey={request.PartnerKey}&Version={request.Version}";
            var result = await GetAsync<BetRecordResponse>(RequestPath, cancellation);

            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }

            return result;
        }
        public async Task<LimitResponse> LimitAsync(LimitRequest request, CancellationToken cancel = default)
        {
            var RequestPath = $"?Method=setmembertemplate&PartnerKey={request.PartnerKey}&UserName={request.UserName}&TemplateName={request.TemplateName}";
            var result = await GetAsync<LimitResponse>(RequestPath, cancel);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }

        public async Task<LanguageInfoResponse> LanguageInfoAsync(LanguageInfoRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=languageinfo&PartnerKey={request.PartnerKey}&Type={request.Type}&ID={request.ID}";
            var result = await GetAsync<LanguageInfoResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }

        public async Task<ParlayBetRecordResponse> ParlayBetRecordAsync(ParlayBetRecordRequest request, CancellationToken cancellation = default)
        {
            var RequestPath = $"?Method=parlaybetrecord&PartnerKey={request.PartnerKey}&SocTransId={request.SocTransID}";
            var result = await GetAsync<ParlayBetRecordResponse>(RequestPath, cancellation);
            if (result.Code != (int)error_code.successed)
            {
                throw new Exception(Enum.GetName(typeof(error_code), result.Code));
            }
            return result;
        }
    }
}

