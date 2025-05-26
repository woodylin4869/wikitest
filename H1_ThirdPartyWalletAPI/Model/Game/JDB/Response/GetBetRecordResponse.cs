using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JDB.Response
{
    public class GetBetRecordResponse : ResponseBaseModel
    {
        public List<CommonBetRecord> Data { get; set; }
    }
}
