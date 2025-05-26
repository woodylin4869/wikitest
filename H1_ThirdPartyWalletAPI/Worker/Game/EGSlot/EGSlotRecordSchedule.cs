using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.EGSlot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Response;

namespace H1_ThirdPartyWalletAPI.Worker.Game.EGSlot
{
    public class EGSlotRecordSchedule : IInvocable
    {
        private readonly ILogger<EGSlotRecordSchedule> _logger;
        private readonly IEGSlotApiService _EGSlotApiService;
        private readonly IEGSlotInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "EGSlotRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public EGSlotRecordSchedule(ILogger<EGSlotRecordSchedule> logger, IEGSlotInterfaceService apiInterfaceService, IEGSlotApiService EGSlotApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _EGSlotApiService = EGSlotApiService;
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
            _logger.LogInformation("Invoke EGSlotRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "EGSlot取得注單排程",
                    description = "EGSlot記錄end_time"
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
                _logger.LogInformation("EGSlot record stop time: {time}", parameter.value);
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
                int[] Status = new int[] { 0, 1 };

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                var req = new TransactionRequest
                {
                    Status = 1,
                    Page = 1,
                    PageSize = 2000,
                    StartTime = (long)(lastEndTime - unixEpoch).TotalMilliseconds,
                    EndTime = (long)(nextTime - unixEpoch).TotalMilliseconds,
                    // StartTime = parameter.value
                };



                if ((nextTime - lastEndTime).TotalMinutes > 60)
                {
                    req.EndTime = (long)(lastEndTime.AddHours(1) - unixEpoch).TotalMilliseconds;
                    parameter.value = lastEndTime.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss");
                }

                await _systemParameterDbService.PutSystemParameter(parameter);

                var res = new List<Datum>();
                var resPK = res.Select(x => new { x.MainTxID, x.Status, x.BetTime }).ToHashSet();


                var Page = 1;
                foreach (var element in Status)
                {
                    req.Status = element;
                    while (true)
                    {
                        req.Page = Page;
                        var betRecord = await _EGSlotApiService.TransactionAsync(req);

                        // 有錯誤就拋
                        if (string.IsNullOrEmpty(betRecord.Message) == false && betRecord.ErrorCode != 0)
                        {
                            throw new Exception(betRecord.Message);
                        }

                        foreach (var itme in betRecord.Data)
                        {
                            if (resPK.Add(new { itme.MainTxID, itme.Status, itme.BetTime }))
                            {
                                res.Add(itme);
                            }
                        }

                        if (!betRecord.Next)
                            break;

                        Page++;

                        await Task.Delay(1000);
                    }
                }


                if (!res.Any())
                {
                    return;
                }
                await _apiInterfaceService.PostEGSlotRecord(res);


            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run EGSLOT record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
