using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class GetBetRecordRequest
    {
        /// <summary>
        /// 依照結算時間查詢注單
        /// </summary>
        public FinishTime finish_time { get; set; }
        /// <summary>
        /// 開始頁碼 默認: 0
        /// </summary>
        public int index { get; set; }
        /// <summary>
        /// 查詢上限 默認: 1000 最大: 5000
        /// </summary>
        public int limit { get; set; }
        public GetBetRecordRequest()
        {
            finish_time = new FinishTime();
        }
    }
    public class FinishTime
    {
        /// <summary>
        /// 開始日期 ex: 2018-10-19T08:54:14+01:00
        /// </summary>
        public DateTime start_time { get; set; }
        /// <summary>
        /// 結束日期 ex: 2018-10-20T11:47:08+01:00
        /// </summary>
        public DateTime end_time { get; set; }
    }

}
