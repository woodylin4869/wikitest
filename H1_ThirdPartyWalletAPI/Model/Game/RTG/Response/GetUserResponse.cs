namespace H1_ThirdPartyWalletAPI.Model.Game.RTG.Response
{
    /// <summary>
    /// 查詢點數
    /// </summary>
    public class GetUserResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int MsgID { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// API 呼叫回傳的 JSON 格式的 object / object array
        /// </summary>
        public DataInfo Data { get; set; }

        public class DataInfo
        {
            /// <summary>
            /// 玩家暱稱
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 當前玩家餘額
            /// </summary>
            public decimal UserBalance { get; set; }
            /// <summary>
            /// 玩家狀態 1=離線 2=在線
            /// </summary>
            public int UserStatus { get; set; }
            /// <summary>
            /// 玩家當前所在遊戲，對應各遊戲ID，不在線則為 0
            /// </summary>
            public int UserGame { get; set; }
            /// <summary>
            /// 玩家權限狀態
            ///  1=正常
            ///  2=停用
            ///  3=狀態異常(玩家於遊戲中資料驗證有誤，暫時停權於查證錯誤後恢復
            /// </summary>
            public int UserAuth { get; set; }
            /// <summary>
            /// 幣別
            /// </summary>
            public string Currency { get; set; }
        }
    }
}