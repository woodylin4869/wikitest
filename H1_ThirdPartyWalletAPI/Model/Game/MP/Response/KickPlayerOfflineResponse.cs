namespace H1_ThirdPartyWalletAPI.Model.Game.MP.Response
{
    public class KickPlayerOfflineResponse
    {

        public string m { get; set; }
        public int s { get; set; }
        public KickPlayerOffline d { get; set; }


        public class KickPlayerOffline
        {
            public int code { get; set; }
        }

    }
}
