using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class GameReport
    {
        public int count { get; set; }
        public long id { get; set; }
        /// <summary>
        /// 遊戲商類型
        /// </summary>
        public string platform { get; set; }
        /// <summary>
        /// 匯總時間
        /// </summary>
        public DateTime report_datetime { get; set; }
        public long total_count { get; set; }
        /// <summary>
        /// 投注金額
        /// </summary>
        public decimal total_bet { get; set; }
        /// <summary>
        /// 當局贏分
        /// </summary>
        public decimal total_win { get; set; }
        /// <summary>
        /// 當局淨贏分
        /// </summary>
        public decimal total_netwin { get; set; }
        /// <summary>
        /// 匯總類型
        /// 0: 遊戲商匯總, 1: 轉帳中心匯總
        /// </summary>
        public int report_type { get; set; }
        public DateTime update_datetime { get; set; }
        public GameReport()
        {
            update_datetime = DateTime.Now;
        }

        public enum e_report_type
        {
            /// <summary>
            /// 遊戲商
            /// </summary>
            FinancalReport,
            /// <summary>
            /// 轉帳中心
            /// </summary>
            GameBetRecord
        }

    }
}
