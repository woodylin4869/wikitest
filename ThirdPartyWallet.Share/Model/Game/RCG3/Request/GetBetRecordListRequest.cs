using System.ComponentModel.DataAnnotations;

namespace ThirdPartyWallet.Share.Model.Game.RCG3.Request
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
