namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class CheckAGReportRequest
    {
        /// <summary>
        /// 搜尋日期時間 開始 
        /// </summary>
        public DateTime datestart { get; set; }

        /// <summary>
        /// 搜尋日期時間 結束
        /// </summary>
        public DateTime dateend { get; set; }

        /// <summary>
        /// 是否結算:   0 為未結算， 1 為已結算
        /// </summary>
        public string settle { get; set; }


        public string timestamp { get; set; }
        public string token { get; set; }
    }
}