using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class CreateuserResponse
    {
        /// <summary>
        /// 狀態
        /// 0  成功
        /// 2  host ID無效
        /// 5  系統錯誤
        /// 9  錢包設置錯誤
        /// 10  無效錢包類型
        /// </summary>
        public int status_code {  get; set; }
        /// <summary>
        /// 帳戶餘額
        /// </summary>
        public decimal balance { get; set; }
        /// <summary>
        /// 其他訊息
        /// </summary>
        public string message { get; set; }


    }
}
