namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses
{
    public class KickMemberResponses
    {
        /// <summary>
        /// 0:成功 101:會員不存在 102:會員不在線上
        /// </summary>
        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
