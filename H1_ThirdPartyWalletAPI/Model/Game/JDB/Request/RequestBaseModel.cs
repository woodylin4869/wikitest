using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class RequestBaseModel
    {
      
        public virtual int Action { get; }
        public long Ts { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public string Parent { get; set; }

    }
}
