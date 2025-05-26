using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE;
public class WEConfig
{
        public const string ConfigKey = "WEConfig";
        public string WE_URL { get; set; }
        public string WE_operatorrID { get; set; }
        public string WE_appSecret { get; set; }
    
}
