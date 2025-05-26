using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetGameHistoryRequest : RequestBaseModel
    {
        public override int Action => 64;
        public DateTime Starttime { get; set; }

        public DateTime Endtime { get; set; }
    }
}
