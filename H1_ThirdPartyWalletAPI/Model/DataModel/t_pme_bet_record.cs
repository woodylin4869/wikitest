using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_pme_bet_record
    {
        /// <summary>
        /// 注单ID
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 会员ID
        /// </summary>
        public long member_id { get; set; }

        /// <summary>
        /// 会员账号
        /// </summary>
        public string member_account { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public long merchant_id { get; set; }

        /// <summary>
        /// 商户账号
        /// </summary>
        public string merchant_account { get; set; }

        /// <summary>
        /// 上级商户ID
        /// </summary>
        public long parent_merchant_id { get; set; }

        /// <summary>
        /// 上级商户账号
        /// </summary>
        public string parent_merchant_account { get; set; }

        /// <summary>
        /// 是否测试会员 
        /// 0-正式 
        /// 1-测试
        /// </summary>
        public short tester { get; set; }

        /// <summary>
        /// 注单类型 
        /// 1-普通注单 
        /// 2-普通串关注单 
        /// 3-局内串关注单, 
        /// 4-复合玩法注单
        /// </summary>
        public short order_type { get; set; }

        /// <summary>
        /// 串关类型 
        /// 1普通注单 
        /// 2:2串1 
        /// 3:3串1 
        /// 4:4串1 
        /// 5:5串1 
        /// 6:6串1 
        /// 7:7串1 
        /// 8:8串1
        /// </summary>
        public short parley_type { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public long game_id { get; set; }

        /// <summary>
        /// 联赛ID
        /// </summary>
        public long tournament_id { get; set; }

        /// <summary>
        /// 联赛名称
        /// </summary>
        public string tournament { get; set; }

        /// <summary>
        /// 赛事ID
        /// </summary>
        public long match_id { get; set; }

        /// <summary>
        /// 赛事类型 
        /// 正常-1 
        /// 冠军-2 
        /// 大逃杀-3 
        /// 篮球-4 
        /// 主播盘-5 
        /// 足球-6
        /// </summary>
        public short match_type { get; set; }

        /// <summary>
        /// 盘口ID
        /// </summary>
        public long market_id { get; set; }

        /// <summary>
        /// 盘口中文名
        /// </summary>
        public string market_cn_name { get; set; }

        /// <summary>
        /// 队伍ID 
        /// 主客队用 , 拼接
        /// </summary>
        public string team_id { get; set; }

        /// <summary>
        /// 队伍名称 
        /// 主客队用 , 拼接
        /// </summary>
        public string team_names { get; set; }

        /// <summary>
        /// 队伍中文名称 
        /// 主客队用 , 拼接
        /// </summary>
        public string team_cn_names { get; set; }

        /// <summary>
        /// 队伍英文名称
        /// 主客队用 , 拼接
        /// </summary>
        public string team_en_names { get; set; }

        /// <summary>
        /// 投注项ID
        /// </summary>
        public long odd_id { get; set; }

        /// <summary>
        /// 投注项名称
        /// </summary>
        public string odd_name { get; set; }

        /// <summary>
        /// 第几局
        /// </summary>
        public int round { get; set; }

        /// <summary>
        /// 赔率
        /// </summary>
        public string odd { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal bet_amount { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal win_amount { get; set; }

        /// <summary>
        /// 赛事阶段 
        /// 1-初盘 
        /// 2-滚盘
        /// </summary>
        public short is_live { get; set; }

        /// <summary>
        /// 注单状态 
        /// 1-待确认 
        /// 2-已拒绝 
        /// 3-待结算 
        /// 4-已取消 
        /// 5-赢(已中奖) 
        /// 6-输(未中奖) 
        /// 7-已撤销 
        /// 8-赢半 
        /// 9-输半 
        /// 10-走水
        /// </summary>
        public short bet_status { get; set; }

        /// <summary>
        /// 确认方式 
        /// 1-自动确认 
        /// 2-手动待确认 
        /// 3-手动确认 
        /// 4-手动拒绝
        /// </summary>
        public short confirm_type { get; set; }

        /// <summary>
        /// 投注时间（毫秒）
        /// </summary>
        public DateTime bet_time { get; set; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public DateTime settle_time { get; set; }

        /// <summary>
        /// 赛事开始时间
        /// </summary>
        public DateTime match_start_time { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime update_time { get; set; }

        /// <summary>
        /// 结算次数
        /// </summary>
        public int settle_count { get; set; }

        /// <summary>
        /// 设备 
        /// 1-PC 
        /// 2-H5 
        /// 3-Android 
        /// 4-IOS
        /// </summary>
        public short device { get; set; }

        /// <summary>
        /// 投注IP
        /// </summary>
        public long bet_ip { get; set; }

        /// <summary>
        /// 基准分
        /// </summary>
        public string score_benchmark { get; set; }

        /// <summary>
        /// 币种编码
        /// </summary>
        public int currency_code { get; set; }

        /// <summary>
        /// 币种汇率
        /// </summary>
        public decimal exchange_rate { get; set; }


        #region Customization Field
        public Guid summary_id { get; set; }

        public decimal pre_bet_amount { get; set; }

        public decimal pre_win_amount { get; set; }
        #endregion
    }

    public class t_pme_bet_record_running
    {
        /// <summary>
        /// 注单ID
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 会员ID
        /// </summary>
        public long member_id { get; set; }

        /// <summary>
        /// 会员账号
        /// </summary>
        public string member_account { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public long merchant_id { get; set; }

        /// <summary>
        /// 商户账号
        /// </summary>
        public string merchant_account { get; set; }

        /// <summary>
        /// 上级商户ID
        /// </summary>
        public long parent_merchant_id { get; set; }

        /// <summary>
        /// 上级商户账号
        /// </summary>
        public string parent_merchant_account { get; set; }

        /// <summary>
        /// 是否测试会员 
        /// 0-正式 
        /// 1-测试
        /// </summary>
        public short tester { get; set; }

        /// <summary>
        /// 注单类型 
        /// 1-普通注单 
        /// 2-普通串关注单 
        /// 3-局内串关注单, 
        /// 4-复合玩法注单
        /// </summary>
        public short order_type { get; set; }

        /// <summary>
        /// 串关类型 
        /// 1普通注单 
        /// 2:2串1 
        /// 3:3串1 
        /// 4:4串1 
        /// 5:5串1 
        /// 6:6串1 
        /// 7:7串1 
        /// 8:8串1
        /// </summary>
        public short parley_type { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public long game_id { get; set; }

        /// <summary>
        /// 联赛ID
        /// </summary>
        public long tournament_id { get; set; }

        /// <summary>
        /// 联赛名称
        /// </summary>
        public string tournament { get; set; }

        /// <summary>
        /// 赛事ID
        /// </summary>
        public long match_id { get; set; }

        /// <summary>
        /// 赛事类型 
        /// 正常-1 
        /// 冠军-2 
        /// 大逃杀-3 
        /// 篮球-4 
        /// 主播盘-5 
        /// 足球-6
        /// </summary>
        public short match_type { get; set; }

        /// <summary>
        /// 盘口ID
        /// </summary>
        public long market_id { get; set; }

        /// <summary>
        /// 盘口中文名
        /// </summary>
        public string market_cn_name { get; set; }

        /// <summary>
        /// 队伍ID 
        /// 主客队用 , 拼接
        /// </summary>
        public string team_id { get; set; }

        /// <summary>
        /// 队伍名称 
        /// 主客队用 , 拼接
        /// </summary>
        public string team_names { get; set; }

        /// <summary>
        /// 队伍中文名称 
        /// 主客队用 , 拼接
        /// </summary>
        public string team_cn_names { get; set; }

        /// <summary>
        /// 队伍英文名称
        /// 主客队用 , 拼接
        /// </summary>
        public string team_en_names { get; set; }

        /// <summary>
        /// 投注项ID
        /// </summary>
        public long odd_id { get; set; }

        /// <summary>
        /// 投注项名称
        /// </summary>
        public string odd_name { get; set; }

        /// <summary>
        /// 第几局
        /// </summary>
        public int round { get; set; }

        /// <summary>
        /// 赔率
        /// </summary>
        public string odd { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal bet_amount { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        public decimal win_amount { get; set; }

        /// <summary>
        /// 赛事阶段 
        /// 1-初盘 
        /// 2-滚盘
        /// </summary>
        public short is_live { get; set; }

        /// <summary>
        /// 注单状态 
        /// 1-待确认 
        /// 2-已拒绝 
        /// 3-待结算 
        /// 4-已取消 
        /// 5-赢(已中奖) 
        /// 6-输(未中奖) 
        /// 7-已撤销 
        /// 8-赢半 
        /// 9-输半 
        /// 10-走水
        /// </summary>
        public short bet_status { get; set; }

        /// <summary>
        /// 确认方式 
        /// 1-自动确认 
        /// 2-手动待确认 
        /// 3-手动确认 
        /// 4-手动拒绝
        /// </summary>
        public short confirm_type { get; set; }

        /// <summary>
        /// 投注时间（毫秒）
        /// </summary>
        public DateTime bet_time { get; set; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public DateTime settle_time { get; set; }

        /// <summary>
        /// 赛事开始时间
        /// </summary>
        public DateTime match_start_time { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime update_time { get; set; }

        /// <summary>
        /// 结算次数
        /// </summary>
        public int settle_count { get; set; }

        /// <summary>
        /// 设备 
        /// 1-PC 
        /// 2-H5 
        /// 3-Android 
        /// 4-IOS
        /// </summary>
        public short device { get; set; }

        /// <summary>
        /// 投注IP
        /// </summary>
        public long bet_ip { get; set; }

        /// <summary>
        /// 基准分
        /// </summary>
        public string score_benchmark { get; set; }

        /// <summary>
        /// 币种编码
        /// </summary>
        public int currency_code { get; set; }

        /// <summary>
        /// 币种汇率
        /// </summary>
        public decimal exchange_rate { get; set; }


        #region Customization Field
        public Guid summary_id { get; set; }

        public string club_id { get; set; }

        public string franchiser_id { get; set; }
        #endregion
    }
}
