using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Enum;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Response
{
    public class BetRecordResponse
    {
        /// <summary>
        /// 流水id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 玩家ID
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 遊戲類型 (slot:老虎機, fishing:捕魚, chess:棋牌)
        /// </summary>
        public string Game_type { get; set; }

        /// <summary>
        /// 遊戲代碼
        /// </summary>
        public string Game_code { get; set; }

        /// <summary>
        /// 注單編號
        /// </summary>
        public string Bet_no { get; set; }

        /// <summary>
        /// 下注總額
        /// </summary>
        public decimal Bet_total { get; set; }

        /// <summary>
        /// 有效投注
        /// </summary>
        public decimal Bet_real { get; set; }

        /// <summary>
        /// 玩家輸贏(淨輸贏)
        /// </summary>
        public decimal Payoff { get; set; }

        /// <summary>
        /// 彩金
        /// </summary>
        public decimal Jackpot { get; set; }

        /// <summary>
        /// 彩金貢獻
        /// </summary>
        public decimal Jackpot_contribute { get; set; }

        /// <summary>
        /// 注單狀態
        ///  (0:未結算, 1:已結算, 9:無效注單)
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// 下注時間(RFC3339格式)
        /// </summary>
        public DateTime Bet_time { get; set; }

        /// <summary>
        /// 派彩時間(RFC3339格式)
        /// </summary>
        public DateTime Payout_time { get; set; }

        /// <summary>
        /// 創建時間(RFC3339格式)
        /// </summary>
        public DateTime Created_at { get; set; }

        /// <summary>
        /// 更新時間(RFC3339格式)
        /// </summary>
        public DateTime Updated_at { get; set; }

        #region db Model

        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime? Report_time { get; set; }

        /// <summary>
        /// 下注總額(前一狀態)
        /// </summary>
        public decimal Pre_Bet_total { get; set; }

        /// <summary>
        /// 有效投注(前一狀態)
        /// </summary>
        public decimal Pre_Bet_real { get; set; }

        /// <summary>
        /// 玩家輸贏(前一狀態)
        /// </summary>
        public decimal Pre_Payoff { get; set; }

        /// <summary>
        /// Club_id (running表)
        /// </summary>
        public string Club_id { get; set; }

        /// <summary>
        /// Franchiser_id (running表)
        /// </summary>
        public string Franchiser_id { get; set; }

        #endregion db Model
    }
}