using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class ResponseBaseModel<TResponse> where TResponse : MessageResult
    {
        public TResponse result { get; set; }
    }
    public class MessageResult
    {
        public int code { get; set; }
        public string msg { get; set; }
    }
    public class TransactionMessageResult : MessageResult { 
        public DateTime? timestamp { get; set; }
    }
   
}
