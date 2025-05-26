using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using System.Linq;
using H1_ThirdPartyWalletAPI.Service.W1API;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetBetRecordUnsettleController : ControllerBase
    {
        private readonly ILogger<GetBetRecordUnsettleController> _logger;
        private readonly IBetRecordService _betRecordService;
        public GetBetRecordUnsettleController(ILogger<GetBetRecordUnsettleController> logger , IBetRecordService betRecordService)
        {
            _logger = logger;
            _betRecordService = betRecordService;
        }
        /// <summary>
        /// 查詢未結算注單
        /// </summary>
        [HttpGet]
        async public Task<dynamic> Get([FromQuery] GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            try
            {
                res = await _betRecordService.GetBetRecordUnsettle(RecordReq);
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
