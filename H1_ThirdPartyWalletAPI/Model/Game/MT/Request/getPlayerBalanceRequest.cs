namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class getPlayerBalanceRequest
    {
            public string playerName { get; set; }
            public string merchantId { get; set; }
            public string data { get; set; }
     
    }

    public class getPlayerBalancerawData
    {
        public string currency { get; set; }

    }
}
