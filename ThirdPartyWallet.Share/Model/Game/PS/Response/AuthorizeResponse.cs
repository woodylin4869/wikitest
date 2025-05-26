using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.PS.Response
{
    public class AuthorizeResponse
    {
        /// <summary>
        /// 狀態  
        /// </summary>
        public int status_code { get; set; }
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string member_id { get; set; }
        /// <summary>
        /// 帳戶餘額
        /// </summary>
        //public int balance { get; set; }
    }
}
