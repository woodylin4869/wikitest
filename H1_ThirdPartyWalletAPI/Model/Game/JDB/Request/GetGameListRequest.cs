using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetGameListRequest : RequestBaseModel
    {
        public override int Action => 49;
        public string Lang { get; set; }

    }
}
