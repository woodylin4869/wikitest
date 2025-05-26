namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class SearchMemberResponse : FCBaseStatusRespones
    {
        /// <summary>
        /// 在线信息(游戏编号)
        /// </summary>
        public int OnlineType { get; set; }
        /// <summary>
        /// FC 游戏内持有点数
        /// </summary>
        public decimal Points { get; set; }
        /// <summary>
        /// 语系编号
        /// </summary>
        public int LanguageID { get; set; }
    }
}
