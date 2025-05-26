namespace H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request
{
    public class UpdateBetLimitRequest : SexyRequestBase
    {
        public string userId { get; set; }
        public string betLimit { get; set; }
    }
}
