using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Common.Response
{
    /// <summary>
    /// 第二層明細(W1五分鐘彙總帳)_體育_未結算
    /// </summary>
    public class RespRecordLevel2_Sport_Unsettle : RespRecordLevel2_Sport
    {
        /// <summary>
        /// Club_id (running表)
        /// </summary>
        public string Club_id { get; set; }

        /// <summary>
        /// Franchiser_id (running表)
        /// </summary>
        public string Franchiser_id { get; set; }

    }
}