namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Request
{
    public class GetPlayerReportRequest
    {
        public string MemberAccount { get; set; }
        /// <summary>
        ///  语系编号（非必要），预设为商户设定之语系
        /// </summary>
        public int? LanguageID { get; set; }

        /// <summary>
        /// 指定是否显示玩家账号 0: 不显示  1: 显示 （预设值）
        /// </summary>
        public int? ShowAccount { get; set; }

        /// <summary>
        /// 游戏类型（请参照游戏类别对应表）
        /// </summary>
        public int? GameType { get; set; }

        /// <summary>
        /// 游戏记录编号（唯一码），长度 24 码
        /// </summary>
        public string RecordID { get; set; }

    }
}
