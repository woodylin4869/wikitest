using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.EGSlot.Response
{
    public class GethourdataResponse
    {


        public List<Datum> Data { get; set; }


        public class Datum
        {
            public string TotalBet { get; set; }
            public string TotalWin { get; set; }
            public int MainTxCount { get; set; }
            public string Currency { get; set; }
            public string GameID { get; set; }
        }

    }
}
