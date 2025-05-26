using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.ClickHouse;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.CR.Request;
using ThirdPartyWallet.Share.Model.Game.CR.Response;

namespace H1_ThirdPartyWalletAPI.Controllers.Game.CH368
{
    [Route("/[controller]")]
    [ApiController]
    public class ClickHouseController : ControllerBase
    {
        private readonly IBetSummaryReportDBService _dbService;
        private readonly ICacheDataService _cacheService;

        public ClickHouseController(IBetSummaryReportDBService dBService, ICacheDataService cacheService)
        {
            _dbService = dBService;
            _cacheService = cacheService;
        }


        /// <summary>
        /// HealthCheck
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("HealthCheck")]
        public async Task<dynamic> HealthCheck()
        {
            return await _dbService.HealthCheck();
        }

        /// <summary>
        /// HealthCheck
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPlayerBetSummary")]
        public async Task<dynamic> GetPlayerBetSummary()
        {
            return await _dbService.GetPlayerBetSummary();
        }
        
    }
}

