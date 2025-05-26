using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.Common.Response
{
    /// <summary>
    /// 第二層明細(W1五分鐘彙總帳)_電子類
    /// </summary>
    public class RespRecordLevel2_Electronic
    {
        /// <summary>
        /// 遊戲代碼(gameCode)
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// 遊戲類型
        /// 依遊戲商提供類型
        /// 如 (slot:老虎機, fishing:捕魚, chess:棋牌 ... )
        /// </summary>
        public string GameType { get; set; }

        /// <summary>
        /// 遊戲注單單號
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// 下注時間
        /// </summary>
        public DateTime BetTime { get; set; }

        /// <summary>
        /// 結算時間
        /// </summary>
        public DateTime SettleTime { get; set; }

        /// <summary>
        /// 下注金額(有效下注金額)
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 贏分
        /// NetWin - BetValidAmount
        /// </summary>
        [JsonIgnore]
        public decimal BetWin => NetWin + BetAmount;

        /// <summary>
        /// 輸贏
        /// </summary>
        public decimal NetWin { get; set; }

        /// <summary>
        /// 彩金
        /// </summary>
        public decimal Jackpot { get; set; }

        /// <summary>
        /// 注單狀態
        /// </summary>
        public string BetStatus { get; set; }
    }
}