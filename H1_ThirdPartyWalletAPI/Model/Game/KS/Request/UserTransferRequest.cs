namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Request
{
    public class UserTransferRequest
    {
        /// <summary>
        ///  要转账的用户名 示例：ceshi01
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 转入：IN / 转出：OUT
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 转账金额，最多支持2位小数，不可为负数
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 转账订单号，只能是数字字母下划线，32位以内
        /// </summary>
        public string ID { get; set; }


        /// <summary>
        /// 转账币种，默认为商户开户币种。如会员币种与商户币种不一致，则此项必填
        /// </summary>
        public string Currency { get; set; }

    }
}
