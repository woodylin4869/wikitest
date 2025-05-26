using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Request
{
    public class TransferoutRequest
    {
        public string product_id { get; set; }
        public string seq { get; set; }
        public string transfer_id { get; set; }
        public string username { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
    }
}
