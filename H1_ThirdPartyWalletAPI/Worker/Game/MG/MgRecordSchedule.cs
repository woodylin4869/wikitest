using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// MG 下注紀錄排程
    /// </summary>
    public class MgRecordSchedule : IInvocable
    {
        private readonly ILogger<MgRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly ICommonService _commonService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ICacheDataService _cacheDataService;
        private const int RECORD_LIMIT = 20000; //between 1 and 20000
        private const string SYSTEM_PARAMETERS_KEY = "MgRecordSchedule";
        private const string STARTING_AFTER_ZERO = "0";
        private readonly string _prefixKey;

        public MgRecordSchedule(ILogger<MgRecordSchedule> logger, IGameApiService gameaApiService
            , ICommonService commonService
            , GameRecordService gameRecordService
            , IDBService dbService
            , ISystemParameterDbService systemParameterDbService
            , ICacheDataService cacheDataService)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _gameRecordService = gameRecordService;
            _dbService = dbService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
            _systemParameterDbService = systemParameterDbService;
            _cacheDataService = cacheDataService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });


            _logger.LogInformation($"Invoke MgRecordSchedule on time : {DateTime.Now.ToLocalTime()}");

            //取得上次請求最大betuid值
            t_system_parameter parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // Bet data is kept for up to 10 days, and data that exceeds 10 days cannot be retrieved.
            DateTime presetTime = DateTime.Now.AddDays(-10);
            presetTime = new DateTime(presetTime.Year, presetTime.Month, presetTime.Day, 0, 0, 0, 0);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                var model = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = STARTING_AFTER_ZERO,
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    max_value = presetTime.ToString("O"),
                    name = "MG 記錄最大betuid與gameEndTimeUTC",
                    description = "MG 取得注單排程"
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

            DateTime paramTime = DateTime.Parse(parameter.max_value);
            if (paramTime < presetTime)// Specify as 10 days ago if it exceeds 10 days.
            {
                paramTime = presetTime;
                _logger.LogWarning($"MG parameter.max_value: {parameter.max_value}, Specify as 10 days ago if it exceeds 10 days. presetTime: {presetTime.ToString("O")}");
            }

            if (int.Parse(parameter.min_value) == 0)// 排程開關 0: 關閉，這邊需要暫停執行排程
            {
                _logger.LogInformation("MG record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }
            try
            {
                //一般拉單
                GetBetRecordHistoryRequest request = new();
                request.startingAfter = parameter.value;
                request.Limit = RECORD_LIMIT;
                var result = await _gameApiService._MgAPI.GetBetRecordHistory(request);
                //取得活動獎金
                var TournamentWinsDatetime = DateTime.Now;
                TournamentWinsDatetime = new DateTime(TournamentWinsDatetime.Year, TournamentWinsDatetime.Month, TournamentWinsDatetime.Day, TournamentWinsDatetime.Hour, TournamentWinsDatetime.Minute, 0, 0);
                TournamentWinsRequest TournamentWinsRequest = new TournamentWinsRequest()
                {
                    fromDate = TournamentWinsDatetime.AddMinutes(-1),
                    toDate = TournamentWinsDatetime,
                    utcOffset = 8,
                    tournaments = new int[] { }
                };
                var TournamentWinsRes = await _gameApiService._MgAPI.TournamentWins(TournamentWinsRequest);
                // 時間統一調整為 +8
                result.BetRecords.ForEach(x =>
                {
                    x.createdDateUTC = x.createdDateUTC.GetValueOrDefault().AddHours(8);
                    x.gameStartTimeUTC = x.gameStartTimeUTC.GetValueOrDefault().AddHours(8);
                    x.gameEndTimeUTC = x.gameEndTimeUTC.GetValueOrDefault().AddHours(8);
                });
                // MG 的 API 回傳不會排除 DEV 與 UAT 環境的資料，所以要依據目前環境排除其他環境的資料
                var betRecords = result.BetRecords
                                                  .Where(x => x.PlayerId.ToLower().StartsWith(_prefixKey.ToLower()))
                                                  .OrderBy(x => x.gameEndTimeUTC)
                                                  .ToList();
                TournamentWinsRes = TournamentWinsRes.Where(x => x.playerId.ToLower().StartsWith(_prefixKey.ToLower()))
                                                  .OrderBy(x => x.creditDate)
                                                  .ToList();
                if (TournamentWinsRes.Count > 0)
                {
                    foreach (var item in TournamentWinsRes)
                    {
                        var BetRecord = new BetRecord()
                        {
                            BetUID = "ER" + item.creditDate.ToString("yyyyMMdd") + item.tournamentId + item.tournamentPeriodId + item.playerId,
                            createdDateUTC = item.creditDate,
                            gameStartTimeUTC = item.creditDate,
                            gameEndTimeUTC = item.creditDate,
                            PlayerId = item.playerId,
                            ProductId = "EventRecord",
                            ProductPlayerId = "",
                            Platform = "",
                            GameCode = "EventRecord",//TODO 替換為統一活動代碼
                            Channel = "",
                            Currency = item.currency,
                            BetAmount = 0,
                            PayoutAmount = 0,
                            BetStatus = 0,
                            ExternalTransactionId = "",
                            jackpotwin = item.winAmount
                        };
                        betRecords.Add(BetRecord);
                    }
                }

                var reportTime = new DateTime(TournamentWinsDatetime.Year, TournamentWinsDatetime.Month, TournamentWinsDatetime.Day, TournamentWinsDatetime.Hour, (TournamentWinsDatetime.Minute / 5) * 5, 0);
                if (reportTime == TournamentWinsDatetime && _prefixKey.ToUpper() !="DEV")
                {
                    string[] rewardTypes = new string[] { "Cash", "FreeGames" };
                    FortuneRewardsRequest FortuneRewardsRequest = new FortuneRewardsRequest()
                    {
                        fromDate = TournamentWinsDatetime.AddDays(-2),
                        toDate = TournamentWinsDatetime,
                        utcOffset = 8,
                        rewardTypes = rewardTypes
                    };
                    var FortuneRewards = await _gameApiService._MgAPI.FortuneRewards(FortuneRewardsRequest);
                    if (FortuneRewards.Count > 0)
                    {
                        foreach (var item in FortuneRewards)
                        {
                            var BetRecord = new BetRecord()
                            {
                                BetUID = "FR" + item.transactionId,
                                createdDateUTC = item.creditDate,
                                gameStartTimeUTC = item.creditDate,
                                gameEndTimeUTC = item.creditDate,
                                PlayerId = item.playerId,
                                ProductId = "FortuneRecord",
                                ProductPlayerId = "",
                                Platform = "",
                                GameCode = item.rewardType.ToString() == "Cash" ? "9999" : "9998",
                                Channel = "",
                                Currency = "THB",
                                BetAmount = 0,
                                PayoutAmount = 0,
                                BetStatus = 0,
                                ExternalTransactionId = "",
                                jackpotwin = item.rewardAmount
                            };
                            betRecords.Add(BetRecord);
                        }
                    }
                }

                if (betRecords.Count > 0)
                {
                    await _gameRecordService._mgInterfaceService.PostMgRecord(betRecords);
                    // 查詢時間寫回 DB，類似 log 紀錄的功能
                    var lastRecord = betRecords.OrderBy(x => x.gameEndTimeUTC).LastOrDefault();
                    parameter.value = lastRecord.BetUID;
                    parameter.max_value = lastRecord.gameEndTimeUTC.GetValueOrDefault().ToString("O");
                    await _systemParameterDbService.PutSystemParameter(parameter);
                }
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, TimeSpan.FromMinutes(1));
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run mg record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Code.Platform PlatformMG = Code.Platform.MG;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = PlatformMG,
                repairParameter = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{PlatformMG}", failoverReq);
        }

    }
}
