namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Response
{
    public class GetUrlResponse : GRResponseBase
    {
        public DataInfo data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
            public string url { get; set; }
        }
    }
}
