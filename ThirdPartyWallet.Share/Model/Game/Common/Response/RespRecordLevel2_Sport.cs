using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Common.Response
{
    /// <summary>
    /// 第二層明細(W1五分鐘彙總帳)_體育
    /// </summary>
    public class RespRecordLevel2_Sport
    {
        /// <summary>
        /// 交易單號，每筆交易的唯一標識。
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// 遊戲的標識碼。
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// 參賽聯賽的英文名稱。
        /// </summary>
        public string LeagueName { get; set; }

        /// <summary>
        /// 主場球隊的英文名稱。
        /// </summary>
        public string HomeTeamName { get; set; }

        /// <summary>
        /// 客場球隊的英文名稱。
        /// </summary>
        public string AwayTeamName { get; set; }

        /// <summary>
        /// 下注的日期和時間。
        /// </summary>
        public DateTime BetTime { get; set; }

        /// <summary>
        /// 下注結算的日期和時間。
        /// </summary>
        public DateTime? SettlementTime { get; set; }

        /// <summary>
        /// 下注金額。
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 下注的球隊名稱。
        /// </summary>
        public string BetTeam { get; set; }

        /// <summary>
        /// 提供給下注的賠率。
        /// </summary>
        public decimal Odds { get; set; }

        /// <summary>
        /// 賠率的類型。
        /// </summary>
        public string OddsType { get; set; }

        /// <summary>
        /// 輸贏的金額。
        /// </summary>
        public decimal NetWin { get; set; }

        /// <summary>
        /// 贏分
        /// NetWin - BetValidAmount
        /// </summary>
        [JsonIgnore]
        public decimal BetWin => NetWin + BetAmount;

    }
}