using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    /// <summary>
    /// 改單紀錄 /api/Record/GetChangeRecordList
    /// </summary>
    public class RCG_GetChangeRecordList_Res
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string systemCode { get; set; }

        /// <summary>
        /// 站台代碼
        /// </summary>
        public string webId { get; set; }

        /// <summary>
        /// Object Array
        /// </summary>
        public List<ChangeRecord> dataList { get; set; }
    }

    /// <summary>
    /// 改單資料內容
    /// </summary>
    public class ChangeRecord
    {
        public string systemCode { get; set; }

        public string webId { get; set; }

        public string memberAccount { get; set; }

        /// <summary>
        /// 文件沒寫 應該是錯的描述... 實測會是最新的注單編號 這個每次改單都會變 阿哩勒
        /// </summary>
        public long id { get; set; }

        /// <summary>
        /// 請取最後一筆資料的modifyId值作為下次請求的MaxId。 
        /// 但輸出的 modifyId 沒有依序排~~ 阿哩勒 能抓全部就抓全部 不然少少的依序抓改單會漏
        /// </summary>
        public long modifyId { get; set; }

        /// <summary>
        /// 阿哩勒 這裡 gameId 回傳是string
        /// </summary>
        public string gameId { get; set; }

        public string serverId { get; set; }

        public string areaId { get; set; }

        public decimal betPoint { get; set; }

        public decimal pointEffective { get; set; }

        public decimal winLosePoint { get; set; }

        public decimal mbDiscountRate { get; set; }

        public string noRun { get; set; }

        public string noActive { get; set; }

        public decimal balance { get; set; }

        public DateTime betDT { get; set; }

        public string ip { get; set; }

        /// <summary>
        /// 改單狀態：5;事後取消、6改牌
        /// 多回傳4的資料 文件沒寫...實測應是一般單
        /// </summary>
        public int status { get; set; }

        public decimal odds { get; set; }

        /// <summary>
        /// 最後修改時間 格式 yyyy-mm-ddTHH:mm:ss.fff
        /// </summary>
        public DateTime lastChangeTime { get; set; }

        /// <summary>
        /// 改單前的「原始」注單編號
        /// </summary>
        public long? recordId { get; set; }
    }
}
