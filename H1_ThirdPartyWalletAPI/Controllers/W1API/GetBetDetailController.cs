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
using H1_ThirdPartyWalletAPI.Service;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.AE;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Service.Game;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Model.Game.JDB;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using H1_ThirdPartyWalletAPI.Model.Game.RSG;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Model.Game.RLG;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API

{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetBetDetailController : ControllerBase
    {
        private readonly ILogger<GetBetDetailController> _logger;
        private readonly IGameApiService _gameaApiService;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ICommonService _commonService;
        public GetBetDetailController(ILogger<GetBetDetailController> logger
            , IGameApiService gameaApiService
            , IGameInterfaceService gameInterfaceService
            , ICommonService commonService)
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
            _gameInterfaceService = gameInterfaceService;
            _commonService = commonService;
        }

        public GetBetRecordReq RecordReq { get; private set; }

        /// <summary>
        /// 使用遊戲trans_id查詢詳細遊戲結果
        /// </summary>
        [HttpGet]
        async public Task<dynamic> Get([FromQuery] GetBetDetailReq RecordDetailReq)
        {
            GetBetDetail res = new GetBetDetail();
            try
            {
                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];

                var RecordDetailRes = await _gameInterfaceService.GameDetailURL(RecordDetailReq);
                res.Data = RecordDetailRes;
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
