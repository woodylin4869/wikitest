using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IGameInterfaceService
    {
        PlatformType GetPlatformType(Platform platform);

        Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user);

        Task<bool> KickUser(Platform platform, GamePlatformUser platform_user);

        Task<bool> KickAllUser(Platform platform);

        Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData);

        Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData);

        Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData);

        Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser);

        Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record);

        Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq);

        Task<string> GameDetailURL(GetBetDetailReq request);

        Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq);
        Task HealthCheck(Platform platform);
        virtual async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime) { return true; }

        virtual async Task<RCGRowData> GameRowData(GetRowDataReq request) { return null; }

        virtual async Task<ResCodeBase> SetLimit(SetLimitReq request, GamePlatformUser platform_user, Wallet memberWalletData) { return ResCodeBase.Failure; }

        virtual async Task<List<object>> GetGameApiList(Platform platform) { return null; }
    }

    public enum RepairMode
    {
        [Description("主資料表模式")]
        Normal = 0,

        [Description("無交易紀錄資料表模式")]
        SQLUnloggedTable = 1,

        [Description("暫存資料表模式")]
        SQLTemporary = 2
    }
}