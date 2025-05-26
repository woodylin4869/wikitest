using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetGameResultResponse : ResponseBaseModel
    {
        public List<GetGameResult> data { get; set; }
    }
    public class GetGameResult
    {
        public string path { get; set; }
    }
}
