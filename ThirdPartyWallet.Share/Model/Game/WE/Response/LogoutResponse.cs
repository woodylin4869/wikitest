using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;

public class LogoutResponse:ResponseBase
{
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public object error { get; set; }

}
