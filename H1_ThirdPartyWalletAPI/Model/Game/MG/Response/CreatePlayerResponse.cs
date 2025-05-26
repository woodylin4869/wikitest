using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    /// <summary>
    /// Create player 创建玩家
    /// </summary>
    public class CreatePlayerResponse
    {
        public string PlayerId { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreateDateUTC { get; set; }

        public string Uri { get; set; }
    }
}
