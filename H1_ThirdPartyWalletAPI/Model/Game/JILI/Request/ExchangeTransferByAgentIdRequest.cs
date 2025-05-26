using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Request
{
    public class ExchangeTransferByAgentIdRequest
    {
        public string Account { get; set; }
        /// <summary>
        /// 可不填自動產生
        /// </summary>
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        /// <summary>
        /// 轉帳類型
        ///1: 從 遊戲商 轉移額度到 平臺商(不看 amount 值, 全部轉出)
        ///2: 從 平臺商 轉移額度到 遊戲商
        ///3: 從 遊戲商 轉移額度到 平臺商
        /// </summary>
        public Byte TransferType { get; set; }
        [Required]
        public DateTime Time { get; set; }
    }

}
