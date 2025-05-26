using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Gemini.Response
{
    public class GameListResponse
    {
        public string seq { get; set; }
        public long timestamp { get; set; }
        public int code { get; set; }
        public string message { get; set; }

        public GameListData data { get; set; }

        public class GameListData
        {
            /// <summary>
            /// 遊戲類型列表
            /// </summary>
            public string[] gametype { get; set; }
        }
    }
}
