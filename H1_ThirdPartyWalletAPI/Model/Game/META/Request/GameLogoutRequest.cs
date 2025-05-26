namespace H1_ThirdPartyWalletAPI.Model.Game.META.Request
{
    public class GameLogoutRequest
    {
        public string Account { get; set; }

        public string Password { get; set; }

        private string gameCode = "fruitevo";
        public string GameCode { get => gameCode; set => gameCode = value; }

    }
}
