using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class Schedule
    {
        public string groupid { get; set; }
        public int seq { get; set; }
        public string execute_cmd { get; set; }
        public string schedule_code { get; set; }
        public string last_result { get; set; }
        public DateTime updtime { get; set; }
        public bool last_status { get; set; }
    }
}