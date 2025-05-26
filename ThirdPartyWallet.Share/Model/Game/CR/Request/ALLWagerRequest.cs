namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class ALLWagerRequest : DataRequestBase
    {
        /// <summary>
        /// 搜尋日期時間 開始 
        /// </summary>
        public DateTime dateStart { get; set; }

        /// <summary>
        /// 搜尋日期時間 結束
        /// </summary>
        public DateTime dateEnd { get; set; }

        /// <summary>
        /// 是否結算:   0 為未結算， 1 為已結算
        /// </summary>
        public int settle { get; set; }

        /// <summary>
        ///  頁次1為第一頁,一頁最多50筆
        /// </summary>
        public int page { get; set; }


        /// <summary>
        ///語系 詳見4.4
        /// </summary>
        public string langx { get; set; }
    }
}