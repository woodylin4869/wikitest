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
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class SystemParameterController : ControllerBase
    {
        private readonly ILogger<SystemParameterController> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        public SystemParameterController(ILogger<SystemParameterController> logger,
             ICommonService commonService,
             IGameApiService gameApiService,
             ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameApiService;
            _systemParameterDbService = systemParameterDbService;
        }
        /// <summary>
        /// 取得所有排程資訊
        /// </summary>
        [HttpGet]
        async public Task<GetSystemParameterRes> Get()
        {
            GetSystemParameterRes res = new GetSystemParameterRes();
            try
            {
                res.Data = await _systemParameterDbService.GetAllSystemParameter();
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("GetSystemParameterRes exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        /// <summary>
        /// 取得指定key 取得所有排程資訊
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public async Task<GetSystemParameterRes> Get(string key)
        {
            GetSystemParameterRes res = new GetSystemParameterRes();
            res.Data = new List<t_system_parameter>();
            try
            {
                var schedule = await _systemParameterDbService.GetSystemParameter(key);
                res.Data.Add(schedule);

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
        /// <summary>
        /// 更新指定key排程資訊
        /// </summary>
        /// <param name="key"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("{key}")]
        public async Task<ResCodeBase> Put(string key, PutSystemParameterReq req)
        {
            ResCodeBase res = new ResCodeBase();
            try
            {
                var schedule = await _systemParameterDbService.GetSystemParameter(key);
                schedule.value = req.value == null ? schedule.value :req.value.ToString();
                //schedule.max_value = req.max_value.ToString();
                schedule.min_value = req.min_value.ToString();
                await _systemParameterDbService.PutMinSystemParameter(schedule);
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message;
                _logger.LogError("GetSystemParameterRes EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
