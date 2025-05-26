namespace H1_ThirdPartyWalletAPI.Model.Game.WM.Response
{
    public class HelloResponse
    {
        public int errorCode { get; set; }
        public string errorMessage { get; set; }

        /// <summary>
        /// 回传:HELLO
        /// </summary>
        public string result { get; set; }
    }
}
