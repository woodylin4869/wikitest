using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class WithdrawResponse
    {
        /// <summary>
        /// 狀態
        /// 0 成功 
        /// 1 用户ID无效 
        /// 2 Host ID 无效 
        /// 3 单号无效
        /// 5 系统错误
        /// 7 总金额为零
        /// 9 钱包设置错误
        /// 10 无效钱包类型
        /// 12 重复储值（1秒） 
        /// </summary>
        public int status_code { get; set; }
        /// <summary>
        /// 餘額
        /// </summary>
        public decimal balance { get; set; }
        /// <summary>
        /// 其他訊息
        /// </summary>
        public string message { get; set; }
    }
}
