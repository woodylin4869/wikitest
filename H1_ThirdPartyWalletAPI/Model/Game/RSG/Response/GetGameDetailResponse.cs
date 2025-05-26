using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response
{
    /// <summary>
    /// 取得遊戲詳細資訊
    /// </summary>
    public class GetGameDetailResponse
    {
        /// <summary>
        /// 錯誤代碼
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public long Timestamp { get; set; }
        /// <summary>
        /// API 呼叫回傳的 JSON 格式的 object / object array
        /// </summary>
        public DataInfo Data { get; set; }

        public class DataInfo
        {
            public List<GameDetail> GameDetail { get; set; }
        }


    }
    public class GameDetail
    {
        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime report_time { get; set; }
        /// <summary>
        /// 幣別代碼
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 站台代碼
        /// </summary>
        public string webid { get; set; }
        /// <summary>
        /// 會員惟一識別碼
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 遊戲紀錄惟一編號
        /// </summary>
        public long sequennumber { get; set; }
        /// <summary>
        /// 遊戲代碼(請參照代碼表)
        /// </summary>
        public int gameid { get; set; }
        /// <summary>
        /// 子遊戲代碼(請參照代碼表)
        /// </summary>
        public int subgametype { get; set; }
        /// <summary>
        /// 下注(小數點兩位)
        /// </summary>
        public decimal betamt { get; set; }
        /// <summary>
        /// 贏分(小數點兩位)
        /// </summary>
        public decimal winamt { get; set; }
        /// <summary>
        /// 遊戲時間
        /// </summary>
        public DateTime playtime { get; set; }
        /// <summary>
        /// Jackpot 貢獻值(小數點五位)
        /// </summary>
        public decimal jackpotcontribution { get; set; }
        /// <summary>
        /// Jackpot 贏分(小數點五位)
        /// </summary>
        public decimal jackpotwin { get; set; }
    }
}