using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Response
{
    public class TransferoutResponse
    {
        public string seq { get; set; }
        public long timestamp { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
