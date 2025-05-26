using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class GetMemberInfoResponse
    {

        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public List<Datum> Data { get; set; }
        public class Datum
        {
            /// <summary>
            /// 會員唯一識別值
            /// </summary>
            public string Account { get; set; }
            /// <summary>
            /// 會員錢包餘額 (帳號不存在時為 0)
            /// </summary>
            public decimal Balance { get; set; }
            /// <summary>
            /// 1: 線上 
            /// 2: 離線
            /// 3: 帳號不存在
            /// </summary>
            public int Status { get; set; }
        }
    }
}
