using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetGameIconListResponse : FCBaseStatusRespones
    {
        public Getgameiconlist GetGameIconList { get; set; }

        public class Getgameiconlist
        {
            public object specialGame { get; set; }
            public object fishing { get; set; }
            public object slot { get; set; }
            public object table { get; set; }
            //public List<GameDetail> table { get; set; }
        }

        public class GameDetail
        {
            public string status { get; set; }
            public string gameNameOfChinese { get; set; }
            public string gameNameOfEnglish { get; set; }
            public string cnUrl { get; set; }
            public string enUrl { get; set; }
        }


    }
}
