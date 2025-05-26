namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response;

/// <summary>
/// 取款
/// </summary>
public class WithdrawResponse
{
    /// <summary>
    /// 交易代碼
    /// </summary>
    public string transaction_id { get; set; }

    /// <summary>
    /// 轉帳來源帳戶
    /// </summary>
    public string source_account { get; set; }

    /// <summary>
    /// 轉帳目標帳戶
    /// </summary>
    public string destination_account { get; set; }

    /// <summary>
    /// 金額
    /// </summary>
    public string amount { get; set; }

    /// <summary>
    /// 遊戲廠商
    /// </summary>
    public string casino { get; set; }

    /// <summary>
    /// 轉帳前餘額
    /// </summary>
    public string balance_before { get; set; }

    /// <summary>
    /// 轉帳後餘額
    /// </summary>
    public string balance_after { get; set; }

    /// <summary>
    /// 遊戲廠商端的交易代碼
    /// </summary>
    public string casino_transid { get; set; }

    /// <summary>
    /// 轉帳狀態
    /// success, failed, processing
    /// </summary>
    public string status { get; set; }
}
