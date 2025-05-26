using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RSG.Request
{
    public class GetAPILogByIdRequest
    {
        /// <summary>
        /// 系統代碼(只限英數)
        /// </summary>
        [MinLength(2)]
        [MaxLength(20)]
        [Required]
        public string SystemCode { get; set; }
        /// <summary>
        /// 彙總注單編號
        /// </summary>
        [Required]
        public long Id { get; set; }
        /// <summary>
        /// 筆數(範圍：100~5000)
        /// </summary>
        [MinLength(100)]
        [MaxLength(5000)]
        [Required]
        public int Rows { get; set; }
    }
}
