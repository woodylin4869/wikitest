using System;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class CheckBalanceRequest : RequestBaseModel
    {
        public string Account { get; set; }
    }
   
}
