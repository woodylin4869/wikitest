namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class GetGameDetailUrlResponse
    {

        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public UrlData Data { get; set; }
        public class UrlData
        {
            public string Url { get; set; }
        }

    
  
    }
}
