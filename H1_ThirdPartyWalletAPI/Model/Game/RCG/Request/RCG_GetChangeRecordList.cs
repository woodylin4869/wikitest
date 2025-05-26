using System;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    /// <summary>
    /// 改單紀錄 /api/Record/GetChangeRecordList
    /// </summary>
    public class RCG_GetChangeRecordList
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string systemCode { get; set; }

        /// <summary>
        /// 站台代碼
        /// </summary>
        public string webId { get; set; }

        /// <summary>
        /// 已處理最大流水號
        /// </summary>
        public long maxId { get; set; }

        /// <summary>
        /// 預設為100筆, 上限為2000筆
        /// </summary>
        public long rows { get; set; }
    }
}
