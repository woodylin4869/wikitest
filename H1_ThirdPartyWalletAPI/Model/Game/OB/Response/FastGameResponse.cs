namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Response
{
    public class FastGameResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public Request request { get; set; }
        public Data data { get; set; }

        public class Request
        {
            public string loginName { get; set; }
            public string loginPassword { get; set; }
            public int deviceType { get; set; }
            public int oddType { get; set; }
            public int lang { get; set; }
            public string backurl { get; set; }
            public int gameTypeId { get; set; }
            public int showExit { get; set; }
            public int version { get; set; }
            public long timestamp { get; set; }
        }

        public class Data
        {
            public string url { get; set; }
        }

    }
}
