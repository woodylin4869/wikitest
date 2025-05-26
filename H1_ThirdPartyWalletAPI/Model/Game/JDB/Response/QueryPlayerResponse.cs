using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class QueryPlayerResponse : ResponseBaseModel
    { 
        public List<QueryPlayer> Data { get; set; }
    }

    public class QueryPlayer
    {
        public string Uid { get; set; }
        public decimal Balance { get; set; }
        public string Parent { get; set; }
        public string Username { get; set; }
        public string Currency { get; set; }
        public int Lvl { get; set; }
        public int locked { get; set; }
        public int closed { get; set; }
        public int jackpotFlag { get; set; }
    }
}
