using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Service.Game.PS;
using ThirdPartyWallet.Share.Model.Game.PS.Request;
using System.Linq;
using ThirdPartyWallet.Share.Model.Game.PS.Response;
using ThirdPartyWallet.GameAPI.Service.Game.PS;
using Microsoft.Extensions.Options;
using ThirdPartyWallet.Share.Model.Game.PS;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Enum;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Worker.Game.PS
{
    public class PSRecordSchedule: IInvocable
    {
        private readonly ILogger<PSRecordSchedule> _logger;
        private readonly IPsApiService _PSApiService;
        private readonly IPsInterfaceService _apiInterfaceService;
        private readonly IDBService _dbService;
        private readonly IOptions<PsConfig> _options;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "PSRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-5);
        private readonly TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public PSRecordSchedule(ILogger<PSRecordSchedule> logger, IPsInterfaceService apiInterfaceService, IPsApiService PSApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, IOptions<PsConfig> options)
        {
            _logger = logger;
            _PSApiService = PSApiService;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemParameterDbService = systemParameterDbService;
            _options = options;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke PSRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "PS取得注單排程",
                    description = "PS記錄end_time"
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
                _logger.LogInformation("PS record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }


            //parameter.value = "2024-08-16 11:43:00";

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (lastEndTime >= nextTime)
            {
                return;
            }
            try
            {
                string[] type_array = new string[] { "SLOT" };
                var req = new GetorderRequest
                {
                    host_id = _options.Value.PS_hostid,
                    game_type = "",
                    start_dtm = lastEndTime,
                    end_dtm = lastEndTime.AddMinutes(1).AddMilliseconds(-1),
                    detail_type = 0
                };

                parameter.value = lastEndTime.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                await _systemParameterDbService.PutSystemParameter(parameter);

                var res = new List<GetorderResponse.BetRecord>();
                var resPK = res.Select(x => new { x.sn, x.s_tm }).ToHashSet();

                foreach (var game_type in type_array)
                {
                    req.game_type = game_type;
                    var betLogs = await _PSApiService.gamehistoryAsync(req);
                    if (betLogs == null)
                    {
                        break;
                    }
                    string member_id = "";
                    foreach (var dateEntry in betLogs)
                    {
                        foreach (var memberEntry in dateEntry.Value)
                        {
                            member_id = memberEntry.Key;

                            foreach (var record in memberEntry.Value)
                            {
                                if (resPK.Add(new { record.sn, record.s_tm }))
                                {
                                    record.member_id = member_id;
                                    res.Add(record);
                                }
                            }
                        }
                    }
                }
                if (!res.Any())
                {
                    return;
                }
                await _apiInterfaceService.PostPsRecord(res);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run PS record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
