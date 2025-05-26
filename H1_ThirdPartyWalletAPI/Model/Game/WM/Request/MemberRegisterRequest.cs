using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Request
{
    public class MemberRegisterRequest
    {
        public string cmd { get; set; }
        public string vendorId { get; set; }
        public string signature { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string username { get; set; }
        public string limitType { get; set; }
        public long timestamp { get; set; }
    }
}
