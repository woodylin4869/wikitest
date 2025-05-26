namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class GetGameResultResponse : IMessage
    {
        public string Message { get; set; }

        public string gameId { get; set; }
        public string number { get; set; }
        public int roundId { get; set; }

        //dragontiger
        public string tiger { get; set; }
        public int value { get; set; }
        public string dragon { get; set; }
        public string result { get; set; }
        public string gameSet { get; set; }
        public int? periode { get; set; }



        //baccarat
        public string banker { get; set; }
        public string player { get; set; }
        public int bankerPair { get; set; }
        public int playerPair { get; set; }


        //24db
        public string multiplier { get; set; }


        //suwit
        public string numberwin { get; set; }


        //pokerdice
        public string number2 { get; set; }
        public string number3 { get; set; }

        public bool success { get; set; }
        public int response_code { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }



    public class dragontiger
    {
        public string tiger { get; set; }
        public int value { get; set; }
        public string dragon { get; set; }
        public string gameId { get; set; }
        public string result { get; set; }
        public string gameSet { get; set; }
        public int periode { get; set; }
        public int roundId { get; set; }
    }

    public class baccarat
    {
        public int value { get; set; }
        public string banker { get; set; }
        public string gameId { get; set; }
        public string player { get; set; }
        public string result { get; set; }
        public string gameSet { get; set; }
        public int periode { get; set; }
        public int roundId { get; set; }
        public int bankerPair { get; set; }
        public int playerPair { get; set; }
    }


}