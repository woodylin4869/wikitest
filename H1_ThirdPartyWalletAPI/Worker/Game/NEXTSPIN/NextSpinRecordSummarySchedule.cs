using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Worker.Game.RGRICH;
using Microsoft.Extensions.Logging;
using System;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model;
using System.Collections.Generic;
using System.Linq;
using H1_ThirdPartyWalletAPI.Code;

namespace H1_ThirdPartyWalletAPI.Worker.Game.NEXTSPIN
{
    public class NextSpinRecordSummarySchedule : IInvocable
    {
        private readonly ILogger<NextSpinRecordSummarySchedule> _logger;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private static DateTime last_endtime;

        // 遊戲注單產生後15分執行
        private const int defaultPastTime = 15;

        public NextSpinRecordSummarySchedule(ILogger<NextSpinRecordSummarySchedule> logger, IGameInterfaceService gameInterfaceService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });
            _logger.LogInformation("Invoke NextSpinRecordSummarySchedule on time : {time}", DateTime.Now);
            try
            {
                t_system_parameter parameter = null;
                var key = "NextSpinRecordSummarySchedule";

                // NextSpin會總排程
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
                        name = "NextSpin-5分鐘匯總排程時間",
                        description = "NextSpin-5分鐘匯總排程時間基準點"
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

                    if (currentTime < Convert.ToDateTime(nextTime))
                    {
                        parameter.value = currentTime.AddMinutes(5).ToString("yyyy-MM-dd HH:mm");
                    }
                    else
                    {
                        return; // 時間不變就結束排程
                    }
                }

                if (int.Parse(parameter.min_value) == 0)
                {
                    _logger.LogInformation("NextSpinRecordSummarySchedule stop time: {time}", parameter.value);
                    await Task.CompletedTask;
                    return;
                }


                // 查詢時間寫回 DB
                await _gameInterfaceService.RecordSummary(Platform.NEXTSPIN, Convert.ToDateTime(parameter.value), DateTime.MinValue, DateTime.MinValue);
                await _systemParameterDbService.PutSystemParameterValue(parameter);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} {message}", nameof(NextSpinRecordSummarySchedule), ex.Message);
            }
        }
    }
}
