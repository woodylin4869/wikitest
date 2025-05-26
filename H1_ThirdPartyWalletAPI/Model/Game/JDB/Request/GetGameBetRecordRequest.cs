using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetGameBetRecordRequest : RequestBaseModel
    {
        public override int Action => 29;
        public DateTime Starttime { get; set; }

        public DateTime Endtime { get; set; }
    }
}
