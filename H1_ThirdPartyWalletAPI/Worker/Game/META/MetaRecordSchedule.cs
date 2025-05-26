using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.META.Request;
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Game.META;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace H1_ThirdPartyWalletAPI.Worker
{
    public class MetaRecordSchedule : IInvocable
    {
        private readonly ILogger<MetaRecordSchedule> _logger;
        private readonly IMETAInterfaceService _metaInterfaceService;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly IMETADBService _metaDbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        const int defaultPastTime = 5;
        private static long LastSerial = 0;
        private static int pendingCount = 0;

        public MetaRecordSchedule(ILogger<MetaRecordSchedule> logger,
            IMETAInterfaceService metaInterfaceService,
            IGameApiService gameaApiService,
            ICommonService commonService,
            IMETADBService metaDbService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _metaInterfaceService = metaInterfaceService;
            _metaDbService = metaDbService;
            _systemParameterDbService = systemParameterDbService;
        }


        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke MetaRecordSchedule on time : {time}", DateTime.Now);

            try
            {
                _logger.LogDebug("Meta Record Schedule countdown : {count}", pendingCount);
                //LastSerial相同時要延遲請求
                if (pendingCount != 0)
                {
                    pendingCount--;
                    await Task.CompletedTask;
                    return;
                }
                BetOrderRecordRequest req = new BetOrderRecordRequest();
                //避免跨日注單無法拉取
                DateTime date = DateTime.Now.AddMinutes(-defaultPastTime);
                var dt2 = new DateTimeOffset(date);
                long _UnixTime = dt2.ToUnixTimeSeconds();
                req.Date = _UnixTime;

                var key = "MetaRecordSchedule";
                // 取得META Max last_serial
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var last_serial = await _metaDbService.GetmetaLastSerial();
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = last_serial.ToString(),
                        min_value = string.Format("{0}", 1),
                        name = "META取得注單排程",
                        description = "META記錄最大last_serial"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        req.LastSerial = last_serial;
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
                        _logger.LogInformation("Meta record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    req.LastSerial = long.Parse(parameter.value);
                }

                BetOrderRecordResponse result = await _gameApiService._MetaApi.BetOrderRecord(req);
                if (result.DecryptStatus)
                {
                    //延遲1分鐘再取
                    if (result.overRows == 0)
                    {
                        pendingCount = 3;
                    }

                    if (result.rows.Count > 0)
                    {
                        await _metaInterfaceService.PostMetaRecord(result.rows);
                        // 查詢時間寫回 DB
                        parameter.value = result.rows.Max(x => x.Serial).ToString();
                        await _systemParameterDbService.PutSystemParameter(parameter);
                    }
                }
                else
                {
                    pendingCount = 3;
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run meta record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
