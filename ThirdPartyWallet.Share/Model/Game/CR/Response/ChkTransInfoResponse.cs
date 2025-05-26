namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class ChkTransInfoResponse : ApiResponseBase
    {
        public Transdata[] transdata { get; set; }
    }


    public class Rootobject
    {
        public string method { get; set; }
        public string responeid { get; set; }
        public string respcode { get; set; }
  
        public string status { get; set; }
        public string timestamp { get; set; }
    }

    public class Transdata
    {
        public string date { get; set; }
        public string gold { get; set; }
        public string payno { get; set; }
        public string memname { get; set; }
        public string pay { get; set; }
        public string currency { get; set; }
        public string id { get; set; }
        public string paycash { get; set; }
        public string recid { get; set; }
        public string memid { get; set; }
    }

}