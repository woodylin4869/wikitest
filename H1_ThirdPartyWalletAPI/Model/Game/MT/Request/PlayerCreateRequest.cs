namespace H1_ThirdPartyWalletAPI.Model.Game.MT.Request
{
    public class PlayerCreateRequest
    {
        public string playerName { get; set; } 
        public string merchantId { get; set; } 
        public string pwd { get; set; } 
        public string code { get; set; } 
        public string data { get; set; } 
    }

    public class PlayerCreaterawData
    { 
      public string nickname { get; set; }
      public int playerLevel { get; set; }
    }
}
