using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Request
{
    public class LogGetRequest
    {
        /// <summary>
        /// 要查询的订单类型，默认值为All。 参数值参见本章节”订单类型”
        ///  None 等待开奖
        ///  Cancel 比赛取消
        ///  Win 赢
        ///  Lose 输
        ///  Revoke 无效订单
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 查询类型，默认值为 UpdateAt。 详情参见本章节“查询类型” UpdateAt / CreateAt / ResultAt /  RewardAt / UserName
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 时间范围开始时间（东八区） / 格式：yyyy-MM-dd HH:mm:ss
        /// </summary>
        [Required]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 时间范围结束时间（东八区） / 格式：yyyy-MM-dd HH:mm:ss
        /// </summary>
        [Required]
        public DateTime EndAt { get; set; }


        /// <summary>
        /// 查询单个用户的订单记录 / Type = UserName 时生效
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 查询页码（默认1）
        /// </summary>
        public int PageIndex { get; set; }


        /// <summary>
        /// 每页记录数量（默认：20，最大值：1024）
        /// </summary>
        public int PageSize { get; set; }

    }
}
