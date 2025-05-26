namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class GameLoginResponse : GetMetaDataDecryptBase
    {
        public int totalRows { get; set; }
        public Row[] rows { get; set; }

        public class Row
        {
            /// <summary>
            /// 進入遊戲的 token
            /// </summary>
            public string token { get; set; }

            /// <summary>
            /// 進入遊戲的連結 lang 預設為 en layout 參考版型表
            /// http://host/fruitevo/?token=*redirecturl=lang=zh-twlayout=1tableid=1
            /// </summary>
            public string url { get; set; }
            // https://g.bighit888.com//fruitevo//?token=ZTJhYmQ5ZmUtNTAwYS00ZDUwLWE3MWEtZWE2NTVhZjA0YmQx&redirecturl=&lang=zh-tw&layout=2
        }


    }
}
