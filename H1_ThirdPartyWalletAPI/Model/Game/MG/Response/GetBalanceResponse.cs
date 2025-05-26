using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.MG.Response
{
    /// <summary>
    /// Get player details 获取玩家信息 (取得balance)
    /// </summary>
    public class GetBalanceResponse
    {
        /// <summary>
        /// 显示玩家是否被锁定
        /// </summary>
        public bool IsLocked { get; set; }
        public string PlayerId { get; set; }
        public string CreateDateUTC { get; set; }
        public Balance Balance { get; set; }
        public string Uri { get; set; }
    }

    public class Balance
    {
        public decimal total { get; set; }
        public bool isPartail { get; set; }
        
        public BalanceDetail Details { get; set; }
    }

    public class BalanceDetail
    {
        public WalletBalance WalletBalance { get; set; }
        public ProductsBalance ProductsBalance { get; set; }
    }

    public class WalletBalance
    {
        public decimal total { get; set; }
    }
    public class ProductsBalance
    {
        public decimal total { get; set; }

        public List<ProductsBalanceDetail> Details { get; set; }
    }

    public class ProductsBalanceDetail
    {
        public string ProductId { get; set; }
        public decimal Balance { get; set; }
    }
}
