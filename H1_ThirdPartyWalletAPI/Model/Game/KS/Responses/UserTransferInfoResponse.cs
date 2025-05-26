using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{
    public class UserTransferInfoResponse
    {

        /// <summary>
        /// 商户的转账订单号
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 会员的币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 会员的余额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 会员的额度（等同余额/此字段将被废弃，请使用 Money 字段）
        /// </summary>
        public decimal Credit { get; set; }



        /// <summary>
        /// 转账的发起时间
        /// </summary>
        public DateTime CreateAt { get; set; }


        /// <summary>
        /// 转账的用户名
        /// </summary>
        public string UserName { get; set; }


        /// <summary>
        /// 转账类型。 IN：转入游戏 / OUT：转出游戏
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 转账状态 示例：（None：未完成、Finish：已完成、Faild：失败）
        /// </summary>
        public string Status { get; set; }


        /// <summary>
        /// 转账之前的余额（废弃字段）
        /// </summary>
        public decimal BalanceBefore { get; set; }


        /// <summary>
        /// 转账之后的会员余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 当前余额（废弃字段）
        /// </summary>
        public decimal CurrentBalance { get; set; }
    }


}
