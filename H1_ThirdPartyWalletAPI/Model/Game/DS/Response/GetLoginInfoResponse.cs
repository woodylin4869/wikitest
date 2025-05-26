namespace H1_ThirdPartyWalletAPI.Model.Game.DS.Response
{
    public class GetLoginInfoResponse : ResponseBaseModel<MessageResult>
    { 
        public LoginInfo Login_Info { get; set; }
    }
    public class LoginInfo
    {
        public int online { get; set; }
    }
}
