namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class BalanceResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal main_wallet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<Game_Wallets> game_wallets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal total_wallet { get; set; }
    }

    public class Game_Wallets
    {
        public int id { get; set; }
        public string game_name { get; set; }
        public string game_slug { get; set; }
        public string balance { get; set; }
        public int is_processing { get; set; }
    }

}