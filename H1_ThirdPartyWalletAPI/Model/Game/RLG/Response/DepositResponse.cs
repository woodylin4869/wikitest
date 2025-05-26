namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// Deposit
    /// </summary>
    public class DepositResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗
        /// </summary>
        public int errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public DepositResponseData data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public class DepositResponseData
        {
            /// <summary>
            /// 會員目前額度
            /// </summary>
            public string balance { get; set; }
            /// <summary>
            /// 交易編號
            /// </summary>
            public string transferno { get; set; }
        }
    }
}
