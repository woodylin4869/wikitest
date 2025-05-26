using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Request
{
    public class RequestModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_agentcode"></param>
        /// <param name="_currency"></param>
        /// <param name="_params"></param>
        /// <param name="_sign"></param>
        public RequestModel(string _agentcode, string _currency, string _params, string _sign)
        {
            AgentCode = _agentcode;
            Currency = _currency;
            Params = _params;
            Sign = _sign;
        }

        public string AgentCode { get; set; }
        public string Currency { get; set; }
        public string Params { get; set; }
        public string Sign { get; set; }
    }

    //public class RequestBaseModel
    //{
    //    public string agent { get; set; }
    //}
    //public class GetOnlineMemberBalanceRequest : RequestBaseModel
    //{
    //}

}
