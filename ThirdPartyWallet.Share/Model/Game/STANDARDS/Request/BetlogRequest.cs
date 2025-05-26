using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Request
{
    public class BetlogRequest
    {
        /// <summary>
        /// 搜尋起始時間(GMT+8)
        /// </summary>
        public string start_time {  get; set; }
        /// <summary>
        /// 搜尋結束時間(GMT+8)
        /// </summary>
        public string end_time { get; set; }
        /// <summary>
        /// 查詢頁數 (從1開始)
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// 每頁顯示筆數 (最小2000, 最大5000)
        /// </summary>
        public int page_size { get; set; }
        /// <summary>
        /// 選填
        /// 1=依投注時間查詢，2=依派彩(結算)時間查詢
        /// </summary>
        public int time_type { get; set; }

    }
}
