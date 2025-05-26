namespace H1_ThirdPartyWalletAPI.Model.Game.META.Response
{
    public class METABaseErroRespones
    {
        public bool status { get; set; }
        public int code { get; set; }
        public string errMsg { get; set; }
    }

    public class METABaseStatusRespones
    {
        public bool status { get; set; }
        public int code { get; set; }
        public string errMsg { get; set; }

        public string data { get; set; }
    }

    public class GetMetaDataDecryptBase
    {
        private bool decryptStatus = false;

        public bool DecryptStatus { get => decryptStatus; set => decryptStatus = value; }

        public int code { get; set; }
        public string errMsg { get; set; }
    }

}
