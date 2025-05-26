using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace H1_ThirdPartyWalletAPI.Model
{
    public class LoginReq
    {
        /// <summary>
        /// Admin account
        /// </summary>
        public string UserAccount { get; set; }
        /// <summary>
        /// Admin password
        /// </summary>
        public string UserPassword { get; set; }
    }
    public class Login : ResCodeBase
    {
        /// <summary>
        /// jwt token
        /// </summary>
        public string token { get; set; }
    }
}
