using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PP.Responses
{
    public class EnvironmentResponses
    {

        public string error { get; set; }
        public string description { get; set; }
        public List<Environment> environments { get; set; }


        public class Environment
        {
            public string envName { get; set; }
            public string apiDomain { get; set; }
        }
    }
}
