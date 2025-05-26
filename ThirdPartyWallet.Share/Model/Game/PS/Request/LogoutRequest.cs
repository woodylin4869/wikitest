using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class LogoutRequest
    {
        /// <summary>
        /// 會員token
        /// </summary>
        public string access_token {  get; set; }
        /// <summary>
        /// 主遊戲ID
        /// </summary>
        public string game_id { get; set; }
        /// <summary>
        /// 次遊戲ID
        /// </summary>
        public string subgame_id { get; set; }
    }
}
