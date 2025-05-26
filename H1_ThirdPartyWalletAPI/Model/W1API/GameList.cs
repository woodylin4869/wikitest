using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;

namespace H1_ThirdPartyWalletAPI.Model.W1API
{
    public class GetGameListReq
    {
        /// <summary>
        /// 遊戲平台名
        /// 1. MG
        /// </summary>
        [StringLength(10)]
        [DefaultValue("MG")]
        public string Platform { get; set; }
        /// <summary>
        /// 頁數
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        [DefaultValue(0)]
        public int Page { get; set; }
        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Required]
        [Range(1, 1000)]
        [DefaultValue(10)]
        public int Count { get; set; }
    }
    public class PutGameListReq
    {
        /// <summary>
        /// 遊戲名稱-緬文       
        /// </summary>
        [StringLength(100)]
        public string game_name_mm { get; set; }
        /// <summary>
        /// 遊戲名稱-英文       
        /// </summary>
        [StringLength(100)]
        public string game_name_en { get; set; }
        /// <summary>
        /// 遊戲名稱-簡中       
        /// </summary>
        [StringLength(100)]
        public string game_name_ch { get; set; }
        /// <summary>
        /// 遊戲名稱-泰文       
        /// </summary>
        [StringLength(100)]
        public string game_name_th { get; set; }
        /// <summary>
        /// 遊戲名稱-越南       
        /// </summary>
        [StringLength(100)]
        public string game_name_vn { get; set; }
        /// <summary>
        /// 遊戲類別
        /// </summary>
        [StringLength(20)]
        public string game_type { get; set; }
        /// <summary>
        /// 遊戲開關
        /// </summary>
        public bool? enable_game { get; set; }
        /// <summary>
        /// 熱門遊戲
        /// </summary>
        public bool? popular_game { get; set; }
        /// <summary>
        /// 推薦遊戲
        /// </summary>
        public bool? recommend_game { get; set; }
        /// <summary>
        /// 新遊戲
        /// </summary>
        public bool? new_game { get; set; }
        /// <summary>
        /// 遊戲圖示
        /// </summary>
        [StringLength(255)]
        public string Icon { get; set; }
    }
    public class PostGameListReq
    {
        /// <summary>
        /// 遊戲平台       
        /// </summary>
        [StringLength(10)]
        public string platform { get; set; }
        /// <summary>
        /// 遊戲名稱-緬文       
        /// </summary>
        [StringLength(100)]
        public string game_name_mm { get; set; }
        /// <summary>
        /// 遊戲名稱-英文       
        /// </summary>
        [StringLength(100)]
        public string game_name_en { get; set; }
        /// <summary>
        /// 遊戲名稱-簡中       
        /// </summary>
        [StringLength(100)]
        public string game_name_ch { get; set; }
        /// <summary>
        /// 遊戲名稱-泰文       
        /// </summary>
        [StringLength(100)]
        public string game_name_th { get; set; }
        /// <summary>
        /// 遊戲名稱-越南       
        /// </summary>
        [StringLength(100)]
        public string game_name_vn { get; set; }
        /// <summary>
        /// 遊戲類別
        /// </summary>
        [StringLength(20)]
        public string game_type { get; set; }
        /// <summary>
        /// 遊戲代碼
        /// </summary>
        [StringLength(100)]
        public string game_no { get; set; }
        /// <summary>
        /// 熱門遊戲
        /// </summary>
        public bool popular_game { get; set; }
        public bool enable_game { get; set; }
        /// <summary>
        /// 新遊戲
        /// </summary>
        public bool new_game { get; set; }
        /// <summary>
        /// 推薦遊戲
        /// </summary>
        public bool recommend_game { get; set; }
        /// <summary>
        /// 遊戲圖示
        /// </summary>
        [StringLength(255)]
        /// <summary>
        /// 遊戲開關
        /// </summary>
        public string icon { get; set; }
        public PostGameListReq()
        {
            enable_game = true;
        }
    }
    public class GetGameList : ResCodeBase
    {
        /// <summary>
        /// 遊戲清單資料
        /// </summary>
        public List<GameList> Data { get; set; }
    }
    public class GetGameListSummary : ResCodeBase
    {
        /// <summary>
        /// 遊戲清單資料
        /// </summary>
        public int Count { get; set; }
    }

    public class GetGameApiList<T> : ResCodeBase
    {
        public List<T> data { get; set; }
    }
}
