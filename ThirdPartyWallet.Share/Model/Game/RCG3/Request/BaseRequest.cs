using System.ComponentModel.DataAnnotations;

namespace ThirdPartyWallet.Share.Model.Game.RCG3.Request
{
    /// <summary>
    /// 請求RCG2共同欄位
    /// </summary>
    public class BaseRequest
    {
        [Required]
        public string systemCode { get; set; }

        [Required]
        public string webId { get; set; }
    }
}
