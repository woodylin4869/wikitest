using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
	public class WalletSession
	{
		public long id { get; set; }
		public Guid session_start_id { get; set; }
		public Guid session_end_id { get; set; }
		public DateTime start_time { get; set; }
		public DateTime end_time { get; set; }
		public decimal start_balance { get; set; }
		public decimal end_balance { get; set; }
		public decimal amount_change { get; set; }
		public decimal netwin { get; set; }
		public SessionStatus status { get; set; }
		public string club_id { get; set; }
		public DateTime update_time { get; set; }
		public decimal total_in { get; set; }
		public decimal total_out { get; set; }
		public WalletSession()
		{
			update_time = DateTime.Now;
			status = SessionStatus.INIT;
		}

		public enum SessionStatus
		{
			ALL = 99,
			INIT = 0,
			UNSETTLE = 1,
			SUCCESS = 2,
			FAIL = 3,
			RUNNING = 4,
			
		}

	}

}
