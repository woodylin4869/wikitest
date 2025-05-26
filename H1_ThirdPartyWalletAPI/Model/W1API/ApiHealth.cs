using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetApiHealthRes : ResCodeBase
    {
        /// <summary>
        /// 玩家錢包Session資料
        /// </summary>
        public List<ApiHealthInfo> Data { get; set; }
    }
    public class PutApiHealthReq
    {
        /// <summary>
        /// 遊戲商
        /// </summary>
        [Required]
        public string Platform { get; set; }
        /// <summary>
        /// API健康狀態
        /// </summary>
        [Required]
        [Range(0, 3)]
        public Status Status { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Operator { get; set; }
    }
    public class ApiHealthInfo
    {
        /// <summary>
        /// 遊戲商
        /// </summary>
        public string Platform { get; set; }
        /// <summary>
        /// API健康狀態
        /// </summary>
        public Status Status { get; set; }
        /// <summary>
        /// 10分鐘內請求次數
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 10分鐘內最高回應時間
        /// </summary>
        public long MaxElapsedMilliseconds { get; set; }
        /// <summary>
        /// 10分鐘內平均回應時間
        /// </summary>
        public long AvgMaxElapsedMilliseconds { get; set; }
        /// <summary>
        /// 10分鐘內Time out 次數
        /// </summary>
        public int TimeOutCount { get; set; }
        /// <summary>
        /// API暫停時間
        /// </summary>
        public DateTime SuspendTime { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
    public enum Status
    {
        NORMAL = 0,  //正常
        DELAY = 1,   //10分鐘內5次請求Response超過3000ms(告警)
        TIMEOUT = 2, //10分鐘內超過5次請求Timeout(暫時關閉轉入遊戲)
        MAINTAIN = 3 //遊戲館維護
    }
}
