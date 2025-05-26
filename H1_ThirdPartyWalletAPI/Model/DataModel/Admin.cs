using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class Admin
    {
        /// <summary>
        /// Admin account
        /// </summary>
        public string User_Account { get; set; }
        /// <summary>
        /// Admin password
        /// </summary>
        public string User_Password { get; set; }
        /// <summary>
        /// Admin role
        /// </summary>
        public string Role { get; set; }
    }
}
