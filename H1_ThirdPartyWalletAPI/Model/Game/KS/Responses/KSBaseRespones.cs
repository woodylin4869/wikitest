using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.KS.Response
{

    public class KSBaseRespones<T>
    {
        public int success { get; set; }
        public string msg { get; set; }

        public string Error { get; set; }
        public T info { get; set; }
    }



    public class Info
    {
        public string Error { get; set; }
    }
}
