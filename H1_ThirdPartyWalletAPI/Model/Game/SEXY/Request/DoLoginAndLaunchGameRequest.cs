using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class DoLoginAndLaunchGameRequest : SexyRequestBase
    {
        public string userId { get; set; }
        public string gameCode { get; set; }
        public string gameType { get; set; }
        public string platform { get; set; }
        //public bool isMobileLogin { get; set; }
        public string externalURL { get; set; }
        public string language { get; set; }
        public string hall { get; set; }
        public string betLimit { get; set; }
        //public string autoBetMode { get; set; }
        public bool enableTable { get; set; }
        public string tid { get; set; }

    }
}
