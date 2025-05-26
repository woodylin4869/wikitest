namespace H1_ThirdPartyWalletAPI.Model.DB.BTI.DBResponse
{
    public class GetBTIPreRecordDBResponse : BTIRecordPrimaryKey
    {
        /// <summary>
        /// 原始
        /// </summary>
        public decimal pre_TotalStake { get; set; }

        /// <summary>
        /// 原始
        /// </summary>
        public decimal pre_ValidStake { get; set; }

        /// <summary>
        /// 原始
        /// </summary>
        public decimal pre_PL { get; set; }

        /// <summary>
        /// 原始
        /// </summary>
        public decimal pre_Return { get; set; }

        /// <summary>
        /// 原始
        /// </summary>
        public string pre_BetStatus { get; set; }
    }
}
