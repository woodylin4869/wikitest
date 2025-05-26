namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 注單編號查詢會員下注內容
    /// </summary>
    public class GetGameDetailUrlResponse : BaseResponse
    {
        /// <summary>
        /// Data object
        /// </summary>
        //public DataInfo Data { get; set; }
        public string? Data { get; set; }

        /// <summary>
        /// 參數 data 裡的欄位資料
        /// </summary>
        public class DataInfo
        {
        }
    }
}
