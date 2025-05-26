using H1_ThirdPartyWalletAPI.Code;
using System;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class PullRecordFailoverRequest
    {
        public Platform platform { get; set; }

        public string repairParameter { get; set; }

        public TimeSpan delay { get; set; }

        public DateTime requestTime { get; set; } = DateTime.Now;
    }

    public class PullRecordFailoverWithTimeOffset : PullRecordFailoverRequest
    {
        public TimeSpan OffTimeSpan { get; set; }
    }
}