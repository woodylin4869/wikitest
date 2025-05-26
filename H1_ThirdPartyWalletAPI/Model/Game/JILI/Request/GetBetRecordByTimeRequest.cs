using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.JILI.Request
{
    public class GetBetRecordByTimeRequest
    {
        /// <summary>
        /// 時間為UTC-4
        /// </summary>
        public DateTime StartTime{ get; set; }
        /// <summary>
        /// 時間為UTC-4
        /// </summary>
        public DateTime EndTime { get; set; }
        public int Page { get; set; }
        public int PageLimit { get; set; }
        /// <summary>
        /// 1:只撈取該(AgentId)代理的注單  0:全拉
        /// </summary>
        [Required]
        public int FilterAgent { get; set; }
    }
}
