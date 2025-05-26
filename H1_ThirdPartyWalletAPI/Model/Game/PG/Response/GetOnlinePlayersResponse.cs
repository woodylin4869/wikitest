using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.PG.Response
{
	/// <summary>
	/// 获取在线玩家列表
	/// </summary>
	public class GetOnlinePlayersResponse
	{
		public List<Data> data { get; set; }
		public Error error { get; set; }

		public class Data
		{
			/// <summary>
			/// 玩家的唯一标识符
			/// </summary>
			public string playerName { get; set; }
			/// <summary>
			/// 游戏的唯一标识符
			/// </summary>
			public int gameId { get; set; }
			/// <summary>
			/// 数据的更新时间（以毫秒为单位的 Unix 时间戳）
			/// </summary>
			public long rowVersion { get; set; }
		}
		public class Error
		{
			public string code { get; set; }
			public string message { get; set; }
			public string traceId { get; set; }
		}
	}
}