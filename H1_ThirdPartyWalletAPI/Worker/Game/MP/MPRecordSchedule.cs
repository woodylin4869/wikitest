using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.MP;
using H1_ThirdPartyWalletAPI.Service.Game.MT;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.MP
{
    public class MPRecordSchedule : IInvocable
    {
        private readonly ILogger<MPRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IMPInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "MPRecordSchedule";
        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        public MPRecordSchedule(ILogger<MPRecordSchedule> logger, IMPInterfaceService apiInterfaceService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
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
            _logger.LogInformation("Invoke MPRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            try
            {
                var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {

                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "MP取得注單排程",
                        description = "MP記錄end_time"
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
                    _logger.LogInformation("MP record stop time: {time}", parameter.value);
                    await Task.CompletedTask;
                    return;
                }
                var lastEndTime = DateTime.Parse(parameter.value);
                parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");

                if (lastEndTime >= nextTime)
                {
                    return;
                }


                var req = new PullGameBettingSlipParam
                {
                    startTime = new DateTimeOffset(lastEndTime).ToUnixTimeMilliseconds().ToString(),
                    endTime = new DateTimeOffset(nextTime).ToUnixTimeMilliseconds().ToString()
                };

                if ((nextTime - lastEndTime).TotalMinutes > 60)
                {
                    req.endTime = new DateTimeOffset(lastEndTime).AddHours(1).ToUnixTimeMilliseconds().ToString();
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                }
                await _systemParameterDbService.PutSystemParameter(parameter);




                var betLogs = await _gameApiService._MPAPI.PullGameBettingSlipAsync(req);
                if (betLogs.Count== 0)
                {
                    return;
                }

                await _apiInterfaceService.PostMPRecord(betLogs);


            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run MP record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
