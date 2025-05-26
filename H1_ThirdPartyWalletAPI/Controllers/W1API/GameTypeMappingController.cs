using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System;
using H1_ThirdPartyWalletAPI.Model;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GameTypeMappingController : ControllerBase
    {
        private readonly ILogger<GameTypeMappingController> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameTypeMappingDBService _gametypemappingbdservice;
        public GameTypeMappingController(ILogger<GameTypeMappingController> logger,
             ICommonService commonService,
             IGameApiService gameApiService,
             IGameTypeMappingDBService gametypemappingbdservice)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameApiService;
            _gametypemappingbdservice = gametypemappingbdservice;
        }
        [HttpGet()]
        public async Task<GetgametypemappingRes> Get([FromQuery] string platformString)
        {
            GetgametypemappingRes res = new GetgametypemappingRes();
            res.Data = new List<t_gametype_mapping>();
            try
            {
                Enum.TryParse<Platform>(platformString.ToUpper(), true, out var platform);
                if (platform.ToString() == "H1")
                {
                    throw new Exception("遊戲館不存在");
                }

                var Gametype = await _gametypemappingbdservice.GetGameTypeMapping(platform);

                res.Data.AddRange(Gametype);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("GetSystemParameterRes exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }

        }
        [HttpPost]
        public async Task<ResCodeBase> Post([FromBody] t_gametype_mapping request)
        {
            var res = new ResCodeBase();

            try
            {
               var data= await _gametypemappingbdservice.CreateGameTypeMapping(request);
                var key = $"{RedisCacheKeys.WEGetGameTypeMapping}";
                await _commonService._cacheDataService.KeyDelete(key);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("PostgametypemappingReq exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        [HttpDelete]
        public async Task<ResCodeBase> Delete([FromQuery] string gameid, [FromQuery] string gametype)
        {
            var res = new ResCodeBase();

            try
            {
                await _gametypemappingbdservice.DeleteGameTypeMapping(gameid, gametype);
                var key = $"{RedisCacheKeys.WEGetGameTypeMapping}";
                await _commonService._cacheDataService.KeyDelete(key);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("DeleteGameTypeMapping exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
