using System.ComponentModel;

namespace ThirdPartyWallet.Share.Model.Game.IDN.Enum
{
    /// <summary>
    /// IDN富遊注單查詢模式
    /// </summary>
    public enum SearchMode
    {
        [Description("下注時間")]
        BetTime = 1,

        [Description("更新時間")]
        UpdatedAt = 2
    }
}