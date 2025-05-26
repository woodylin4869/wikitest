using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class hoursummaryResponse
    {
        /// <summary>
        /// 遊戲局數
        /// </summary>
        public int rnd { get; set; }
        /// <summary>
        /// 總投注
        /// </summary>
        public int bet { get; set; }
        /// <summary>
        /// 總贏分  /100==1元
        /// </summary>
        public int win { get; set; }
        /// <summary>
        /// 免費遊戲贏分
        /// </summary>
        public int bn { get; set; }
        /// <summary>
        /// 比倍遊戲贏分
        /// </summary>
        public int gb { get; set; }
        /// <summary>
        /// 彩金贏分
        /// </summary>
        public int jp { get; set; }
        /// <summary>
        /// 彩金貢獻金
        /// </summary>
        public int jc { get; set; }
    }
}
