using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response
{
    /// <summary>
    /// 取得下注紀錄 /api/Record/GetBetRecordList
    /// </summary>
    public class GetBetRecordListResponse
    {
        public string systemCode { get; set; }
        public string webId { get; set; }
        public List<RCG2BetRecord> dataList { get; set; }
    }

    public class RCG2BetRecord
    {
        ///// <summary>
        ///// Guid
        ///// </summary>
        //public Guid summary_id { get; set; }

        /// <summary>
        /// 系統代碼
        /// </summary>
        public string systemCode { get; set; }

        /// <summary>
        /// 站台代碼
        /// </summary>
        public string webId { get; set; }

        /// <summary>
        /// 玩家帳號
        /// </summary>
        public string memberAccount { get; set; }

        /// <summary>
        /// 注單編號
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 遊戲類別
        /// 1	Bacc
        /// 2	LongHu
        /// 3	LunPan
        /// 4	ShaiZi
        /// 5	FanTan
        /// 6	InsuBacc
        /// 7	PokDeng
        /// 9	Sambo
        /// 11	BCBacc
        /// 12	BCLongHu
        /// 15	AndarBahar
        /// 17	HiLo
        /// 18	BCSDD
        /// </summary>
        public int gameId { get; set; }

        /// <summary>
        /// 遊戲桌別
        /// </summary>
        public string desk { get; set; }

        /// <summary>
        /// 下注區名稱
        /// </summary>
        public string betArea { get; set; }

        /// <summary>
        /// 下注金額，小數4位
        /// </summary>
        public decimal bet { get; set; }

        /// <summary>
        /// 有效下注金額，小數4位
        /// </summary>
        public decimal available { get; set; }

        /// <summary>
        /// 輸贏，小數4位
        /// </summary>
        public decimal winLose { get; set; }

        /// <summary>
        /// 退水比例(100為沒退水)
        /// </summary>
        public decimal waterRate { get; set; }

        /// <summary>
        /// 輪號
        /// </summary>
        public string activeNo { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        public string runNo { get; set; }

        /// <summary>
        /// 剩餘額度
        /// </summary>
        public decimal balance { get; set; }

        /// <summary>
        /// 押注時間 格式 yyyy-mm-ddTHH:mm:ss
        /// </summary>
        public DateTime dateTime { get; set; }

        /// <summary>
        /// 結算時間 格式 yyyy-mm-ddTHH:mm:ss.fff
        /// </summary>
        public DateTime reportDT { get; set; }

        /// <summary>
        /// IP位置
        /// </summary>
        public string ip { get; set; }

        /// <summary>
        /// 狀態 3當局取消、4正常注單、5事後取消、6改牌
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 注單賠率
        /// </summary>
        public decimal odds { get; set; }

        /// <summary>
        /// 改單原編號：-1未修正資料 
        /// 改單前的原始注單編號
        /// </summary>
        public long originRecordId { get; set; }

        /// <summary>
        /// 改單最初編號：-1未修正資料
        /// 改單前的最初注單編號
        /// </summary>
        public long rootRecordId { get; set; }

        /// <summary>
        /// [原始]下注金額
        /// </summary>
        public decimal pre_bet { get; set; }

        /// <summary>
        /// [原始]有效下注
        /// </summary>
        public decimal pre_available { get; set; }

        /// <summary>
        /// [原始]輸贏
        /// </summary>
        public decimal pre_winlose { get; set; }

        /// <summary>
        /// [原始]狀態 3當局取消、4正常注單、5事後取消、6改牌
        /// </summary>
        public int pre_status { get; set; }

        /// <summary>
        /// [原始]注單編號
        /// </summary>
        public long real_id { get; set; }

        #region db Model

        /// <summary>
        /// 當前建立時間
        /// </summary>
        public DateTime? Create_time { get; set; }

        /// <summary>
        /// 彙總帳時間
        /// </summary>
        public DateTime? Report_time { get; set; }

        public DateTime Partition_time { get; set; }
        #endregion db Model
    }
}
