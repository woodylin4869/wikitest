using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class PullGameBettingSlipResponse
    {



        public string m { get; set; }
        public int s { get; set; }
        public PullGameBettingSlip d { get; set; }


        public class PullGameBettingSlip
        {
            public int code { get; set; }
            public long start { get; set; }
            public long end { get; set; }
            public int count { get; set; }
            public DataList list { get; set; }
        }

        public class DataList
        {
            public string[] GameID { get; set; }
            public string[] Accounts { get; set; }
            public int[] ServerID { get; set; }
            public int[] KindID { get; set; }
            public int[] TableID { get; set; }
            public int[] ChairID { get; set; }
            public int[] UserCount { get; set; }
            public string[] CellScore { get; set; }
            public string[] AllBet { get; set; }
            public string[] Profit { get; set; }
            public string[] Revenue { get; set; }
            public string[] NewScore { get; set; }
            public string[] GameStartTime { get; set; }
            public string[] GameEndTime { get; set; }
            public string[] CardValue { get; set; }
            public int[] ChannelID { get; set; }
            public string[] LineCode { get; set; }
        }

    }


    public class MPData
    {
        public string GameID { get; set; }
        public string Accounts { get; set; }
        public int ServerID { get; set; }
        public int KindID { get; set; }
        public int TableID { get; set; }
        public int ChairID { get; set; }
        public int UserCount { get; set; }
        public string CellScore { get; set; }
        public string AllBet { get; set; }
        public string Profit { get; set; }
        public string Revenue { get; set; }
        public string NewScore { get; set; }
        public DateTime GameStartTime { get; set; }
        public DateTime GameEndTime { get; set; }
        public string CardValue { get; set; }
        public int ChannelID { get; set; }
        public string LineCode { get; set; }
        public Guid summary_id { get; set; }
    }
}
