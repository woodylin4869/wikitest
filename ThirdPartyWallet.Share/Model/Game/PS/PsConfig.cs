using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS
{
    public class PsConfig
    {
        public const string ConfigKey = "PsConfig";
        public string PS_URL { get; set; }
        public string PS_hostid { get; set; }
        /// <summary>
        /// 後台API網址 第三層明細用
        /// </summary>
        public string PS_BACKURL { get; set; }
        /// <summary>
        /// 後台APItoken
        /// </summary>
        public string PS_token { get; set; }
    }
}
