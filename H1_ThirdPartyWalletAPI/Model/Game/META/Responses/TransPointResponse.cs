namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class TransPointResponse : GetMetaDataDecryptBase
    {
        public int totalRows { get; set; }
        public Rows rows { get; set; }

        public class Rows
        {
            /// <summary>
            /// string 50 會員帳號
            /// </summary>
            public string MemberAccount { get; set; }

            /// <summary>
            /// 10+小數 4 轉入(出)點數
            /// </summary>
            public decimal TranPoint { get; set; }

            /// <summary>
            /// 轉入(出)前點數
            /// </summary>
            public decimal BeforeChangePoint { get; set; }

            /// <summary>
            /// 轉入(出)後點數
            /// </summary>
            public decimal AfterChangePoint { get; set; }

            /// <summary>
            /// 轉入(出)時間
            /// </summary>
            public string DateTran { get; set; }

            /// <summary>
            /// 所產生之交易單號
            /// </summary>
            public long TranOrder { get; set; }
        }
    }
}
