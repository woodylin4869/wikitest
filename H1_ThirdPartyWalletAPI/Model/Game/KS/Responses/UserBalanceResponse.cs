namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{
    public class UserBalanceResponse
    {
        /// <summary>
        /// 会员的币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 会员的余额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 会员的额度（等同余额/此字段将被废弃，请使用 Money 字段）
        /// </summary>
        public decimal Credit { get; set; }
    }
}
