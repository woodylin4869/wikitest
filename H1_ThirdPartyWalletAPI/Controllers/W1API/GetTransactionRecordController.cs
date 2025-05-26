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
namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetTransactionRecordController : ControllerBase
    {
        private readonly ILogger<GetTransactionRecordController> _logger;
        private readonly IDBService _serviceDB;
        public GetTransactionRecordController(ILogger<GetTransactionRecordController> logger, IDBService serviceDB)
        {
            _logger = logger;
            _serviceDB = serviceDB;
        }
        /// <summary>
        /// 使用交易紀錄(Transfer Record)的id查詢遊戲的逐筆交易紀錄(Transaction)
        /// </summary>
        [HttpGet]
        async public Task<dynamic> Get([FromQuery] GetTransactionRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            try
            {
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                switch (RecordReq.Platform)
                {
                    case nameof(Platform.RCG):
                        IEnumerable<dynamic> results = await _serviceDB.GetRcgTransactionBySummaryId(Guid.Parse(RecordReq.summary_id));
                        res.Data = results.ToList();
                        return res;
                    default:
                        throw new Exception("unknow game platform");

                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get transaction EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetGameRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
    }
}
