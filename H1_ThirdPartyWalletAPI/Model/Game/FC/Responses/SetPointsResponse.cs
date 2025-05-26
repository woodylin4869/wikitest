namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class SetPointsResponse : FCBaseStatusRespones
    {
        /// <summary>
        /// 交易单号(唯一值，由 FC 游戏提供
        /// </summary>
        public long BankID { get; set; }
        /// <summary>
        /// 对应单号(唯一值，由商户端提供，若未提供则此参数为空)
        /// </summary>
        public string TrsID { get; set; }

        /// <summary>
        /// 充提后持有点数
        /// </summary>
        public decimal AfterPoint { get; set; }

        /// <summary>
        /// 提款或存款点数
        /// </summary>
        public decimal Points { get; set; }

    }
}
