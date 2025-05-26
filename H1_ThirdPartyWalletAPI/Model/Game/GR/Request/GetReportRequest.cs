using Microsoft.IdentityModel.Tokens;
using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    /// <summary>
    /// 0023 - 平台取每日報表
    /// </summary>
    public class GetReportRequest
    {
        /// <summary>
        /// 開始時間
        /// </summary> 
        public string start_date {  get; set; }
        /// <summary>
        /// 結束時間
        /// </summary>
        public string end_date { get; set; }
        /// <summary>
        /// 現在所在分頁
        /// </summary>
        public int page_index { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int page_size { get; set; }
    }
}
