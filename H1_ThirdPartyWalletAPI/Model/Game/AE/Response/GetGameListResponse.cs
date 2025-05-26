using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.AE.Response
{
    public class GetGameListResponse : AEResponseBase
    {
        public List<Game> games { get; set; }
    }

    public class Game
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool free_spin { get; set; }
        public Locale locale { get; set; }
    }

    public class Locale
    {
        public LangContent enUS { get; set; }
        public LangContent zhCN { get; set; }
        public LangContent zhTW { get; set; }
    }

    public class LangContent
    {
        public string name { get; set; }
    }


  

}
