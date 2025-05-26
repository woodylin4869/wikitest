namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class GetBalanceRequest : SexyRequestBase
    {

        public string userIds { get; set; }

        /// <summary>
        /// 预设值为0   1 : 回传余额>0的资料  0 : 回传所有玩家的余额资料>=0 当alluser=1时此参数才凑效
        /// </summary>
        public int isFilterBalance { get; set; }
        public int alluser { get; set; }
    }
}
