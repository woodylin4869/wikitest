using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class hoursummaryRequest
    {
        public string  host_id {  get; set; }
        public DateTime start_dt { get; set; }
        public DateTime end_dt { get; set; }
        public int group_by { get; set; }
    }
}
