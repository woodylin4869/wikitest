using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class GetgametypemappingRes : ResCodeBase
    {
        /// <summary>
        /// t_system_parameter資料
        /// </summary>
        public List<t_gametype_mapping> Data { get; set; }
    }
    public class t_gametype_mapping
    {
        public string game_id { get; set; }

        public string gametype { get; set; }

        public string groupgametype { get; set; }

        public int groupgametype_id { get; set; }

        public string memo { get; set; }
    }
}
