using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace H1_ThirdPartyWalletAPI.Worker
{
    public class RtgRecordSchedule : IInvocable
    {
        private readonly ILogger<RtgRecordSchedule> _logger;
        private readonly IRtgInterfaceService _rtgInterfaceService;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        const int defaultPastTime = 5;
        const int pageLimit = 100;
        const int getDelayMS = 200;

        public RtgRecordSchedule(ILogger<RtgRecordSchedule> logger,
            IRtgInterfaceService rtgInterfaceService,
            IGameApiService gameaApiService,
            ICommonService commonService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _rtgInterfaceService = rtgInterfaceService;
            _systemParameterDbService = systemParameterDbService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            try
            {
                // 取得當前時間，計算下一個帳務比對的時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddMinutes(-defaultPastTime);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                var key = "RtgRecordSchedule";
                // 取得RTG Record 時間
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                var req = new GameSettlementRecordRequest();
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        min_value = string.Format("{0}", 1),
                        name = "RTG取得注單排程",
                        description = "RTG每分鐘注單排程"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        req.StartTime = parameter.value;
                        req.EndTime = DateTime.Parse(parameter.value).AddMinutes(1).AddSeconds(-1).ToString();
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
                        _logger.LogInformation("Rtg record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        req.StartTime = parameter.value;
                        req.EndTime = DateTime.Parse(parameter.value).AddMinutes(1).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss");
                        parameter.value = DateTime.Parse(parameter.value).AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        _logger.LogInformation("Rtg record same excute time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return; // 時間不變就結束排程
                    }
                }                
                
                //從RTG取得的原始資料
                List<Record> rtgBetRecord = new List<Record>();
                var gamelist = Model.Game.RTG.RTG.GameList.Keys.ToList();
                foreach (var game in gamelist)
                {
                    List<Record> rtgBetRecordByGame = new List<Record>();
                    var pageIndex = 1;
                    var isEnable = true;
                    while (isEnable)
                    {
                        var betRecord = await _gameApiService._RtgAPI.GameSettlementRecord(new GameSettlementRecordRequest()
                        {
                            SystemCode = Config.CompanyToken.RTG_SystemCode,
                            WebId = Config.CompanyToken.RTG_WebID,
                            GameId = game,
                            StartTime = req.StartTime,
                            EndTime = req.EndTime,
                            Page = pageIndex,
                            Rows = pageLimit
                        });

                        foreach(var record in betRecord.Data.Record)
                        {
                            record.game_id = game;
                            rtgBetRecordByGame.Add(record);
                        }

                        if (rtgBetRecordByGame.Count >= Convert.ToInt32(betRecord.Data.TotalCount))
                        {
                            isEnable = false;
                        }
                        else
                        {
                            pageIndex++;
                            await Task.Delay(getDelayMS);
                        }
                    }
                    rtgBetRecord.AddRange(rtgBetRecordByGame);
                }
                if (rtgBetRecord.Count > 0)
                {
                    await _rtgInterfaceService.PostRtgRecord(rtgBetRecord);
                }
                await _systemParameterDbService.PutSystemParameter(parameter);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run RTG record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }

}
