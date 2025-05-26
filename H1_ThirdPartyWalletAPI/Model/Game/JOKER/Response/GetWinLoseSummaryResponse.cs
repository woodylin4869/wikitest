using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class GetWinLoseSummaryResponse
{
    public List<GetWinLoseSummaryResponseData> Winloss { get; set; }

    public class GetWinLoseSummaryResponseData
    {
        /// <summary>
        /// 玩家
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 注單號
        /// </summary>
        public string OCode { get; set; }
        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 結果
        /// </summary>
        public decimal Result { get; set; }
    }
}