using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Response;
public class TransferResponse:ResponseBase
{
    /// <summary>
    /// 資料筆數
    /// </summary>
    public int dataCount { get; set; }
    public Datum[] data { get; set; }
  
    public class Datum
    {
        /// <summary>
        /// 營運商ID
        /// </summary>
        public string operatorID { get; set; }
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string playerID { get; set; }
        /// <summary>
        /// 交易編號
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 交易編號
        /// </summary>
        public string refID { get; set; }
        /// <summary>
        /// 交易類型
        /// </summary>
        public string transferType { get; set; }
        /// <summary>
        /// 時間(UNIX)
        /// </summary>
        public long transferTime { get; set; }
        /// <summary>
        /// 交易金額(單位:分)
        /// </summary>
        public decimal tranAmount { get; set; }
        /// <summary>
        /// 主機ID
        /// </summary>
        public string trackID { get; set; }
        /// <summary>
        /// 餘額
        /// </summary>
        public decimal balance { get; set; }
    }

}
