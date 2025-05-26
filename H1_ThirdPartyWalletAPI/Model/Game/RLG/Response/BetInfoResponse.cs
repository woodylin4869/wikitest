namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 注單資訊
    /// </summary>
    public class BetInfoResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗，可參詳 ErrorMessage 欄位內容
        /// </summary>
        public string errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }

        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public BetInfoResponseData data { get; set; }

        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public class BetInfoResponseData
        {
            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string userid { get; set; }
            /// <summary>
            /// 注單編號
            /// </summary>
            public string ordernumber { get; set; }
            /// <summary>
            /// 期數
            /// </summary>
            public string numberofperiod { get; set; }
            /// <summary>
            /// 彩別代號，詳見 I.3 彩別代號
            /// </summary>
            public string gamecode { get; set; }
            /// <summary>
            /// 彩別名稱，詳見 I.3 彩別代號
            /// </summary>
            public string gamename { get; set; }
            /// <summary>
            /// 彩別群組代號，詳見 I.5 彩別群組代號
            /// </summary>
            public string gamegroupcode { get; set; }
            /// <summary>
            /// 彩別群組名稱，詳見 I.5 彩別群組代號
            /// </summary>
            public string gamegroupname { get; set; }
            /// <summary>
            /// 注數
            /// </summary>
            public string betnumber { get; set; }
            /// <summary>
            /// 賠率
            /// </summary>
            public double odds { get; set; }
            /// <summary>
            /// 下注內容
            /// </summary>
            public string content { get; set; }
            /// <summary>
            /// 投注金額
            /// </summary>
            public string totalamount { get; set; }
            /// <summary>
            /// 玩法名稱
            /// </summary>
            public string gameplayname { get; set; }
            /// <summary>
            /// 投注時間
            /// </summary>
            public string createtime { get; set; }
            /// <summary>
            /// 注單狀態(0:未结算，1:已结算，2:取消，3:删除)
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// 中奖金额
            /// </summary>
            public string bettingbalance { get; set; }
            /// <summary>
            /// 退水金额
            /// </summary>
            public string totalkickback { get; set; }
            /// <summary>
            /// 开奖结果
            /// </summary>
            public string result { get; set; }
            /// <summary>
            /// 中奖状态(0:输，1：平，2：赢)
            /// </summary>
            public string winningstatus { get; set; }
            /// <summary>
            /// 是否有重新開獎過(0:無、1:有)
            /// </summary>
            public string isadjust { get; set; }

            public string gameplaycode { get; set; }
            public string contentcode { get; set; }
        }
    }

    public class BetInfourlResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗，可參詳 ErrorMessage 欄位內容
        /// </summary>
        public string errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }
        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public DataInfo data { get; set; }

        public class DataInfo
        {
            /// <summary>
            /// token key
            /// </summary>
            public string urltoken { get; set; }
            /// <summary>
            /// 遊戲平台網址
            /// </summary>
            public string url { get; set; }
        }
    }
}
