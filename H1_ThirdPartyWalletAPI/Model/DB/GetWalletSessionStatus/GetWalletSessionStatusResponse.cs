using System.Runtime.InteropServices.ComTypes;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.DB.GetWalletSessionStatus;

public class GetWalletSessionStatusResponse
{
    public WalletSessionV2.SessionStatus Status;
    public string club_id { get; set; }
    public string franchiser_id { get; set; } //代理id
}
