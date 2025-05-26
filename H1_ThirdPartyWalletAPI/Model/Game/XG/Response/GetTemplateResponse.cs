namespace H1_ThirdPartyWalletAPI.Model.Game.XG.Response
{
    /// <summary>
    /// 取得會員限注
    /// </summary>
    public class GetTemplateResponse : BaseResponse
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

            /// <summary>
            /// 限紅 ID，多個用逗號隔，可從後台遊戲設定或 GetAgentTemplate api 取得限紅 ID
            /// example: 51,52,53
            /// </summary>
            public string Template { get; set; }
        }
    }
}
