using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 會員點數交易分頁列表
    /// </summary>
    public class TransferRecordRequest
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; }
        /// <summary>
        /// 站台代碼，即代理唯一識別碼 ID
        /// </summary>
        public string WebId { get; set; }
        /// <summary>
        /// 玩家的唯一識別碼 可空值
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// 交易編號可空值,[ TransferNo ]不為空值時，將忽略 [ StartTime ] 及 [ EndTime ] 傳入的值，單純以[TransferNo] 參數做查詢
        /// </summary>
        public string? TransferNo { get; set; }
        /// <summary>
        /// 開始時間,格式:”2018-01-01 02:00:00”
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 結束時間,格式:”2018-01-01 02:00:00”，目前結束時間與起始時間不能相差超過 24 小時
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 起始页，正整数，从 1 开始计数

        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 每页筆数，正整数，最大值 100
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }
    }
}
