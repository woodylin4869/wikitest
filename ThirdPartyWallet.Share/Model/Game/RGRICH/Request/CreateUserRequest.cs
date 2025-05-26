using ThirdPartyWallet.Share.Model.Game.RGRICH.Request;

namespace ThirdPartyWallet.Share.Model.Game.RGRICH.Request
{
    public class CreateUserRequest : DataRequestBase
    {
        /// <summary>
        /// 玩家用戶名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 姓名或暱稱
        /// </summary>
        public string RealName { get; set; }
    }
}