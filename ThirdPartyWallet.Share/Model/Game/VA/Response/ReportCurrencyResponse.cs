namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class ReportCurrencyResponse
    {
        /// <summary>
        /// 報表數據
        /// </summary>
        public List<CurrencyData> CurrencyList { get; set; }
    }
    public class CurrencyData
    {
        /// <summary>
        /// 幣別 (請洽詢我方商務)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 總下注
        /// </summary>
        public decimal TotalBet { get; set; }

        /// <summary>
        /// 總派彩
        /// </summary>
        public decimal TotalPayout { get; set; }

        /// <summary>
        /// 總輸贏
        /// </summary>
        public decimal TotalWinLose { get; set; }

        /// <summary>
        /// 筆數
        /// </summary>
        public int Count { get; set; }
    }
}
