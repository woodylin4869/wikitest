using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Response
{
    public class BetHistoryRecordResponse
    {

        public string code { get; set; }
        public string message { get; set; }
        public Request request { get; set; }
        public Data data { get; set; }


        public class Request
        {
            public string startTime { get; set; }
            public string endTime { get; set; }
            public int pageIndex { get; set; }
            public long timestamp { get; set; }
        }

        public class Data
        {
            public int pageSize { get; set; }
            public int pageIndex { get; set; }
            public int totalRecord { get; set; }
            public int totalPage { get; set; }
            public List<Record> record { get; set; }
        }

        public class Record
        {
            public long id { get; set; }
            public string agentCode { get; set; }
            public string playerName { get; set; }
            public decimal betAmount { get; set; }
            public decimal validBetAmount { get; set; }
            public decimal netAmount { get; set; }
            public decimal payAmount { get; set; }
            public long createdAt { get; set; }
            public long netAt { get; set; }
            public long updatedAt { get; set; }
            public long recalcuAt { get; set; }
            public int gameTypeId { get; set; }
            public string gameTypeName { get; set; }
            public int platformId { get; set; }
            public string platformName { get; set; }
            public int betStatus { get; set; }
            public int betFlag { get; set; }
            public int betPointId { get; set; }
            public string odds { get; set; }
            public string betPointName { get; set; }
            public string currency { get; set; }
            public string tableCode { get; set; }
            public string tableName { get; set; }
            public string roundNo { get; set; }
            public string bootNo { get; set; }
            public int recordType { get; set; }
            public int gameMode { get; set; }
            public string dealerName { get; set; }
            public decimal realDeductAmount { get; set; }
            public int bettingRecordType { get; set; }
            public string addstr1 { get; set; }
            public string addstr2 { get; set; }
            public Guid summary_id { get; set; }

            public decimal pre_betAmount { get; set; }
            public decimal pre_validBetAmount { get; set; }
            public decimal pre_netAmount { get; set; }
            public decimal pre_payAmount { get; set; }
        }


        public class FromDateRecord
        {
            public long id { get; set; }
            public string agentCode { get; set; }
            public string playerName { get; set; }
            public decimal betAmount { get; set; }
            public decimal validBetAmount { get; set; }
            public decimal netAmount { get; set; }
            public decimal payAmount { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime netAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime recalcuAt { get; set; }
            public int gameTypeId { get; set; }
            public string gameTypeName { get; set; }
            public int platformId { get; set; }
            public string platformName { get; set; }
            public int betStatus { get; set; }
            public int betFlag { get; set; }
            public int betPointId { get; set; }
            public string odds { get; set; }
            public string betPointName { get; set; }
            public string currency { get; set; }
            public string tableCode { get; set; }
            public string tableName { get; set; }
            public string roundNo { get; set; }
            public string bootNo { get; set; }
            public int recordType { get; set; }
            public int gameMode { get; set; }
            public string dealerName { get; set; }
            public decimal realDeductAmount { get; set; }
            public int bettingRecordType { get; set; }
            public string addstr1 { get; set; }
            public string addstr2 { get; set; }
            public Guid summary_id { get; set; }
            public decimal pre_betAmount { get; set; }
            public decimal pre_validBetAmount { get; set; }
            public decimal pre_netAmount { get; set; }
            public decimal pre_payAmount { get; set; }
        }
    }
}
