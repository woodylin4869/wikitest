using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 GetBalance 获取余额
    /// </summary>
    public class GetBalanceRequest : BaseRequest
    {
        /// <summary>
        /// string(50) 玩家在 BTI 账号是唯一登入的。
        /// </summary>
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string MerchantCustomerCode { get; set; }
    }
}
