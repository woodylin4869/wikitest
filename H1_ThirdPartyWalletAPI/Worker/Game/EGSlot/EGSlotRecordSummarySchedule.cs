using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model;
using System.Linq;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Worker.Game.EGSlot
{
    public class EGSlotRecordSummarySchedule : IInvocable
    {
        private readonly ILogger<EGSlotRecordSummarySchedule> _logger;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ICommonService _commonService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private static DateTime last_endtime;
        const int defaultPastTime = 15;
        public EGSlotRecordSummarySchedule(ILogger<EGSlotRecordSummarySchedule> logger,
            IGameInterfaceService gameInterfaceService,
            ICommonService commonService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
            _commonService = commonService;
            last_endtime = DateTime.Now.AddMinutes(-defaultPastTime);
            last_endtime = new DateTime(last_endtime.Year, last_endtime.Month, last_endtime.Day, last_endtime.Hour, (last_endtime.Minute / 5) * 5, 0);
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });
            _logger.LogInformation("Invoke EGSlotRecordSummarySchedule on time : {time}", DateTime.Now);


            try
            {
                t_system_parameter parameter = null;
                var key = "EGSLOTRecordSummarySchedule";

                // EGSLOT會總排程
                parameter = await _systemParameterDbService.GetSystemParameter(key);

                // 取得當前時間，計算下一個匯總的時間
                var dt = DateTime.Now.ToLocalTime().AddMinutes(-defaultPastTime);
                var nextTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm"),
                        min_value = "1", // 排程開關 0: 關閉, 1: 開啟
                        name = "EGSlot-5分鐘匯總排程時間",
                        description = "EGSlot-5分鐘匯總排程時間基準點"
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
                else
                {
                    // 紀錄的時間要是五分鐘匯總，不然找不到
                    var currentTime = Convert.ToDateTime(parameter.value);
                    var currentSummaryTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day,
                        currentTime.Hour, (currentTime.Minute / 5) * 5, 0);

                    if (currentSummaryTime < Convert.ToDateTime(nextTime))
                    {
                        parameter.value = currentSummaryTime.AddMinutes(5).ToString("yyyy-MM-dd HH:mm");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }

                if (int.Parse(parameter.min_value) == 0)
                {
                    _logger.LogInformation("EGSLOT summary record stop time: {time}", parameter.value);
                    await Task.CompletedTask;
                    return;
                }

                DateTime? startTime = null;
                DateTime? endTime = null;

                //  reporttime > playtime 取出
                var redisKey = $"{RedisCacheKeys.EGSlotBetSummaryTime}:{Convert.ToDateTime(parameter.value).ToString("yyyy-MM-dd HH:mm")}";
                var timeStringList = await _commonService._cacheDataService.ListGetAsync<string>(redisKey);
                var timeList = timeStringList?.Distinct().Select(Convert.ToDateTime).ToList();


                if (timeList != null && timeList.Any())
                {
                    // 找出最大最小值
                    startTime = timeList.Min().AddMinutes(-15);
                    endTime = timeList.Max().AddMinutes(15);
                }


                // 預設值
                if (startTime == null || endTime == null)
                {
                    startTime = Convert.ToDateTime(parameter.value).AddDays(-2);
                    endTime = Convert.ToDateTime(parameter.value).AddHours(1);
                }


                var timeSpan = new TimeSpan(endTime.Value.Ticks - startTime.Value.Ticks);
                if (timeSpan.TotalHours > 240)
                {
                    // 從最近的時間往前推 240 小時
                    startTime = endTime.Value.AddHours(-240);
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                // 查詢時間寫回 DB
                await _gameInterfaceService.RecordSummary(Platform.EGSLOT, Convert.ToDateTime(parameter.value), startTime.Value, endTime.Value);
                sw.Stop();
                _logger.LogInformation("EGSLOT summary record 寫入完成時間 {time}, 五分鐘匯總帳時間: {reporttime}, 開始時間: {starttime} 結束時間: {endtime}",
                    sw.ElapsedMilliseconds,
                    parameter.value,
                    startTime.Value.ToString("yyyy-MM-dd HH:mm"),
                    endTime.Value.ToString("yyyy-MM-dd HH:mm"));
                await _systemParameterDbService.PutSystemParameterValue(parameter);
                await _commonService._cacheDataService.KeyDelete(redisKey);
                await Task.CompletedTask;

            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run EGSLOTRecordSummarySchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }


        }
    }

}
