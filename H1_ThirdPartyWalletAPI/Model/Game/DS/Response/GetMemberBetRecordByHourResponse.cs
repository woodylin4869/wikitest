using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class GetMemberBetRecordByHourResponse : ResponseBaseModel<MessageResult>
    {
        public List<MemberBetRecords> Rows { get; set; }
    }
    public class MemberBetRecords
    {
        public DateTime statistics_date { get; set; }
        public int bet_count { get; set; }
        public int bet_amount { get; set; }
        public int real_bet_amount { get; set; }
        public int payout_amount { get; set; }
        public int valid_amount { get; set; }
        public int fee_amount { get; set; }
        public int jp_amount { get; set; }
        public int win_loss_amount { get; set; }
    }
}
