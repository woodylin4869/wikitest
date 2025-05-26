namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 用戶餘額
    /// </summary>
    public class BalanceResponse
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

            public Datum[] Data { get; set; }
        }

        public class Datum
        {
            /// <summary>
            /// 使用者名稱
            /// </summary>
            public string UserName { get; set; }
            /// <summary>
            /// 用戶可用餘額
            /// </summary>
            public decimal BetAmount { get; set; }
            /// <summary>
            /// 用戶未結算餘額
            /// </summary>
            public float Outstanding { get; set; }
        }

    }
