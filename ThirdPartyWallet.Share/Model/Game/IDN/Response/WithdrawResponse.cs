namespace ThirdPartyWallet.Share.Model.Game.IDN.Response
{
    public class WithdrawResponse
    {
        /// <summary>
        /// 廠商流水號
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 提現額度
        /// </summary>
        public decimal Balance { get; set; }
    }
}