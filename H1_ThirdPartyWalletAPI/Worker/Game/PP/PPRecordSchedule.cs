using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Responses;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.PP
{
    public class PPRecordSchedule : IInvocable
    {
        private readonly ILogger<PPRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "PPRecordSchedule";

        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-15);
        private readonly TimeSpan BATCH_OFFSET = TimeSpan.FromHours(3);

        public PPRecordSchedule(ILogger<PPRecordSchedule> logger, GameRecordService gameRecordService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService)
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
            _logger.LogInformation("Invoke PPRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


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
                    name = "PP取得注單排程",
                    description = "PP記錄end_time"
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
                _logger.LogInformation("PP record stop time: {time}", parameter.value);
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

                if ((nextTime - lastEndTime).TotalMinutes >10)
                {
                    parameter.value = lastEndTime.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:00");
                }

                await _systemParameterDbService.PutSystemParameter(parameter);

                DateTimeOffset dto = new DateTimeOffset(DateTime.Parse(parameter.value));
                var req = new GetRecordRequest
                {
                    login = Config.CompanyToken.PP_SecureLogin,
                    password = Config.CompanyToken.PP_Key,
                    timepoint = dto.ToUnixTimeMilliseconds().ToString()
                };
                List<GetRecordResponses> res = new List<GetRecordResponses>();

                res = await _gameApiService._PPAPI.GetRecordAsync(req);
               
                if (res.Count != 0)
                {
                    foreach(var group in res.GroupBy(r => r.StartDate.Ticks / BATCH_OFFSET.Ticks * BATCH_OFFSET.Ticks))
                    {
                        _logger.LogInformation("PostPPRecord Group Key:{key} Count:{count}", new DateTime(group.Key), group.Count());
                        try
                        {
                            await _gameRecordService._PPInterfaceService.PostPPRecord(group.ToList());
                        }
                        catch(Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError(ex, "Run PP record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine} StartDate:{StartDate}", ex.GetType().FullName, ex.Message, errorFile, errorLine, new DateTime(group.Key));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run PP record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
