namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 查詢投注量統計
/// </summary>
public class StatisticsResponse
{
    /// <summary>
    /// 幣別
    /// </summary>
    public string currency { get; set; }

    /// <summary>
    /// 總注單數
    /// </summary>
    public int bet_count { get; set; }

    /// <summary>
    /// 總投注額
    /// </summary>
    public decimal bet_amount { get; set; }

    /// <summary>
    /// 總有效投注額
    /// </summary>
    public decimal bet_value { get; set; }

    /// <summary>
    /// 總派彩
    /// </summary>
    public decimal bet_result { get; set; }
}
