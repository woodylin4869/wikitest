namespace H1_ThirdPartyWalletAPI.Model
{
    /// <summary>
    /// 快取名稱
    /// </summary>
    public class RedisCacheKeys
    {
        /// <summary>
        /// WalletTransaction
        /// </summary>
        public const string WalletTransaction = "WalletTransaction";
        public const string PlatformUser = "PlatformUser";
        public const string GameInfo = "GameInfo";
        public const string OnlineUser = "OnlineUser";
        public const string RcgToken = "RcgToken";
        public const string RlgToken = "RlgToken";
        public const string StreamerToken = "StreamerToken";
        public const string RsgToken = "RsgToken";
        public const string WalletSession = "WalletSession";
        public const string ApiHealthCheck = "ApiHealthCheck";
        public const string UserWallet = "UserWallet";
        public const string JackpotHistory = "JackpotHistory";
        public const string RsgSlotRecordPK = "RsgSlotRecordPK";
        public const string RecordPrimaryKey = "RecordPrimaryKey";
        public const string RsgGetBetRecords = "RsgGetBetRecords";
        public const string RsgGameDetailURL = "RsgGameDetailURL";
        public const string RsgBetSummaryTime = "RsgBetSummaryTime";
        public const string RsgGetJackpotPoolValue = "RsgGetJackpotPoolValue";
        public const string RsgSystemCodeWebId = "RsgSystemCodeWebId";
        public const string ElectronicDepositRecord = "ElectronicDepositRecord";
        public const string LoginToken = "LoginToken";
        public const string DomainCache = "DomainCache";
        public const string PullRecordFailOver = "PullRecordFailOver";
        public const string RepairBetRecord = "RepairBetRecord";
        public const string GeminiBetSummaryTime = "GeminiBetSummaryTime";
        public const string GeminiGetBetRecords = "GeminiGetBetRecords";
        public const string PSBetSummaryTime = "PSBetSummaryTime";
        public const string PSGetBetRecords = "PSGetBetRecords";

        public const string PpBetSummaryTime = "PpBetSummaryTime";

        // jdb 後匯總 Redis key
        public const string JdbBetSummaryTime = "JdbBetSummaryTime";

        // jdb 第二層明細 root Redis key
        public const string JdbGetBetRecords = "JdbGetBetRecords";

        // jdb 第三層明細 Redis key
        public const string JdbGameDetailURL = "JdbGameDetailURL";

        // rg富遊 後匯總 Redis key
        public const string RGRICHBetSummaryTime = "RGRICHBetSummaryTime";

        // rg富遊 第二層明細 root Redis key
        public const string RGRICHGetBetRecords = "RGRICHGetBetRecords";

        // rg富遊 第三層明細 Redis key
        public const string RGRICHGameDetailURL = "RGRICHGameDetailURL";
        // EGSlot 後匯總 Redis key
        public const string EGSlotBetSummaryTime = "EGSlotBetSummaryTime";

        // CR 後匯總 Redis key
        public const string CRBetSummaryTime = "CRBetSummaryTime";

        // CR 第二層明細 root Redis key
        public const string CRGetBetRecords = "CRGetBetRecords";
        // WE 後匯總 Redis key
        public const string WEBetSummaryTime = "WEBetSummaryTime";

        public const string WEGetGameTypeMapping = "WEGetGameTypeMapping";

        public const string NextSpinBetSummaryTime = "NextSpinBetSummaryTime";

        public const string JiliSummaryTime = "JiliSummaryTime";
        public const string JiliBetSummaryTime = "JiliBetSummaryTime";
        public const string MgBetSummaryTime = "MgBetSummaryTime";

        // Ds 5分鐘匯總Redis Key
        public const string DsBetSummaryTime = "DsBetSummaryTime";

        // Ds 第二層明細 root Redis key
        public const string DsGetBetRecords = "DsGetBetRecords";

        // Ds 第三層明細 Redis key
        public const string DsGameDetailURL = "DsGameDetailURL";

        // tp 後匯總 Redis key
        public const string TpBetSummaryTime = "TpBetSummaryTime";
        // tp 第二層明細 root Redis key
        public const string TpGetBetRecords = "TpGetBetRecords";
        // tp 第三層明細 Redis key
        public const string TpGameDetailURL = "TpGameDetailURL";

        // IDN 後匯總 Redis key
        public const string IDNBetSummaryTime = "IDNBetSummaryTime";

        //Fc 後匯總 Redis Key
        public const string FcBetSummaryTime = "FcBetSummaryTime";
        // Fc 第二層明細 root Redis key
        public const string FcGetBetRecords = "FcGetBetRecords";
        // Fc 第三層明細 Redis key
        public const string FcGameDetailURL = "FcGameDetailURL";
        // gr 後彙總 Redis key
        public const string GrBetSummaryTime = "GrBetSummaryTime";

        // gr 第二層明細 root Redis key

        // ae 後匯總 Redis key
        public const string AeBetSummaryTime = "AeBetSummaryTime";

        // ae 第二層明細 root Redis key
        public const string AeGetBetRecords = "AeGetBetRecords";

        // ae 第三層明細 Redis key
        public const string AeGameDetailURL = "AeGameDetailURL";

        // Joker 後匯總 Redis key
        public const string JokerBetSummaryTime = "JokerBetSummaryTime";
        // Joker 第二層明細 root Redis key
        public const string JokerGetBetRecords = "JokerGetBetRecords";
        // Joker 第三層明細 Redis key
        public const string JokerGameDetailURL = "JokerGameDetailURL";
        // PME 後匯總 Redis key
        public const string PMEBetSummaryTime = "PMEBetSummaryTime";
        // VA 後彙總 Redis key
        public const string VABetSummaryTime = "VABetSummaryTime";
        // 共用後匯總 Redis key
        public const string BetSummaryTime = "BetSummaryTime";
    }

    public class L2RedisCacheKeys
    {
        /// <summary>
        /// WalletTransaction
        /// </summary>
        public const string club = "club";
        public const string session_id = "session_id";
        public const string franchiser = "franchiser";
        public const string all_session = "all_session";
        public const string api_response = "api_response";
        public const string withdraw_list = "withdraw_list";
        public const string refund_list = "refund_list";
        public const string franchiser_user = "franchiser_user";
        public const string game_user = "game_user";
    }

    public class LockRedisCacheKeys
    {
        public const string ForwardGame = "ForwardGame";
        public const string W1WalletLock = "W1WalletLock";
    }
}