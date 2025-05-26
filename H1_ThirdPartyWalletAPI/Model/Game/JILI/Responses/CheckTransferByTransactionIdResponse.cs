using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class CheckTransferByTransactionIdResponse
    {

            public int ErrorCode { get; set; }
            public string Message { get; set; }
            public TransactionData Data { get; set; }
        

        public class TransactionData
        {
            public string Account { get; set; }
            public string TransactionId { get; set; }
            public DateTime TransferTime { get; set; }
            public decimal Amount { get; set; }
            public int Status { get; set; }
            public int TransferType { get; set; }
        }

    }
}
