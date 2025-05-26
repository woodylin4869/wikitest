using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class SexyRecordSchedule : IInvocable
    {
        private readonly ILogger<SexyRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ISEXYDBService _sexydbService;

        private const string SYSTEM_PARAMETERS_KEY = "SexyRecordSchedule";


        public SexyRecordSchedule(ILogger<SexyRecordSchedule> logger, GameRecordService gameRecordService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ISEXYDBService SEXYDBService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _gameRecordService = gameRecordService;
            _systemParameterDbService = systemParameterDbService;
            _sexydbService = SEXYDBService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke SexyRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.AddMinutes(-1);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得上次結束時間
            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                nextTime = await _sexydbService.GetsexyLastupdatetime();
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "SEXY取得注單排程",
                    description = "SEXY記錄end_time"
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
                _logger.LogInformation("sexy record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastUpdateTime = DateTime.Parse(parameter.value);
            parameter.value = nextTime.ToString("yyyy-MM-dd HH:mm:ss");


            try
            {


                if ((DateTime.Now - lastUpdateTime.AddSeconds(-1)).Days >= 1)
                {
                    lastUpdateTime = DateTime.Now.AddDays(-1).AddMinutes(1);
                    parameter.value = lastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                var req = new GetTransactionByUpdateDateRequest
                {
                    cert = Config.CompanyToken.SEXY_Cert,
                    agentId = Config.CompanyToken.SEXY_Agent,
                    timeFrom = lastUpdateTime,
                    platform = "SEXYBCRT",
                    currency = "THB",
                    delayTime = 10000
                };



                if (lastUpdateTime >= nextTime)
                {
                    return;
                }


                GetTransactionByUpdateDateResponse res = new GetTransactionByUpdateDateResponse()
                {
                    transactions = new List<Record>()
                };

                GetTransactionByUpdateDateResponse betLogs = await _gameApiService._SexyApi.GetTransactionByUpdateDate(req);

                if (betLogs.status == (int)ErrorCodeEnum.Success)
                {
                    if (betLogs.transactions.Count > 0)
                    {
                        res.transactions.AddRange(betLogs.transactions);
                        await _gameRecordService._SEXYInterfaceService.PostSexyRecord(res.transactions);

                        lastUpdateTime = res.transactions.Max(x => x.updateTime).AddSeconds(1);
                        parameter.value = lastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run sexy record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }
    }
}
