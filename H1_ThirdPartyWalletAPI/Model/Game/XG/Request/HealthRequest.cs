using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Request
{
    public class HealthRequest : BaseRequest
    {
        /// <summary>
        /// 公鑰(代理編號)
        /// </summary>
        [Required]
        public string AgentId { get; set; }
        public string? ApiLang { get; set; }
    }
}
