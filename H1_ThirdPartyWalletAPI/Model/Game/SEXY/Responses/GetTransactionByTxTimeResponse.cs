using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response
{
    /// <summary>
    /// 取得遊戲帳務
    /// </summary>
    public class GetTransactionByTxTimeResponse : SEXYBaseStatusRespones
    {
        public List<Record> transactions { get; set; }
    }
}