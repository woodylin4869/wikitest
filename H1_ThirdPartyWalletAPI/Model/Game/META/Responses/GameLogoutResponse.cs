namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class GameLogoutResponse : GetMetaDataDecryptBase
    {
        /// <summary>
        /// true：登出成功 false：代理狀態異常
        /// </summary>
        public bool Logout { get; set; }
    }
}
