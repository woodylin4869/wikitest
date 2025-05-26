using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class LogoutAllRequest
{
    /// <summary>
    /// 在 WE 註冊的營運商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 玩家ID可輸入單一或多個玩家ID,需以逗號隔開.(例如:player1,player2)
    /// </summary>
    [DefaultValue("")]
    public string playerID { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }

}
