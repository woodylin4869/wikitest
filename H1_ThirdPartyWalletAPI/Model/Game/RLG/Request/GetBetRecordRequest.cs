using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Request
{
    /// <summary>
    /// 會員投注紀錄分頁列表
    /// </summary>
    public class GetBetRecordRequest
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
        /// 彩別代號，詳見 I.3 彩別代號可空值
        /// </summary>
        public string? GameId { get; set; }
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
        /// 每页筆数，正整数，最大值 1000
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 0:全部(投注時間)、1:未結算(投注時間)、2:已結算(實際開獎時間),SetOption參數帶入2時，為查詢已結算的注單，是以實際開獎時間為主，建議開始時間設當前時間1小時之前；建議拉取區間為 10 分鐘，最大不能超過 1 天。
        /// </summary>
        public int SetOption { get; set; }
        /// <summary>
        /// 語系
        /// </summary>
        public string Language { get; set; }
    }
}
