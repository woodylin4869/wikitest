using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class LoginRequest
{
    /// <summary>
    /// 在 WE 註冊的營運商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string playerID { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }

    /// <summary>
    /// 玩家IP
    /// </summary>
    public string clientIP { get; set; }
    /// <summary>
    /// 使用裝置
    /// </summary>
    public string uiMode { get; set; }
    /// <summary>
    /// 語系
    /// </summary>
    public string lang { get; set; }
    /// <summary>
    /// 游戏桌
    /// </summary>
    public string tableID { get; set; }
    /// <summary>
    /// 大廳
    /// </summary>
    public string category { get; set; }
    /// <summary>
    /// 轉跳URL
    /// </summary>
    public string redirectUrl { get; set; }
}
