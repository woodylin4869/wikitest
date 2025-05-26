namespace H1_ThirdPartyWalletAPI.Model.DB.RCG3.DBResponse
{
    public class RCG3PreRecordDBResponse : RCG3RecordPrimaryKey
    {
        /// <summary>
        /// [原始]下注金額
        /// </summary>
        public decimal pre_bet { get; set; }

        /// <summary>
        /// [原始]有效下注
        /// </summary>
        public decimal pre_available { get; set; }

        /// <summary>
        /// [原始]輸贏
        /// </summary>
        public decimal pre_winlose { get; set; }

        /// <summary>
        /// [原始]狀態 3當局取消、4正常注單、5事後取消、6改牌
        /// </summary>
        public int pre_status { get; set; }

        /// <summary>
        /// [原始]注單編號
        /// </summary>
        public long real_id { get; set; }
    }
}
