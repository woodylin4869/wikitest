using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response
{
    public class GetagentsResponse
    {
        public List<Datum> Data { get; set; }


        public class Datum
        {
            public string Name { get; set; }
            public string Currency { get; set; }
        }


    }
}
