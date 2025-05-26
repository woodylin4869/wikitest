using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetMaintenanceInfo : ResCodeBase
    {
        /// <summary>
        /// 各遊戲商維護資料
        /// </summary>
        public List<MaintenanceInfo> Data { get; set; }
    }

    public class MaintenanceInfo
    {
        /// <summary>
        /// 遊戲商平台
        /// </summary>
        public string Platform { get; set; }
        /// <summary>
        /// 是否在維護中
        /// </summary>
        public bool? IsMT { get; set; }
        /// <summary>
        /// 維護開始時間
        /// </summary>
        public DateTime? MTStartTime { get; set; }
        /// <summary>
        /// 維護結束時間
        /// </summary>
        public DateTime? MTEndTime { get; set; }
    }
}
