using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetDailyReportRequest: RequestBaseModel
    {
        public override int Action => 42;
        public int gType { get; set; }

        public string date { get; set; }
    }
}
