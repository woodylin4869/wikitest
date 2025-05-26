using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class TransferOutAllCreditResponse
{
	/// <summary>
    /// 唯一的密钥，用于验证转账到提供者系统的金额
    /// </summary>
    public string RequestID { get; set; }
    /// <summary>
    /// 玩家用户名
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// 转账时间
    /// </summary>
    public DateTime Time { get; set; }
    /// <summary>
    /// 玩家当前的信用余额转移至提供者的系统中
    /// 如金额为零，这表示玩家未在提供者的系统上存入信用余额
    /// 系统不支持交易金额为零的 "验证转移信用额"
    /// </summary>
    public decimal Amount { get; set; }
}