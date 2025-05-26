using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using System.Threading.Tasks;
using System;

namespace H1_ThirdPartyWalletAPI.Service.Game.DS
{
    public interface IDsInterfaceService : IGameInterfaceService
    {
        /// <summary>
        /// 新增 5 分鐘匯總帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public Task PostDsRecordDetail(List<DSBetRecord> recordData);
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