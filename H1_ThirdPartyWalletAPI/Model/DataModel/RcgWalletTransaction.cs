using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
	public class RcgWalletTransaction
	{
		public Guid tran_id { get; set; }
		public Guid req_id { get; set; }
		public string desk_id { get; set; }
		public string game_name { get; set; }
		public string shoe_no { get; set; }
		public string round_no { get; set; }
		public DateTime create_datetime { get; set; }
		public decimal before_balance { get; set; }
		public decimal after_balance { get; set; }
		public string club_id { get; set; }
		public decimal amount { get; set; }
		public string franchiser_id { get; set; }
		public string tran_type { get; set; }
		public Guid summary_id { get; set; }
		public string tran_rid { get; set; }
	}

	public class JDBWalletTransaction
	{
		public Guid tran_id { get; set; }
		public Guid req_id { get; set; }
		public string desk_id { get; set; }
		public string game_name { get; set; }
		public string shoe_no { get; set; }
		public string round_no { get; set; }
		public DateTime create_datetime { get; set; }
		public decimal before_balance { get; set; }
		public decimal after_balance { get; set; }
		public string club_id { get; set; }
		public decimal amount { get; set; }
		public string franchiser_id { get; set; }
		public string tran_type { get; set; }
		public Guid summary_id { get; set; }
		public string tran_rid { get; set; }
	}
}