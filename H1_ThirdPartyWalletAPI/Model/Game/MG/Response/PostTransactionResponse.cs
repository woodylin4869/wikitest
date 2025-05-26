using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    /// <summary>
    /// Create transaction 创建资金交易
    /// </summary>
    public class PostTransactionResponse
    {
        public string Id { get; set; }
        public TransactionStatus? status { get; set; }
        public TransactionType Type { get; set; }
        public string PlayerId { get; set; }
        public decimal Amount { get; set; }
        public string ExternalTransactionId { get; set; }
        public string IdempotencyKey { get; set; }

        public DateTime CreateDateUTC { get; set; }
        public List<TransactionDetail> details { get; set; }
        public string Uri { get; set; }

    }
    public class TransactionDetail
    {
        public string Product { get; set; }
        public TransationDetailStatus Status{ get; set; }
        public decimal Amount { get; set; }
    }
}
