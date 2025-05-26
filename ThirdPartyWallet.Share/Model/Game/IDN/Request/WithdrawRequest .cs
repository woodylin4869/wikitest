namespace ThirdPartyWallet.Share.Model.Game.IDN.Request
{
    public class WithdrawRequest
    {
        /// <summary>
        /// 提現金額（正數）
        /// </summary>
        public decimal amount { get; set; }
        public int payment_id { get; set; }

        /// <summary>
        /// 流水號（站點生成）
        /// </summary>
        public string order_id { get; set; }
        public string domain { get; set; }
        public string platform { get; set; }
        public string user_agent { get; set; }
        public string device { get; set; }
        public int is_mobile { get; set; }
    }


}