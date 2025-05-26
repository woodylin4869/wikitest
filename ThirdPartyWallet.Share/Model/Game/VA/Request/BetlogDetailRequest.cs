using System.ComponentModel;

namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class BetlogDetailRequest
    {
        /// <summary>
        /// 注單編號 (允許字元:a-z A-Z 0-9 _-)
        /// </summary>
        public string BetId { get; set; }

        /// <summary>
        /// 遊戲語系 (請參照支援語系表，預設為en-US)
        /// </summary>
        [DefaultValue("en-US")]
        public string Lang { get; set; }
    }
}
