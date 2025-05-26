using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class ValidTransferCreditResponse
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
    /// 转账金额 正/负数
    /// </summary>
    public decimal Amount { get; set; }
}