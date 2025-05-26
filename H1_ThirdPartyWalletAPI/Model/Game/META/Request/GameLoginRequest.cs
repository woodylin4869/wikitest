using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.META.Request
{
    public class GameLoginRequest
    {

        /// <summary>
        /// 會員唯一識別值
        /// </summary>

        public string Account { set; get; }

        public string Password { get; set; }

        private string gameCode = "fruitevo";
        public string GameCode { get => gameCode; set => gameCode = value; }

        /// <summary>
        /// 返回平台網址 N 客戶欲返回自家平台網址
        /// </summary>
        [DefaultValue(null)]
        public string? RedirectUrl { set; get; }

        [DefaultValue(null)]
        public string? Lang { set; get; }
        /// <summary>
        /// 遊戲桌號 N 遊戲桌號 ID，請參考遊戲桌號
        /// </summary>

        [DefaultValue(null)]
        public string? TableId { set; get; }
    }
}
