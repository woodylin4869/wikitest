namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class bethistoryResponse
    {
        public bethistoryResponse(List<Bet_History> bet_history)
        {
            this.bet_history = bet_history;
        }

        public List<Bet_History> bet_history { get; set; }
    }

    public class Bet_History
    {
        private string round_id1 = "";
        private string bet_id1 = "";
        private string match_id1 = "";
        private string game_id1 = "";
        private string bet_type1 = "";
        private string raw_data1 = "";
        private string game_name1 = "";
        private string game_result1 = "";

        /// <summary>
        /// Date period (format Y-m-d)
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// Round ID is a transId from game
        /// </summary>
        public string round_id { get => round_id1; set => round_id1 = value; }

        /// <summary>
        /// Bet ID is a transId from game
        /// </summary>
        public string bet_id { get => bet_id1; set => bet_id1 = value; }

        /// <summary>
        /// Match ID is a roundId from game
        /// </summary>
        public string match_id { get => match_id1; set => match_id1 = value; }

        /// <summary>
        /// Game ID of Game
        /// </summary>
        public string game_id { get => game_id1; set => game_id1 = value; }

        /// <summary>
        /// Bet Type has 4 type Lose, Win, Revision and Refund
        /// </summary>
        public string bet_type { get => bet_type1; set => bet_type1 = value; }

        /// <summary>
        /// Player Bet on game
        /// </summary>
        public decimal bet { get; set; }

        /// <summary>
        /// Player Win on game
        /// </summary>
        public decimal win { get; set; }


        /// <summary>
        /// game username player on game
        /// </summary>
        public string game_username { get; set; }


        /// <summary>
        /// Id of Bet History
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// Detail of Player Betting
        /// </summary>
        public string raw_data { get => raw_data1; set => raw_data1 = value; }

        /// <summary>
        /// Detail game result
        /// </summary>
        public string game_result { get => game_result1; set => game_result1 = value; }

        /// <summary>
        /// Name of Game
        /// </summary>
        public string game_name { get => game_name1; set => game_name1 = value; }


        #region db Model

        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime? Report_time { get; set; }

        /// <summary>
        /// 下注總額(前一狀態)
        /// </summary>
        public decimal Pre_bet { get; set; }

        /// <summary>
        /// 輸贏(前一狀態)
        /// </summary>
        public decimal Pre_win { get; set; }



        /// <summary>
        /// Club_id (running表)
        /// </summary>
        public string Club_id { get; set; }

        /// <summary>
        /// Franchiser_id (running表)
        /// </summary>
        public string Franchiser_id { get; set; }


        /// <summary>
        /// 寫入注單用遊戲種類代碼
        /// </summary>
        public int groupgametype { get; set; }
        #endregion db Model
    }

}