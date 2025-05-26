using Newtonsoft.Json;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class GetGameListResponse
    {


        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public List<Datum> Data { get; set; }


        public class Datum
        {
            public int GameId { get; set; }
            public Name name { get; set; }
            public int GameCategoryId { get; set; }
            public bool JP { get; set; }
        }

        public class Name
        {
            [JsonProperty("en-US")]
            public string enUS { get; set; }

            [JsonProperty("zh-CN")]
            public string zhCN { get; set; }

            [JsonProperty("zh-TW")]
            public string zhTW { get; set; }
        }

  


    }
}
