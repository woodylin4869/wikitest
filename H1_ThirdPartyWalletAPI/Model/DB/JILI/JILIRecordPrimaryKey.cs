using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.JILI
{
    public class JILIRecordPrimaryKey
    {
        /// <summary>
        /// 會員唯一識別值
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 在遊戲內注單唯一值
        /// </summary>
        public long WagersId { get; set; }

        /// <summary>
        /// 投注時間
        /// </summary>
        public DateTime WagersTime { get; set; }
    }
}
