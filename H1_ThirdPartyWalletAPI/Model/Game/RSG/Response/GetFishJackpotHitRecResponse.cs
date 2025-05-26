using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;

public class GetFishJackpotHitRecResponse
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
        public List<JackpotHitRec> JackpotHitRec { get; set; }
    }

    public class JackpotHitRec
    {
        /// <summary>
        /// 中獎唯一識別號
        /// </summary>
        public long FishJackpotHitID { get; set; }
        /// <summary>
        /// 遊戲紀錄惟一編號
        /// </summary>
        public long SequenNumber { get; set; }
        /// <summary>
        /// 幣別代碼
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 站台代碼
        /// </summary>
        public string WebId { get; set; }
        /// <summary>
        /// 會員惟一識別碼
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 遊戲代碼(請參照代碼表)
        /// </summary>
        public int GameId { get; set; }
        /// <summary>
        /// 中獎 Jackpot 類型(請參照補充說明)
        /// Jackpot Type: 0: GRAND, 1: MAJOR, 2: MINOR, 3: MINI
        /// </summary>
        public int FishJackpotType { get; set; }
        /// <summary>
        /// 彩金(小數點兩位)
        /// </summary>
        public decimal JackpotWin { get; set; }
        /// <summary>
        /// 中獎時間
        /// </summary>
        public string HitTime { get; set; }
    }
}
