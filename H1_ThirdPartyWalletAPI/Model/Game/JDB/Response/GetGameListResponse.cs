using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetGameListResponse : ResponseBaseModel
    { 
        public List<GameList> Data { get; set; }
        
    }
    public class GameList
    {
        public GameType gtype { get; set; }

        public List<JDBGameInfo> list { get; set; }
    }

    public class JDBGameInfo
    {
        public int mType { get; set; }

        public bool isNew { get; set; }

        public string image { get; set; }
        public string name { get; set; }
    }
}
