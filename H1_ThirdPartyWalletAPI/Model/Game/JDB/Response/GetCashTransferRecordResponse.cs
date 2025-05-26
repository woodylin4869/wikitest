using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetCashTransferRecordResponse : ResponseBaseModel {
        public List<CashTransferRecord> Data { get; set; }
    }
    public class CashTransferRecord
    {
        public string uid { get; set; }
        public string action { get; set; }
        public decimal amount { get; set; }
        public decimal plsBalance { get; set; }

        public DateTime date { get; set; }
        public long pid { get; set; }
        public string remark { get; set; }

        public decimal afterBalance { get; set; }
    }
}
