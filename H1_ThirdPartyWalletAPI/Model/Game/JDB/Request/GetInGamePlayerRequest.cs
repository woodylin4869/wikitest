using System;
using System.Collections.Generic;
using System.Text;
namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetInGamePlayerRequest : RequestBaseModel
    {
        public string uid { get; set; }

        public override int Action => 52;
    }
}
