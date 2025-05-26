using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GetTransactionByUpdateDateRequest : SexyRequestBase
    {
        public DateTime timeFrom { get; set; }
        public string platform { get; set; }
        //public int status { get; set; }
        public string currency { get; set; }
        //public string gameType { get; set; }
        //public string gameCode { get; set; }

        public decimal? delayTime { get; set; }

    }
}