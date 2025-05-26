using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Request
{
    /// <summary>
    /// 取得遊戲列表
    /// </summary>
    public class GetGameRequest
    {
        /// <summary>
        ///  語系
        /// </summary>
        [Required]
        public string Language { get; set; }
    }
}