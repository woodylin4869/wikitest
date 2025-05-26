using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Enum
{
    public enum Status
    {
        [Description("未結算")]
        UnSellte = 0,

        [Description("已結算")]
        Sellte = 1,

        [Description("無效注單")]
        Invalid = 9,

        [Description("重複注單")]
        Duplicate = 32767
    }
}