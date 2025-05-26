using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲詳細資訊
    /// </summary>
    public class GetGameHourReportResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// API 呼叫回傳的 JSON 格式的 object / object array
        /// </summary>
        public ReportInfo Data { get; set; }
    }
    public class ReportInfo
    {
        public List<ReportData> GameReport { get; set; }
    }

    public class ReportData
    {
        /// <summary>
        /// 幣別代碼
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 站台代碼
        /// </summary>
        public string webid { get; set; }
        /// <summary>
        /// 會員惟一識別碼
        /// </summary>
        public string userid { get; set; }
        /// 遊戲代碼(請參照代碼表)
        /// </summary>
        public int gameid { get; set; }
        /// <summary>
        /// 下注(小數點兩位)
        /// </summary>
        public decimal betsum { get; set; }
        /// <summary>
        /// 贏分(小數點兩位)
        /// </summary>
        public decimal winsum { get; set; }
        /// <summary>
        ///彩金(小數點兩位
        /// </summary>
        public decimal jackpotwinsum { get; set; }
        /// <summary>
        ///總輸贏(小數點兩位)
        /// </summary>
        public decimal netwinsum { get; set; }
        /// <summary>
        ///筆數
        /// </summary>
        public int recordcount { get; set; }
        /// <summary>
        ///報表時間
        /// </summary>
        public DateTime create_datetime { get; set; }
    }
}