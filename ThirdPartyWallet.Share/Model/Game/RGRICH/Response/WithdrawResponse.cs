using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Response
{
    public class WithdrawResponse
    {
        /// <summary>
        /// 廠商流水號
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 提現額度
        /// </summary>
        public decimal Balance { get; set; }
    }
}