using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public interface IAuthErrorMessage
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string hint { get; set; }
        public string message { get; set; }

    }

    public class AuthResponseBase : IAuthErrorMessage
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string hint { get; set; }
        public string message { get; set; }

    }
    public interface IMessage
    {
        /// <summary>
        /// 狀態(true:成功; false:失敗)
        /// </summary>
        public bool success { get; set; }

        public int response_code { get; set; }
        public string Message { get; set; }

        public Dictionary<string, List<string>> Errors { get; set; }

    }
    public class ResponseBase<T> : IMessage
    {
        public ResponseBase()
        {
            Errors = new Dictionary<string, List<string>>();
        }
        /// <summary>
        /// 狀態(true:成功; false:失敗)
        /// </summary>
        public bool success { get; set; }
        public int response_code { get; set; }
        public string Message { get; set; }
        public T data { get; set; }

        public Dictionary<string, List<string>> Errors { get; set; }

    }

    //public class ResponseBase<T> : IMessage
    //{
    //    public ResponseBase()
    //    {
    //    }

    //    public string error { get; set; }
    //    public string error_description { get; set; }
    //    public string hint { get; set; }
    //    public string message { get; set; }

    //}

    public class ResponseBaseWithMeta<T> where T : class
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