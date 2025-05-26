using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetInGamePlayerResponse : ResponseBaseModel
    {
        public InGamePlayer data { get; set; }
    }

    public class InGamePlayer { 
        public GameType gType { get; set; }
        public int mType { get; set; }
        public string loginFrom { get; set; }
        public string ipAddr { get; set; }
        public DateTime LoginTime { get; set; }
        public decimal Balance { get; set; }

    }
}
