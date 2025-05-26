using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0006-2 - 平台取得 Slot 使用者下注歷史資料 get_slot_all_bet_details
    /// 0006-3 - 平台取得魚機使用者下注歷史資料 get_fish_all_bet_details
    /// </summary>
    public class CommBetDetailsRequest
    {
        /// <summary>
        /// 使用者帳號需包含後綴碼 {account}@{site_code}
        /// </summary>
        [MinLength(3)]
        [MaxLength(25)]
        [DefaultValue(null)]
        public string? account { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>

        [DefaultValue(null)]
        public DateTime start_time { get; set; }

        /// <summary>
        /// 結束時間
        /// </summary>
        [DefaultValue(null)]
        public DateTime end_time { get; set; }

        /// <summary>
        /// 目前所在分頁
        /// </summary>
        [Required]
        public int page_index { get; set; }

        /// <summary>
        /// 每頁筆數(每頁筆數{page_size}限制在 10~1000 中。)
        /// </summary>
        [Required]
        [Range(10, 100)]
        public int page_size { get; set; }
    }
}
