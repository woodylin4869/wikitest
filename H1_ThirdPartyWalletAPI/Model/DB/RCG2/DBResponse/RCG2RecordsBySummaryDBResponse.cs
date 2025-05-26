using System;

namespace H1_ThirdPartyWalletAPI.Model.DB.RCG2.DBResponse
{
    public class RCG2RecordsBySummaryDBResponse : RCG2RecordPrimaryKey
    {
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
        //public string betArea { get; set; }

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
        /// 輪號
        /// </summary>
        //public string activeNo { get; set; }

        /// <summary>
        /// 局號
        /// </summary>
        //public string runNo { get; set; }

        /// <summary>
        /// 押注時間 格式 yyyy-mm-ddTHH:mm:ss
        /// </summary>
        public DateTime dateTime { get; set; }

        /// <summary>
        /// 狀態 3當局取消、4正常注單、5事後取消、6改牌
        /// </summary>
        //public int status { get; set; }

        /// <summary>
        /// 注單賠率
        /// </summary>
        //public decimal odds { get; set; }

        /// <summary>
        /// [原始]注單編號
        /// </summary>
        public long real_id { get; set; }
    }
}
