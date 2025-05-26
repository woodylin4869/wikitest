using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetMemberBalanceReq
    {
        /// <summary>
        /// H1 club_id
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public string Club_id { get; set; }     
    }
    public class GetMemberBalance : ResCodeBase
    {
        /// <summary>
        /// H1 club_id
        /// </summary>
        [Required]
        [StringLength(20)]
        [DefaultValue("10003")]
        public List<MemberBalance> Data { get; set; }
    }
    public class MemberBalance : ResCodeBase
    {
        /// <summary>
        /// 錢包位置  1.W1 2.SABA
        /// </summary>
        public string Wallet { get; set; }
        /// <summary>
        /// 錢包餘額
        /// </summary>
        public decimal Amount { get; set; }
    }
}
