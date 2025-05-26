using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Exceptions
{
    public class MGInternalException : Exception
    {
        public string Code { get; set; }

        public string ErrorMessage { get; set; }
        public MGInternalException(string code,string message) : base(message)
        {
            this.Code = code;
            this.ErrorMessage = message;
        }
    }
}
