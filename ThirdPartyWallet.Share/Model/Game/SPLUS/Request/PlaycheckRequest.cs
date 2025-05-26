namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Request
{
    public class PlaycheckRequest
    {
        /// <summary>
        /// 注單單號 (注單唯一值)
        /// </summary>
        public string bet_id {  get; set; }
        /// <summary>
        /// 語系(預設en-us)
        /// </summary>
        public string lang {  get; set; }

    }
}
