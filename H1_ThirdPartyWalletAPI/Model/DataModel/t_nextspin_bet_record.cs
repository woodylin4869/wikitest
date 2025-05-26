using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_nextspin_bet_record
    {
        /// <summary>
        /// 下注单号
        /// </summary>
        public long ticketId { get; set; }

        /// <summary>
        /// 用户标识 ID
        /// </summary>
        public string acctId { get; set; }

        /// <summary>
        /// 游戏种类
        /// SM or TB or AD or BN
        /// </summary>
        public string categoryId { get; set; }

        /// <summary>
        /// 游戏代码
        /// </summary>
        public string gameCode { get; set; }

        /// <summary>
        /// 下注时间
        /// </summary>
        public DateTime ticketTime { get; set; }

        /// <summary>
        /// 用户下注 IP
        /// </summary>
        public string betIp { get; set; }

        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal betAmount { get; set; }

        /// <summary>
        /// 用户输赢
        /// </summary>
        public decimal winLoss { get; set; }

        /// <summary>
        /// 货币 ISO 代码
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// </summary>
        public decimal jackpotAmount { get; set; }

        /// <summary>
        /// </summary>
        public long luckyDrawId { get; set; }

        /// <summary>
        /// 是否已结束
        /// </summary>
        public bool completed { get; set; }

        /// <summary>
        /// 游戏 log ID
        /// </summary>
        public long roundId { get; set; }

        /// <summary>
        /// 0 =没赢 jackpot
        /// 0, 1, 2, 3
        /// </summary>
        public int sequence { get; set; }

        /// <summary>
        /// 注单来自手机或网
        /// Mobile/Web
        /// </summary>
        public string channel { get; set; }

        /// <summary>
        /// 上轮余额
        /// </summary>
        public decimal balance { get; set; }

        /// <summary>
        /// 积宝赢额
        /// </summary>
        public decimal jpWin { get; set; }

        public Guid summary_id { get; set; }
    }
}
