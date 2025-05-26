using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 會員點數交易分頁列表
    /// </summary>
    public class TransferRecordResponse
    {
        public string errorcode { get; set; }
        public string errormessage { get; set; }
        public TransferRecordResponseData data { get; set; }
        public class TransferRecordResponseData
        {
            public string systemcode { get; set; }
            public string webid { get; set; }
            public int currentpage { get; set; }
            public int totalpage { get; set; }
            public string totalcount { get; set; }
            public string timestamp { get; set; }
            public List<TransferRecordResponseDataList> datalist { get; set; }
        }

        public class TransferRecordResponseDataList
        {
            public string userid { get; set; }
            public string transferno { get; set; }
            public string amount { get; set; }
            public string status { get; set; }
            public string createtime { get; set; }
        }




    }
}
