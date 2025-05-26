using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class GetGameInfoStateListResponse : ResponseBaseModel<MessageResult>
    {
        public List<DSGameInfo> game_info_state_list { get; set; }
    }
    public class DSGameInfo
    {
        public string id { get; set; }
        public string type { get; set; }
        public bool active { get; set; }
        public LangNames names { get; set; }
    }

    public class LangNames
    {
        public string en_us { get; set; }
        public string th_th { get; set; }
        public string vi_vn { get; set; }
        public string zh_cn { get; set; }
    }
}
