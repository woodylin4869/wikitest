using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class GetgameRequest
    {
        /// <summary>
        /// Host ID (PS token)
        /// </summary>
        public string host_id {  get; set; }
        /// <summary>
        /// 主遊戲ID
        /// </summary>
        public string game_id { get; set; }
        /// <summary>
        /// 次遊戲ID
        /// </summary>
        public int subgame_id { get; set; }
        /// <summary>
        /// 語系
        /// </summary>
        public string lang { get; set; }
        /// <summary>
        /// 會員token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// 返回鍵的網址 (選用)
        /// </summary>
        public string return_url {  get; set; }
        /// <summary>
        /// 返回導向的窗口  (選用)
        /// </summary>
        public string return_target { get; set; }



    }
}
