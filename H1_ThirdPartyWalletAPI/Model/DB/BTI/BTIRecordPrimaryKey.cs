using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.BTI.DBResponse
{
    public class BTIRecordPrimaryKey
    {
        /// <summary>
        /// 注单购买 ID 
        /// </summary>
        public string PurchaseID { get; set; }

        /// <summary>
        /// BetStatus 下注状态: (注意. “Declined” 拒绝是在系统有误才会有的。)
        /// Opened 未结算
        /// Won 赢
        /// Lost 输
        /// Half Won 半赢
        /// Half Lost 半输
        /// Canceled 取消
        /// Cashout 提前兑现
        /// Draw 平局
        /// Declined 拒绝
        /// </summary>
        public string BetStatus { get; set; }

        /// <summary>
        /// 下注时间（毫秒）
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 更新时间（毫秒）
        /// -> 第三層輸出的結算時間UpdateDate為null時 表示為未結算
        /// </summary>
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// BTI資料可被搜尋到的時間 不是下注時間 不是更新时间 (先不加入PK 應該有index就好 也不用輸出做何用途)
        /// </summary>
        //public DateTime SearchDateTime { get; set; }
    }
}
