using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class ForwardGameReq
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public string Club_id { get; set; }
        /// <summary>
        /// 遊戲平台名
        /// 1. SABA
        /// </summary>
        [Required]
        [StringLength(10)]
        [DefaultValue("SABA")]
        public string Platform { get; set; }
        /// <summary>
        /// 遊戲名稱
        /// </summary>
        [StringLength(20)]
        [DefaultValue("SPORT")]
        public string Game_Name { get; set; }
        /// <summary>
        /// 停利額度
        /// </summary>
        [DefaultValue(-1)]
        public decimal? Stop_balance { get; set; }
        /// <summary>
        /// 其他遊戲參數
        ///  "gameConfig": {"oddstype": "Malay_Odds","device" : "DESKTOP","lang" : "zh-TW"}
        /// </summary>
        public Dictionary <string, string> GameConfig { get; set; } 
        
        public ForwardGameReq()
        {
            GameConfig = new Dictionary<string, string>();
        }
    }

    public class ForwardGame : ResCodeBase
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public string Club_id { get; set; }
        /// <summary>
        /// 開啟遊戲連結
        /// </summary>
        public string Url { get; set; }
    }
}
