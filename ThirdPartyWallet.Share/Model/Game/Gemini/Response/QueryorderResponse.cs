using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Response
{
    public class QueryorderResponse
    {
        public string seq { get; set; }
        public long timestamp { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public Data data { get; set; }

        public class Data
        {
            public string username { get; set; }
            public string currency { get; set; }
            public string amount { get; set; }
            public string before { get; set; }
            public string after { get; set; }
            public int time { get; set; }
            public int status { get; set; }
        }

    }
}
