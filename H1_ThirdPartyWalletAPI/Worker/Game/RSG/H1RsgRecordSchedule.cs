using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class H1RsgRecordSchedule : IInvocable
    {
        private readonly ILogger<H1RsgRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public H1RsgRecordSchedule(ILogger<H1RsgRecordSchedule> logger,
            IGameApiService gameaApiService,
            ICommonService commonService,
            GameRecordService gameRecordService,
            ISystemParameterDbService systemParameterDbService
            )
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _gameRecordService = gameRecordService;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            try
            {
                var key = "H1RsgRecordSchedule";
                // 取得RSG MaxId
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);

                var getAPILogMaxIdRequest = new GetAPILogByIdRequest();
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    long maxID = 0;
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = maxID.ToString(),
                        min_value = string.Format("{0}", 1),
                        name = "H1_RSG取得注單排程",
                        description = "H1_RSG記錄MaxId"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        getAPILogMaxIdRequest.Id = maxID;
                    }
                    else
                    {
                        return; // 新增失敗就結束排程
                    }
                }
                else
                {
                    if (int.Parse(parameter.min_value) == 0)
                    {
                        _logger.LogInformation("Rsg record stop max id: {id}", long.Parse(parameter.value));
                        await Task.CompletedTask;
                        return;
                    }
                    getAPILogMaxIdRequest.Id = long.Parse(parameter.value);
                }
                getAPILogMaxIdRequest.SystemCode = Config.CompanyToken.RSG_SystemCode;
                getAPILogMaxIdRequest.Rows = 2000;
                var getAPILogMaxIdResponse = await _gameApiService._RsgAPI.H1GetAPILogById(getAPILogMaxIdRequest);

                if (getAPILogMaxIdResponse.Data.GameReport.Count > 0)
                {
                    await _gameRecordService._rsgH1InterfaceService.PostRsgRecord(getAPILogMaxIdResponse.Data.GameReport);
                    var maxid = getAPILogMaxIdResponse.Data.GameReport.Max(x => x.id);
                    parameter.value = maxid.ToString();
                    await _systemParameterDbService.PutSystemParameter(parameter);
                }

            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run H1Rsg record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }

}
