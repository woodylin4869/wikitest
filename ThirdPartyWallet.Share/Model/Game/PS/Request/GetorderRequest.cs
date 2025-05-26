using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class GetorderRequest
    {
        /// <summary>
        /// hostid(PS提供)
        /// </summary>
        public string host_id {  get; set; }
        /// <summary>
        /// 起始時間  格是 yyyy-MM-ddTHH:mm:ss
        /// </summary>
        public DateTime start_dtm {  get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime end_dtm { get; set;}
        /// <summary>
        /// 明細類型
        /// </summary>
        public int detail_type { get; set; }
        /// <summary>
        /// 遊戲類型
        /// </summary>
        public string game_type {  get; set; }
    }
}
