using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.PG.Service
{
    public interface IPGInterfaceService : IGameInterfaceService
    {
        /// <summary>
        /// 新增 5 分鐘匯總帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        Task PostPgRecord(List<GetHistoryResponse.Data> recordData);
        /// <summary>
        /// 新增 遊戲商小時匯總帳
        /// </summary>
        /// <returns></returns>
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        /// <summary>
        /// 新增 W1小時匯總帳
        /// </summary>
        /// <returns></returns>
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    }
}