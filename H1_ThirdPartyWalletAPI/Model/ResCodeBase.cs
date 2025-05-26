using H1_ThirdPartyWalletAPI.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model
{
    public class ResCodeBase
    {
        /// <summary>
        /// api response code
        /// 0:success others:fail  
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// api response error message
        /// </summary>
        public string Message { get; set; }
        public ResCodeBase()
        {
            code = (int)ResponseCode.Success;
            Message = MessageCode.Message[(int)ResponseCode.Success];
        }

        public static ResCodeBase Success => new();

        public static ResCodeBase Failure { 
            get {
                var result = new ResCodeBase
                {
                    code = (int)ResponseCode.Fail,
                    Message = MessageCode.Message[(int)ResponseCode.Fail]
                };
                return result;
            }
        }
    }
}
