using System;
using System.Collections.Generic;
using System.Text;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Request
{
    public class GetDemoAccountToken : RequestBaseModel
    {
        public string Lang { get; set; }
        public string GType { get; set; }

        public string MType { get; set; }

        public string WindowMode { get; set; } 

        public bool IsApp { get; set; }

        public string lobbyURL { get; set; }

        public int MoreGame { get; set; }

        public bool IsShowDollarSign { get; set; }

        public int Mute { get; set; }

    }
}
