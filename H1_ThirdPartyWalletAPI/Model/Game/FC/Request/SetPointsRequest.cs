namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Request
{
    public class SetPointsRequest
    {
        /// <summary>
        /// 玩家账号
        /// </summary>
        public string MemberAccount { get; set; }

        /// <summary>
        /// 对应单号为商户端的交易单编号（英数字 30 字符）
        /// </summary>
        public string TrsID { get; set; }

        /// <summary>
        /// 0: 不全部提领（默认值）1: 全部提领（包含所有小数字金额）
        /// </summary>
        public int AllOut { get; set; }
        /// <summary>
        /// 提款或存款点数（请以正数至小数后两位） 正数: 存款 负数: 提款 ※当 AllOut 为 0 时，此参数为必填
        /// </summary>
        public decimal? Points { get; set; }

    }
}
