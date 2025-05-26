using static H1_ThirdPartyWalletAPI.Model.Game.PG.Response.GetPlayersWalletResponse;
using System;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Model.Game.TP.Response
{
    public class TpResponse<TDataType>
    {
        /// <summary>
        /// 狀態欄
        /// </summary>
        public TpStatus status { get; set; }

        /// <summary>
        /// 資料
        /// </summary>
        public TDataType data { get; set; }

        public bool IsSuccess => status.code == (int)error_code.success;

        public TpResponse<TDataType> EnsureSuccessStatusCode()
        {
            if (IsSuccess)
                return this;

            throw new ExceptionMessage(status.code, Enum.GetName(typeof(error_code), status.code));
        }
    }

    public class TpStatus
    {
        /// <summary>
        /// 狀態碼
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 狀態訊息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 回應時間
        /// </summary>
        public long timestamp { get; set; }
    }
}
