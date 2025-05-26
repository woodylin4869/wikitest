using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Request
{
    public class LoginWithoutRedirectRequest
    {   
        /// <summary>
        /// 會員唯一識別值
        /// </summary>

        public string Account { set; get; }
        /// <summary>
        /// 遊戲唯一識別值(同等 GameList 各遊戲的 GameId)
        /// </summary>

        public int GameId { set; get; }
        /// <summary>
        /// UI 語系, 請參考 附錄 – 語系參數
        /// </summary>

        public string Lang { set; get; }
        /// <summary>
        /// 不列入 md5 加密,遊戲回主頁功能導向位置
        /// </summary>
        [Required]
        public string HomeUrl { set; get; }
        /// <summary>
        /// 不列入 md5 加密,帶入 web 或是 app
        /// </summary>
        [Required]
        public string platform { set; get; }
    }
}
