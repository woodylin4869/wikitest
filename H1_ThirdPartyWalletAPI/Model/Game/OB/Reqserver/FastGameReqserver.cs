
using System.Text.Json.Serialization;

namespace H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver
{
    public class FastGameReqserver
    {
        public string loginName { set; get; }
        public string loginPassword { set; get; }
        public int deviceType { set; get; }
        public int oddType { set; get; }
        public int lang { set; get; }
        public string backurl { set; get; }
        public int showExit { set; get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? gameTypeId { set; get; }
        public string timestamp { set; get; }

        public bool ShouldSerializegameTypeId()
        {
            // don't serialize the Manager property if an employee is their own manager
            return (gameTypeId != null);
        }
    }
}
