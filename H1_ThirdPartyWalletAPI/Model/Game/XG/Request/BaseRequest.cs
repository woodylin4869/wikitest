using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    /// <summary>
    /// XG 共用必填輸入參數
    /// </summary>
    public class BaseRequest
    {
        /// <summary>
        /// 加密後的 Key
        /// </summary>
        [Required]
        public string Key { get; set; }
    }
}
