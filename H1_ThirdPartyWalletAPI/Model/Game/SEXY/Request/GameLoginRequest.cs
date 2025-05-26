using System.ComponentModel;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class GameLoginRequest : SexyRequestBase
    {
        public string userId { get; set; }
        //public bool isMobileLogin { get; set; }
        public string externalURL { get; set; }
        //public string gameForbidden { get; set; }
        //public string gameType { get; set; }
        public string platform { get; set; }
        public string language { get; set; }
        public string betLimit { get; set; }
        //public string autoBetMode { get; set; }

        public string hall { get; set; }

    }
}
