using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_ae_bet_record
    {
        /// <summary>
        /// 匯總ID
        /// </summary>
        public Guid summary_id { get; set; }
        public string account_name { get; set; }
        /// <summary>
        /// 玩家的货币
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// 本局游戏的投注额
        /// </summary>
        public string bet_amt { get; set; }
        /// <summary>
        /// 本局游戏的派彩金额
        /// </summary>
        public string payout_amt { get; set; }
        /// <summary>
        /// 本局游戏的投注时间。（UTC时区，+00:00）格式为: YYYY-MM-DDThh:mm:ssTZD
        /// </summary>
        public DateTime bet_at { get; set; }
        /// <summary>
        /// 玩家於本局游戏结束时的钱包余额
        /// </summary>
        public string end_balance { get; set; }
        /// <summary>
        /// 返水开启时返水至玩家余额, 应大于或等于0
        /// </summary>
        public string rebate_amt { get; set; }
        /// <summary>
        /// 游戏ID
        /// </summary>
        public int game_id { get; set; }
        /// <summary>
        /// 每局游戏的唯一ID
        /// </summary>
        public long round_id { get; set; }
        /// <summary>
        /// 当本局游戏免费时为true, 免费转即是本局投注额不会从玩家钱包中扣除
        /// </summary>
        public bool free { get; set; }
        /// <summary>
        /// 本局游戏的结束时间。（UTC时区，+00:00）格式为: YYYY-MM-DDThh:mm:ssTZD 此时间用于对帐
        /// </summary>
        public DateTime completed_at { get; set; }
        /// <summary>
        /// 彩池开启时玩家货币下的彩池累积金额
        /// </summary>
        public string jp_pc_con_amt { get; set; }
        /// <summary>
        /// 彩池中奬时玩家货币下的派彩金额
        /// </summary>
        public string jp_pc_win_amt { get; set; }
        /// <summary>
        /// 彩池开启时彩池货币下的彩池累积金额
        /// </summary>
        public string jp_jc_con_amt { get; set; }
        /// <summary>
        /// 彩池中奬时彩池货币下的派彩金额
        /// </summary>
        public string jp_jc_win_amt { get; set; }
        /// <summary>
        /// 彩池中奬时彩池中奖编号
        /// </summary>
        public string jp_win_id { get; set; }
        /// <summary>
        /// 彩池中奬时彩池中奖级别
        /// </summary>
        public int jp_win_lv { get; set; }
        /// <summary>
        /// 彩池中奬时若把jp_pc_win_amt 直接存入玩家钱包则为true
        /// </summary>
        public bool jp_direct_pay { get; set; }
        /// <summary>
        /// 本局游戏中获得红包时
        /// rpcash – 红包的奖励为现金。
        /// rpfreespin – 红包的奖励为免费转。
        /// </summary>
        public string prize_type { get; set; }
        /// <summary>
        /// 本局游戏中获得红包时
        /// 如果红包类型为rpcash，则这个值是总奖励金额。
        /// 如果红包类型为 rpfreespin，则这个值是总奖励局数
        /// </summary>
        public string prize_amt { get; set; }
        /// <summary>
        /// request参数中使用group时 这局游戏的营运商识别代码
        /// </summary>
        public int side_id { get; set; }
    }
}