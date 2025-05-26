using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game.BTI;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker.Game.BTI
{
    public class BTIRecordSchedule : IInvocable
    {
        private readonly ILogger<BTIRecordSchedule> _logger;
        private readonly IDBService _dbService;
        private readonly IBTIInterfaceService _apiInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const string SYSTEM_PARAMETERS_KEY = "BTIRecordSchedule";
        // 注單投注之後需要約5~10分鐘才會進到dataapi
        // 所以剛投注完的確撈不到 需要等一下 我們建議的排成設定是每10分鐘往前撈10分鐘
        // 比如說15:30撈15:10~15:20，15:40撈15:20~15:30
        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-10);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public BTIRecordSchedule(
            ILogger<BTIRecordSchedule> logger,
            IDBService dbService,
            IBTIInterfaceService apiInterfaceService,
            ISystemParameterDbService systemParameterDbService
        )
        {
            _logger = logger;
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
            _logger.LogInformation("Invoke {schedule} on time : {time}", SYSTEM_PARAMETERS_KEY, DateTime.Now.ToLocalTime());

            // 取得當前時間，計算下一個拉帳的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得排程設定值
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
                    name = "BTI取得注單排程",
                    description = "BTI記錄end_time"
                };

                // 取得上次結束時間
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

            // 排程開關 0: 關閉, 1: 開啟
            if (int.Parse(parameter.min_value) == 0)
            {
                _logger.LogInformation("{schedule} stop time: {time}", SYSTEM_PARAMETERS_KEY, parameter.value);
                await Task.CompletedTask;
                return;
            }

            if (Convert.ToDateTime(parameter.value) >= nextTime)
            {
                _logger.LogInformation("return {schedule} current time : {now} reportTime : {reportTime}", SYSTEM_PARAMETERS_KEY, now, parameter.value);
                await Task.CompletedTask;
                return; // 時間不變就結束排程
            }

            try
            {
                var lastEndTime = DateTime.Parse(parameter.value);
                parameter.value = lastEndTime.Add(RANGE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss");

                // 更新參數
                await _systemParameterDbService.PutSystemParameter(parameter);

                // 調用廠商拉帳 BTI 是 ( 起始時間 <= 要查的範圍 <= 結束時間)
                var betInfos = await _apiInterfaceService.CallBTIRecord(lastEndTime, lastEndTime.Add(RANGE_OFFSET).AddSeconds(-1));

                if (!betInfos.Any())
                    return;

                // 寫入注單
                await _apiInterfaceService.PostBTIRecord(betInfos);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("{schedule} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", SYSTEM_PARAMETERS_KEY, ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
