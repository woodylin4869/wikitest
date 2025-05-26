using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class GetOnlineMemberResponse
    {
            public int ErrorCode { get; set; }
            public string Message { get; set; }
            public List<string> Data { get; set; }
    }
}
