using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API

{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetRowDataController : ControllerBase
    {
        private readonly ILogger<GetRowDataController> _logger;
        private readonly IGameInterfaceService _gameInterfaceService;
        public GetRowDataController(ILogger<GetRowDataController> logger

            , IGameInterfaceService gameInterfaceService
           )
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
        }


        /// <summary>
        /// 使用遊戲trans_id查詢詳細遊戲結果
        /// </summary>
        [HttpGet]
        async public Task<GetRowDataRespone> Get([FromQuery] GetRowDataReq RecordDetailReq)
        {
            GetRowDataRespone res = new GetRowDataRespone();
            try
            {
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];

                var RecordDetailRes = await _gameInterfaceService.GameRowData(RecordDetailReq);
                res.Data = RecordDetailRes;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("GetRowData EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.GetGameRecordFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameRecordFail] + " | " + ex.Message.ToString();
                return res;
            }
        }
    }
}
