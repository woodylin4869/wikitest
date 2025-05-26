using System;

namespace H1_ThirdPartyWalletAPI.Model
{
    /// <summary>
    /// 補單日誌 
    /// </summary>
    public class RepairLogModel
    {
        public string GameId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }

        public string Message { get; set; }
    }

}