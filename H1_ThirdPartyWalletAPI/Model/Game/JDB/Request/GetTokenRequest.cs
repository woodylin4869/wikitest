using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetTokenRequest : RequestBaseModel
    {
        public override int Action => 11;
        public string Uid { get; set; }
        public string Lang { get; set; }
        public string gType { get; set; }
        public string mType { get; set; }
        public string remark { get; set; }
        public string windowMode { get; set; }
        public bool IsApp { get; set; }
        public string lobbyURL { get; set; }
        public int moreGame { get; set; }
        public int mute { get; set; }
        public string CardGameGroup { get; set; }
        public bool isShowDollarSign { get; set; }

    }

    //public class GetTokenRequest: RequestBaseModel
    //{
    //    public string parent { get; set; }
    //    public string uid { get; set; }
    //    public int balance { get; set; }
    //    public int gType { get; set; }
    //    public int mType { get; set; }
    //}

}
