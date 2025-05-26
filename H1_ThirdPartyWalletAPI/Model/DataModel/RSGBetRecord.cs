using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel;

/// <summary>
/// 皇電第二層明細(舊表、新表)
/// </summary>
public class RSGBetRecord
{
    public long sequenNumber { get; set; }
    public DateTime playtime { get; set; }
    public decimal betamt { get; set; }
    public decimal winamt { get; set; }
    public decimal jackpotwin { get; set; }
    /// <summary>
    /// 遊戲代碼(gameCode)
    /// </summary>
    public string GameId { get; set; }
}