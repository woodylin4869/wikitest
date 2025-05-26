using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class RcgRecordSchedule : IInvocable
    {
        private readonly ILogger<RcgRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly GameRecordService _gameRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const int _limit = 2000;
        private const int memory_cache_min = 15; //分鐘
        private const string memory_cache_key = "RCG_System_Web_Code";
        private readonly IMemoryCache _memoryCache;
        private const string SYSTEM_PARAMETERS_KEY = "RcgRecordSchedule";

        public RcgRecordSchedule(ILogger<RcgRecordSchedule> logger
            , IGameApiService gameaApiService
            , GameRecordService gameRecordService
            , ICommonService commonService
            , IMemoryCache memoryCache
            , ISystemParameterDbService systemParameterDbService
        )
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _gameRecordService = gameRecordService;
            _memoryCache = memoryCache;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            _logger.LogInformation("Invoke RcgRecordSchedule on time : {time}", DateTime.Now);

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
                        name = "RCG取得注單排程",
                        description = "RCG排程開關"
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
                    //_logger.LogInformation("RcgRecordSchedule stop time: {time}", parameter.value);
                    _logger.LogInformation("RcgRecordSchedule stop");
                    await Task.CompletedTask;
                    return;
                }

                RCG_GetBetRecordList request = new RCG_GetBetRecordList();
                RCG_GetChangeRecordList requestChange = new RCG_GetChangeRecordList();
                var systemWebCodeList = await _memoryCache.GetOrCreateAsync(memory_cache_key, async entry =>
                {
                    IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetSystemWebCode();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(memory_cache_min));
                    _memoryCache.Set(memory_cache_key, systemWebCode, cacheEntryOptions);
                    return systemWebCode;
                });

                string recordKey = string.Empty;
                string recordChangeKey = string.Empty;
                foreach (var code in systemWebCodeList)
                {
                    request.systemCode = requestChange.systemCode = code.system_code;
                    request.webId = requestChange.webId = code.web_id;
                    recordKey = SYSTEM_PARAMETERS_KEY + "," + request.systemCode + "/" + request.webId;
                    recordChangeKey = recordKey + ",change";

                    // 當前站台拉單recordFlag
                    var recordFlag = await _systemParameterDbService.GetSystemParameter(recordKey);
                    // 當前站台拉改單recordFlag
                    var recordChangeFlag = await _systemParameterDbService.GetSystemParameter(recordChangeKey);

                    // 拉單 檢查有無資料，沒資料的話新增預設值
                    if (recordFlag == null)
                    {
                        var model = new t_system_parameter()
                        {
                            key = recordKey,
                            value = "0",
                            min_value = null,
                            name = "RCG取得注單排程 站台當前流水號",
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

                    // 拉改單 檢查有無資料，沒資料的話新增預設值
                    if (recordChangeFlag == null)
                    {
                        var model = new t_system_parameter()
                        {
                            key = recordChangeKey,
                            value = "0",
                            min_value = null,
                            name = "RCG取得注單排程 站台當前改單流水號",
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
                    long currentMaxId = request.maxId = int.Parse(recordFlag.value);
                    request.rows = _limit;
                    var RecordResponse = await _gameApiService._RcgAPI.GetBetRecordList(request);
                    if (RecordResponse.data.dataList.Count > 0)
                    {
                        recordFlag.value = RecordResponse.data.dataList.Max(t => t.id.ToString());

                        await _gameRecordService._rcgInterfaceService.PostRcgRecord(RecordResponse.data.dataList, RecordResponse.data.systemCode, RecordResponse.data.webId);

                        await _systemParameterDbService.PutSystemParameter(recordFlag);
                    }

                    // 拉改單
                    /*requestChange.maxId = int.Parse(recordChangeFlag.value);
                    requestChange.rows = _limit;
                    var ChangeRecordResponse = await _gameApiService._RcgAPI.GetChangeRecordList(requestChange);
                    if (ChangeRecordResponse.data.dataList.Count > 0)
                    {
                        // 判斷改單不能超車一般拉單 若本批有超車的下次再拉
                        if (currentMaxId < ChangeRecordResponse.data.dataList.Min(t => t.id))
                        {
                            return;
                        }

                        recordFlag.value = ChangeRecordResponse.data.dataList.Max(t => t.id.ToString());

                        await _gameRecordService._rcgInterfaceService.PostRcgRecord(RecordResponse.data.dataList, RecordResponse.data.systemCode, RecordResponse.data.webId);

                        await _commonService._serviceDB.PutSystemParameter(recordChangeFlag);
                    }*/
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RcgRecordSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
