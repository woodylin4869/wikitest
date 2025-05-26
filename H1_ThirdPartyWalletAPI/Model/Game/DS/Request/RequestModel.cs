using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Request
{
    public class RequestModel
    {
        public RequestModel(string channel, string data, string sign)
        {
            Channel = channel;
            Data = data;
            Sign = sign;
        }

        public string Channel { get; set; }
        public string Data { get; set; }
        public string Sign { get; set; }
    }

    public class RequestBaseModel { 
        public string agent { get; set; }
    }
    public class GetOnlineMemberBalanceRequest : RequestBaseModel
    {
    }

}
