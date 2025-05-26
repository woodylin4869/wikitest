namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class DepositResponse : ApiResponseBase
    {
        public Moneydata moneydata { get; set; }
    }

    public class Moneydata
    {
        /// <summary>
        /// 餘額
        /// </summary>
        public string gold { get; set; }

        /// <summary>
        /// 貴方提供存提款紀錄（流水編號）
        /// </summary>
        public string payno { get; set; }
        /// <summary>
        /// 金額
        /// </summary>
        public string pay_gold { get; set; }

        /// <summary>
        /// 幣別 詳見4.7
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 支付方式  1存款 -1提款
        /// </summary>
        public string pay_way { get; set; }

        /// <summary>
        /// 提供存提款紀錄（流水編號）
        /// </summary>
        public string recid { get; set; }

        /// <summary>
        /// 會員名稱 
        /// </summary>
        public string username { get; set; }
    }

}