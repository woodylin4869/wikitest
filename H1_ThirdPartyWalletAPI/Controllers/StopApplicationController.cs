using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Dapper;
using H1_ThirdPartyWalletAPI.Model.Config;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Common;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StopApplicationController : ControllerBase
    {
        /// <summary>
        /// 呼叫 W1-Schedule 重啟服務 (為了重新讀取排程作業)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("StopApplication")]
        public async Task<string> StopApplication()
        {
            var url = Config.W1ScheduleConfig.Url + "/StopApplication/StopApplication";
            using (var request = new HttpClient())
            {
                request.Timeout = TimeSpan.FromSeconds(30);
                var response = await request.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
