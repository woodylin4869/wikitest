using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.JOKER;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response.GetBetDetailResponse;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class JokerRecordSchedule : IInvocable
    {
        private readonly ILogger<JokerRecordSchedule> _logger;
        private readonly IJOKER_InterfaceService _jokerInterfaceService;
        private readonly IRepairBetRecordService _repairBetRecordService;
        private readonly IDBService _dbService;
        private readonly ISystemParameterDbService _systemParameterDbService;

        private const string SYSTEM_PARAMETERS_KEY = "JokerRecordSchedule_V2";
        // 延遲查詢時間
        private readonly TimeSpan GAP_TIME = TimeSpan.FromMinutes(-10);
        // 查詢注單時間區間
        private TimeSpan RANGE_OFFSET = TimeSpan.FromMinutes(1);


        public JokerRecordSchedule(ILogger<JokerRecordSchedule> logger, IJOKER_InterfaceService jokerInterfaceService, IDBService dbService, IRepairBetRecordService repairBetRecordService, ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _jokerInterfaceService = jokerInterfaceService;
            _dbService = dbService;
            _repairBetRecordService = repairBetRecordService;
            _systemParameterDbService = systemParameterDbService;
        }

        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });

            _logger.LogInformation("Invoke JokerRecordSchedule on time : {time}", DateTime.Now.ToLocalTime());

            // 取得當前時間，計算下一個匯總的時間
            var now = DateTime.Now.ToLocalTime();
            now = now.Add(GAP_TIME);
            var nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

            // 取得上次結束時間
            var parameter = await _systemParameterDbService.GetSystemParameter(SYSTEM_PARAMETERS_KEY);

            // 檢查有無資料，沒資料的話新增預設值
            if (parameter == null)
            {
                parameter = new t_system_parameter()
                {
                    key = SYSTEM_PARAMETERS_KEY,
                    value = nextTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    min_value = "1",    // 排程開關 0: 關閉, 1: 開啟
                    name = "Joker取得注單排程",
                    description = "紀錄排程時間基準點"
                };

                var postSystemParameter = await _systemParameterDbService.PostSystemParameter(parameter);
                if (!postSystemParameter)
                {
                    return; // 新增失敗就結束排程
                }
            }

            if (int.Parse(parameter.min_value) == 0)
            {
                _logger.LogInformation("Joker record stop time: {time}", parameter.value);
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
                var list = new List<t_joker_bet_record>();
                var nextId = string.Empty;

                while (true)
                {
                    var result = await _jokerInterfaceService.GetJokerRecords(new GetBetDetailRequest()
                    {
                        StartDate = lastEndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndDate = lastEndTime.Add(RANGE_OFFSET).ToString("yyyy-MM-dd HH:mm:ss"),
                        NextId = nextId
                    });

                    // NextId 不為空就用一樣的時間 + NextId 重新請求，NextId 為空代表已經沒資料了
                    if (string.IsNullOrEmpty(result.nextId))
                    {
                        break;
                    }

                    // 遊戲注單
                    if (result.data.Game?.Any() == true)
                    {
                        var resultMapping = MapBetRecord(result.data.Game, BetTypeEnum.Game);
                        list.AddRange(resultMapping);
                    }


                    // 彩金注單
                    if (result.data.Jackpot?.Any() == true)
                    {
                        var resultMapping = MapBetRecord(result.data.Jackpot, BetTypeEnum.Jackpot);
                        list.AddRange(resultMapping);
                    }


                    // 競賽注單
                    if (result.data.Competition?.Any() == true)
                    {
                        var resultMapping = MapBetRecord(result.data.Competition, BetTypeEnum.Competition);
                        list.AddRange(resultMapping);
                    }

                    nextId = result.nextId;
                }

                await _systemParameterDbService.PutSystemParameter(parameter);

                // 寫五分鐘匯總帳 & 注單明細
                if (list.Any())
                {
                    await _jokerInterfaceService.PostJokerRecordDetail(list);
                }

                // 每5分鐘執行一次強制補單
                if (lastEndTime.Minute % 5 == 0)
                {
                    _ = Task.Run(() => RepairRecord(lastEndTime.AddMinutes(-30), lastEndTime));
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run joker record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        private async Task RepairRecord(DateTime start, DateTime end)
        {
            try
            {
                await _repairBetRecordService.RepairGameRecord(new()
                {
                    StartTime = start,
                    EndTime = end,
                    game_id = Platform.JOKER.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} {level} {message}", "JokerRepairRecord", LogLevel.Error, ex.Message);
            }
        }

        /// <summary>
        /// 依照注單類型整理Joker下注紀錄資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="betType"></param>
        /// <returns></returns>
        private List<t_joker_bet_record> MapBetRecord<T>(IEnumerable<T> records, BetTypeEnum betType) where T : BetRecordTypeBase
        {
            return records.Select(x => new t_joker_bet_record
            {
                Ocode = x.OCode,
                Username = x.Username,
                Gamecode = x.GameCode,
                Description = x.Description,
                Type = x.Type,
                Amount = x.Amount,
                Result = x.Result,
                Time = x.Time,
                Roundid = x.RoundID,
                Transactionocode = x.TransactionOCode,
                BetType = betType
            }).ToList();
        }
    }
}
