using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RCG3
{
    public class RCG3Config
    {
        public const string ConfigKey = "RCG3Config";
        public string RCG3_URL { get; set; }
        public string RCG3_ClientID { get; set; }
        public string RCG3_ClientSecret { get; set; }
        public string RCG3_DesKey { get; set; }
        public string RCG3_DesIV { get; set; }
        public string RCG3_SystemCode { get; set; }
        public string RCG3_WebId { get; set; }
    }
}
