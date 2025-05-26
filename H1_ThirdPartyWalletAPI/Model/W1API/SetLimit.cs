using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class SetLimitReq
    {
        /// <summary>
        /// 使用者帳號id
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
        /// 設定額度
        ///   "bet_setting": [
        ///   {"sport_type": "1","min_bet": 1,"max_bet": 10,"max_bet_per_match": 10}
        ///   ,{"sport_type": "2","min_bet": 1,"max_bet": 10,"max_bet_per_match": 10}
        ///   ]
        ///</summary>
        [Required]
        [DefaultValue(0)]
        public object bet_setting { get; set; }
    }
}
