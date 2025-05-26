using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel;

/// <summary>
/// JOKER 第二層明細(新表)
/// </summary>
public class JOKERBetRecord
{
    /// <summary>
    /// 彩金
    /// </summary>
    public decimal JackpotWin { get; set; }

    public string Ocode { get; set; }
    public string Username { get; set; }
    public string Gamecode { get; set; }

    public decimal Amount { get; set; }
    public decimal Result { get; set; }
    public DateTime Time { get; set; }
}