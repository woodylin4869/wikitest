using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
	public class WalletSessionV2
	{
		public Guid session_id { get; set; }  //pk
		public DateTime start_time { get; set; }
		public DateTime end_time { get; set; }
		public decimal start_balance { get; set; } //開分餘額(期初餘額)
		public decimal end_balance { get; set; }   //洗分餘額(期末餘額)
		public decimal amount_change { get; set; } //異動餘額
		public decimal netwin { get; set; } //遊戲輸贏
		public SessionStatus status { get; set; }  //partition
		public string club_id { get; set; }
		public DateTime update_time { get; set; }
		public decimal total_in { get; set; } //遊戲總入
		public decimal total_out { get; set; } //遊戲總出
		public Int16 push_times { get; set; } //累計推送次數
		public string franchiser_id { get; set; } //代理id
		public WalletSessionV2()
		{
			update_time = DateTime.Now;
			status = SessionStatus.INIT;
		}
		public enum SessionStatus
		{
			INIT = 0, 
			DEPOSIT = 1, //開分中
			WITHDRAW = 2, //發起洗分
			REFUND = 3,	//轉出完成,未退款
			UNSETTLE = 4, //已退款注單未結算
			SETTLE = 5,  //結算完成
		}

	}

}
