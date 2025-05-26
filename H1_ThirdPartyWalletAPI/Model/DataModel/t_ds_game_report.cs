using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_ds_game_report
    {
        /// <summary>
        /// 每小時匯總時間
        /// </summary>
        public DateTime create_datetime { get; set; }
        /// <summary>
        /// 代理帳號
        /// </summary>
        public string agent { get; set; }
        /// <summary>
        /// 下注數量
        /// </summary>
        public string bet_count { get; set; }
        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal bet_amount { get; set; }
        /// <summary>
        /// 遊戲贏分(未扣除手續費)
        /// </summary>
        public decimal payout_amount { get; set; }
        /// <summary>
        /// 有效金額
        /// </summary>
        public decimal valid_amount { get; set; }
        /// <summary>
        /// 手續費
        /// </summary>
        public decimal fee_amount { get; set; }
        /// <summary>
        /// 彩金金額
        /// </summary>
        public decimal jp_amount { get; set; }
    }
}