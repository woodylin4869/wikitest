using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class JackpotHistory
    {
        /// <summary>
        /// 中獎唯一識別號
        /// </summary>
        public long JackpotId { get; set; }

        /// <summary>
        /// 遊戲紀錄惟一編號
        /// </summary>
        public long Seq { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 站台代碼
        /// </summary>
        public string WebId { get; set; }

        /// <summary>
        /// 會員惟一識別碼
        /// </summary>
        public string ClubId { get; set; }

        /// <summary>
        /// 遊戲代碼
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// 中獎 Jackpot 類型代碼(請參照補充說明)
        /// </summary>
        public int JackpotType { get; set; }

        /// <summary>
        /// 彩金(小數點兩位)
        /// </summary>
        public decimal JackpotWin { get; set; }

        /// <summary>
        /// 中獎時間
        /// </summary>
        public string HitTime { get; set; }
    }

    public class JackpotHistoryRes : ResCodeBase
    {
        public List<JackpotHistory> Data { get; set; }
    }

    public class JackpotPoolValueRes : ResCodeBase
    {
        public List<GetJackpotPoolValueResponse.JackpotPool> Data { get; set; }
    }
}
