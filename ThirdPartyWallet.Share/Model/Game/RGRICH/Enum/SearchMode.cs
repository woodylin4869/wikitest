using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Enum
{
    /// <summary>
    /// RGRICH富遊注單查詢模式
    /// </summary>
    public enum SearchMode
    {
        [Description("下注時間")]
        BetTime = 1,

        [Description("更新時間")]
        UpdatedAt = 2
    }
}