using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetOnlineUserReq
    {
        /// <summary>
        /// 遊戲平台名
        /// 1. SABA
        /// </summary>
        [StringLength(10)]
        [DefaultValue("SABA")]
        public string Platform { get; set; }
    }
    public class GetOnlineUser : ResCodeBase
    {
        /// <summary>
        /// 線上玩家清單
        /// </summary>
        public List<GetOnlineUserData> Data { get; set; }
    }

    public class GetOnlineUserData
    {
        public string club_id { get; set; }
        public string last_platform { get; set; }
    }

    public class GetOnlineUserCount : ResCodeBase
    {
        /// <summary>
        /// 線上玩家清單
        /// </summary>
        public List<OnlinUserCount> Data{ get; set; }
    }
    public class OnlinUserCount
    {
        /// <summary>
        /// 線上玩家數量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 遊戲館名稱
        /// </summary>
        public string Platform { get; set; }
    }



    public class GetOnlineUserListReq
    {
        /// <summary>
        /// 遊戲平台名
        /// 1. SABA
        /// 2. RCG
        /// </summary>
        [StringLength(10)]
        [DefaultValue("SABA")]
        public string Platform { get; set; }
        /// <summary>
        /// 經銷商
        /// </summary>
        [StringLength(20)]
        public string Franchiser { get; set; }
    }
    public class GetOnlineUserListRes : ResCodeBase
    {
        /// <summary>
        /// 線上玩家清單
        /// </summary>
        public List<OnlineUserData> UserList { get; set; }

        public GetOnlineUserListRes()
        {
            UserList = new List<OnlineUserData>();
        }
    }

    public class OnlineUserData
    {
        /// <summary>
        /// 使用者ID
        /// </summary>
        public string Club_id { get; set; }
        /// <summary>
        /// 使用者帳號
        /// </summary>
        public string Club_Ename { get; set; }
        /// <summary>
        /// 經銷商
        /// </summary>
        public string Franchiser { get; set; }
        /// <summary>
        /// 遊戲平台
        /// </summary>
        public string Platform { get; set; }
    }
}
