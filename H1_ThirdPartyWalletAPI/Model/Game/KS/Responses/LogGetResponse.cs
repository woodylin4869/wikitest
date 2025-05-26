using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{
    public class LogGetResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public int RecordCount { get; set; }

        /// <summary>
        /// 查询页码（默认1）
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页记录数量（默认：20，最大值：1024）
        /// </summary>
        public int PageSize { get; set; }

        ///// <summary>
        ///// ??
        ///// </summary>
        //public object data { get; set; }


        public List<Record> list { get; set; }
    }



    public class Record
    {
        public string club_id { get; set; }

        public string franchiser_id { get; set; }
        public Guid summary_id { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderID { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 订单类型。详情参见本章节订单类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 订单状态。详情参见本章节订单状态 None 等待开奖 / Cancel 比赛取消 / Win 赢 /Lose 输 / Revoke 无效订单
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 有效投注金额
        /// </summary>
        public decimal BetMoney { get; set; }

        /// <summary>
        /// 盈亏金额
        /// </summary>
        public decimal Money { get; set; }


        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 结果产生时间
        /// </summary>
        public DateTime? ResultAt { get; set; }

        /// <summary>
        /// 派奖时间
        /// </summary>
        public DateTime? RewardAt { get; set; }

        /// <summary>
        /// 订单数据的更新时间
        /// </summary>
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// 投注IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 语言环境
        /// </summary>

        public string Language { get; set; }

        ///// <summary>
        ///// 设备
        ///// </summary>
        //public string[] Platform { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 是否是测试订单
        /// </summary>
        public int IsTest { get; set; }

        /// <summary>
        /// 本订单重新结算的次数     单关订单/串关订单/主播订单
        /// </summary>
        public int ReSettlement { get; set; }

        /// <summary>
        /// 盘口类型（参见附录7：盘口类型）  单关订单/串关订单/主播订单/虚拟电竞订单
        /// </summary>
        public string OddsType { get; set; }

        /// <summary>
        /// 赔率
        /// </summary>
        public decimal? Odds { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public string CateID { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 投注内容 单关订单/趣味游戏/主播订单
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 开奖结果 单关订单/趣味游戏/主播订单
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 比赛ID  单关订单
        /// </summary>
        public string MatchID { get; set; }

        /// <summary>
        /// 比赛标题
        /// </summary>
        public string Match { get; set; }

        /// <summary>
        /// 盘口ID
        /// </summary>
        public string BetID { get; set; }

        /// <summary>
        /// 盘口名称
        /// </summary>
        public string Bet { get; set; }

        /// <summary>
        /// 改單前投注金额
        /// </summary>
        public decimal Pre_BetAmount { get; set; }

        /// <summary>
        /// 改單前有效投注金额
        /// </summary>
        public decimal Pre_BetMoney { get; set; }

        /// <summary>
        /// 改單前盈亏金额
        /// </summary>
        public decimal Pre_Money { get; set; }


        /// <summary>
        /// 联赛名称
        /// </summary>
        public string League { get; set; }

        public Detail[] Details { get; set; }
    }

    public class Detail
    {
        public string DetailID { get; set; }
        public string CateID { get; set; }
        public string Category { get; set; }
        public string LeagueID { get; set; }
        public string League { get; set; }
        public string MatchID { get; set; }
        public string Match { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string BetID { get; set; }
        public string Bet { get; set; }
        public string Content { get; set; }
        public string ResultAt { get; set; }
        public string Result { get; set; }
        public string OddsType { get; set; }
        public string Odds { get; set; }
        public string Status { get; set; }
    }

}
