using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Model.H1API
{
    public class RefundAmountReq
    {
        /// <summary>
        /// H1 Session_id
        /// </summary>
        [Required]
        public Guid Session_id { get; set; }
        /// <summary>
        /// 退款額度
        /// </summary>
        [Required]
        [Range(0, 100000000)]
        [DefaultValue(10)]
        public decimal Amount { get; set; }
        /// <summary>
        /// 遊戲館交易紀錄
        /// </summary>
        public List<WalletTransferRecord> GameTransferData { get; set; }
        public WalletSessionV2 SessionData { get; set; }
    }
    public class RefundAmountRes
    {
        public int code { get; set; }
        public string message { get; set; }
        public RefundAmountRes()
        {
            code = (int)ResponseCode.SessionRefundFail;
            message = MessageCode.Message[(int)ResponseCode.SessionRefundFail];
        }
        public RefundAmountRes(int Code, string Message)
        {
            code = Code;
            message = Message;
        }
    }
}
