using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 設定維護狀態
    /// </summary>
    public class SetMaintainStatusRequest
    {
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 維護設定(1:設定維護 0:解除維護)
        /// </summary>
        public int Maintain { get; set; }
    }
}