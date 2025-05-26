namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class RegistrationResponse
    {     /// <summary>
          /// 玩家id
          /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string username { get; set; }

        public string fullname { get; set; }
        public object balance { get; set; }
        public object total_dps { get; set; }
        public object total_dpsa { get; set; }
        public object total_wd { get; set; }
        public object total_wda { get; set; }
        public string signupip { get; set; }
        public string signupdate { get; set; }
        public object lastloginip { get; set; }
        public string lastloginsession { get; set; }
        public string lastlogindate { get; set; }
        public string lastactivitydate { get; set; }
        public int isactive { get; set; }
        public object isbot { get; set; }
        public object level { get; set; }
        public object date { get; set; }
        public int currency_id { get; set; }
        public int whitelabel_id { get; set; }
        public string rank { get; set; }
        public int turn_over_total { get; set; }
        public string next_turn_over { get; set; }
        public int weekly_bonus_flag { get; set; }
        public int level_up_flag { get; set; }
        public object lastbetdate { get; set; }
        public object last_played_game_id { get; set; }
        public int is_mobile { get; set; }
        public object token { get; set; }
        public object change_password { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }


}