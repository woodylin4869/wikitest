using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Request
{
    public class QueryorderRequest
    {
        public string product_id { get; set; }
        public string seq { get; set; }
        public string transfer_id { get; set; }
    }
}
