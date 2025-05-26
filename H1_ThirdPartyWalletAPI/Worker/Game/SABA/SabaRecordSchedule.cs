using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class SabaRecordSchedule : IInvocable
    {
        private readonly ILogger<SabaRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly ISabaDbService _sabaDbService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        //private readonly IMemoryCache _memoryCache;
        private static long version_key = 0;
        private static int pendingCount = 0;

        public SabaRecordSchedule(ILogger<SabaRecordSchedule> logger
            , IGameApiService gameApiService
            , GameRecordService gameRecordService
            , ISystemParameterDbService systemParameterDbService
            , ISabaDbService sabaDbService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _gameRecordService = gameRecordService;
            _sabaDbService = sabaDbService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke SabaRecordSchedule on time : {time}", DateTime.Now);

            try
            {
                _logger.LogDebug("Saba Record Schedule countdown : {count}", pendingCount);
                //version_key相同時要延遲請求
                if (pendingCount != 0)
                {
                    pendingCount--;
                    await Task.CompletedTask;
                    return;
                }
                SABA_GetBetDetail req = new SABA_GetBetDetail();

                var key = "SabaRecordSchedule";
                // 取得SABA Max last_version_key
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var last_version_key = await _sabaDbService.GetSabaLastVersionKey("H1royal");
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = DateTime.Now.ToString(),
                        min_value = string.Format("{0}", 1),
                        name = "SABA取得注單排程",
                        description = "SABA記錄最大last_version_key"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        req.version_key = last_version_key;
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
                        _logger.LogInformation("Saba record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    req.version_key = long.Parse(parameter.value);
                }
                version_key = req.version_key;

                //if (version_key == 0)
                //{
                //    //從Record取得最大version_key
                //    long lastVersion = await _commonService._serviceDB.GetSabaLastVersionKey();
                //    req.version_key = lastVersion;
                //    version_key = lastVersion;
                //}
                //else
                //{
                //    req.version_key = version_key;
                //}
                SABA_GetBetDetail_Res result = await _gameApiService._SabaAPI.GetBetDetail(req);
                //version_key與上次相同延遲1分鐘再取
                if (result.Data.last_version_key == version_key)
                {
                    pendingCount = 3;
                }
                //version_key推進
                if (result.Data.last_version_key > version_key)
                {
                    version_key = result.Data.last_version_key;
                }
                if (result.Data.BetDetails.Count > 0)
                {
                    //if (Config.OneWalletAPI.RCGMode == "H1")
                    //    await _gameRecordService._SabaSessionRecordService.PostSabaRecord(result.Data);
                    //else
                    await _gameRecordService._SabaInterfaceService.PostSabaRecord(result.Data);
                }
                // version_key寫回 DB
                parameter.value = version_key.ToString();
                await _systemParameterDbService.PutSystemParameter(parameter);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run saba record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
