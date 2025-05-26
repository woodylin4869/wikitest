using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response;

public class TransferoutResponse : ErrorCodeResponse
{
    /// <summary>
    /// 玩家幣別。大寫
    /// </summary>
    public string Currency { get; set; }
    /// <summary>
    /// 實際轉帳數值
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// 轉點前，玩家餘額
    /// </summary>
    public string BeforeBalance { get; set; }
    /// <summary>
    /// 轉點後，玩家餘額
    /// </summary>
    public string AfterBalance { get; set; }
}
