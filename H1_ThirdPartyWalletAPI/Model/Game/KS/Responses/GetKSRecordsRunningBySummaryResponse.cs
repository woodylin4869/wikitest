using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{

    public class GetKSRecordsRunningBySummaryResponse : GetKSRecordsBySummaryResponse
    {
        public string club_id { get; set; }
        public string franchiser_id { get; set; }
    }


}
