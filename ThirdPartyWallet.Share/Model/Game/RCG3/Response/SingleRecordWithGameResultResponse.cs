namespace ThirdPartyWallet.Share.Model.Game.RCG3.Response
{
    /// <summary>
    /// W3取得注單資訊With開牌(SingleRecord/WithGameResult)
    /// 此方法沒有文件 而是使用以下網址測試
    /// https://api2tool.bacc55.com/W3/GetSingleBetRecordWithGameResult
    /// </summary>
    public class SingleRecordWithGameResultResponse
    {
        public int msgId { get; set; }
        public string message { get; set; }
        public int timestamp { get; set; }
        public string data { get; set; }
    }
}
