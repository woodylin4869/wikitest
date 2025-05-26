using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.JDB
{
    public class JDBRecordPrimaryKey
    {
        public long seqno { get; set; }
        public string historyid { get; set; }
        public DateTime lastmodifytime { get; set; }
    }
}
