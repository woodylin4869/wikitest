using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GetTransactionByTxTimeRequest : SexyRequestBase
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        //public string userId { get; set; }
        //public int status { get; set; }
        public string platform { get; set; }
        //public string currency { get; set; }
        //public string gameType { get; set; }
        //public string gameCode { get; set; }

    }
}