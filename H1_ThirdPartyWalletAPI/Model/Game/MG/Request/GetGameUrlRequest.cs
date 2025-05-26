using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    /// <summary>
    /// Get content URL 获取内容网址
    /// </summary>
    public class GetGameUrlRequest:BaseRequest
    {
        /// <summary>
        /// gameCode 可以由遊戲列表取得
        /// </summary>
        public string contentCode { get; set; }
        /// <summary>
        /// 游戏平台 enum (Unknown, Desktop, Mobile)
        /// </summary>
        public Platform platform { get; set; }
        /// <summary>
        /// 语言 - 语言指示符是代表一种语言的代码。使用双字母标准 ISO 639-1. * 区域 -区域指示符是代表一个国家的代码.使用双字母标准 ISO 3166-2
        /// </summary>
        public string langCode { get; set; }
        /// <summary>
        /// 自订游戏大厅链接(URL) 必须使用绝对链接
        /// </summary>
        public string homeUrl { get; set; }
    }
}
