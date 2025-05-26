namespace H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response
{
    /// <summary>
    /// 取款
    /// </summary>
    public class WithdrawResponse
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
            /// 錯誤訊息
            /// </summary>
            public string Message { get; set; }

            public Data Data { get; set; }

        }
    
    }




