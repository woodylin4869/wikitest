using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH
{
    public class RGRICHConfig
    {
        public const string ConfigKey = "RGRICHConfig";
        public string RGRICH_URL { get; set; }
        public string RGRICH_AppKey { get; set; }
        public string RGRICH_AppSecret { get; set; }
    }
}