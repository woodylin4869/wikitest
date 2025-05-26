using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Request;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
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
    public class FcRecordSchedule : IInvocable
    {
        private readonly ILogger<FcRecordSchedule> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly GameRecordService _gameRecordService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly ICacheDataService _cacheDataService;

        private const string SYSTEM_PARAMETERS_KEY = "FcRecordSchedule";
        private readonly int defaultPastTime = 5;
        private readonly string _prefixKey;
        private TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);

        public FcRecordSchedule(ILogger<FcRecordSchedule> logger, GameRecordService gameRecordService, IGameApiService gameApiService, IDBService dbService, ISystemParameterDbService systemParameterDbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _gameRecordService = gameRecordService;
            _systemParameterDbService = systemParameterDbService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
            _cacheDataService = cacheDataService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });
            _logger.LogInformation("Invoke FcRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());


            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.AddMinutes(-defaultPastTime);
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
                    name = "FC取得注單排程",
                    description = "FC記錄end_time"
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
                _logger.LogInformation("fc record stop time: {time}", parameter.value);
                await Task.CompletedTask;
                return;
            }

            var lastEndTime = DateTime.Parse(parameter.value);
            parameter.value = lastEndTime.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");


            try
            {
                //nextTime 是現在時間-5分鐘 拉住單最多15分鐘 所以要再多減10
                if ((now - lastEndTime).Minutes >= 15)
                {
                    lastEndTime = nextTime.AddMinutes(-10);
                    parameter.value = lastEndTime.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss");
                }

                var req = new GetRecordListRequest
                {
                    StartDate = lastEndTime.AddHours(-12),
                    EndDate = lastEndTime.AddHours(-12).AddMinutes(1).AddMilliseconds(-1)
                };

                if (lastEndTime >= nextTime)
                {
                    return;
                }
                List<Record> Records = new List<Record>();
                var resPK = Records.Select(x => new { x.recordID, x.bdate }).ToHashSet();

                //取得注單
                GetRecordListResponse betLogs = await _gameApiService._FcAPI.GetRecordList(req);
                if (betLogs.Result == (int)ErrorCodeEnum.Success)
                {
                    if (betLogs.Records.Count > 0)
                    {
                        Records.AddRange(betLogs.Records);
                    }
                }

                var reqEve = new GetBillListRequest
                {
                    StartDateTime = lastEndTime.AddHours(-12),
                    EndDateTime = lastEndTime.AddHours(-12).AddMinutes(1).AddMilliseconds(-1)
                };
                //取得活動派彩
                GetBillListResponse Event = await _gameApiService._FcAPI.GetBillList(reqEve);
                if (Event.Result == (int)ErrorCodeEnum.Success)
                {
                    if (Event.Bank.Count > 0)
                    {
                        foreach (var item in Event.Bank.Where(x => !string.IsNullOrEmpty(x.eventID)))
                        {
                            var BetRecord = new Record()
                            {
                                bet = 0,
                                prize = 0,
                                winlose = 0,
                                before = item.before,
                                after = item.after,
                                jptax = 0,
                                jppoints = item.points,
                                recordID = item.trsID,
                                account = item.account,
                                gameID = 99999, //TODO 替換為統一活動代碼
                                gametype = 0,
                                jpmode = 1,
                                bdate = item.createDateTime,
                                isBuyFeature = false
                            };
                            Records.Add(BetRecord);
                        }
                    }
                }
                List<Record> res = new List<Record>();
                foreach (var dateEntry in Records)
                {
                    if (resPK.Add(new { dateEntry.recordID, dateEntry.bdate }))
                    {
                        res.Add(dateEntry);
                    }
                }
                if (res.Count > 0)
                {
                    await _gameRecordService._FCInterfaceService.PostFcRecord(res);
                }

                await _systemParameterDbService.PutSystemParameter(parameter);
            }
            catch (Exception ex)
            {
                TriggerFailOver(parameter, RANGE_OFFSET);
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run fc record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }

        }

        private async void TriggerFailOver(t_system_parameter parameter, TimeSpan offTimeSpan)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            Platform Platform = Platform.FC;

            var failoverReq = new PullRecordFailoverWithTimeOffset()
            {
                platform = Platform,
                repairParameter = parameter.value, // 已經是新的時間
                delay = TimeSpan.FromMinutes(5),
                OffTimeSpan = -offTimeSpan         // 轉換成負值，是要找回起始時間
            };

            await _cacheDataService.ListPushAsync($"{RedisCacheKeys.PullRecordFailOver}:{Platform}", failoverReq);
        }
    }
}
