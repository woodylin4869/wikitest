using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Collections.Generic;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model;
using Npgsql;
using Microsoft.AspNetCore.Authorization;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class ApiHealthController : ControllerBase
    {
        private readonly ILogger<ApiHealthController> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        public ApiHealthController(ILogger<ApiHealthController> logger,
             ICommonService commonService,
             IGameApiService gameApiService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameApiService;
        }
        /// <summary>
        /// 取得Api健康資訊
        /// </summary>
        [HttpGet]
        async public Task<GetApiHealthRes> Get()
        {
            GetApiHealthRes res = new GetApiHealthRes();
            try
            {
                res.Data = await _commonService._apiHealthCheck.GetAllHealthInfo();
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("get GetApiHealth exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得指定id Api健康資訊
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        [HttpGet("{platform}")]
        public async Task<GetApiHealthRes> Get(string platform)
        {
            GetApiHealthRes res = new GetApiHealthRes();
            try
            {                
                res.Data = new List<ApiHealthInfo>();
                var platformInfo = await _commonService._apiHealthCheck.GetPlatformHealthInfo((Platform)Enum.Parse(typeof(Platform), platform.ToUpper()));
                res.Data.Add(platformInfo);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("get api health exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 更新指定ID Api健康資訊
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public async Task<ResCodeBase> Put(PutApiHealthReq req)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                await _commonService._apiHealthCheck.SetPlatformHealthInfo(req);
                //RSG設定維護
                var request = new SetMaintainStatusRequest();
                if (req.Status == Status.MAINTAIN && req.Platform == nameof(Platform.RSG))
                {
                    request.SystemCode = Config.CompanyToken.RSG_SystemCode;
                    request.Maintain = 1; //設定維護
                    var response = await _gameApiService._RsgAPI.H1SetMaintainStatus(request);

                }
                else if (req.Status == Status.NORMAL && req.Platform == nameof(Platform.RSG))
                {
                    request.SystemCode = Config.CompanyToken.RSG_SystemCode;
                    request.Maintain = 0; //解除維護
                    var response =  await _gameApiService._RsgAPI.H1SetMaintainStatus(request);
                }
                return res;
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
        /// <summary>
        /// 刪除Api健康資訊
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ResCodeBase> Delete()
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                await _commonService._apiHealthCheck.DeleteAllHealthInfo(); 
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("delete api health id EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
