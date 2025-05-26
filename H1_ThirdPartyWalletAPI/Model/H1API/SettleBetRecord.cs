using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class SettleBetRecordReq
    {
        /// <summary>
        /// H1 Session_id
        /// </summary>
        [Required]
        public Guid Session_id { get; set; }
        /// <summary>
        /// Session注單紀錄
        /// </summary>
        public List<BetRecordSession> BetRecordData { get; set; }
    }
    public class SettleBetRecordRes : ResCodeBase
    {
        public SettleBetRecordRes()
        {
        }
        public SettleBetRecordRes(int code, string message)
        {
            this.code = code;
            this.Message = message;
        }
    }
}
