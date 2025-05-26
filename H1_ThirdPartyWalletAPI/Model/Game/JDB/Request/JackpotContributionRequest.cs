using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class JackpotContributionRequest : RequestBaseModel
    {
        public override int Action => 43;
        public string uid { get; set; }

        public int JackPotFlag { get; set; } // 0:on , 1:off
    }
}
