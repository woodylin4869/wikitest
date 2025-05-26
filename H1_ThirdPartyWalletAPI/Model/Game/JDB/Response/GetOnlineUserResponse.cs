using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetOnlineUserResponse : ResponseBaseModel
    {
        public List<UserData> data { get; set; }
    }
    public class UserData
    {
        public string uid { get; set; }
        public decimal balance { get; set; }
    }
}
