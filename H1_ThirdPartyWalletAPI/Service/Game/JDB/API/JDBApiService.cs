using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Interface;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game.JDB.API
{
    public class JDBApiService : JDBApiServiceBase, IJDBApiService
    {
        public JDBApiService(IHttpClientFactory httpClientFactory, ILogger<JDBApiService> logger, IApiHealthCheckService apiHealthCheckService) : base(httpClientFactory, logger)
        {


        }
        private async Task<TResponse> Post<TRequest, TResponse>(TRequest request) where TRequest : RequestBaseModel
        {
            var Response = await PostAysnc(request);
            var result = await ResultHandler<TResponse>(Response);
            return result;
        }


        public async Task<GetTokenResponse> Action11_GetToken(GetTokenRequest request)
        {
            return await Post<GetTokenRequest, GetTokenResponse>(request);
        }


        public async Task<ResponseBaseModel> Action12_CreatePlayer(CreatePlayerRequest request)
        {
            return await Post<CreatePlayerRequest, ResponseBaseModel>(request);
        }

        public async Task<QueryPlayerResponse> Action15_QueryPlayer(QueryPlayerRequest request)
        {
            return await Post<QueryPlayerRequest, QueryPlayerResponse>(request);
        }

        public async Task<ResponseBaseModel> Action17_KickOut(KickOutRequest request)
        {
            return await Post<KickOutRequest, ResponseBaseModel>(request);
        }

        public async Task<DepositOrWithdrawResponse> Action19_DepositOrWithdraw(DepositOrWithdrawRequest request)
        {
            return await Post<DepositOrWithdrawRequest, DepositOrWithdrawResponse>(request);
        }

        public async Task<BetRecordCollection> Action29_GetGameBetRecord(GetGameBetRecordRequest request)
        {
            var result = await Post<GetGameBetRecordRequest, GetBetRecordResponse>(request);
            var _result = result.Data.GroupBy(x => x.gType).ToDictionary(x => x.Key, y => y.ToList());
            return new BetRecordCollection
            {
                SlotBetRecords = _result.Keys.Contains(GameType.Slot) ? _result[GameType.Slot].Select(x => new SlotBetRecord(x)).ToList() : null,
                ArcadeBetRecords = _result.Keys.Contains(GameType.Arcade) ? _result[GameType.Arcade].Select(x => new ArcadeBetRecord(x)).ToList() : null,
                FishBetRecords = _result.Keys.Contains(GameType.Fish) ? _result[GameType.Fish].Select(x => new FishBetRecord(x)).ToList() : null,
                LotteryBetRecords = _result.Keys.Contains(GameType.Lottery) ? _result[GameType.Lottery].Select(x => new LotteryBetRecord(x)).ToList() : null,
                PokerBetRecords = _result.Keys.Contains(GameType.Poker) ? _result[GameType.Poker].Select(x => new PokerBetRecord(x)).ToList() : null,
            };
        }
        public async Task<GetBetRecordResponse> Action29_GetGameBetRecord_NoClassification(GetGameBetRecordRequest request)
        {
            return await Post<GetGameBetRecordRequest, GetBetRecordResponse>(request);
        }
        public async Task<GetDailyReportRepsonse> Action42_DailyReport(GetDailyReportRequest request)
        {
            return await Post<GetDailyReportRequest, GetDailyReportRepsonse>(request);
        }
        public async Task<ResponseBaseModel> Action43_JackpotContribution(JackpotContributionRequest request)
        {
            return await Post<JackpotContributionRequest, ResponseBaseModel>(request);
        }
        //public async Task<string> Action45_JackpotInfo(JackpotInfoRequest request)
        //{
        //    return await Post<JackpotInfoRequest, string>(request);
        //}
        //public async Task<string> Action47_GetDemoAccountToken(GetDemoAccountToken request)
        //{
        //    return await Post<GetDemoAccountToken, string>(request);

        //}

        public async Task<GetGameListResponse> Action49_GetGameList(GetGameListRequest request)
        {
            return await Post<GetGameListRequest, GetGameListResponse>(request);
        }

        public async Task<GetInGamePlayerResponse> Action52_GetInGamePlayer(GetInGamePlayerRequest request)
        {
            return await Post<GetInGamePlayerRequest, GetInGamePlayerResponse>(request);
        }

        public async Task<GetGameResultResponse> Action54_GetGameResult(GetGameResultRequest request)
        {
            return await Post<GetGameResultRequest, GetGameResultResponse>(request);
        }

        public async Task<GetCashTransferRecordResponse> Action55_GetCashTransferRecord(GetCashTransferRecordRequest request)
        {
            return await Post<GetCashTransferRecordRequest, GetCashTransferRecordResponse>(request);
        }
        public async Task<ResponseBaseModel> Action58_KickOutOfflineUsers(KickoutOfflineUsersRequest request)
        {
            return await Post<KickoutOfflineUsersRequest, ResponseBaseModel>(request);
        }

        public async Task<BetRecordCollection> Action64_GetGameHistory(GetGameHistoryRequest request)
        {

            var result = await Post<GetGameHistoryRequest, GetBetRecordResponse>(request);
            var _result = result.Data.GroupBy(x => x.gType).ToDictionary(x => x.Key, y => y.ToList());
            return new BetRecordCollection
            {
                SlotBetRecords = _result.Keys.Contains(GameType.Slot) ? _result[GameType.Slot].Select(x => new SlotBetRecord(x)).ToList() : null,
                ArcadeBetRecords = _result.Keys.Contains(GameType.Arcade) ? _result[GameType.Arcade].Select(x => new ArcadeBetRecord(x)).ToList() : null,
                FishBetRecords = _result.Keys.Contains(GameType.Fish) ? _result[GameType.Fish].Select(x => new FishBetRecord(x)).ToList() : null,
                LotteryBetRecords = _result.Keys.Contains(GameType.Lottery) ? _result[GameType.Lottery].Select(x => new LotteryBetRecord(x)).ToList() : null,
                PokerBetRecords = _result.Keys.Contains(GameType.Poker) ? _result[GameType.Poker].Select(x => new PokerBetRecord(x)).ToList() : null,
            };
        }
        public async Task<GetBetRecordResponse> Action64_NoClassification(GetGameHistoryRequest request)
        {
            return await Post<GetGameHistoryRequest, GetBetRecordResponse>(request);
        }
        public async Task<GetOnlineUserResponse> Action65_GetOnlineUser(GetOnlineUserRequest request)
        {
            return await Post<GetOnlineUserRequest, GetOnlineUserResponse>(request);
        }
    }
}
