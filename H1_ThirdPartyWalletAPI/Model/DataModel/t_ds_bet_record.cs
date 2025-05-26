using System;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_ds_bet_record
    {
        public Guid summary_id { get; set; }
        /// <summary>
        /// 下注編號
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 下注時間
        /// </summary>
        public DateTime bet_at { get; set; }
        /// <summary>
        /// 結算時間
        /// </summary>
        public DateTime finish_at { get; set; }
        /// <summary>
        /// 代理帳號
        /// </summary>
        public string agent { get; set; }
        /// <summary>
        /// 玩家帳號
        /// </summary>
        public string member { get; set; }
        /// <summary>
        /// 遊戲編號
        /// </summary>
        public string game_id { get; set; }
        /// <summary>
        /// 遊戲流水號
        /// </summary>
        public string game_serial { get; set; }
        /// <summary>
        /// 遊戲類型
        /// </summary>
        public int game_type { get; set; }
        /// <summary>
        /// 遊戲回合id
        /// </summary>
        public string round_id { get; set; }
        /// <summary>
        /// 下注金額
        /// </summary>
        public decimal bet_amount { get; set; }
        /// <summary>
        /// 遊戲贏分(未扣除手續費)
        /// </summary>
        public decimal payout_amount { get; set; }
        /// <summary>
        /// 有效金額
        /// </summary>
        public decimal valid_amount { get; set; }
        /// <summary>
        /// 下注狀態
        /// 1	正常
        /// 2	退款
        /// 3	拒絕投注
        /// 4	注單作廢
        /// 5	取消
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 手續費
        /// </summary>
        public decimal fee_amount { get; set; }
        /// <summary>
        /// 彩金金額
        /// </summary>
        public decimal jp_amount { get; set; }
    }
}