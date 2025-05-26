namespace H1_ThirdPartyWalletAPI.Model.Game.PME.Response
{
    public class TicketOrderQueryResponse : BaseResponse
    {
        public new bool status { get; set; }

        public override bool IsSuccess { get{ return status; } }

        public long lastOrderID { get; set; }

        public int pageSize { get; set; }

        public int total { get; set; }

        public TicketOrder[] ticketOrder { get; set; }

        public class TicketOrder
        {
            /// <summary>
            /// 注单ID
            /// </summary>
            public long order_id { get; set; }

            /// <summary>
            /// 期号
            /// </summary>
            public string ticket_plan_no { get; set; }

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
            /// 顶级商户Id
            /// </summary>
            public long top_merchant_id { get; set; }

            /// <summary>
            /// 顶级商户账号
            /// </summary>
            public string top_merchant_account { get; set; }

            /// <summary>
            /// 游戏ID
            /// </summary>
            public long game_id { get; set; }

            /// <summary>
            /// 投注金额 （元）
            /// </summary>
            public decimal bet_amount { get; set; }

            /// <summary>
            /// 投注号码
            /// </summary>
            public string bet_num { get; set; }

            /// <summary>
            /// 彩系名称
            /// </summary>
            public string series_name { get; set; }

            /// <summary>
            /// 投注时间（毫秒）
            /// </summary>
            public long bet_time { get; set; }

            /// <summary>
            /// 彩系开始时间
            /// </summary>
            public long plan_sales_start_time { get; set; }

            /// <summary>
            /// 投注注数
            /// </summary>
            public int bet_nums { get; set; }

            /// <summary>
            /// 投注倍数
            /// </summary>
            public decimal bet_multiple { get; set; }

            /// <summary>
            /// 注单状态 3-待结算 5-已中奖 6-未中奖 7-已撤销
            /// </summary>
            public short bet_status { get; set; }

            /// <summary>
            /// 撤单状态 0未撤单 1撤单
            /// </summary>
            public int cancel_status { get; set; }

            /// <summary>
            /// 撤单类型
            /// 1-个人撤单
            /// 2-系统撤单
            /// 3:中奖停追撤单
            /// 4：不中停追撤单
            /// </summary>
            public int cancel_type { get; set; }

            /// <summary>
            /// 玩法群名
            /// </summary>
            public string play_level { get; set; }

            /// <summary>
            /// 玩法名
            /// </summary>
            public string play_name { get; set; }

            /// <summary>
            /// 前台投注内容
            /// </summary>
            public string bet_content { get; set; }

            /// <summary>
            /// 中奖金额）
            /// </summary>
            public decimal win_amount { get; set; }

            /// <summary>
            /// 中奖注数
            /// </summary>
            public int win_nums { get; set; }

            /// <summary>
            /// 追号id
            /// </summary>
            public long chase_id { get; set; }

            /// <summary>
            /// 注单类型
            /// 0.普通注单
            /// 1.追号注单
            /// </summary>
            public int chase_order { get; set; }

            /// <summary>
            /// 结算时间
            /// </summary>
            public long settle_time { get; set; }

            /// <summary>
            /// 更新时间
            /// </summary>
            public long update_time { get; set; }

            /// <summary>
            /// 理论奖金
            /// </summary>
            public decimal theory_bonus { get; set; }

            /// <summary>
            /// 设备 
            /// 1-PC 
            /// 2-H5
            /// 3-Android 
            /// 4-IOS
            /// </summary>
            public int device { get; set; }

            /// <summary>
            /// 币种编码 
            /// </summary>
            public int currency_code { get; set; }

            /// <summary>
            /// 是否测试账户 1 测试 0 非测试
            /// </summary>
            public int tester { get; set; }

            /// <summary>
            /// 币种汇率
            /// </summary>
            public decimal exchange_rate { get; set; }

            /// <summary>
            /// 赔率
            /// </summary>
            public string odd { get; set; }

            /// <summary>
            /// 结算状态 
            /// 0待结算 
            /// 1正常结算 
            /// 2 二次结算
            /// </summary>
            public int settle_status { get; set; }

            /// <summary>
            /// 盘口名（中文）
            /// </summary>
            public string play_name_cn { get; set; }

            /// <summary>
            /// 盘口名（英文）
            /// </summary>
            public string play_name_en { get; set; }

            /// <summary>
            /// 注单详情名 （英文）
            /// </summary>
            public string bet_content_en { get; set; }

            /// <summary>
            /// 注单详情名 （中文）
            /// </summary>
            public string bet_content_cn { get; set; }

            /// <summary>
            /// 彩系名称 （中文）
            /// </summary>
            public string ticket_name_cn { get; set; }

            /// <summary>
            /// 彩系名称（英文）
            /// </summary>
            public string ticket_name_en { get; set; }
        }
    }
}
