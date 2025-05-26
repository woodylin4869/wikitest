namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class LoginGameRequest : RequestBaseModel
    {
        public string Game_id { get; set; }
        public string Account { get; set; }
        public string Lang { get; set; }
        public string Oper { get; set; }
        public string Backurl { get; set; }
        public bool Is_demo { get; set; }
        public string Btn { get; set; }
        public string Extra { get; set; }
        public decimal Max_bet { get; set; }
        public decimal Min_bet { get; set; }

    }



}
