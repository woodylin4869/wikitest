using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.RCG3;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.RCG3.Request;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RCG3
{
    public class Rcg3RecordSchedule : IInvocable
    {
        private readonly ILogger<Rcg3RecordSchedule> _logger;
        private readonly IDBService _dbService;
        private readonly IRCG3InterfaceService _apiInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const string SYSTEM_PARAMETERS_KEY = "Rcg3RecordSchedule";
        private const int _limit = 2000;

        public Rcg3RecordSchedule(
            ILogger<Rcg3RecordSchedule> logger,
            IDBService dbService,
            IRCG3InterfaceService apiInterfaceService,
            ISystemParameterDbService systemParameterDbService
        )
        {
            _logger = logger;
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
            _logger.LogInformation("Invoke {schedule} on time : {time}", SYSTEM_PARAMETERS_KEY, DateTime.Now.ToLocalTime());

            try
            {
                // 排程開關
                var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = SYSTEM_PARAMETERS_KEY,
                        value = null,
                        min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                        name = "RCG3取得注單排程",
                        description = "RCG3排程開關"
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
                    _logger.LogInformation("{schedule} stop time: {time}", SYSTEM_PARAMETERS_KEY, parameter.value);
                    await Task.CompletedTask;
                    return;
                }

                GetBetRecordListRequest request = new GetBetRecordListRequest();
                string recordKey = SYSTEM_PARAMETERS_KEY+ ",SerialNumber";

                // 當前站台拉單recordFlag
                var recordFlag = await _systemParameterDbService.GetSystemParameter(recordKey);

                // 拉單 檢查有無資料，沒資料的話新增預設值
                if (recordFlag == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = recordKey,
                        value = "0",
                        min_value = null,
                        name = "RCG3取得注單排程 站台當前流水號",
                        description = "RCG3流水號"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        recordFlag = model;
                    }
                    else
                    {
                        return; // 新增失敗就結束排程
                    }
                }

                // 一般拉單
                request.maxId = long.Parse(recordFlag.value);
                request.rows = _limit;
                var RecordResponse = await _apiInterfaceService.CallRCG3Record(request);

                if (RecordResponse.data.dataList.Count > 0)
                {
                    // 本次回傳的最大流水號
                    recordFlag.value = RecordResponse.data.dataList.Max(t => t.id).ToString();

                    // 寫入注單
                    await _apiInterfaceService.PostRcg3RecordDetail(RecordResponse.data.dataList, RecordResponse.data.systemCode, RecordResponse.data.webId);

                    // 更新參數
                    await _systemParameterDbService.PutSystemParameter(recordFlag);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("{schedule} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", SYSTEM_PARAMETERS_KEY, ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
