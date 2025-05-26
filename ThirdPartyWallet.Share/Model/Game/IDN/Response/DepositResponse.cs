namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class DepositResponse
    {
        public Deposit deposit { get; set; }
    }

    public class Deposit
    {
        public long deposit_user_id { get; set; }

        /// <summary>
        /// 充值額度
        /// </summary>
        public decimal deposit_amount { get; set; }
        public int deposit_date { get; set; }
        public string deposit_param { get; set; }
        public int deposit_payment_id { get; set; }
        public int whitelabel_id { get; set; }
        public int deposit_currency_id { get; set; }
        public int deposit_is_mobile { get; set; }
        public int deposit_count { get; set; }

        /// <summary>
        /// 交易狀態
        /// </summary>
        public int deposit_status { get; set; }
        public object deposit_external_id { get; set; }
        public object deposit_external_id2 { get; set; }

        /// <summary>
        /// orderID
        /// </summary>
        public string deposit_external_id3 { get; set; }
        public object deposit_expire_at { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }

        /// <summary>
        /// 廠商流水號
        /// </summary>
        public long deposit_id { get; set; }
    }

}