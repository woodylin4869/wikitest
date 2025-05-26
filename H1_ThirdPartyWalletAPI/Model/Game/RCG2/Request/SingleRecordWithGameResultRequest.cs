using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request
{
    /// <summary>
    /// W3取得注單資訊With開牌(SingleRecord/WithGameResult)
    /// 此方法沒有文件 而是使用以下網址測試
    /// https://api2tool.bacc55.com/W3/GetSingleBetRecordWithGameResult
    /// </summary>
    public class SingleRecordWithGameResultRequest : BaseRequest
    {
        /// <summary>
        /// 注單編號
        /// </summary>
        [Required]
        public long RecordId { get; set; }

        /// <summary>
        /// 語系 非必填 
        /// 亞洲商務=zh-cn
        /// </summary>
        [DefaultValue("zh-cn")]
        public string Lang { get; set; }

        /// <summary>
        /// ShowVideoRecord 非必填 預設true
        /// 亞洲商務=false
        /// </summary>
        [DefaultValue(false)]
        public bool ShowVideoRecord { get; set; }
    }
}
