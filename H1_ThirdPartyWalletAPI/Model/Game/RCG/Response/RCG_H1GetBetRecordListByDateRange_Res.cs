using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Model.Game
{
    /// <summary>
    /// 輸入時間範圍查詢注單 沒文件 說是什麼H1特規 但補帳指定時間範圍較符合用 所以偷偷來用
    /// 偷叫方法 /api/H1/GetBetRecordListByDateRange
    /// 接續使用 來源專案 H1WalletAPI\Common\Service\Response\GetRCGBetDetailServiceResponse.cs
    /// </summary>
    public class RCG_H1GetBetRecordListByDateRange_Res
    {
        public int total { get; set; }
        public string systemCode { get; set; }
        public string webId { get; set; }
        public List<Datalist> dataList { get; set; }
    }

    public class Datalist
    {
        public long recordId { get; set; }
        public int gameId { get; set; }
        public long id { get; set; }
        public string serverId { get; set; }
        public string gameName { get; set; }
        public string serverType { get; set; }
        public string noRun { get; set; }
        public string noActive { get; set; }
        public string txNo { get; set; }
        public decimal balance { get; set; }
        public string areaId { get; set; }
        public string areaName { get; set; }
        public decimal odds { get; set; }
        public int betPoint { get; set; }
        public decimal winLosePoint { get; set; }
        public int pointEffective { get; set; }
        public decimal winPoint { get; set; }
        public DateTime betDT { get; set; }
        public string company { get; set; }
        public string systemCode { get; set; }
        public string webId { get; set; }
        public string agentId { get; set; }
        public string agentName { get; set; }
        public string agentAccount { get; set; }
        public long memberId { get; set; }
        public string memberName { get; set; }
        public string memberAccount { get; set; }
        public string currency { get; set; }
        public decimal currencyRate { get; set; }
        public int fN1Percentage { get; set; }
        public int fN2Percentage { get; set; }
        public int fN3Percentage { get; set; }
        public int fN4Percentage { get; set; }
        public int fN5Percentage { get; set; }
        public decimal fN1DiscountRate { get; set; }
        public decimal fN2DiscountRate { get; set; }
        public decimal fN3DiscountRate { get; set; }
        public decimal fN4DiscountRate { get; set; }
        public decimal fN5DiscountRate { get; set; }
        public decimal mbDiscountRate { get; set; }
        public string ip { get; set; }
        public int deviceType { get; set; }
        public DateTime reportDT { get; set; }
        public int status { get; set; }
        public string sessionNo { get; set; }
        public DateTime insertDateTime { get; set; }
        public long originRecordId { get; set; }
    }
}
