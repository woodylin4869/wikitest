
namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetGamePlatformUserResponse : ResCodeBase
    {
        public GamePlatformUser[] Datas { get; set; } = new GamePlatformUser[0];

        public class GamePlatformUser
        {
            public string club_id { get; set; }

            public string platform { get; set; }

            public string platform_id { get; set; }
        }
    }
}
