using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.BTI.Request
{
    /// <summary>
    /// 請求 共同參數
    /// </summary>
    public class BaseRequest
    {
        /// <summary>
        /// BTi 代理商用户的名 merchant ID 
        /// </summary>
        [Required]
        public string AgentUserName { get; set; }

        /// <summary>
        /// BTi 代理商用户密码 password 
        /// </summary>
        [Required]
        public string AgentPassword { get; set; }
    }
}
