using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.RLG
{
    public class RLGRecordPrimaryKey
    {
        /// <summary>
        /// 注單編號
        /// </summary>
        public string ordernumber { get; set; }

        /// <summary>
        /// 投注時間
        /// </summary>
        public DateTime createtime { get; set; }

        /// <summary>
        /// 開獎時間(未開獎時為關盤時間)
        /// </summary>
        public DateTime drawtime { get; set; }
    }
}
