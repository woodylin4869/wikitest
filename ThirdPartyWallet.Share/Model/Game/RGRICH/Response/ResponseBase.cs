using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Response
{
    public interface IMessage
    {
        string Message { get; set; }
    }

    public class ResponseBase<T> : IMessage
    {
        public ResponseBase()
        {
        }

        /// <summary>
        /// 狀態(true:成功; false:失敗)
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 回傳物件
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// ErrorCode，Success( http status 200) 不會提供
        /// </summary>
        public int? Code { get; set; }
    }

    public class ResponseBaseWithMeta<T> : ResponseBase<T> where T : class
    {
        public ResponseBaseWithMeta()
        {
        }

        /// <summary>
        /// 回傳物件
        /// </summary>
        public Meta Meta { get; set; }
    }

    public class Meta
    {
        public Meta()
        {
        }

        /// <summary>
        /// 最後頁碼
        /// </summary>
        public int Last_page { get; set; }

        /// <summary>
        /// 一頁幾筆
        /// </summary>
        public int Per_page { get; set; }

        /// <summary>
        /// 當前頁
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 數據總數
        /// </summary>
        public int Total { get; set; }
    }

    public class ErrorModel
    {
        public object[] Data { get; set; }
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}