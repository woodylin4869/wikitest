namespace ThirdPartyWallet.Share.Model.Game.CR.Request
{
    public class WithdrawRequest : DataRequestBase
    {
        /// <summary>
        /// 會員名稱帳號
        /// </summary>
        public string memname { get; set; }

        /// <summary>
        /// 提款總數
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// 貴方提供存提款紀錄（流水編號）  長度40
        /// </summary>
        public string payno { get; set; }
    }

}