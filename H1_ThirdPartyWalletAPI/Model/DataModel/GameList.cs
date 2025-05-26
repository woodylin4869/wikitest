using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class GameList
    {
		public long id { get; set; }
		public string platform { get; set; }
		public string game_name_mm { get; set; }
		public string game_name_en { get; set; }
		public string game_name_ch { get; set; }
		public string game_name_th { get; set; }
		public string game_name_vn { get; set; }
		public string game_type { get; set; }
		public string game_no { get; set; }
		public bool popular_game { get; set; }
		public bool enable_game { get; set; }
		public bool new_game { get; set; }
		public bool recommend_game { get; set; }
		public string icon { get; set; }
		public GameList()
        {
			enable_game = true;
		}
	}
}
