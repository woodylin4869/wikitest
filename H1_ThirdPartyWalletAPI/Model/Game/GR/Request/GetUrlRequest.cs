using H1_ThirdPartyWalletAPI.Model.Game.GR.Response;
using static H1_ThirdPartyWalletAPI.Model.Game.GR.Response.GetreportResponse;

namespace H1_ThirdPartyWalletAPI.Model.Game.GR.Request
{
    public class GetUrlRequest : GRResponseBase
    {
        /// <summary>
        /// 注單號
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 會員帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 遊戲代號
        /// </summary>
        public string game_type { get; set; }

    }
}
