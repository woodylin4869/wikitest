using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.TP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.TP.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.TP;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class TpRecordSchedule : IInvocable
    {
        private readonly ILogger<TpRecordSchedule> _logger;
        private readonly ITPInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "TpRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-16);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public TpRecordSchedule(ILogger<TpRecordSchedule> logger, ITPInterfaceService apiInterfaceService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _apiInterfaceService = apiInterfaceService;
            _dbService = dbService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke TpRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得上次結束時間
            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                var value = DateTime.Now.ToLocalTime();
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = value.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "TP取得注單排程",
                    description = "TP記錄end_time"
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
                _logger.LogInformation("tp record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            //parameter.value = "2024-08-29 02:39:00";
            if (Convert.ToDateTime(parameter.value) >= nextTime)
            {
                _logger.LogInformation("return tp record schedule current Time : {now} report time : {reportTime} ", now, parameter.value);
                return; // 時間不變就結束排程
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = lastEndTime.Add(RANGE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                var betLogs = await _apiInterfaceService.GetTpRecords(lastEndTime, lastEndTime.Add(RANGE_OFFSET));

                if (!betLogs.Any())
                {
                    await _systemParameterDbService.PutSystemParameter(parameter);
                    return;
                }

                var allFail = true;

                //真人目前為串接
                //try
                //{
                //    if (betLogs.Any(b => b.isLiveRecord))
                //        await _apiInterfaceService.PostTpLiveRecord(betLogs.Where(b => b.isLiveRecord).ToList());
                //    allFail = false;
                //}
                //catch (Exception ex)
                //{
                //    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                //    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                //    _logger.LogError("Run tp record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                //}

                try
                {
                    if (betLogs.Any(b => b.isElectronicRecord))
                        await _apiInterfaceService.PostTpRecord(betLogs.Where(b => b.isElectronicRecord).ToList());

                    allFail = false;
                }
                catch (Exception ex)
                {
                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                    _logger.LogError("Run tp record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }

                if (!allFail)
                    await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run tp record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
