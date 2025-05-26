namespace H1_ThirdPartyWalletAPI.Model.Game
{
    public class STREAMER : RCG
    {       
    }
    public class STREAMER_Login
    {
        public string systemCode { get; set; }
        public string webid { get; set; }
        public string clubId { get; set; }
        public string loginUrl { get; set; }
        
    }
    public class STREAMER_CreateOrSetUser
    {
        public string displayName { get; set; }
        public string pictureUrl { get; set; }
        public string clubId { get; set; }
        public string clubEname { get; set; }
        public string systemCode { get; set; }
        public string webId { get; set; }
    }
}
