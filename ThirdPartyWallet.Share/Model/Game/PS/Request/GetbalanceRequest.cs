using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Request
{
    public class GetbalanceRequest
    {
        /// <summary>
        /// host id 
        /// </summary>
        public string host_id {  get; set; }
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string member_id { get; set; }
        /// <summary>
        /// 錢包類型 
        /// 0 一般（默设值） 
        /// 1 捕鱼机（仅适用于共享钱包） 
        /// 3 拉密/拉密2
        /// 4 决战52张
        /// 5 U4
        /// 7 color game
        /// 8 Tongits 
        /// 129 博八博九
        /// 130 总钱包 
        /// </summary>
        public int purpose {  get; set; }
    }
}
