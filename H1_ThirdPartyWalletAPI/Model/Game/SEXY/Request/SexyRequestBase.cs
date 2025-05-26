using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class SexyRequestBase
    {
        [Required]
        public string cert { get; set; }
        [Required]
        public string agentId { get; set; }
    }
}
