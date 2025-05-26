using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model.Game;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class PgRecordSchedule : IInvocable
    {
        private readonly ILogger<PgRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly GameRecordService _gameRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const int record_limit = 3000; //Min 1500

        public PgRecordSchedule(ILogger<PgRecordSchedule> logger, IGameApiService gameaApiService, ICommonService commonService, GameRecordService gameRecordService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
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
            _logger.LogInformation("Invoke PgRecordSchedule on time : {time}", DateTime.Now);
            try
            {
                GetHistoryRequest request = new GetHistoryRequest();
                request.bet_type = 1;
                request.count = record_limit;
                request.hands_status = 0;
                request.row_version = 1;
                request.operator_token = Config.CompanyToken.PG_Token;
                request.secret_key = Config.CompanyToken.PG_Key;

                var key = "PgRecordSchedule";
                //取得上次請求最大row_version值
                t_system_parameter parameter = await _systemParameterDbService.GetSystemParameter(key);
                if (parameter.value == null)
                {
                    DateTime dt = DateTime.Now.AddMinutes(-10);
                    request.row_version = (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds + 1;
                }
                else
                {
                    if (int.Parse(parameter.min_value) == 0)
                    {
                        _logger.LogInformation("Pg record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    request.row_version = long.Parse(parameter.value);
                }
                var result = await _gameApiService._PgAPI.GetHistoryAsync(request);

                if (result.data.Count > 0)
                {
                    await _gameRecordService._pgInterfaceService.PostPgRecord(result.data);
                    // 查詢時間寫回 DB
                    parameter.value = result.data.Max(x => x.rowVersion).ToString();
                    //parameter.value = request.row_version.ToString();
                    await _systemParameterDbService.PutSystemParameter(parameter);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run pg record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

    }

}
