using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;

public class BetRecordResponse:ResponseBase
{
    /// <summary>
    /// 資料總筆數
    /// </summary>
    public int totalCount { get; set; }
    /// <summary>
    /// 資料筆數
    /// </summary>
    public int dataCount { get; set; }
    public Datum[] data { get; set; }
    /// <summary>
    /// 資料筆數限制
    /// </summary>
    public int limit { get; set; }

    public class Datum
    {
        /// <summary>
        /// 注單ID
        /// </summary>
        public string betID { get; set; }
        /// <summary>
        /// 營運商ID
        /// </summary>
        public string operatorID { get; set; }
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string playerID { get; set; }
        /// <summary>
        /// WE的玩家ID
        /// </summary>
        public string wEPlayerID { get; set; }
        /// <summary>
        /// 下注時間(UNIX)
        /// </summary>
        public long betDateTime { get; set; }
        /// <summary>
        /// 結算時間
        /// </summary>
        public long settlementTime { get; set; }
        /// <summary>
        /// 注單狀態
        /// </summary>
        public string betStatus { get; set; }
        /// <summary>
        /// 賠率
        /// </summary>
        public float odds { get; set; }
        /// <summary>
        /// 投注內容
        /// </summary>
        public string betCode { get; set; }
        /// <summary>
        /// 有效投注額
        /// </summary>
        public decimal validBetAmount { get; set; }
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string gameResult { get; set; }
        /// <summary>
        /// 投注設備
        /// </summary>
        public string device { get; set; }
        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal betAmount { get; set; }
        /// <summary>
        /// 贏輸金額
        /// </summary>
        public decimal winlossAmount { get; set; }
        /// <summary>
        /// 遊戲組別
        /// </summary>
        public string category { get; set; }
        /// <summary>
        /// 遊戲類別
        /// </summary>
        public string gameType { get; set; }
        /// <summary>
        /// 遊戲局ID
        /// </summary>
        public string gameRoundID { get; set; }
        /// <summary>
        /// 遊戲桌ID
        /// </summary>
        public string tableID { get; set; }
        /// <summary>
        /// IP位址
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 卡牌結果
        /// </summary>
        public Cardresult cardresult { get; set; }
        /// <summary>
        /// 主機ID
        /// </summary>
        public string trackID { get; set; }
        /// <summary>
        /// 重新結算時間
        /// </summary>
        public long resettleTime { get; set; }
        /// <summary>
        /// 小費金額
        /// </summary>
        public decimal tipAmount { get; set; }
        /// <summary>
        /// 交易類型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 下注金額(前一狀態)
        /// </summary>
        public decimal Pre_Bet { get; set; }
        /// <summary>
        /// 派彩金額(前一狀態)
        /// </summary>
        public decimal Pre_Win { get; set; }
        /// <summary>
        /// 淨贏金額(前一狀態)
        /// </summary>
        public decimal Pre_NetWin { get; set; }
        /// <summary>
        /// 彙總時間
        /// </summary>
        public DateTime report_time { get; set; }
        /// <summary>
        /// Club_id (running表)
        /// </summary>
        public string Club_id { get; set; }

        /// <summary>
        /// Franchiser_id (running表)
        /// </summary>
        public string Franchiser_id { get; set; }

        public string GroupGameType { get; set; }

        public int Groupgametype_id { get; set; }


    }
    public class W1Datum
    {
        /// <summary>
        /// 注單ID
        /// </summary>
        public string betID { get; set; }
        /// <summary>
        /// 營運商ID
        /// </summary>
        public string operatorID { get; set; }
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string playerID { get; set; }
        /// <summary>
        /// WE的玩家ID
        /// </summary>
        public string wEPlayerID { get; set; }
        /// <summary>
        /// 下注時間(UNIX)
        /// </summary>
        public DateTime betDateTime { get; set; }
        /// <summary>
        /// 結算時間
        /// </summary>
        public DateTime settlementTime { get; set; }
        /// <summary>
        /// 注單狀態
        /// </summary>
        public string betStatus { get; set; }
        /// <summary>
        /// 賠率
        /// </summary>
        public float odds { get; set; }
        /// <summary>
        /// 投注內容
        /// </summary>
        public string betCode { get; set; }
        /// <summary>
        /// 有效投注額
        /// </summary>
        public decimal validBetAmount { get; set; }
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string gameResult { get; set; }
        /// <summary>
        /// 投注設備
        /// </summary>
        public string device { get; set; }
        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal betAmount { get; set; }
        /// <summary>
        /// 贏輸金額
        /// </summary>
        public decimal winlossAmount { get; set; }
        /// <summary>
        /// 遊戲組別
        /// </summary>
        public string category { get; set; }
        /// <summary>
        /// 遊戲類別
        /// </summary>
        public string gameType { get; set; }
        /// <summary>
        /// 遊戲局ID
        /// </summary>
        public string gameRoundID { get; set; }
        /// <summary>
        /// 遊戲桌ID
        /// </summary>
        public string tableID { get; set; }
        /// <summary>
        /// IP位址
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 卡牌結果
        /// </summary>
        public Cardresult cardresult { get; set; }
        /// <summary>
        /// 主機ID
        /// </summary>
        public string trackID { get; set; }
        /// <summary>
        /// 重新結算時間
        /// </summary>
        public DateTime resettleTime { get; set; }
        /// <summary>
        /// 小費金額
        /// </summary>
        public decimal tipAmount { get; set; }
        /// <summary>
        /// 交易類型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 下注金額(前一狀態)
        /// </summary>
        public decimal Pre_Bet { get; set; }
        /// <summary>
        /// 派彩金額(前一狀態)
        /// </summary>
        public decimal Pre_Win { get; set; }
        /// <summary>
        /// 淨贏金額(前一狀態)
        /// </summary>
        public decimal Pre_NetWin { get; set; }
        /// <summary>
        /// 彙總時間
        /// </summary>
        public DateTime report_time { get; set; }
        /// <summary>
        /// Club_id (running表)
        /// </summary>
        public string Club_id { get; set; }

        public string GroupGameType { get; set; }

        public int Groupgametype_id { get; set; }
    }

    public class Cardresult
    {
        public string A1 { get; set; }
        public string A2 { get; set; }
        public string A3 { get; set; }
        public string B1 { get; set; }
        public string B2 { get; set; }
        public string B3 { get; set; }
    }

}
