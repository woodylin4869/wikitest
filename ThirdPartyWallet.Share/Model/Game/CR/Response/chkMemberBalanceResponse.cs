namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class chkMemberBalanceResponse : ApiResponseBase
    {
        public string memname { get; set; }
        public string balance { get; set; }
     
        public string currency { get; set; }
  
        public string memid { get; set; }
    }
}