namespace ThirdPartyWallet.Share.Model.Game.VA.Response
{
    public class GetGameListResponse
    {
        /// <summary>
        /// 遊戲列表
        /// </summary>
        public List<Gamelist> gameList { get; set; }

    }

    public class Gamelist
    {
        public int gameId { get; set; }
        public string gameType { get; set; }
        public string gameName { get; set; }
        public List<string> currency { get; set; }
    }


}
