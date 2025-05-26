using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    public class GetOpenGameResponse
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
        public GetOpenGameData[] data { get; set; } = Array.Empty<GetOpenGameData>();
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }

        public class GetOpenGameData
        {
            public string gameID { get; set; }
        }
    }
}
