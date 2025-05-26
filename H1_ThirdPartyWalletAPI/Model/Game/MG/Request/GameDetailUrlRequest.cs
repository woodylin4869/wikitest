namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    /// <summary>
    /// Get bet playcheck. 获取下注 playcheck
    /// </summary>
    public class GameDetailUrlRequest
    {
        /// <summary>
        /// 玩家编码不能超过 50 个字符。请只使用数字、英文字母、连字符号 (-) 和 下划线(\_)。
        /// </summary>
        public string playerId { get; set; }
        /// <summary>
        /// UTC偏移
        /// </summary>
        public int utcOffset { get; set; }
        /// <summary>
        /// 独一的下注编号。 字段长度应正好是 36 个字符
        /// </summary>
        public string betUid { get; set; }
        /// <summary>
        /// 语言指示符是代表一种语言的代码。
        /// </summary>
        public string langCode { get; set; }
    }
}
