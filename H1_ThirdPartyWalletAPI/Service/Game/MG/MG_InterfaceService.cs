using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using BetRecord = H1_ThirdPartyWalletAPI.Model.Game.MG.Response.BetRecord;
using Platform = H1_ThirdPartyWalletAPI.Code.Platform;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IMgInterfaceService : IGameInterfaceService
    {
        /// <summary>
        /// 寫注單明細、5分鐘會總帳
        /// </summary>
        /// <param name="mgBetRecord"></param>
        /// <returns></returns>
        public Task<(ResCodeBase, int)> PostMgRecord(List<Model.Game.MG.Response.BetRecord> mgBetRecord);
        /// <summary>
        /// 新增 W1小時匯總帳
        /// 【轉帳中心】(report_type = 1)統計交易資料，加總再寫到 t_game_report
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public Task SummaryW1Report(DateTime date);
        /// <summary>
        /// 產生Game MG 每小時報表
        /// 【遊戲商】(report_type = 0)統計交易資料，加總再寫到 t_game_report
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public Task SummaryGameProviderReport(DateTime date);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }
    public class MG_RecordService : IMgInterfaceService
    {
        private readonly ILogger<MG_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly ISummaryDBService _summaryDBService;
        const int _cacheSeconds = 600;
        private readonly ICacheDataService _cacheService;
        private readonly IDBService _dbService;
        private readonly IMgDbService _mgDbService;
        private readonly IGameReportDBService _gameReportDBService;
        private const int _cacheFranchiserUser = 1800;
        private const int RECORD_LIMIT = 20000;//between 1 and 20000
        private readonly string _prefixKey;

        public MG_RecordService(ILogger<MG_RecordService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IMgDbService mgDbService,
            ISummaryDBService summartDBService,
            IGameReportDBService gameReportDBService
            )
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameaApiService;
            _dbService = commonService._serviceDB;
            _mgDbService = mgDbService;
            _summaryDBService = summartDBService;
            _cacheService = commonService._cacheDataService;
            _gameReportDBService = gameReportDBService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
        }
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            Model.Game.MG.Request.GetBalanceRequest req = new Model.Game.MG.Request.GetBalanceRequest();
            req.PlayerId = platform_user.game_user_id;
            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _gameApiService._MgAPI.GetBalance(req);
                Balance.Amount = res.Balance.total;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Mg餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.MG);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            //MG沒有踢線功能
            return true;
        }

        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Deposit (存款)
        /// </summary>
        /// <param name="platform_user"></param>
        /// <param name="walletData"></param>
        /// <param name="RecordData"></param>
        /// <returns></returns>
        /// <response code="200">玩家已经存在。存在的玩家已经获取</response>
        /// <response code="201">成功创建玩家</response>
        /// <response code="400">请求无效 - 输入验证失败</response>
        /// <response code="401">未经授权</response>
        /// <response code="500">内部服务器错误</response>
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            try
            {
                var transfer_amount = RecordData.amount;
                PostTransactionRequest requestData = new()
                {
                    PlayerId = platform_user.game_user_id,
                    Type = TransactionType.Deposit,
                    Amount = transfer_amount,
                    ExternalTransactionId = RecordData.id.ToString(),
                    IdempotencyKey = RecordData.id.ToString()
                };
                var responseData = await _gameApiService._MgAPI.PostTransaction(requestData);
                if (responseData.status == TransactionStatus.Succeeded)
                {
                    RecordData.status = nameof(TransferStatus.success);
                }
                else
                {
                    RecordData.status = nameof(TransferStatus.pending);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("MG TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("FundTransferInMgFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var game_balance = RecordData.amount;
            try
            {
                PostTransactionRequest requestData = new()
                {
                    PlayerId = platform_user.game_user_id,
                    Type = TransactionType.Withdraw,
                    Amount = game_balance,
                    ExternalTransactionId = RecordData.id.ToString(),
                    IdempotencyKey = RecordData.id.ToString()
                };
                var responseData = await _gameApiService._MgAPI.PostTransaction(requestData);
                if (responseData.status == TransactionStatus.Succeeded)
                {
                    RecordData.status = nameof(TransferStatus.success);
                }
                else
                {
                    RecordData.status = nameof(TransferStatus.pending);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("MG TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("MG TransferOut fail ex : {ex}", ex.Message);
                RecordData.status = nameof(TransferStatus.pending);
            }
            return RecordData.status;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.MG.MG.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //Step 1 Create Member
            Model.Game.MG.Request.CreatePlayerRequest requestData = new();
            //依照環境變數調整Prefix
            requestData.PlayerId = _prefixKey + userData.Club_id;
            try
            {
                var result = await _gameApiService._MgAPI.CreatePlayer(requestData);
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.CreateMgUserFail, MessageCode.Message[(int)ResponseCode.CreateMgUserFail] + "|" + ex.Message.ToString());
            }
            var gameUser = new GamePlatformUser
            {
                club_id = userData.Club_id,
                game_user_id = requestData.PlayerId,
                game_platform = request.Platform
            };
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 3 Get Game URL
            Model.Game.MG.Request.GetGameUrlRequest UrlRequest = new Model.Game.MG.Request.GetGameUrlRequest();
            UrlRequest.PlayerId = platformUser.game_user_id;
            if (request.GameConfig.ContainsKey("device"))
            {
                if (request.GameConfig["device"].ToLower() == "desktop")
                {
                    UrlRequest.platform = Model.Game.MG.Enum.Platform.Desktop;
                }
                else if (request.GameConfig["device"].ToLower() == "mobile")
                {
                    UrlRequest.platform = Model.Game.MG.Enum.Platform.Mobile;
                }
                else if (request.GameConfig["device"].ToLower() == "web")// MG 沒有 web 這個選項
                {
                    UrlRequest.platform = Model.Game.MG.Enum.Platform.Desktop;
                }
                else
                {
                    UrlRequest.platform = Model.Game.MG.Enum.Platform.Desktop;// MG 只有 Desktop 與 Mobile 才能正常進線
                }
            }
            else
            {
                UrlRequest.platform = Model.Game.MG.Enum.Platform.UnKnown;
            }
            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.homeUrl = request.GameConfig["lobbyURL"];
            }
            if (request.GameConfig.ContainsKey("gameCode"))
            {
                UrlRequest.contentCode = request.GameConfig["gameCode"];
            }
            if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && Model.Game.MG.MG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.langCode = Model.Game.MG.MG.lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.langCode = Model.Game.MG.MG.lang["en-US"];
            }
            try
            {
                var token_res = await _gameApiService._MgAPI.GetGameUrl(UrlRequest);
                return token_res.Url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }
        /// <summary>
        /// 查詢交易結果
        /// </summary>
        /// <param name="transfer_record"></param>
        /// <returns></returns>
        /// <response code="200">交易信息已经获取</response>
        /// <response code="400">交易不存在</response>
        /// <response code="401">未经授权</response>
        /// <response code="500">内部服务器错误</response>
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var checkTransferRecordResponse = new CheckTransferRecordResponse();

            var response = new GetTransactionResponse();
            try
            {
                var req = new GetTransactionRequest()
                {
                    idempotencyKey = transfer_record.id.ToString()
                };

                response = await _gameApiService._MgAPI.GetTransaction(req);
            }
            catch (ExceptionMessage ex)
            {
                if (ex.MsgId == (int)HttpStatusCode.BadRequest)
                {
                    // 回傳 400 【交易不存在】確定為失敗
                    response.status = TransactionStatus.Failed;
                }
            }

            if (response.status == TransactionStatus.Succeeded)
            {
                if (transfer_record.target == nameof(Platform.MG))//轉入 MG 直接改訂單狀態為成功
                {
                    checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else
                {
                    checkTransferRecordResponse.CreditChange = transfer_record.amount;
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = nameof(TransferStatus.success);
                transfer_record.success_datetime = DateTime.Now;
            }
            else if (response.status == TransactionStatus.Failed)
            {
                if (transfer_record.target == nameof(Platform.MG))//轉入 MG 直接改訂單狀態為失敗
                {
                    checkTransferRecordResponse.CreditChange = transfer_record.amount;
                    checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.MG))
                {
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = nameof(TransferStatus.fail);
                transfer_record.success_datetime = DateTime.Now;
                transfer_record.after_balance = transfer_record.before_balance;
            }
            // 只處理成功與失敗，其他都保持原來的狀態，例如: pending
            checkTransferRecordResponse.TRecord = transfer_record;
            return checkTransferRecordResponse;
        }
        /// <summary>
        /// 第二層明細
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var batRecords = new List<dynamic>();
            GetBetRecord res = new();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _mgDbService.SumMgBetRecordByreport_time(createTime, RecordReq.ReportTime, Config.OneWalletAPI.Prefix_Key + RecordReq.ClubId);
                foreach (var result in results)
                {
                    batRecords.Add(result);
                }
            }

            if (batRecords.Count == 0)
            {
                batRecords.AddRange(await _mgDbService.GetMgRecordBySummary(RecordReq));
            }

            res.Data = batRecords.OrderByDescending(e => e.gameendtimeutc).ToList();
            return res;
        }
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var record = await _mgDbService.GetMgRecordByBetUid(RecordDetailReq.record_id, RecordDetailReq.ReportTime);

            if (record.Count == 0)
            {
                record = await _mgDbService.GetMgRecordByBetUidV1(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
                if (record.Count == 0)
                {
                    string error = $"{RecordDetailReq.record_id} 找不到此注單";
                    _logger.LogWarning(error);
                    return error;
                }
            }
            GameDetailUrlResponse result = await _gameApiService._MgAPI.GameDetailURL(new GameDetailUrlRequest()
            {
                playerId = record[0].playerid,
                betUid = RecordDetailReq.record_id,
                langCode = RecordDetailReq.lang,
                utcOffset = 8
            });

            return result.url;
        }

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            DateTime StartTime = new(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
            DateTime EndTime = new(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, RepairReq.EndTime.Second);

            DateTime compareTime = new DateTime(2024, 9, 30, 12, 0, 0);

            if (DateTime.Compare(StartTime, compareTime) < 0)
            {
                string errormsg = $"Cannot RepairBet before {compareTime.ToString("yyyy-MM-dd HH:mm")}";
                _logger.LogWarning(errormsg);
                return errormsg;
            }

            var postResult = 0;

            #region 拉尾號
            // 拉尾號
            var firstOne = await _mgDbService.GetLatestMgRecord(StartTime);

            if (firstOne == null)
            {
                string errormsg = $"Cannot find the previous data, it may be over 12 hours.";
                _logger.LogWarning(errormsg);
                return errormsg;
            }

            var first = firstOne.betuid;

            while (true)
            {
                var result = await _gameApiService._MgAPI.GetBetRecordHistory(new()
                {
                    startingAfter = first,
                    Limit = RECORD_LIMIT
                });

                // 時間統一調整為 +8
                result.BetRecords.ForEach(x =>
                {
                    x.createdDateUTC = x.createdDateUTC.GetValueOrDefault().AddHours(8);
                    x.gameStartTimeUTC = x.gameStartTimeUTC.GetValueOrDefault().AddHours(8);
                    x.gameEndTimeUTC = x.gameEndTimeUTC.GetValueOrDefault().AddHours(8);
                });
                // MG 的 API 回傳不會排除 DEV 與 UAT 環境的資料，所以要依據目前環境排除其他環境的資料
                var betRecords = result.BetRecords
                                                  .Where(x => x.PlayerId.ToLower().Substring(0, _prefixKey.Length).Equals(_prefixKey.ToLower()))
                                                  .OrderBy(x => x.gameEndTimeUTC)
                                                  .ToList();




                if (betRecords.Count == 0) break;
                if (betRecords.All(x => x.gameEndTimeUTC > EndTime)) break;

                var list = betRecords
                                     .Where(x => x.gameEndTimeUTC >= StartTime && x.gameEndTimeUTC <= EndTime)
                                     .ToList();
                //活動單寫入LIST


                if (list.Any())
                {
                    (var postMgRecord, int postMgResult) = await PostMgRecord(list);
                    postResult += postMgResult;
                }

                first = betRecords.MaxBy(x => x.gameEndTimeUTC)?.BetUID;
                await Task.Delay(200);
            }

            #endregion 拉尾號

            #region 活動補單

            //活動單
            DateTime TournamentWinsDatetime = new(StartTime.Year, StartTime.Month, StartTime.Day, 0, 0, 0, 0, 0);
            TournamentWinsRequest TournamentWinsRequest = new TournamentWinsRequest()
            {
                fromDate = TournamentWinsDatetime,
                toDate = TournamentWinsDatetime.AddDays(1),
                utcOffset = 8,
                tournaments = new int[] { }
            };
            var TournamentWinsRes = await _gameApiService._MgAPI.TournamentWins(TournamentWinsRequest);

            TournamentWinsRes = TournamentWinsRes.Where(x => x.playerId.ToLower().StartsWith(_prefixKey.ToLower()))
                    .OrderBy(x => x.creditDate)
                    .ToList();

            var Activitylist = new List<Model.Game.MG.Response.BetRecord>();

            foreach (var item in TournamentWinsRes)
            {
                var BetRecord = new Model.Game.MG.Response.BetRecord()
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
                Activitylist.Add(BetRecord);
            }

            string[] rewardTypes = new string[] { "Cash", "FreeGames" };
            FortuneRewardsRequest FortuneRewardsRequest = new FortuneRewardsRequest()
            {
                fromDate = TournamentWinsDatetime,
                toDate = TournamentWinsDatetime.AddDays(1),
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
                    Activitylist.Add(BetRecord);
                }
            }


            if (Activitylist.Any())
            {
                (var postMgRecord, int postMgResult) = await PostMgRecord(Activitylist);
                postResult += postMgResult;
            }

            #endregion
            #region 重產匯總帳
            DateTime start = new(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0);
            var offSet = TimeSpan.FromHours(1);
            while (start < EndTime)
            {
                var end = start.Add(offSet);

                await SummaryW1Report(start);
                await SummaryGameProviderReport(start);

                start = end;
                await Task.Delay(100);
            }
            #endregion 重產匯總帳            

            return $"Game: {Platform.MG} 新增資料筆數: {postResult}";
        }

        /// <summary>
        /// 寫注單明細、5分鐘會總帳
        /// 從資料表 t_mg_bet_record 取舊注單(往前12小時)，比對重複單
        /// 注單寫到 t_mg_bet_record
        /// 5分鐘匯總寫到 t_bet_record_summary
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public async Task<(ResCodeBase, int)> PostMgRecord(List<BetRecord> source)
        {
            int postResult = 0;

            ResCodeBase res = new();
            using (NpgsqlConnection conn = new(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                foreach (IEnumerable<BetRecord> group in source.Chunk(20000))
                {
                    await using var tran = await conn.BeginTransactionAsync();
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var betDetailData = new List<BetRecord>();
                    var dt = DateTime.Now;
                    var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                    // 紀錄 reportTime 跟 playTime 的關聯
                    var dic = new Dictionary<string, HashSet<string>>();

                    foreach (var item in group)//loop club id bet detail
                    {
                        item.report_time = reportTime;
                        var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                        if (!dic.ContainsKey(summaryTime))
                        {
                            dic.Add(summaryTime, new HashSet<string>());
                        }
                        //因Datetime 可為NULL,判斷是否有值 
                        if (item.gameEndTimeUTC.HasValue)
                        {
                            dic[summaryTime].Add(item.gameEndTimeUTC.Value.ToString("yyyy-MM-dd HH:mm"));
                        }

                        betDetailData.Add(item);
                    }

                    postResult += await _mgDbService.PostMgRecord(conn, tran, betDetailData);
                    await tran.CommitAsync();

                    // 記錄到 Redis reportTime 跟 playTime 的關聯
                    foreach (var item in dic)
                    {
                        var key = $"{RedisCacheKeys.MgBetSummaryTime}:{item.Key}";
                        await _commonService._cacheDataService.SortedSetAddAsync(key,
                            item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                    }

                    dic.Clear();

                    sw.Stop();
                    _logger.LogDebug("MgSlotRecordSchedule 寫入{count}筆資料時間 : {time} MS", postResult, sw.ElapsedMilliseconds);
                }
                await conn.CloseAsync();
            }
            return (res, postResult);
        }
        /// <summary>
        /// 新增 W1小時匯總帳
        /// 【轉帳中心】(report_type = 1)統計交易資料，加總再寫到 t_game_report
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public async Task SummaryW1Report(DateTime startTime)
        {
            // 每小時投注資料
            // 撈 t_mg_bet_record 注單資料
            var ReportData = await _mgDbService.SumMgBetRecordByBetTime(startTime, startTime.AddHours(1));
            GameReport reportData = new()
            {
                platform = nameof(Platform.MG),
                report_datetime = startTime,
                report_type = (int)GameReport.e_report_type.GameBetRecord,// 標註為 1 【轉帳中心】來源資料，作為比對參考用
                total_win = ReportData.total_win == null ? 0 : ReportData.total_win,
                total_bet = ReportData.total_bet == null ? 0 : ReportData.total_bet,
                total_count = ReportData.total_cont == null ? 0 : ReportData.total_cont
            };
            reportData.total_netwin = reportData.total_win - reportData.total_bet;

            // 撈 t_mg_bet_record 加總再寫到 t_game_report
            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);
        }
        /// <summary>
        /// 產生Game MG 每小時報表
        /// 【遊戲商】(report_type = 0)統計交易資料，加總再寫到 t_game_report
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task SummaryGameProviderReport(DateTime date)
        {
            DateTime fromDate = date;
            DateTime toDate = date.AddHours(1);

            GetFinacialRequest req = new()
            {
                fromDate = fromDate.ToString("O"),
                toDate = toDate.ToString("O"),
                timeAggregation = TimeAggregation.Hourly,
                utcOffset = 8,
                Currency = Model.Game.MG.MG.Currency["THB"]
            };

            // 呼叫 MG API 每小時報表
            GetFinacialResponse ReportData = await _gameApiService._MgAPI.GetFinancial(req);

            GameReport TotalReportData = new()
            {
                platform = nameof(Platform.MG),
                report_datetime = date,
                report_type = (int)GameReport.e_report_type.FinancalReport,// 標註為 0 【遊戲商】來源資料，作為比對參考用
            };

            foreach (var data in ReportData.data)
            {
                GameReport reportData = new()
                {
                    platform = nameof(Platform.MG),
                    report_datetime = date,
                    report_type = (int)GameReport.e_report_type.FinancalReport,// 標註為 0 【遊戲商】來源資料，作為比對參考用
                    total_bet = data.Income,
                    total_win = data.Payout,
                    total_count = data.NumOfBets
                };
                reportData.total_netwin = reportData.total_win - reportData.total_bet;

                TotalReportData.total_bet += reportData.total_bet;
                TotalReportData.total_win += reportData.total_win;
                TotalReportData.total_count += reportData.total_count;
                TotalReportData.total_netwin += reportData.total_netwin;
            }

            await _gameReportDBService.DeleteGameReport(TotalReportData);
            await _gameReportDBService.PostGameReport(TotalReportData);
        }

        /// <summary>
        ///五分鐘會總
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="reportDatetime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            // TODO: RecordSummary需要移除DateTime startTime, DateTime endTime，Schedule 跟著把DateTime startTime, DateTime endTime 邏輯處理刪除
            // 取得匯總需要的起始和結束時間
            (DateTime StartTime, DateTime EndTime) = await GetRecordSummaryDateTime(reportDatetime);
            startTime = StartTime;
            endTime = EndTime;

            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _mgDbService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid.Substring(3)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();


            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key.Substring(3), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.MG);
                summaryData.Game_type = 3;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.win) - summaryRecord.Sum(x => x.bet);
                summaryRecordList.Add(summaryData);

                foreach (var item in summaryRecord)
                {
                    var mapping = new t_summary_bet_record_mapping()
                    {
                        summary_id = summaryData.id,
                        report_time = reportDatetime,
                        partition_time = item.createtime
                    };
                    summaryBetRecordMappings.Add(mapping);
                }
            }

            var Chucklist = summaryRecordList.Chunk(10000);
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    foreach (IEnumerable<BetRecordSummary> group in Chucklist)
                    {

                        var sw = System.Diagnostics.Stopwatch.StartNew();


                        await _summaryDBService.BatchInsertRecordSummaryAsync(conn, group.ToList());



                        sw.Stop();
                        _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", group.Count(), sw.ElapsedMilliseconds);
                    }

                    await _summaryDBService.BulkInsertSummaryBetRecordMapping(tran, summaryBetRecordMappings);
                    await tran.CommitAsync();
                    await conn.CloseAsync();
                }
            }
            return true;
        }


        /// <summary>
        /// 取得匯總需要的起始和結束時間
        /// </summary>
        /// <param name="reportTime">排程執行匯總時間</param>
        /// <returns>匯總需要的起始和結束時間</returns>
        private async Task<(DateTime StartTime, DateTime EndTime)> GetRecordSummaryDateTime(DateTime reportTime)
        {
            DateTime? startTime = null;
            DateTime? endTime = null;

            var redisKey = $"{RedisCacheKeys.MgBetSummaryTime}:{reportTime.ToString("yyyy-MM-dd HH:mm")}";

            var (timeStart, _) = await _commonService._cacheDataService.SortedSetPopMinAsync(redisKey);
            var (timeEnd, _) = await _commonService._cacheDataService.SortedSetPopMaxAsync(redisKey);

            if (timeStart != default && timeEnd != default)
            {
                // 找出最大最小值
                startTime = DateTime.Parse(timeStart).AddMinutes(-15);
                endTime = DateTime.Parse(timeEnd).AddMinutes(15);
            }

            // 預設值
            if (startTime == null || endTime == null)
            {
                startTime = reportTime.AddDays(-2);
                endTime = reportTime.AddHours(1);
            }

            // 檢查時間範圍不可超過 50 小時，超過就截斷
            var timeSpan = new TimeSpan(endTime.Value.Ticks - startTime.Value.Ticks);
            if (timeSpan.TotalHours > 240)
            {
                // 從最近的時間往前推 240 小時
                startTime = endTime.Value.AddHours(-240);
            }

            var result = await Task.FromResult((startTime.Value, endTime.Value));
            return result;
        }
        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._MgAPI.HeartBeat();
        }
        private BetRecordSummary Calculate(BetRecordSummary SummaryData, Model.Game.MG.Response.BetRecord r)
        {
            if (r.GameCode == "EventRecord")
            {
                SummaryData.RecordCount++;
                SummaryData.Bet_amount += 0;
                SummaryData.Turnover += 0;
                SummaryData.Netwin += 0;
                SummaryData.Win += 0;
                SummaryData.updatedatetime = DateTime.Now;
                SummaryData.JackpotWin += r.jackpotwin;
                return SummaryData;
            }

            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.BetAmount;
            SummaryData.Turnover += r.BetAmount;
            SummaryData.Netwin += r.PayoutAmount - r.BetAmount;
            SummaryData.Win += r.PayoutAmount;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }
        private async Task<Wallet> GetWalletCache(string Club_id)
        {
            Wallet walletData = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}",
            async () =>
            {
                try
                {
                    IEnumerable<Wallet> result = await _commonService._serviceDB.GetWallet(Club_id);
                    if (result.Count() != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                    }
                    return result.SingleOrDefault();
                }
                catch
                {
                    return null;
                }
            },
            _cacheSeconds);
            return walletData;
        }

        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(RecordReq.Club_id, Platform.MG);
            if (gameUser == null)
            {
                throw new Exception("No mg user");
            }
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            var results = await _gameApiService._MgAPI.GetIncompleteBets(new GetIncompleteBetsRequest() { PlayerId = gameUser.game_user_id });
            results.IncompleteBet = results.IncompleteBet.OrderBy(p => p.Product).ToList();
            res.Data = results.IncompleteBet.Select(b => (dynamic)b).ToList();
            return res;
        }
    }

}
