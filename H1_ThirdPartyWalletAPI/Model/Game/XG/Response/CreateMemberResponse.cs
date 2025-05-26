namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 創建會員帳號
    /// </summary>
    public class CreateMemberResponse : BaseResponse
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
            /// 會員帳號，限英數字及_線，長度4~30字
            /// </summary>
            public string Account { get; set; }
        }
    }
}
