using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using H1_ThirdPartyWalletAPI.Model.DataModel;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetWalletSessionV2Req
    {
        /// <summary>
        /// User Id
        /// </summary>
        [StringLength(20)]
        public string Club_id { get; set; }
        /// <summary>
        /// Franchiser Id
        /// </summary>
        /// [StringLength(20)]
        public string Franchiser { get; set; }
    }
    public class GetWalletSessionV2Res : ResCodeBase
    {
        /// <summary>
        /// 玩家錢包Session資料
        /// </summary>
        public List<WalletSessionV2> Data { get; set; }
    }
    public class GetUserWalletSessionReq
    {
        /// <summary>
        /// User Id
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Club_id { get; set; }
    }
    public class GetUserWalletSessionRes : ResCodeBase
    {
        /// <summary>
        /// 玩家錢包Session資料
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// 玩家錢包餘額
        /// </summary>
        public decimal Amount { get; set; }
    }
    public class WalletSessionClub
    {
        public short status { get; set; }
        public string club_id { get; set; }
        public  WalletSessionClub(short r_status, string r_club_id)
        {
            status = r_status;
            club_id = r_club_id;
        }
    }
    public class GetClubSessionV2Res : ResCodeBase
    {
        /// <summary>
        /// 玩家錢包Session資料
        /// </summary>
        public List<WalletSessionClub> Data { get; set; }
        public GetClubSessionV2Res()
        {
            Data = new List<WalletSessionClub>();
        }
    }

    public class GetWalletSessionv2OptimizeReq
    {
        /// <summary>
        /// ID
        /// </summary>
        [Required]
        public Guid? Id { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        [Required]
        public DateTime? StartTime { get; set; }
    }
}
