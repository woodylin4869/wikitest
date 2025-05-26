using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class DepositRequest
{
    /// </summary>
    /// 運營商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string playerID { get; set; }
    /// <summary>
    /// 當前交易編號 (最大長度: 50字符)
    /// </summary>
    public string uid { get; set; }
    /// <summary>
    /// 金額 (單位:分)
    /// </summary>
    public decimal amount { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }

}
