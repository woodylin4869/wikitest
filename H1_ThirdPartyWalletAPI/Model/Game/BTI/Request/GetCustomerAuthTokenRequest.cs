using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 GetCustomerAuthToken 获取令牌参数值
    /// </summary>
    public class GetCustomerAuthTokenRequest : BaseRequest
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
