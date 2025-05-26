namespace H1_ThirdPartyWalletAPI.Model.DB.PME.Response
{
    public class GetPMERecordsPreAmountByIdResponse : PMERecordPrimaryKey
    {
        /// <summary>
        /// 原始投注金額
        /// </summary>
        public decimal pre_bet_amount { get; set; }

        /// <summary>
        /// 原始輸贏金額
        /// </summary>
        public decimal pre_win_amount { get; set; }
    }
}
