namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class GetCreditResponse
{
	/// <summary>
    /// 玩家用户名
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 玩家的当前信用余额
    /// </summary>
    public decimal Credit { get; set; }
    /// <summary>
    /// 玩家的剩余信用
    /// </summary>
    public decimal OutstandingCredit { get; set; }
    public decimal FreeCredit { get; set; }
    public decimal OutstandingFreeCredit { get; set; }
}