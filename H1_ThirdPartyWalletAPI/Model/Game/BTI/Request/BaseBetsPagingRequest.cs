using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 注單共同欄位
    /// </summary>
    public class BaseBetsPagingRequest
    {
        /// <summary>
        /// 玩家的账号 ID
        /// </summary>
        //public string playerUserName { get; set; }

        /// <summary>
        /// 开始的日期 请注意，响应和发布数据的时区都是 UTC+0。
        /// </summary>
        [Required]
        public DateTime From { get; set; }

        /// <summary>
        /// 结束的日期. 请注意，响应和发布数据的时区都是 UTC+0。
        /// </summary>
        [Required]
        public DateTime To { get; set; }

        /// <summary>
        /// Pagination 物件
        /// </summary>
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        /// <summary>
        /// 搜寻的目标页数, 起始值为 0
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// 每页的数据量, 最大值为 1000.假设 "rowperpage" 为提供则每页预设回传量将会是 1000.
        /// </summary>
        public int rowperpage { get; set; }
    }
}
