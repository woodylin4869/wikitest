using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;

public class GameListResponse : ResponseBase
{
    /// <summary>
    /// 資料總筆數
    /// </summary>
    public int totalCount { get; set; }
        public Datum[] data { get; set; }

    public class Datum
    {
        /// <summary>
        /// 桌號ID
        /// </summary>
        public string gameID { get; set; }
        /// <summary>
        /// 遊戲維護狀態
        /// </summary>
        public bool isMaintain { get; set; }
        /// <summary>
        /// 遊戲類型
        /// </summary>
        public string gameType { get; set; }
        /// <summary>
        /// 簡體中文
        /// </summary>
        public string cn { get; set; }
        /// <summary>
        /// 英文
        /// </summary>
        public string en { get; set; }
        /// <summary>
        /// 繁體中文
        /// </summary>
        public string zh { get; set; }
        /// <summary>
        /// 印尼文
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 泰文
        /// </summary>
        public string th { get; set; }
        /// <summary>
        /// 越南文
        /// </summary>
        public string vi { get; set; }
        /// <summary>
        /// 韓文
        /// </summary>
        public string ko { get; set; }
        /// <summary>
        /// 日文
        /// </summary>
        public string ja { get; set; }
        /// <summary>
        /// 葡萄牙文
        /// </summary>
        public string pt { get; set; }
    }

}
