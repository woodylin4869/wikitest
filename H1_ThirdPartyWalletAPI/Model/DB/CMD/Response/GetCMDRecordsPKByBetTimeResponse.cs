namespace H1_ThirdPartyWalletAPI.Model.DB.CMD.Response
{
    public class GetCMDRecordsPKByBetTimeResponse : CMDRecordPrimaryKey
    {
        /// <summary>
        /// 原始投注金額
        /// </summary>
        public decimal pre_betamount { get; set; }
        /// <summary>
        /// 原始輸贏金額
        /// </summary>
        public decimal pre_winamount { get; set; }
    }
}
