using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game.RCG2;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RCG2
{
    public class Rcg2RecordSchedule : IInvocable
    {
        private readonly ILogger<Rcg2RecordSchedule> _logger;
        private readonly IDBService _dbService;
        private readonly IRCG2InterfaceService _apiInterfaceService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const string SYSTEM_PARAMETERS_KEY = "Rcg2RecordSchedule";
        private const int _limit = 2000;
        private string _systemCode;
        private string _webId;

        public Rcg2RecordSchedule(
            ILogger<Rcg2RecordSchedule> logger,
            IDBService dbService,
            IRCG2InterfaceService apiInterfaceService,
            ISystemParameterDbService systemParameterDbService
        )
        {
            _logger = logger;
            _dbService = dbService;
            _apiInterfaceService = apiInterfaceService;
            _systemCode = Config.CompanyToken.RCG2_SystemCode;
            _systemParameterDbService = systemParameterDbService;
            _webId = Config.CompanyToken.RCG2_WebId;
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
                        name = "RCG2取得注單排程",
                        description = "RCG2排程開關"
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
                string recordKey = string.Empty;
                request.systemCode = _systemCode;
                request.webId = _webId;
                recordKey = SYSTEM_PARAMETERS_KEY + "," + request.systemCode + "/" + request.webId;

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
                        name = "RCG2取得注單排程 站台當前流水號",
                        description = request.systemCode + "/" + request.webId
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
                var RecordResponse = await _apiInterfaceService.CallRCG2Record(request);

                if (RecordResponse.data.dataList.Count > 0)
                {
                    // 本次回傳的最大流水號
                    recordFlag.value = RecordResponse.data.dataList.Max(t => t.id).ToString();

                    // 寫入注單
                    await _apiInterfaceService.PostRcg2RecordDetail(RecordResponse.data.dataList, RecordResponse.data.systemCode, RecordResponse.data.webId);

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
