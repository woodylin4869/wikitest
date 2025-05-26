using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB
{
    public class JDB
    {
        public static Dictionary<string, string> lang = new Dictionary<string, string>()
        {
            {"en-US", "en"},
            {"th-TH", "th"},
            {"vi-VN", "vn"},
            {"zh-TW", "ch"},
            {"zh-CN", "ch"},
        };
        public static Dictionary<string, string> Currency = new Dictionary<string, string>()
        {
            {"THB", "THB"},
        };

        public enum gType
        {
            SLOT = 0,
            FISH = 7,
            ARCADE = 9,
            BINGO = 12,
            CARD = 18
        }
    }
    #region GameBetRequest
    public class GameBetBaseModel : RequestBaseModel
    {
        public int TransferId { get; set; }
        public long GameSeqNo { get; set; }
        public string Uid { get; set; }
        public int GType { get; set; }
        public int MType { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime GameDate { get; set; }
        public string Currency { get; set; }
        public decimal Bet { get; set; }
        public decimal Win { get; set; }
        public decimal NetWin { get; set; }
        public decimal Denom { get; set; }
        public string IpAddress { get; set; }
        public string ClientType { get; set; }
        public int SystemTakeWin { get; set; }
        public DateTime LastModifyTime { get; set; }
        public decimal Mb { get; set; }
    }
    public class ArcadeGameRequest : GameBetBaseModel
    {
        public int hasGamble { get; set; }
        public int hasBonusGame { get; set; }
    }
    public class FishGameRequest : GameBetBaseModel
    {
        public int roomType { get; set; }
    }
    public class LotteryGameRequest : GameBetBaseModel
    {
        public int hasBonusGame { get; set; }
    }
    public class SlotGameRequest : GameBetBaseModel
    {
        public int jackpotWin { get; set; }
        public float jackpotContribute { get; set; }
        public int hasFreegame { get; set; }
        public int hasGamble { get; set; }
    }
    #endregion
    #region Request
    public class BalanceRequest : RequestBaseModel
    {
        public string uid { get; set; }

        public string currency { get; set; }
    }
    public class BetRequest : RequestBaseModel
    {

    }
    public class CancelRequest : RequestBaseModel
    {
        public long transferId { get; set; }
        public string uid { get; set; }
    }
    public class ErrorHandle
    {
        public string Status { get; set; }
        public string Err_text { get; set; }
    }
    public class RequestBaseModel
    {
        public int action { get; set; }
        public long ts { get; set; }
    }
    #endregion
    #region Response
    public class ResponseBaseModel
    {
        public string Status { get; set; }

        public decimal Balance { get; set; }
    }
    public class ErrorResponseModel
    {
        public string Status { get; set; }

        public string Err_text { get; set; }
    }
    public class GameBetResponse : ResponseBaseModel
    {
        public GameBetResponse()
        {
        }

        public GameBetResponse(decimal balance)
        {
            Status = "0000";
            Balance = balance;
        }

        //public decimal Balance { get; set; }
    }
    public class GetBalanceResponse : ResponseBaseModel
    {
        public GetBalanceResponse()
        {
        }

        public GetBalanceResponse(decimal balance)
        {
            Status = "0000";
            Balance = balance;
        }

        //public decimal Balance { get; set; }
    }
    #endregion
    #region TransactionResult
    public class TransactionResultModel
    {
        public int status { get; set; }
        public int SiteId { get; set; }

        public string MemberId { get; set; }

        public decimal BeforeBalance { get; set; }

        public decimal AfterBalance { get; set; }

        public decimal Amount { get; set; }

        public int Result { get; set; }
    }
    public class ValidResultModel
    {
        public bool IsSuccessful { get; set; }
        public string Status { get; set; }

        public bool IsValidBet { get; set; }

        public TransactionResultModel transactionResult { get; set; }
    }
    #endregion
    #region Interface
    public interface IActionService
    {
        Task<ResponseBaseModel> GetResult(string body);
    }
    public interface IGameBetService
    {
        void SetRequest(string body);
        Task<GameBetResponse> Bet();

    }
    public interface IPostService
    {
    }
    #endregion
}



