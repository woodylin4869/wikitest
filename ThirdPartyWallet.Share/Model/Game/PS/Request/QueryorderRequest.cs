using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class QueryorderRequest
    {
        public string  host_id {  get; set; }
        public string memver_id { get; set; }
        public string txn_id { get; set; }
    }
}
