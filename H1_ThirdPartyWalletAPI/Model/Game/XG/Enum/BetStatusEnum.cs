using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Enum
{
    /// <summary>
    /// 注單狀態
    /// 1	中獎	玩家在任一注區有贏錢
    /// 2	未中獎 玩家沒有在任何注區贏錢
    /// 3	和局 輸贏金額 0 且 有效金額 0
    /// 4	進行中 非同步注單不會有進行中的單 -> XX 只回已結算的單
    /// 6	取消單	
    /// 7	改單
    /// </summary>
    public enum BetStatusEnum
    {
        [Description("中獎")]
        Win = 1,

        [Description("未中獎")]
        Lose = 2,

        [Description("和局")]
        Tie = 3,

        [Description("進行中")]
        Running = 6,

        [Description("取消單")]
        Cancel = 6,

        [Description("改單")]
        Change = 7,
    }
}
