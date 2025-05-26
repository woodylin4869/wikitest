using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class KSRecordSchedule : IInvocable
    {
        private readonly ILogger<KSRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "KSRecordSchedule";
        private readonly int defaultPastTime = 5;

        public KSRecordSchedule(ILogger<KSRecordSchedule> logger, GameRecordService gameRecordService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _gameRecordService = gameRecordService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke KSRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.AddMinutes(-defaultPastTime);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得上次結束時間
            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "KS取得注單排程",
                    description = "KS記錄end_time"
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
                _logger.LogInformation("KS record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");


            try
            {
                if ((now - lastEndTime).TotalDays > 20)
                {
                    lastEndTime = now.AddDays(-20);
                    parameter.value = lastEndTime.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                }

                var Page = 1;
                var req = new LogGetRequest
                {
                    OrderType = "All",
                    Type = "UpdateAt",
                    PageIndex = Page,
                    PageSize = 1000,
                    StartAt = lastEndTime,
                    EndAt = nextTime.AddSeconds(-1)
                };



                if (lastEndTime >= nextTime)
                {
                    return;
                }


                LogGetResponse res = new LogGetResponse()
                {
                    list = new List<Record>()
                };
                while (true)
                {
                    req.PageIndex = Page;
                    KSBaseRespones<LogGetResponse> betLogs = await _gameApiService._KSAPI.LogGet(req);

                    if (betLogs.success == (int)ErrorCodeEnum.success)
                    {
                        if (betLogs.info.list.Count > 0)
                        {
                            res.list.AddRange(betLogs.info.list);
                        }

                        if (Page * req.PageSize >= betLogs.info.RecordCount)
                        {
                            break;
                        }
                        Page++;
                        //api建議4秒爬一次
                        await Task.Delay(4000);
                    }
                    else
                    {
                        break;
                    }
                }
                if (!res.list.Any())
                {
                    await _systemParameterDbService.PutSystemParameter(parameter);
                    return;
                }
                await _gameRecordService._KSInterfaceService.PostKSRecord(res.list);

                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run KS record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
