namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 轉帳
    /// </summary>
    public class TransferResponse : BaseResponse
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
            /// 交易編號，需全域唯一(global unique)，限英數字，長度 4 ~ 40 字
            /// </summary>
            public string TransactionId { get; set; }

            /// <summary>
            /// 交易狀態，1 = 成功，2 = 失敗，9 = 處理中
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// 轉帳後額度
            /// </summary>
            public decimal Balance { get; set; }
        }
    }
}
