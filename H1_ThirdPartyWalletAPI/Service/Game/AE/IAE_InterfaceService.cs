using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.AE
{
    public interface IAeInterfaceService : IGameInterfaceService
    {
        /// <summary>
        /// 新增 5 分鐘匯總帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public Task PostAeRecordDetail(List<BetHistory> recordData);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    }
}