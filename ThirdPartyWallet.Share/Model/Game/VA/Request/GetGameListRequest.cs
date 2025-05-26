namespace ThirdPartyWallet.Share.Model.Game.VA.Request
{
    public class GetGameListRequest
    {

        /// <summary>
        ///遊戲語系 (請參照支援語系表，預設為en-US)
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// 建構函數，初始化 RequestData 類
        /// </summary>
        /// <param name="channelId">渠道編號</param>
        /// <param name="lang">遊戲語系</param>
        public GetGameListRequest(string lang)
        {
            Lang = lang;
        }
    }
}
