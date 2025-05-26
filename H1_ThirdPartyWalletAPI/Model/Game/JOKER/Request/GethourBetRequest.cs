using System.ComponentModel.DataAnnotations;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;

namespace H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request
{
    public class GethourBetRequest
    {
        public string Method { get; set; } = "TS";
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string NextId { get; set; }
        public int Delay { get; set; } = 0;
        public int Timestamp { get; set; } = Helper.GetCurrentTimestamp();
    }
}
