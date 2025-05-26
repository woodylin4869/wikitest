using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.JDB.Response
{
    public class GetJdbRecordBySummaryResponse : JDBRecordPrimaryKey
    {
        public int mtype { get; set; }

        public DateTime gamedate { get; set; }

        public decimal bet { get; set; }

        public decimal total { get; set; }  
    }
}
