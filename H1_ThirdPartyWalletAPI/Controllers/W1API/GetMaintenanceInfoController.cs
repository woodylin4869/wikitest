using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.W1API;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Service.Game;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum;
using H1_ThirdPartyWalletAPI.Model.Config;

namespace H1_ThirdPartyWalletAPI.Controllers.W1API
{
    [ApiController]
    [Route("w1api/[controller]")]
    public class GetMaintenanceInfoController : ControllerBase
    {
        private readonly ILogger<GetMaintenanceInfoController> _logger;
        private readonly IGameApiService _gameaApiService;


        public GetMaintenanceInfoController(ILogger<GetMaintenanceInfoController> logger, IGameApiService gameaApiService)
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
        }
        /// <summary>
        /// 取得各遊戲維護狀態與時間
        /// </summary>
        [HttpGet]
        async public Task<GetMaintenanceInfo> Get()
        {
            GetMaintenanceInfo res = new GetMaintenanceInfo();
            res.Data = new List<MaintenanceInfo>();
            try
            {
                //Chack platform RCG
                RCG_ResBase<RCG_GetMaintenanceInfo_Res> RCG_MT = await _gameaApiService._RcgAPI.GetMaintenanceInfo();
                if (RCG_MT.msgId == (int)RCG.msgId.Success)
                {
                    MaintenanceInfo MTdata = new MaintenanceInfo();
                    MTdata.Platform = nameof(Platform.RCG);
                    if(DateTime.Now > RCG_MT.data.StartDate && DateTime.Now < RCG_MT.data.EndDate)
                    {
                        MTdata.IsMT = true;
                    }
                    else
                    {
                        MTdata.IsMT = false;
                    }
                    MTdata.MTStartTime = RCG_MT.data.StartDate;
                    MTdata.MTEndTime = RCG_MT.data.EndDate;
                    res.Data.Add(MTdata);
                }
                else
                {
                    MaintenanceInfo MTdata = new MaintenanceInfo();
                    MTdata.IsMT = null;
                    MTdata.Platform = nameof(Platform.RCG);
                    MTdata.MTStartTime = null;
                    MTdata.MTEndTime = null;
                    res.Data.Add(MTdata);
                }

                //Check platform SABA
                SABA_GetMaintenanceTime requestData = new SABA_GetMaintenanceTime();
                SABA_GetMaintenanceTime_Res result = await _gameaApiService._Saba2API.GetMaintenanceTime(requestData);
                if(result.error_code == (int)SABA_GetMaintenanceTime_Res.ErrorCode.Success)
                {
                    MaintenanceInfo MTdata = new MaintenanceInfo();
                    MTdata.IsMT = result.Data.IsUM;
                    MTdata.Platform = nameof(Platform.SABA);
                    MTdata.MTStartTime = result.Data.UMStartDateTime;
                    MTdata.MTEndTime = result.Data.UMEndDateTime;
                    res.Data.Add(MTdata);
                }
                else
                {
                    MaintenanceInfo MTdata = new MaintenanceInfo();
                    MTdata.IsMT = null;
                    MTdata.Platform = nameof(Platform.SABA);
                    MTdata.MTStartTime = null;
                    MTdata.MTEndTime = null;
                    res.Data.Add(MTdata);
                }
                //Check platform RSG
                var GetMaintainStatusRequest = new GetMaintainStatusRequest();
                GetMaintainStatusRequest.SystemCode = Config.CompanyToken.RSG_SystemCode;
                var GetMaintainStatusResponse = await _gameaApiService._RsgAPI.H1GetMaintainStatus(GetMaintainStatusRequest);
                if (GetMaintainStatusResponse.ErrorCode != (int)ErrorCodeEnum.OK)
                {
                    MaintenanceInfo MTdata = new MaintenanceInfo();
                    MTdata.IsMT = null;
                    MTdata.Platform = nameof(Platform.RSG);
                    MTdata.MTStartTime = null;
                    MTdata.MTEndTime = null;
                    res.Data.Add(MTdata);

                }
                else
                {
                    MaintenanceInfo MTdata = new MaintenanceInfo();
                    MTdata.IsMT = GetMaintainStatusResponse.Data.MaintainStatus == 0 ? false: true;
                    MTdata.Platform = nameof(Platform.RSG);
                    MTdata.MTStartTime = null;
                    MTdata.MTEndTime = null;
                    res.Data.Add(MTdata);
                }

                res.code = (int)ResponseCode.Success;
                res.Message = MessageCode.Message[(int)ResponseCode.Success];
                return res;
            }
            catch (Exception ex)
            {
                res.code = (int)ResponseCode.GetGameMaintenanceFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameMaintenanceFail];
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Get Maintenance Info exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
    }
}
