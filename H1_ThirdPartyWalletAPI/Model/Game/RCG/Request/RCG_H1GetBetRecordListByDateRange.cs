using System;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    /// <summary>
    /// 輸入時間範圍查詢注單 沒文件 說是什麼H1特規 但補帳指定時間範圍較符合用 所以偷偷來用
    /// 偷叫方法 /api/H1/GetBetRecordListByDateRange
    /// 接續使用 來源專案 H1WalletAPI\Common\Service\Request\GetRCGBetDetailServiceRequest.cs
    /// </summary>
    public class RCG_H1GetBetRecordListByDateRange
    {
        public string systemCode { get; set; }
        public string webId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }
}
