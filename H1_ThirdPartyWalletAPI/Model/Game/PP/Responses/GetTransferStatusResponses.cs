namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Responses
{
    public class GetTransferStatusResponses
    {   
            public string error { get; set; }
            public string description { get; set; }
            public object transactionId { get; set; }
            public string status { get; set; }
            public object amount { get; set; }
            public object balance { get; set; }
    }
}
