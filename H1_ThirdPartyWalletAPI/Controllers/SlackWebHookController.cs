using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class SlackWebHookController : ControllerBase
    {
        private readonly ILogger<SlackWebHookController> _logger;
        private readonly ICacheDataService _cacheDataService;
        public SlackWebHookController(ILogger<SlackWebHookController> logger, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _cacheDataService = cacheDataService;
        }

        /// <summary>
        /// SlackWebHook   SendMessage
        /// </summary>
        /// <response code="200">OK</response> /// 
        [HttpPost("SendMessageV2")]
        public async Task<dynamic> SendMessageV2()
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                string Platform = "PP";
                string url = "";
                string requestData = "";

                var GetHealthCheckUrl = await _cacheDataService.StringGetAsync<dynamic>($"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{Platform}/LOG");
                if (GetHealthCheckUrl != null)
                {
                    url = GetHealthCheckUrl.url;
                    requestData = GetHealthCheckUrl.requestData;
                }
                Status Status = Status.NORMAL;
                DateTime SuspendTime = DateTime.Now;
                SlackWebHook _slack = new SlackWebHook();
                await _slack.SendMessageAsync(Platform, Status.NORMAL.ToString(), DateTime.MinValue, url, requestData, $"TEST_Operator 啟用 遊戲館:{Platform} TIMEOUT => NORMAL", "W1Api健康狀態資訊");
                return new
                {
                    Platform,
                    Status,
                    SuspendTime
                };
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("put api health id EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
