using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Request
{
    public class LaunchRequest
    {
        public string product_id { get; set; }
        public string seq { get; set; }
        public string username { get; set; }
        public string gametype { get; set; }
        public string lang { get; set; }
    }
}
