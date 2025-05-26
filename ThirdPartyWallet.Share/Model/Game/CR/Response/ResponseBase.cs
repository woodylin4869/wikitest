namespace ThirdPartyWallet.Share.Model.Game.CR.Response
{
    public class ApiResponseBase
    {
        public string Method { get; set; }
        public string ResponeId { get; set; }
        public string RespCode { get; set; }
        public string Status { get; set; }
        public long Timestamp { get; set; }

        public string Error { get; set; }
    }

    public interface IRespCode
    {
        public string RespCode { get; set; }
    }


}