using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game.RLG.Response
{
    /// <summary>
    /// 會員投注紀錄分頁列表
    /// </summary>
    public class GetBetRecordResponse
    {
        /// <summary>
        /// 000000 即為成功，其它代碼皆為失敗，可參詳 ErrorMessage 欄位內容
        /// </summary>
        public string errorcode { get; set; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errormessage { get; set; }
        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public GetBetRecordResponseData data { get; set; }
        /// <summary>
        /// 時間戳記
        /// </summary>
        public string timestamp { get; set; }

        /// <summary>
        /// 以 JSON 表示的 object
        /// </summary>
        public class GetBetRecordResponseData
        {
            public string last_version_key { get; set; }
            /// <summary>
            /// 系統代碼
            /// </summary>
            public string systemcode { get; set; }
            /// <summary>
            /// 站台代碼，即代理唯一識別碼 ID
            /// </summary>
            public string webid { get; set; }
            /// <summary>
            /// 目前頁數
            /// </summary>
            public int currentPage { get; set; }
            /// <summary>
            /// 總頁數
            /// </summary>
            public int totalPage { get; set; }
            /// <summary>
            /// 總筆數
            /// </summary>
            public string totalCount { get; set; }
            /// <summary>
            /// 注單資料
            /// </summary>
            public List<GetBetRecordResponseDataList> datalist { get; set; }
        }
        /// <summary>
        /// 注單資料
        /// </summary>
        public class GetBetRecordResponseDataList
        {
            public Guid summary_id { get; set; }
            public string last_version_key { get; set; }

            /// <summary>
            /// 玩家的唯一識別碼
            /// </summary>
            public string userid { get; set; }
            /// <summary>
            /// 注單編號
            /// </summary>
            public string ordernumber { get; set; }
            /// <summary>
            /// 期數
            /// </summary>
            public string numberofperiod { get; set; }
            /// <summary>
            /// 彩別代號，詳見 I.3 彩別代號
            /// </summary>
            public string gamecode { get; set; }
            /// <summary>
            /// 彩別名稱，詳見 I.3 彩別代號
            /// </summary>
            public string gamename { get; set; }
            /// <summary>
            /// 彩別群組代號，詳見 I.5 彩別群組代號
            /// </summary>
            public string gamegroupcode { get; set; }
            /// <summary>
            /// 彩別群組名稱，詳見 I.5 彩別群組代號
            /// </summary>
            public string gamegroupname { get; set; }
            /// <summary>
            /// 注數
            /// </summary>
            public string betnumber { get; set; }
            /// <summary>
            /// 賠率
            /// </summary>
            public double odds { get; set; }
            /// <summary>
            /// 下注內容
            /// </summary>
            public string content { get; set; }
            /// <summary>
            /// 投注金額
            /// </summary>
            public decimal totalamount { get; set; }
            /// <summary>
            /// 玩法名稱
            /// </summary>
            public string gameplayname { get; set; }
            /// <summary>
            /// 投注時間
            /// </summary>
            public DateTime createtime { get; set; }
            /// <summary>
            /// 注單狀態(0:未结算，1:已结算，2:取消，3:删除)
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// 中奖金额
            /// </summary>
            public decimal bettingbalance { get; set; }
            /// <summary>
            /// 退水金额
            /// </summary>
            public string totalkickback { get; set; }
            /// <summary>
            /// 代理佔成金額
            /// </summary>
            public string agpdamount { get; set; }
            /// <summary>
            /// 开奖结果
            /// </summary>
            public string result { get; set; }
            /// <summary>
            /// 開獎時間(未開獎時為關盤時間)
            /// </summary>
            public DateTime drawtime { get; set; }
            /// <summary>
            /// 中奖状态(0:输，1：平，2：赢)
            /// </summary>
            public string winningstatus { get; set; }
            /// <summary>
            /// 是否有重新開獎過(0:無、1:有)
            /// </summary>
            public string isadjust { get; set; }
            /// <summary>
            /// 下注裝置(0：web，1：mobile)
            /// </summary>
            public string device { get; set; }
            public string gameplaycode { get; set; }
            public string contentcode { get; set; }

            public decimal pre_bettingbalance { get; set; } //原始輸或贏的金額

            public decimal pre_totalamount { get; set; } //原始投注

            public string club_id { get; set; }
            public string franchiser_id { get; set; }
            /// <summary>
            /// 分區時間
            /// </summary>
            public DateTime partition_time { get; set; }
            /// <summary>
            /// 報表時間
            /// </summary>
            public DateTime report_time { get; set; }
            /// <summary>
            /// 建立時間
            /// </summary>
            public DateTime create_time { get; set; }
            public List<RLG_ResettlementInfo> resettlementinfo { get; set; }//重新结算信息(若有重新结算才会出现
        }

        public class RLG_ResettlementInfo
        {
            public DateTime actiondate { get; set; } //重新結算時間
            public bool balancechange { get; set; } //余额是否更动.(false 或 true)
            public decimal? winlost { get; set; } //前次结算输或赢的金额
        }
    }
}
