namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    public class GetBetTotalListResponse
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
        /// 以 JSON 表示的 object
        /// </summary>
        public GetBetTotalData data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }


        public class GetBetTotalData
        {
            /// <summary>
            /// 系統代碼
            /// </summary>
            public string systemcode { get; set; }
            /// <summary>
            /// 站台代碼，即代理唯一識別碼 ID
            /// </summary>
            public string webid { get; set; }
            /// <summary>
            /// 注單資料
            /// </summary>
            public Datalist[] datalist { get; set; }
        }

        public class Datalist
        {
            /// <summary>
            /// 彩別代號，詳見 I.3 彩別代號
            /// </summary>
            public string gamecode { get; set; }
            /// <summary>
            /// 彩別群組代號，詳見 I.5 彩別群組代號
            /// </summary>
            public string gamegroupcode { get; set; }
            /// <summary>
            /// 下注總筆數
            /// </summary>
            public int totalbet { get; set; }
            /// <summary>
            /// 下注金額總計
            /// </summary>
            public decimal bettotalmoney { get; set; }
            /// <summary>
            /// 中獎金額總計
            /// </summary>
            public decimal wintotalmoney { get; set; }
            /// <summary>
            /// 輸贏總計
            /// </summary>
            public decimal winlosemoney { get; set; }
        }

    }
}
