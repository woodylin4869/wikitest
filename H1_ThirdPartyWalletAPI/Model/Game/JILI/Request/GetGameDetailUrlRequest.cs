using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Request
{
    public class GetGameDetailUrlRequest
    {
        public long WagersId { get; set; }
        [Required]
        public string Lang { get; set; }
    }
}
