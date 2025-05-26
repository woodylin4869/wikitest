using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Service;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class AeRecordSchedule : IInvocable
    {
        private readonly ILogger<AeRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly GameRecordService _gameRecordService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const int defaultPastTime = 5;

        public AeRecordSchedule(ILogger<AeRecordSchedule> logger,
            IGameApiService gameaApiService,
            ICommonService commonService,
            GameRecordService gameRecordService,
            ISystemParameterDbService systemParameterDbService)
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
            try
            {
                _logger.LogInformation("Invoke AeRecordSchedule on time : {time}", DateTime.Now);
                // 取得當前時間
                var now = DateTime.Now.ToLocalTime();
                now = now.AddMinutes(-defaultPastTime);
                var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                var req = new GetBetHistoriesRequest();
                var key = "AeRecordSchedule_V2";
                // 取得AE Record 時間
                t_system_parameter parameter = null;
                parameter = await _systemParameterDbService.GetSystemParameter(key);
                // 檢查有無資料，沒資料的話新增預設值
                if (parameter == null)
                {
                    var model = new t_system_parameter()
                    {
                        key = key,
                        value = nextTime.ToString("yyyy-MM-dd HH:mm:sszzz"),
                        min_value = string.Format("{0}", 1),
                        name = "Ae取得注單排程",
                        description = "Ae每分鐘注單排程"
                    };

                    var postSystemParameter = await _systemParameterDbService.PostSystemParameter(model);
                    if (postSystemParameter)
                    {
                        parameter = model;
                        req.from_time = DateTime.Parse(parameter.value);
                        req.to_time = req.from_time.AddMinutes(1);
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
                        _logger.LogInformation("Ae record stop time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return;
                    }
                    if (Convert.ToDateTime(parameter.value) < nextTime)
                    {
                        req.from_time = DateTime.Parse(parameter.value);
                        req.to_time = req.from_time.AddMinutes(1);
                        parameter.value = req.to_time.ToString("yyyy-MM-dd HH:mm:sszzz");
                    }
                    else
                    {
                        _logger.LogInformation("Ae record same excute time: {time}", parameter.value);
                        await Task.CompletedTask;
                        return; // 時間不變就結束排程
                    }
                }
                req.site_id = Config.CompanyToken.AE_SiteId;
                // 取資料
                var responseData = await _gameApiService._AeAPI.GetBetHistories(req);
                // 請求失敗
                if (responseData.Error_code != "OK")
                {
                    throw new Exception(responseData.Error_code);
                }
                // 沒資料
                if (!responseData.bet_histories.Any())
                {
                    await Task.CompletedTask;
                    await _systemParameterDbService.PutSystemParameter(parameter);
                    return;
                }
                // 設定預設值
                foreach (var item in responseData.bet_histories)
                {
                    if (item.bet_amt == null)
                    {
                        item.bet_amt = "0";
                    }

                    if (item.payout_amt == null)
                    {
                        item.payout_amt = "0";
                    }

                    if (item.end_balance == null)
                    {
                        item.end_balance = "0";
                    }

                    if (item.rebate_amt == null)
                    {
                        item.rebate_amt = "0";
                    }

                    if (item.jp_pc_con_amt == null)
                    {
                        item.jp_pc_con_amt = "0";
                    }

                    if (item.jp_jc_con_amt == null)
                    {
                        item.jp_jc_con_amt = "0";
                    }

                    if (item.jp_pc_win_amt == null)
                    {
                        item.jp_pc_win_amt = "0";
                    }

                    if (item.jp_jc_win_amt == null)
                    {
                        item.jp_jc_win_amt = "0";
                    }

                    if (item.prize_amt == null)
                    {
                        item.prize_amt = "0";
                    }
                }
                // 資料彙總處理
                await _gameRecordService._aeInterfaceService.PostAeRecordDetail(responseData.bet_histories);
                await _systemParameterDbService.PutSystemParameter(parameter);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run ae record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}