namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class ChkTransInfoRequest
    {
        /// <summary>
        /// 會員名稱
        /// </summary>
        public string memname { get; set; }

        /// <summary>
        /// 0是使用我方存提款記錄 recid 1是貴方提供存提款記錄 payno0是使用我方存提款記錄 recid 1是貴方提供存提款記錄 payno
        /// </summary>
        public string transidtype { get; set; }
        /// <summary>
        /// 需要查詢的id         (流水號)
        /// </summary>
        public string transid { get; set; }
        public string token { get; set; }
        public long timestamp { get; set; }
    }
}