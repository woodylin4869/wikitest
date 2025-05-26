using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.PME
{
    public class PMERecordPrimaryKey
    {
        /// <summary>
        /// 注单ID
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 注单状态 
        /// 1-待确认 
        /// 2-已拒绝 
        /// 3-待结算 
        /// 4-已取消 
        /// 5-赢(已中奖) 
        /// 6-输(未中奖) 
        /// 7-已撤销 
        /// 8-赢半 
        /// 9-输半 
        /// 10-走水
        /// </summary>
        public short bet_status { get; set; }

        /// <summary>
        /// 投注时间（毫秒）
        /// </summary>
        public DateTime bet_time { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime update_time { get; set; }
    }
}
