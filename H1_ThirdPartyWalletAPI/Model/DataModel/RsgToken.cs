using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class RcgToken
    {
        public string club_id { get; set; }
        public string system_code { get; set; }
        public string web_id { get; set; }
        public string auth_token { get; set; }
    }
    public class StreamerToken : RcgToken
    {

    }
}
