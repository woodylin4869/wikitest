using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class JdbRecordSchedule : IInvocable
    {
        private readonly ILogger<JdbRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private static DateTime last_endtime;
        private const int defaultPastTime = 5;

        public JdbRecordSchedule(ILogger<JdbRecordSchedule> logger
            , IGameApiService gameaApiService
            , ICommonService commonService
            , GameRecordService gameRecordService
            , ISystemParameterDbService systemParameterDbService
            )
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _gameRecordService = gameRecordService;
            _commonService = commonService;
            last_endtime = DateTime.Now.AddMinutes(-defaultPastTime);
            last_endtime = new DateTime(last_endtime.Year, last_endtime.Month, last_endtime.Day, last_endtime.Hour, last_endtime.Minute, 0);
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke JdbRecordSchedule on time : {time}", DateTime.Now);
            try
            {
                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddMinutes(-defaultPastTime);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                GetGameBetRecordRequest req = new GetGameBetRecordRequest();
                var key = "JdbRecordSchedule_V2";
                // 取得JDB Record 時間
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = string.Format("{0}", 1),
                        name = "Jdb取得注單排程",
                        description = "Jdb每分鐘注單排程"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        req.Starttime = DateTime.Parse(parameter.value);
                        req.Endtime = req.Starttime.AddMinutes(1);
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
                        _logger.LogInformation("Jdb record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        if ((nextTime - (DateTime.Parse(parameter.value))).TotalMinutes > 120)
                            req.Starttime = nextTime.AddHours(-2).AddMinutes(5);
                        else
                            req.Starttime = DateTime.Parse(parameter.value);
                        req.Endtime = req.Starttime.AddMinutes(1);
                        parameter.value = req.Endtime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        _logger.LogInformation("Jdb record same excute time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return; // 時間不變就結束排程
                    }
                }
                GetBetRecordResponse jdbBetRecord = await _gameApiService._JdbAPI.Action29_GetGameBetRecord_NoClassification(req);
                if (jdbBetRecord.Data.Count() > 0)
                {
                    await _gameRecordService._JdbInterfaceService.PostJdbRecordDetail(jdbBetRecord.Data);
                }
                await _systemParameterDbService.PutSystemParameter(parameter);
                await Task.CompletedTask;
            }
            catch (JDBBadRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run jdb record schedule exception status : {status}  MSG : {Message} ", ex.status, ex.err_text);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run Jdb record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}