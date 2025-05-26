using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;
public class BetlimitResponse : ResponseBase
{
    /// <summary>
    /// 幣別
    /// </summary>
    public string currency { get; set; }
    /// <summary>
    /// 預設限紅組別
    /// </summary>
    public Betlimit[] betLimit { get; set; }
    public class Betlimit
    {
        /// <summary>
        /// 限紅名稱
        /// </summary>
        public string groupName { get; set; }
        /// <summary>
        /// 最小籌碼
        /// </summary>
        public int min { get; set; }
        /// <summary>
        /// 最大籌碼
        /// </summary>
        public int max { get; set; }
        /// <summary>
        /// 籌碼組
        /// </summary>
        public int[] chips { get; set; }
    }

}
