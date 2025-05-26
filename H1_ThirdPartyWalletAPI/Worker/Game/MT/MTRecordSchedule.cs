using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using System;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using System.Linq;
using MTsetup = H1_ThirdPartyWalletAPI.Model.Game.MT.MT;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker.Game.MT
{
    public class MTRecordSchedule : IInvocable
    {
        private readonly ILogger<MTRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IMTInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "MTRecordSchedule";

        public MTRecordSchedule(ILogger<MTRecordSchedule> logger, IMTInterfaceService apiInterfaceService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {

            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke MTRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {

                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = "0",
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "MT取得注單排程",
                    description = "MT記錄end_time"
                };

                var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                if (postSystemParameter)
                {
                    parameter = model;
                }
                else
                {
                    return; // 新增失敗就結束排程
                }
            }

            if (int.Parse(parameter.min_value) == 0)
            {
                _logger.LogInformation("MT record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            try
            {
                var req = new QueryMerchantGameRecord2rawData
                {
                    recordID = long.Parse(parameter.value),
                    gameType = "2",
                    startTime = null,
                    endTime = null,
                    currency = MTsetup.Currency["THB"]
                };


                var betLogs = await _gameApiService._MTAPI.queryMerchantGameRecord2Async(req);
                if (betLogs.transList.Count == 0)
                {
                    return;
                }

                parameter.value = betLogs.transList.Max(x => x.rowID);
                await _systemParameterDbService.PutSystemParameter(parameter);
                await _apiInterfaceService.PostMTRecord(betLogs.transList);


            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run MT record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
