using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Response
{
    public class BetlistResponse
    {

        public string seq { get; set; }
        public long timestamp { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public Data data { get; set; }

        public class Data
        {
            

            public int total { get; set; }

            public decimal? totalbet { get; set; }


            public decimal? totalwin { get; set; }
            public Datalist[] datalist { get; set; }
        }

        public class Datalist
        {
            public string username { get; set; }
            public string billNo { get; set; }
            public string billstatus { get; set; }
            public string grouptype { get; set; }
            public string gametype { get; set; }
            public string gamecode { get; set; }
            public long createtime { get; set; }
            public long reckontime { get; set; }
            public string PlayType { get; set; }
            public string currency { get; set; }

            public decimal betamount { get; set; }

            public decimal wonamount { get; set; }

            public decimal turnover { get; set; }

            public decimal winLose { get; set; }

            public decimal pre_betamount { get; set; }
            public decimal pre_wonamount { get; set; }
            public decimal pre_turnover { get; set; }
            public decimal pre_winLose { get; set; }

            public string club_id { get; set; }
            public string franchiser_id { get; set; }

            public DateTime report_time { get; set; }
        }


        public class W1Datalist
        {
            public string username { get; set; }
            public string billNo { get; set; }
            public string billstatus { get; set; }
            public string grouptype { get; set; }
            public string gametype { get; set; }
            public string gamecode { get; set; }
            public DateTime createtime { get; set; }
            public DateTime reckontime { get; set; }
            public string PlayType { get; set; }
            public string currency { get; set; }
            public decimal betamount { get; set; }
            public decimal wonamount { get; set; }
            public decimal turnover { get; set; }
            public decimal winLose { get; set; }

            public decimal pre_betamount { get; set; }
            public decimal pre_wonamount { get; set; }
            public decimal pre_turnover { get; set; }
            public decimal pre_winLose { get; set; }

            public string club_id { get; set; }
            public string franchiser_id { get; set; }

            public DateTime report_time { get; set; }
        }

    }
}
