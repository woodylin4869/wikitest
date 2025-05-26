using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    /// <summary>
    /// 取得點數不為 0 的會員帳戶資訊(已離開遊戲)
    /// </summary>
    public class GetUnwithdrawnRequest
    {
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
    }
}