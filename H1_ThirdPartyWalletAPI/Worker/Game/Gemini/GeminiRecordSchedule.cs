using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Service.Game.Gemini;
using ThirdPartyWallet.Share.Model.Game.Gemini.Request;
using System.Linq;
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;

namespace H1_ThirdPartyWalletAPI.Worker.Game.Gemini
{
    public class GeminiRecordSchedule: IInvocable
    {
        private readonly ILogger<GeminiRecordSchedule> _logger;
        private readonly IGeminiApiService _geminiApiService;
        private readonly IGeminiInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "GEMINIRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public GeminiRecordSchedule(ILogger<GeminiRecordSchedule> logger, IGeminiInterfaceService apiInterfaceService, IGeminiApiService geminiApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _geminiApiService = geminiApiService;
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
            _logger.LogInformation("Invoke GeminiRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "Gemini取得注單排程",
                    description = "Gemini記錄end_time"
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
                _logger.LogInformation("Gemini record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                string[] type_array = new string[] { "Create", "Reckon" };
                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var req = new BetlistRequest
                {
                    timetype = "",
                    begintime = (long)(lastEndTime - unixEpoch).TotalMilliseconds,
                    endtime = (long)(nextTime.AddMilliseconds(-1) - unixEpoch).TotalMilliseconds,
                    page = 0,
                    num = 1000
                };

                if ((nextTime - lastEndTime).TotalMinutes > 60)
                {
                    req.endtime = (long)(lastEndTime.AddHours(1).AddMilliseconds(-1) - unixEpoch).TotalMilliseconds;
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                }
                await _systemParameterDbService.PutSystemParameter(parameter);

                var res = new List<BetlistResponse.Datalist>();
                var resPK = res.Select(x => new { x.billNo, x.billstatus, x.createtime }).ToHashSet();

                foreach (var time_type in type_array)
                {
                    req.timetype = time_type;
                    var Page = 0;
                    while (true)
                    {
                        req.page = Page;
                        var betLogs = await _geminiApiService.BetlistAsync(req);

                        if (betLogs.data.total == 0)
                        {
                            break;
                        }
                        foreach (var itme in betLogs.data.datalist)
                        {
                            if (resPK.Add(new { itme.billNo, itme.billstatus, itme.createtime }))
                            {
                                res.Add(itme);
                            }
                        }

                        if (Page > betLogs.data.total / 1000)
                            break;

                        Page++;
                    
                        await Task.Delay(1000);
                    }
                    await Task.Delay(1000);
                }
                if (!res.Any())
                {
                    return;
                }
                await _apiInterfaceService.PostGeminiRecord(res);


            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run Gemini record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
