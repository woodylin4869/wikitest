using System;

namespace H1_ThirdPartyWalletAPI.Model.DataModel;

public class t_tp_game_report
{
    public DateTime reportTime { get; set; }

    /// <summary>
    /// 娛樂城代碼
    /// </summary>
    public string gamehall { get; set; }

    /// <summary>
    /// 娛樂城全名
    /// </summary>
    public string fullname { get; set; }

    /// <summary>
    /// 遊戲代碼
    /// </summary>
    public string gamecode { get; set; }

    /// <summary>
    /// 遊戲英文名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 遊戲中文名
    /// </summary>
    public string name_cn { get; set; }

    /// <summary>
    /// 遊戲分類
    /// </summary>
    public string category { get; set; }

    /// <summary>
    /// 幣別
    /// </summary>
    public string currency { get; set; }

    /// <summary>
    /// 總投注額
    /// </summary>
    public decimal bet_amount { get; set; }

    /// <summary>
    /// 總有效投注額
    /// </summary>
    public decimal bet_value { get; set; }

    /// <summary>
    /// 總注單數
    /// </summary>
    public int bet_count { get; set; }

    /// <summary>
    /// 總派彩
    /// </summary>
    public decimal bet_result { get; set; }

    /// <summary>
    /// 贏的注單數
    /// </summary>
    public int win_count { get; set; }

    /// <summary>
    /// 贏的注單派彩
    /// </summary>
    public decimal win_point { get; set; }

    /// <summary>
    /// 總損益比
    /// 四捨五入至小數點後四位
    /// </summary>
    public decimal profit_result_percent { get; set; }

    /// <summary>
    /// RTP
    /// 四捨五入至小數點後四位
    /// </summary>
    public decimal rtp { get; set; }
}
