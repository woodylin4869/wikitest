using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class BetRecordSession
    {
        /// <summary>
        /// guid
        /// </summary>
        public Guid id { get; set; }
        /// <summary>
        /// H1 Session_id
        /// </summary>
        public Guid Session_id { get; set; }
        /// <summary>
        /// club id
        /// </summary>        
        public string Club_id { get; set; }
        public string Franchiser_id { get; set; }
        public string Game_id { get; set; }
        public int Game_type { get; set; }
        public int Bet_type { get; set; }
        /// <summary>
        /// 投注金額
        /// </summary>
        public decimal Bet_amount { get; set; }
        /// <summary>
        /// 有效投注
        /// </summary>
        public decimal Turnover { get; set; }
        /// <summary>
        /// 贏分
        /// </summary>
        public decimal Win { get; set; }
        /// <summary>
        /// 淨贏分
        /// </summary>
        public decimal Netwin { get; set; }
        /// <summary>
        /// 退水
        /// </summary>
        public decimal Reward { get; set; }
        /// <summary>
        /// 手續費
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// 彩金貢獻
        /// </summary>
        public decimal JackpotCon { get; set; }
        /// <summary>
        /// 彩金贏分
        /// </summary>
        public decimal JackpotWin { get; set; }
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime? StartDatetime { get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndDatetime { get; set; }
        /// <summary>
        /// 貨幣
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 匯總筆數
        /// </summary>
        public int RecordCount { get; set; }
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime UpdateDatetime { get; set; }
        public Recordstatus status { get; set; }
        /// <summary>
        /// 投注Session
        /// </summary>
        public Guid bet_session_id { get; set; }
        public BetRecordSession()
        {
            id = Guid.NewGuid();
            RecordCount = 0;
            Club_id = null;
            Currency = null;
            Franchiser_id = null;
            Game_id = "";
            Game_type = 0;
            Bet_type = 0;
            Bet_amount = 0;
            Turnover = 0;
            Win = 0;
            Netwin = 0;
            Reward = 0;
            Fee = 0;
            JackpotCon = 0;
            JackpotWin = 0;
            UpdateDatetime = DateTime.Now;
        }
        public enum Recordstatus
        {
            InSession = 1,
            Overdue,
            BetSessionNotFound,
            SessionNotFound
        }

    }
}
