using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.DS.Response
{
    public class GetDSRecordsBySummaryReponse : DSRecordPrimaryKey
    {
        public string id { get; set; }
        public decimal fee_amount { get; set; }
        public DateTime? finish_at { get; set; }
        public DateTime? bet_at { get; set; }
        public decimal bet_amount { get; set; }
        public decimal valid_amount { get; set; }
        public decimal payout_amount { get; set; }
        public string member { get; set; }
    }
}
