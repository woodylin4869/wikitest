using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class GetOnlineMembersResponse : ResponseBaseModel<MessageResult>
    {
        public List<OnlineMember> List { get; set; }
    }

    public class OnlineMember
    {
        public string agent { get; set; }
        public string account { get; set; }
        public string game_id { get; set; }
        public DateTime login_time { get; set; }
    }

   


}
