namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 取得單筆轉帳資料
    /// </summary>
    public class CheckTransferResponse : BaseResponse
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
            /// 交易時間(UTC-4)
            /// example: 2020-02-10T00:30:30
            /// </summary>
            public string TransferTime { get; set; }

            /// <summary>
            /// 轉帳金額，需大於 0，可到小數點後兩位
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// 轉帳類型，1 = 轉出，2 = 轉入
            /// </summary>
            public int TransferType { get; set; }

            /// <summary>
            /// 交易狀態，1 = 成功，2 = 失敗，9 = 處理中
            /// </summary>
            public int Status { get; set; }

            /// <summary>
            /// 轉帳後額度
            /// </summary>
            public decimal Balance { get; set; }

            /// <summary>
            /// 幣別 幣別需該代理有啟用才能使用
            /// </summary>
            public string Currency { get; set; }
        }
    }
}
