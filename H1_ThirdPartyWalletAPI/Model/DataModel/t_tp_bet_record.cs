using System;
using System.ComponentModel.DataAnnotations;

namespace H1_ThirdPartyWalletAPI.Model.DataModel;

public class t_tp_bet_record
{
    /// <summary>
    /// 遊戲廠商
    /// </summary>
    [MaxLength(10)]
    public string hall { get; set; }

    /// <summary>
    /// 注單單號
    /// 同遊戲商內注單唯一值
    /// </summary>
    [MaxLength(50)]
    public string rowid { get; set; }

    /// <summary>
    /// 遊戲局號
    /// </summary>
    [MaxLength(255)]
    public string round { get; set; }

    /// <summary>
    /// 遊戲分類
    /// </summary>
    [MaxLength(20)]
    public string category { get; set; }

    /// <summary>
    /// 遊戲代碼
    /// </summary>
    [MaxLength(50)]
    public string gameid { get; set; }

    /// <summary>
    /// 遊戲名稱
    /// </summary>
    [MaxLength(50)]
    public string game_name { get; set; }

    /// <summary>
    /// 玩家帳號
    /// </summary>
    [MaxLength(20)]
    public string casino_account { get; set; }

    /// <summary>
    /// 有效投注金額
    /// </summary>
    [MaxLength(15)]
    public decimal betvalid { get; set; }

    /// <summary>
    /// 投注金額
    /// </summary>
    [MaxLength(15)]
    public decimal betamount { get; set; }

    /// <summary>
    /// 派彩金額
    /// </summary>
    [MaxLength(15)]
    public decimal betresult { get; set; }

    /// <summary>
    /// 彩池貢獻金
    /// </summary>
    [MaxLength(15)]
    public decimal pca_contribute { get; set; }

    /// <summary>
    /// 彩池中獎金額
    /// </summary>
    [MaxLength(15)]
    public decimal pca_win { get; set; }

    /// <summary>
    /// 抽水金額
    /// </summary>
    [MaxLength(15)]
    public decimal revenue { get; set; }

    /// <summary>
    /// 投注時間
    /// </summary>
    [MaxLength(25)]
    public DateTime bettime { get; set; }

    /// <summary>
    /// 派彩時間
    /// Optional
    /// </summary>
    [MaxLength(25)]
    public DateTime? payout_time { get; set; }

    /// <summary>
    /// 注單爬回時間
    /// </summary>
    [MaxLength(25)]
    public DateTime reporttime { get; set; }

    /// <summary>
    /// 是否追號
    /// true = 是, false = 否
    /// </summary>
    public bool trace { get; set; }

    /// <summary>
    /// 是否包含免費遊戲
    /// 0:不包含, 1:包含, 預設為null:未提供
    /// </summary>
    public int? freegame { get; set; }

    /// <summary>
    /// 下注類型
    /// 預設為null:未提供
    /// </summary>
    public string bettype { get; set; }

    /// <summary>
    /// 遊戲結果
    /// 預設為null:未提供
    /// </summary>
    public string gameresult { get; set; }

    /// <summary>
    /// 注單狀態
    /// 0:未派彩, 1:已派彩, 2:已派彩但注單修改過, 3:撤單
    /// </summary>
    [MaxLength(1)]
    public string status { get; set; }
    public Guid summary_id { get; set; }

    public DateTime db_report_time { get; set; }
}
