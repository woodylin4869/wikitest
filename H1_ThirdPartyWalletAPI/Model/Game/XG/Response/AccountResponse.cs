namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 取得會員資料
    /// </summary>
    public class AccountResponse : BaseResponse
    {
        /// <summary>
        /// Data object
        /// </summary>
        public DataInfo Data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            /// <summary>
            /// 會員帳號，限英數字及_線，長度4~30字
            /// </summary>
            public string Account { get; set; }

            /// <summary>
            /// 額度
            /// </summary>
            public decimal Balance { get; set; }

            /// <summary>
            /// 會員狀態
            /// 3	正常
            /// 2	停止下注
            /// -2	停用(無法登入，已登入會踢出)
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// Online
            /// true / false
            /// </summary>
            public bool Online { get; set; }
        }
    }
}
