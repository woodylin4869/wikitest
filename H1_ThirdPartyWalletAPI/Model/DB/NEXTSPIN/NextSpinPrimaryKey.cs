using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.NEXTSPIN
{
    public class NextSpinPrimaryKey : IEquatable<NextSpinPrimaryKey>
    {

        /// <summary>
        /// 下注单号
        /// </summary>
        public long ticketId { get; set; }

        /// <summary>
        /// 下注时间
        /// </summary>
        public DateTime ticketTime { get; set; }

        /// <summary>
        /// 判斷指定的 <see cref="NextSpinPrimaryKey"/> 是否等於當前的 <see cref="NextSpinPrimaryKey"/>。
        /// </summary>
        /// <param name="other">與當前對象進行比較的 <see cref="NextSpinPrimaryKey"/>。</param>
        /// <returns>如果指定的 <see cref="NextSpinPrimaryKey"/> 等於當前的 <see cref="NextSpinPrimaryKey"/>，則為 true；否則為 false。</returns>
        public bool Equals(NextSpinPrimaryKey other)
        {
            if (other == null)
                return false;

            // 检查引用是否相同（如果是同一实例，则肯定相等）
            if (ReferenceEquals(this, other))
                return true;

            return this.ticketId == other.ticketId && this.ticketTime == other.ticketTime;
        }

        /// <summary>
        /// 為當前 <see cref="NextSpinPrimaryKey"/> 返回一個適合做為哈希算法和數據結構（如哈希表）的哈希碼。
        /// </summary>
        /// <returns>一個哈希碼，用於哈希算法和數據結構（如哈希表）。</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(ticketId, ticketTime);
        }

        /// <summary>
        /// 確定指定的對象是否等於當前的 <see cref="NextSpinPrimaryKey"/>。
        /// </summary>
        /// <param name="obj">與當前對象進行比較的對象。</param>
        /// <returns>如果指定的對象等於當前的 <see cref="NextSpinPrimaryKey"/>，則為 true；否則為 false。</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as NextSpinPrimaryKey);
        }
    }
}
