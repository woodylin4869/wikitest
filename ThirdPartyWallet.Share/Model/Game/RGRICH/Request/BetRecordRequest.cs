using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Enum;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class BetRecordRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 查詢模式 1:下注時間查詢 2:結算時間查詢 預設1
        /// </summary>
        public SearchMode? SearchMode { get; set; }

        /// <summary>
        /// 流水號（站點生成）
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 流水號（站點生成）
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 一頁幾筆(預設500筆)
        /// </summary>
        public int? PerPage { get; set; }
    }
}