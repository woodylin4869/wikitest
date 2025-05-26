namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 查詢玩家投注量統計
/// </summary>
public class PlayerBettingStatisticsResponse
{
    /// <summary>
    /// 娛樂城代碼
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 玩家帳號
    /// </summary>
    public string player_account { get; set; }

    /// <summary>
    /// 總投注數
    /// </summary>
    public decimal bet_amount { get; set; }

    /// <summary>
    /// 總有效投注數
    /// </summary>
    public decimal bet_value { get; set; }

    /// <summary>
    /// 注單數
    /// </summary>
    public decimal bet_count { get; set; }

    /// <summary>
    /// 總派彩
    /// </summary>
    public decimal bet_result { get; set; }

    /// <summary>
    /// 贏的注單數
    /// </summary>
    public decimal win_count { get; set; }

    /// <summary>
    /// 贏的注單派彩
    /// </summary>
    public decimal win_point { get; set; }
}
