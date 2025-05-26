using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class healthcheckResponse
    {
        public int status_code { get; set; }
        public Host_Api_Info host_api_info { get; set; }
        public class Host_Api_Info
        {
            public string base_url { get; set; }
            public string auth { get; set; }
            public string logout { get; set; }
            public string bet { get; set; }
            public string result { get; set; }
            public string refundbet { get; set; }
            public string bonusaward { get; set; }
            public string resultex { get; set; }
            public string getbalance { get; set; }
        }
    }
}
