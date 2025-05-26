namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Request
{
    public class WithdrawRequest
    {
        /// <summary>
        /// 玩家帳號
        /// </summary>
        public string account {  get; set; }
        /// <summary>
        /// 交易代碼需要是唯一值(允許字元:a-z A-Z 0-9 _-)
        /// </summary>
        public string transaction_id { get; set; }
        /// <summary>
        /// 交易金額最少需允許小數點後2位
        /// </summary>
        public decimal amount { get; set; }
    }
}
