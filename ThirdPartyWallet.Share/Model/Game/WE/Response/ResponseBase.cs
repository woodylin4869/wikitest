using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response
{

    public class ResponseBase
    {
        public int code { get; set; }
        public string detail { get; set; }
        public string error { get; set; }

    }

}

