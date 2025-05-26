using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.STANDARDS.Response
{
    public class ResponseBase<T>
    {
        /// <summary>
        /// 資料
        /// </summary>
        public T data { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public Status status { get; set; }

        public class Status
        {
            /// <summary>
            /// 狀態碼
            /// </summary>
            public string code { get; set; }
            /// <summary>
            /// 狀態訊息
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 回應時間 UNIX時間戳
            /// </summary>
            public int timestamp { get; set; }
        }
    }
}
