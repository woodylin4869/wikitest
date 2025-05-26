using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Request;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.GR;
using H1_ThirdPartyWalletAPI.Code;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class GrRecordSchedule : IInvocable
    {
        private readonly ILogger<GrRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly IDBService _dbService;
        private readonly IGRInterfaceService _grInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const string SYSTEM_PARAMETERS_KEY = "GrRecordSchedule_V2";
        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public GrRecordSchedule(ILogger<GrRecordSchedule> logger, IGRInterfaceService grInterfaceService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _grInterfaceService = grInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke GrRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
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
                    name = "GR取得注單排程",
                    description = "GR記錄end_time"
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
                _logger.LogInformation("GrRecordSchedule stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            if (lastEndTime >= nextTime)
            {
                return;
            }

            // 20230605 加入 (now - delay時間) 如果 > 上次執行時間超過10分鐘
            // true:
            TimeSpan s = new TimeSpan(nextTime.Ticks - lastEndTime.Ticks);
            if (s.Minutes > 10)
            {
                nextTime = Convert.ToDateTime(lastEndTime.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:00"));
                parameter.value = lastEndTime.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss");
            }
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");
            await _systemParameterDbService.PutSystemParameter(parameter);

            try
            {
                // 頁數1
                var Page = 1;
                var req = new CommBetDetailsRequest
                {
                    // 請求 GR 起始時間 與 結束時間 是包含等於
                    // 起始時間 <= 回傳資料時間 <= 結束時間
                    start_time = lastEndTime.AddHours(0).AddSeconds(1),
                    end_time = nextTime.AddHours(0).AddSeconds(0),
                    page_index = Page,
                    page_size = 1000
                };

                // 共用注單model
                CommBetDetailsResponse.DataInfo res = new CommBetDetailsResponse.DataInfo()
                {
                    bet_details = new List<CommBetDetailsResponse.CommBetDetails>()
                };

                // 拉取 SLOT 注單
                while (true)
                {
                    req.page_index = Page;
                    // todo: 是否每 loop 回傳抓到資料 就先更新 parameter & page_index
                    var betData = await _gameApiService._GrAPI.GetSlotAllBetDetails(req);

                    // 拉單 廠商回失敗
                    if (betData.status != "Y")
                    {
                        throw new ExceptionMessage((int)ResponseCode.GetGameRecordFail, betData.code + "|" + betData.message);
                    }

                    if (betData.data.total_elements == 0)
                    {
                        break;
                    }
                    res.bet_details.AddRange(betData.data.bet_details);

                    Page++;
                    if (Page > betData.data.total_pages)
                        break;

                    //api建議 ? 秒爬一次
                    await Task.Delay(1000);
                }

                // 拉取 FISH 注單
                // reset 頁數1
                Page = 1;
                while (true)
                {
                    req.page_index = Page;
                    // todo: 是否每 loop 回傳抓到資料 就先更新 parameter & page_index
                    var betData = await _gameApiService._GrAPI.GetFishAllBetDetails(req);

                    // 拉單 廠商回失敗
                    if (betData.status != "Y")
                    {
                        throw new ExceptionMessage((int)ResponseCode.GetGameRecordFail, betData.code + "|" + betData.message);
                    }

                    if (betData.data.total_elements == 0)
                    {
                        break;
                    }
                    res.bet_details.AddRange(betData.data.bet_details);

                    Page++;
                    if (Page > betData.data.total_pages)
                        break;

                    //api建議 ? 秒爬一次
                    await Task.Delay(1000);
                }

                if (!res.bet_details.Any())
                {
                    await _systemParameterDbService.PutSystemParameter(parameter);
                    return;
                }

                await _grInterfaceService.PostGRRecordDetail(res.bet_details);

                // 20230605 移到最上面執行
                // await _dbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("GrRecordSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}