using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Code
{
    public class ExceptionMessage : Exception
    {
        public int MsgId { get; set; }
        public ExceptionMessage() { }
        public ExceptionMessage(int msgId, string message) : base(message)
        {
            MsgId = msgId;
        }
        public ExceptionMessage(ResponseCode code) : base(MessageCode.Message[(int)code])
        {
            MsgId = (int)code;
        }
        public ExceptionMessage(ResponseCode code, string message) : base(MessageCode.Message[(int)code] + "|" + message)
        {
            MsgId = (int)code;
        }
    }
}
