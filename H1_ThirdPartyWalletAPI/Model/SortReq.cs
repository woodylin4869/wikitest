using H1_ThirdPartyWalletAPI.Utility;

namespace H1_ThirdPartyWalletAPI.Model
{
    public class SortReq
    {
        /// <summary>
        /// 排序欄位
        /// </summary>
        public virtual string? SortColumnName { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public virtual SortType? SortType { get; set; }
    }
}
