using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
public class TransferinRequest
{
    /// <summary>
    /// 轉帳錢包：玩家帳號，唯一值
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 運營商帳號
    /// </summary>
    public string AgentName { get; set; }
    /// <summary>
    /// 轉帳金額。負數為提領，正數為存入
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// 轉帳參考碼。唯一值。用於後續需要查詢紀錄
    /// </summary>
    public string ReferenceCode { get; set; }
    /// <summary>
    /// 是否全部提領。若為 true，則 Amount 無效，會提領所有餘額
    /// </summary>
    public bool TakeAll { get; set; } = false;

}
