using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class QueryorderResponse
    {
        public long txn_id { get; set; }
        public int type { get; set; }
        public string member_id { get; set; }
        public decimal amount { get; set; }
        public DateTime dtm { get; set; }
        public decimal balance { get; set; }
        public string Note { get; set; }
    }
}
