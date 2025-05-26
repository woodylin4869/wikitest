using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.RCG2.DBResponse
{
    /// <summary>
    /// 查詢注單之查輪局桌別
    /// </summary>
    public class RCG2RunNoDBResponse : RCG2RecordPrimaryKey
    {
        /// <summary>
        /// 桌號
        /// </summary>
        public string desk { get; set; }

        /// <summary>
        /// 輪號
        /// </summary>
        public string activeNo { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string runNo { get; set; }

        /// <summary>
        /// 結算時間
        /// </summary>
        public DateTime reportdt { get; set; }
    }
}
