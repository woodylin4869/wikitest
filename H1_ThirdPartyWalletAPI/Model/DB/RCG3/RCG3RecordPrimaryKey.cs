using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.RCG3.DBResponse
{
    public class RCG3RecordPrimaryKey
    {
        /// <summary>
        /// 注單編號
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 結算時間 格式 yyyy-mm-ddTHH:mm:ss.fff
        /// </summary>
        public DateTime reportDT { get; set; }

        /// <summary>
        /// 改單原編號：-1未修正資料 
        /// 改單前的原始注單編號
        /// </summary>
        public long originRecordId { get; set; }

        /// <summary>
        /// 改單最初編號：-1未修正資料
        /// 改單前的最初注單編號
        /// </summary>
        public long rootRecordId { get; set; }
    }
}
