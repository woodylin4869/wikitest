namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 存取款單交易狀態
    /// </summary>
    public class GetWDTResponse
    {

        /// <summary>
        /// 操作批次號
        /// </summary>
        public string SerialKey { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
            public long Timestamp { get; set; }
        /// <summary>
        /// 錯誤代碼
        /// </summary>
            public int Code { get; set; }
        /// <summary>
        /// 訊息說明
        /// </summary>
            public string Message { get; set; }
            public Datu[] Data { get; set; }
        }

        public class Datu
        {
        /// <summary>
        /// 合作商交易唯一單據號
        /// </summary>
        public string TicketNo { get; set; }
        /// <summary>
        /// 支付單號
        /// </summary>
        public int PaymentId { get; set; }
        /// <summary>
        /// 合作商名稱
        /// </summary>
        public string PartnerName { get; set; }
        /// <summary>
        /// 會員在合作平台的標示
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// 交易狀態標示0申請 , 1成功, 2失敗
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 操作金額
        /// </summary>
        public float Amount { get; set; }
        /// <summary>
        /// 操作後金額
        /// </summary>
        public float Balance { get; set; }
        /// <summary>
        /// 操作日期
        /// </summary>
        public long CreateTs { get; set; }
        /// <summary>
        /// 操作日期
        /// </summary>
        public int CreateTsInt { get; set; }
        /// <summary>
        /// 更新日期
        /// </summary>
        public long UpdateTs { get; set; }
        }

    }

