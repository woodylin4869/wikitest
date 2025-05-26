using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.FC.Response
{
    public class GetHistoryRecordListResponse : FCBaseStatusRespones
    {
        public List<Record> Records { get; set; }
    }
}
