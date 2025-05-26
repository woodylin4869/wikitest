namespace ThirdPartyWallet.Share.Model.Game.SPLUS.Response
{
    public class HourbetlogResponse
    {
        /// <summary>
        /// 會員帳號
        /// </summary>
        public string account { get; set; }
        /// <summary>
        /// 總下注金額
        /// </summary>
        public decimal bet_amount { get; set; }
        /// <summary>
        /// 總有效下注金額
        /// </summary>
        public decimal bet_valid_amount { get; set; }
        /// <summary>
        /// 派彩金額 (純贏分)
        /// </summary>
        public decimal pay_off_amount { get; set; }
        /// <summary>
        /// 彩金獲得金額
        /// </summary>
        public decimal jp_win { get; set; }
        /// <summary>
        /// 住單數量
        /// </summary>
        public int Bet_quantity { get; set; }
    }
}
