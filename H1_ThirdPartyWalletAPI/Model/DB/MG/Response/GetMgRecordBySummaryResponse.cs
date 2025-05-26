using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.MG.Response
{
    public class GetMgRecordBySummaryResponse : MGRecordPrimaryKey
    {
        public DateTime createddateutc { get; set; }

        public string gamecode { get; set; }

        public decimal betamount { get; set; }

        public decimal payoutamount { get; set; }
        public decimal jackpotWin { get; set; }

    }
}
