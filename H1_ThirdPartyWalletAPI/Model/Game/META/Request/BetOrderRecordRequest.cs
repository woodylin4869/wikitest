namespace H1_ThirdPartyWalletAPI.Model.Game.META.Request
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class BetOrderRecordRequest
    {
        /// <summary>
        /// 查詢日期 Y 時間戳(Unix timestamp)
        /// </summary>
        public long Date { get; set; }

        /// <summary>
        /// 會員帳號 N 多帳號請用逗點分開
        /// </summary>
        public string? Account { get; set; }

        /// <summary>
        ///  筆數 N 預設1000
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        ///  流水序號 N 最後一筆流水序號，如沒填寫從0查詢
        /// </summary>
        public long? LastSerial { get; set; }


        private int? collect = 1;
        /// <summary>
        ///  是否已對獎 N 填寫1時只取得已對獎的單
        /// </summary>
        public int? Collect { get => collect; set => collect = value; }
    }
}