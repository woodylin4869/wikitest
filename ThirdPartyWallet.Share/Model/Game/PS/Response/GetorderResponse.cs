using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ThirdPartyWallet.Share.Model.Game.RCG3.RCG3;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class GetorderResponse
    {
        public Dictionary<string, Dictionary<string, List<BetRecord>>> Data { get; set; }
        public class BetRecord
        {
            /// <summary>
            /// PS產生單號(注單號)
            /// </summary>
            public long sn { get; set; }
            /// <summary>
            /// 主遊戲ID
            /// </summary>
            public string gid { get; set; }
            /// <summary>
            /// 開始時間
            /// </summary>
            public DateTime s_tm { get; set; }
            /// <summary>
            /// 結束時間
            /// </summary>
            public string tm { get; set; }
            /// <summary>
            /// 下注金額
            /// </summary>
            public decimal bet { get; set; }
            /// <summary>
            /// 總贏分
            /// </summary>
            public decimal win { get; set; }

            /// <summary>
            /// 有效下注金額
            /// </summary>
            public decimal betamt { get; set; }
            /// <summary>
            /// 有效贏分
            /// </summary>
            public decimal winamt { get; set; }
            /// <summary>
            /// 免費遊戲贏分
            /// </summary>
            public decimal bn { get; set; }
            /// <summary>
            /// 遊戲類型
            /// </summary>
            public string gt { get; set; }
            /// <summary>
            /// 彩金贏分
            /// </summary>
            public decimal jp { get; set; }
            public string member_id { get; set; }
            public decimal pre_betamount { get; set; }
            public decimal pre_wonamount { get; set; }
            public decimal pre_turnover { get; set; }
            public decimal pre_winlose { get; set; }

            public string club_id { get; set; }
            public string franchiser_id { get; set; }
            public DateTime createtime { get; set; }
            public DateTime report_time { get; set; }
            /// <summary>
            /// 開始時間
            /// </summary>
            public DateTime partition_time { get; set; }
        }
    }
}
