using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    /// <summary>
    /// PG 遊戲匯總報表
    /// </summary>
    public class t_pg_game_report
    {
        /// <summary>
        /// 每小时记录的日期和时间
        /// </summary>
        public DateTime datetime { get; set; }
        /// <summary>
        /// 游戏投注总计数
        /// </summary>
        public int totalhands { get; set; }
        /// <summary>
        /// 记录中玩家使用的货币
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 总投注额
        /// </summary>
        public decimal totalbetamount { get; set; }
        /// <summary>
        /// 派彩总金额
        /// </summary>
        public decimal totalwinamount { get; set; }
        /// <summary>
        /// 玩家的总输赢
        /// </summary>
        public decimal totalplayerwinlossamount { get; set; }
        /// <summary>
        /// 公司的总输赢
        /// </summary>
        public decimal totalcompanywinlossamount { get; set; }
        /// <summary>
        /// 交易类别：
        /// 1: 现金
        /// 2: 红利
        /// 3: 免费游戏
        /// </summary>
        public int transactiontype { get; set; }
        /// <summary>
        /// 消除的普通旋转总数
        /// </summary>
        public int totalcollapsespincount { get; set; }
        /// <summary>
        /// 消除的免费旋转总数
        /// </summary>
        public int totalcollapsefreespincount { get; set; }
    }
}