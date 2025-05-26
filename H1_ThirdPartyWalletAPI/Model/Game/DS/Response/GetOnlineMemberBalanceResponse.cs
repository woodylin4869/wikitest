using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class GetOnlineMemberBalanceResponse : ResponseBaseModel<MessageResult>
    {
        public List<OnlineMemberBalance> list { get; set; }
    }
    public class OnlineMemberBalance
    {
        public string agent { get; set; }
        public string account { get; set; }
        public string game_id { get; set; }
        public string login_time { get; set; }
        public decimal balance { get; set; }
    }

}
