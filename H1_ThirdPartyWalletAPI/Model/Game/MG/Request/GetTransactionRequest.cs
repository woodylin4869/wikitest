namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Request
{
    /// <summary>
    /// Get transaction details by idempotencyKey 通过幂等键获取交易详细信息
    /// </summary>
    public class GetTransactionRequest: BaseRequest { 
        public string idempotencyKey { get; set; }
    }
}
