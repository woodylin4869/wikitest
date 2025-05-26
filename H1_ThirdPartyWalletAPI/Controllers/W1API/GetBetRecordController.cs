using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.W1API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetBetRecordController : ControllerBase
    {
        private readonly ILogger<GetBetRecordController> _logger;
        private readonly IBetRecordService _betRecordService;
        public GetBetRecordController(ILogger<GetBetRecordController> logger
            , IBetRecordService betRecordService)
        {
            _logger = logger;
            _betRecordService = betRecordService;
        }
        /// <summary>
        /// 使用彙總注單的id查詢逐筆注單
        /// </summary>
        [HttpGet]
        async public Task<dynamic> Get([FromQuery] GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            try
            {
                res = await _betRecordService.GetBetRecord(RecordReq);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Record EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetGameRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
    }
}
