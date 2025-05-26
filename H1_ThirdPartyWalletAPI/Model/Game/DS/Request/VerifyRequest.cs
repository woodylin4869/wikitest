using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class VerifyRequest : RequestBaseModel
    {
        public Guid Serial { get; set; }
        public string Account { get; set; }
    }

}
