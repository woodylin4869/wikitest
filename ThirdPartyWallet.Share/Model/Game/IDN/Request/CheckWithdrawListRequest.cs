namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class CheckWithdrawListRequest
    {
        /// <summary>
        /// start time (format Y-m-d hh:mm:ss)
        /// 2022-05-05 15:38:08.000
        /// </summary>
        public string from { get; set; }
        /// <summary>
        ///end time (format Y-m-d hh:mm:ss)
        /// 2022-05-05 15:38:08.000
        /// </summary>
        public string to { get; set; }

        /// <summary>
        /// 分頁每頁筆數
        /// </summary>
        public int paginate { get; set; }
    }
}