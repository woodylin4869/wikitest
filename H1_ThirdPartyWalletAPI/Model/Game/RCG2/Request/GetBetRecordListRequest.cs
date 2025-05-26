using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request
{
    /// <summary>
    /// 取得下注紀錄 /api/Record/GetBetRecordList
    /// </summary>
    public class GetBetRecordListRequest : BaseRequest
    {
        [Required]
        public long maxId { get; set; }

        public long rows { get; set; }
    }
}
