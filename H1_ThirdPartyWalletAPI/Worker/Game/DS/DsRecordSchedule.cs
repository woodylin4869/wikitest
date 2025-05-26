using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
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
    public class DsRecordSchedule : IInvocable
    {
        private readonly ILogger<DsRecordSchedule> _logger;
        private readonly GameRecordService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const int defaultPastTime = 3;
        private const int pageLimit = 5000;
        private const int getDelayMS = 10000;

        public DsRecordSchedule(ILogger<DsRecordSchedule> logger
            , IGameApiService gameaApiService
            , ICommonService commonService
            , GameRecordService gameRecordService
            , ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameRecordService = gameRecordService;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            try
            {
                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddMinutes(-defaultPastTime);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                var key = "DsRecordSchedule_V2";
                // 取得DS Record 時間
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                GetBetRecordRequest req = new GetBetRecordRequest();
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = string.Format("{0}", 1),
                        name = "DS取得注單排程",
                        description = "DS每分鐘注單排程"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        req.finish_time.start_time = DateTime.Parse(parameter.value);
                        req.finish_time.end_time = req.finish_time.start_time.AddMinutes(1).AddSeconds(-1);
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
                        _logger.LogInformation("Ds record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        req.finish_time.start_time = DateTime.Parse(parameter.value);
                        req.finish_time.end_time = req.finish_time.start_time.AddMinutes(1).AddSeconds(-1);
                        parameter.value = req.finish_time.start_time.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        _logger.LogInformation("Ds record same excute time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return; // 時間不變就結束排程
                    }
                }
                var isEnable = true;
                var pageIndex = 0;
                //從DS取得的原始資料
                List<DSBetRecord> dsBetRecord = new List<DSBetRecord>();
                //要寫入的資料
                List<DSBetRecord> dsPostBetRecords = new List<DSBetRecord>();
                while (isEnable)
                {
                    var betRecord = await _gameApiService._DsAPI.GetBetRecord(new GetBetRecordRequest()
                    {
                        finish_time = new FinishTime
                        {
                            start_time = req.finish_time.start_time,
                            end_time = req.finish_time.end_time
                        },
                        index = pageIndex,
                        limit = pageLimit
                    });

                    dsBetRecord.AddRange(betRecord.rows);

                    if (dsBetRecord.Count >= Convert.ToInt32(betRecord.total))
                    {
                        isEnable = false;
                    }
                    else
                    {
                        pageIndex++;
                        await Task.Delay(getDelayMS); // 查詢注單限制 十秒內只允許五次查詢
                    }
                }
                //排除重複注單
                if (dsBetRecord.Count > 0)
                {
                    foreach (DSBetRecord r in dsBetRecord)
                    {
                        var dsBetRecordCount = dsPostBetRecords.Where(x => x.id == r.id).ToList();
                        if (dsBetRecordCount.Count() == 0)
                        {
                            //確認Agnet id
                            if (r.agent == Config.CompanyToken.DS_AGENT)
                            {
                                dsPostBetRecords.Add(r);
                            }
                        }
                    }
                }
                if (dsPostBetRecords.Count > 0)
                {
                    await _gameRecordService._dsInterfaceService.PostDsRecordDetail(dsPostBetRecords);
                }
                await _systemParameterDbService.PutSystemParameter(parameter);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run Ds record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}