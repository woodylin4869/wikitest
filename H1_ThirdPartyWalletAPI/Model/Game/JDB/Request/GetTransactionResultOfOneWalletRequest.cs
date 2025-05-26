using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetTransactionResultOfOneWalletRequest : RequestBaseModel
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

    }
}
