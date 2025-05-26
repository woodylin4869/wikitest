namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class CheckAGReportResponse : ApiResponseBase
    {
        /// <summary>
        /// 查詢區間的總有效金額 
        /// </summary>
        public string totalvgold { get; set; }

        /// <summary>
        /// 查詢區間的總筆數 
        /// </summary>
        public string totalcount { get; set; }

        /// <summary>
        /// 查詢區間的總下注金額 
        /// </summary>
        public string totalgold { get; set; }

        /// <summary>
        /// 查詢區間的總輸贏金額 
        /// </summary>

        public string totalwingold { get; set; }

    }


}