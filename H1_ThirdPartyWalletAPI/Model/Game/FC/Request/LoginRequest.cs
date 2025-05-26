namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Request
{
    public class LoginRequest
    {
        public string MemberAccount { get; set; }
        /// <summary>
        /// 游戏编号，GameID 与 LoginGameHall 需带入其一
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// 语系编号（非必要），预设商户指定语系
        /// </summary>
        public int LanguageID { get; set; }
        /// <summary>
        /// 回首页按钮的 URL（非必要），此参数结合游戏表现
        /// </summary>
        public string HomeUrl { get; set; }

        /// <summary>
        /// 未带入则预设不开放
        /// </summary>
        public bool JackpotStatus { get; set; }
        /// <summary>
        /// 进入 FC 大厅，GameID 与 LoginGameHall 需带入其一
        /// </summary>
        public bool LoginGameHall { get; set; }

        ///// <summary>
        ///// FC 大厅游戏清单，依据 GameType 显示
        ///// </summary>
        //public int[] GameHallGameType { get; set; }

    }
}
