using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;

/// <summary>
/// 取得 Jackpot 目前 Pool 值 
/// </summary>
public class GetJackpotPoolValueResponse
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
        public List<JackpotPool> JackpotPool { get; set; }
    }

    public class JackpotPool
    {
        /// <summary>
        /// Jackpot 類型
        /// Type: 0, Name: GRAND
        /// Type: 1, Name: MAJOR
        /// Type: 2, Name: MINOR
        /// Type: 3, Name: MINI
        /// </summary>
        public string JackpotType { get; set; }
        /// <summary>
        /// 彩金目前累積值(小數點兩位)
        /// </summary>
        public string JackpotPoolAmt { get; set; }
    }
}