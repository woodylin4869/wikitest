namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetCurrencyReportResponse : FCBaseStatusRespones
    {
        /// <summary>
        /// 下注点数
        /// </summary>
        public decimal Bet { get; set; }

        /// <summary>
        /// 彩金抽水 (支持到小数第六位
        /// </summary>
        public decimal jptax { get; set; }
        /// <summary>
        /// 赢分点数
        /// </summary>
        public decimal Win { get; set; }
        /// <summary>
        /// 彩金点数
        /// </summary>
        public decimal jppoints { get; set; }

        /// <summary>
        /// 输赢点数（含下注）
        /// </summary>
        public decimal Winlose { get; set; }

        /// <summary>
        /// 游戏次数
        /// </summary>
        public int Round { get; set; }

    }
}
