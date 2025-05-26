using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyWallet.Share.Model.Game.WE.Request;
public class TransferRequest
{
    /// <summary>
    /// 在 WE 註冊的營運商ID
    /// </summary>
    public string operatorID { get; set; }
    /// <summary>
    /// 搜尋開始時間 (UNIX), 與結束時間間隔不得大於一個月
    /// </summary>
    public long startTime { get; set; }
    /// <summary>
    /// 搜尋結束時間 (UNIX), 與開始時間間隔不得大於一個月
    /// </summary>
    public long endTime { get; set; }
    /// <summary>
    /// 玩家ID
    /// </summary>
    [DefaultValue("")]
    public string playerID { get; set; }
    /// <summary>
    /// 當前交易編號 (最大長度: 50字符)
    /// </summary>
    public string uid { get; set; }
    /// <summary>
    /// 資料限制筆數 (預設:50 最大:500)
    /// </summary>
    public int limit { get; set; }
    /// <summary>
    /// 請求時間限制兩分鐘內 (UNIX)
    /// </summary>
    public long requestTime { get; set; }
}
