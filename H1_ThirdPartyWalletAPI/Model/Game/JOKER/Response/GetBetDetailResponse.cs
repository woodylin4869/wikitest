using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;

public class GetBetDetailResponse
{
    /// <summary>
    /// 密钥是事务类型。例如 Game 或Jackpot 或 Competition。值是以下模型的数组。
    /// </summary>
    public GetBetDetailResponseData data { get; set; }
    /// <summary>
    /// 如果返回的 NextId 不是空，则请求具有相同 StartDate 和 EndDate 的 NextId的 API。重复此操作直到 NextId 是空
    /// </summary>
    public string nextId { get; set; }

    public class GetBetDetailResponseData
    {
        public List<Game> Game { get; set; }
        public List<Jackpot> Jackpot { get; set; }
        public List<Competition> Competition { get; set; }
    }

    /// <summary>
    /// 注單紀錄類型
    /// </summary>
    public class BetRecordTypeBase
    {
        /// <summary>
        /// 事务的唯一标识符
        /// </summary>
        public string OCode { get; set; }
        /// <summary>
        /// 与事务关联的玩家
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 事务的游戏代码
        /// </summary>
        public string GameCode { get; set; }
        /// <summary>
        /// 敘述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// TransactionOCode
        /// </summary>
        public string TransactionOCode { get; set; }
        /// <summary>
        /// RoundID
        /// </summary>
        public string RoundID { get; set; }
        /// <summary>
        /// 事务时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 下注结果
        /// 输赢：结果 – 金额
        /// </summary>
        public decimal Result { get; set; }
    }

    /// <summary>
    /// 遊戲注單
    /// </summary>
    public class Game : BetRecordTypeBase
    {

    }

    /// <summary>
    /// 彩金注單
    /// </summary>
    public class Jackpot: BetRecordTypeBase
    {

    }

    /// <summary>
    /// 競賽注單
    /// </summary>
    public class Competition: BetRecordTypeBase
    {

    }
}