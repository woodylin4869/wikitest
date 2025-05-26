using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Request
{
    public class BetlistRequest
    {
        public BetlistRequest()
        {
            status = new string[] { "Settled", "Cancelled", "Unsettlement", "Abnormal", "Rollback" };
        }


        public string seq { get; set; }
        public string supplier { get; set; }
        public string product_id { get; set; }
        /// <summary>
        /// 時間過濾，僅接受Create與Reckon，預設為投注時間
        /// Create：投注時間，代表 API 參數的begintime/endtime 會以注單投注時間搜尋區間
        /// Reckon：結算時間，代表 API 參數的begintime/endtime 會以注單結算時間搜尋區間
        /// </summary>
        public string timetype { get; set; }
        public long begintime { get; set; }
        public long endtime { get; set; }
        public int page { get; set; }
        public int num { get; set; }

        public string[] status { get; set; }
    }
}
