namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Response
{
    public class ReportAgentResponse
    {     
            public string code { get; set; }
            public string message { get; set; }
            public Request request { get; set; }
            public Data data { get; set; }
        

        public class Request
        {
            public int startDate { get; set; }
            public int endDate { get; set; }
            public int pageIndex { get; set; }
            public long timestamp { get; set; }
        }

        public class Data
        {
            public int pageSize { get; set; }
            public int pageIndex { get; set; }
            public int totalRecord { get; set; }
            public int totalPage { get; set; }
            public Record[] record { get; set; }
            public Summary summary { get; set; }
        }

        public class Summary
        {
            public int betCount { get; set; }
            public decimal betAmount { get; set; }
            public decimal validBetAmount { get; set; }
            public decimal netAmount { get; set; }
        }

        public class Record
        {
            public int id { get; set; }
            public int agentId { get; set; }
            public int reportDate { get; set; }
            public int parentId { get; set; }
            public int betCount { get; set; }
            public decimal betAmount { get; set; }
            public decimal validBetAmount { get; set; }
            public decimal netAmount { get; set; }
        }

    }
}
