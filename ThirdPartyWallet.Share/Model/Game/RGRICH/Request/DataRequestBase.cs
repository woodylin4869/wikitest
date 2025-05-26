using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class DataRequestBase
    {
        /// <summary>
        /// 接口請求授權的唯一key
        /// </summary>
        public string AppKey { get; set; }
    }
}