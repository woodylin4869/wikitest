using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PP.Responses;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses.GetBetRecordByTimeResponse;
using static H1_ThirdPartyWalletAPI.Model.Game.RLG.Response.GetBetRecordResponse;

namespace H1_ThirdPartyWalletAPI.Service.Game.PP
{
    public interface IPPInterfaceService : IGameInterfaceService
    {
        Task<int> PostPPRecord(List<GetRecordResponses> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class PP_InterfaceService : IPPInterfaceService
    {

        private readonly ILogger<PP_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly ICacheDataService _cacheService;
        private readonly IPPDBService _ppDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IBetLogsDbConnectionStringManager _betLogsDbConnectionStringManager;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        private readonly TimeSpan BATCH_OFFSET = TimeSpan.FromHours(3);

        public PP_InterfaceService(ILogger<PP_InterfaceService> logger,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IGameApiService gameaApiService,
            IDBService dbService,
            ICacheDataService cacheService,
            IPPDBService ppDBService,
            IGameReportDBService gameReportDBService,
             IBetLogsDbConnectionStringManager betLogsDbConnectionStringManager
        )
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _dbService = dbService;
            _cacheService = cacheService;
            _ppDBService = ppDBService;
            _gameReportDBService = gameReportDBService;
            _betLogsDbConnectionStringManager = betLogsDbConnectionStringManager;
        }

        #region GameInterfaceService
        /// <summary>
        /// 取得餘額
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platform_user"></param>
        /// <returns></returns>
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                var responseData = await _gameApiService._PPAPI.GetBalanceAsync(new GetBalanceRequest()
                {
                    secureLogin = Config.CompanyToken.PP_SecureLogin,
                    externalPlayerId = platform_user.game_user_id
                });

                if (responseData.error != "0")
                {
                    throw new Exception(responseData.description);
                }
                Balance.Amount = responseData.balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("PP 餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.PP);
            return Balance;
        }
        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platform_user"></param>
        /// <returns></returns>
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var responseData = await _gameApiService._PPAPI.TerminateSessionAsync(new TerminateSessionRequest()
                {
                    secureLogin = Config.CompanyToken.PP_SecureLogin,
                    externalPlayerId = platform_user.game_user_id
                });

                if (responseData.error != "0")
                {
                    throw new Exception(responseData.description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出PP使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 存款
        /// </summary>
        /// <param name="platform_user"></param>
        /// <param name="walletData"></param>
        /// <param name="RecordData"></param>
        /// <returns></returns>
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            try
            {
                var responseData = await _gameApiService._PPAPI.TransferAsync(new TransferRequest
                {
                    secureLogin = Config.CompanyToken.PP_SecureLogin,
                    externalPlayerId = platform_user.game_user_id,
                    amount = RecordData.amount,
                    externalTransactionId = RecordData.id.ToString()
                });

                if (responseData.error != "0")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("PP Deposit: {Message}", responseData.description);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PP TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PP Deposit: {Message}", ex.Message);
            }
            return RecordData.status;
        }

        /// <summary>
        /// 取款
        /// </summary>
        /// <param name="platform_user"></param>
        /// <param name="walletData"></param>
        /// <param name="RecordData"></param>
        /// <returns></returns>
        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {

            try
            {
                var responseData = await _gameApiService._PPAPI.TransferAsync(new TransferRequest
                {
                    secureLogin = Config.CompanyToken.PP_SecureLogin,
                    externalPlayerId = platform_user.game_user_id,
                    amount = -1 * (RecordData.amount),
                    externalTransactionId = RecordData.id.ToString()
                });

                if (responseData.error != "0")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("PP Withdraw : {ex}", responseData.description);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PP TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PP Withdraw : {ex}", ex.Message);
            }
            return RecordData.status;
        }

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.PP.PP.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreatePlayerRequest()
                {
                    secureLogin = Config.CompanyToken.PP_SecureLogin,
                    externalPlayerId = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    currency = userData.Currency
                };
                var response = await _gameApiService._PPAPI.CreateMemberAsync(req);
                if (response.error == "0" && response.description == "OK")
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.externalPlayerId;
                    gameUser.game_platform = Platform.PP.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("PP建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "PP " + ex.Message.ToString());
            }
        }
        /// <summary>
        /// 進入遊戲
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userData"></param>
        /// <param name="platformUser"></param>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            Model.Game.PP.PP.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            //Step 3 Get Game URL
            StartGameRequest UrlRequest = new StartGameRequest();
            UrlRequest.secureLogin = Config.CompanyToken.PP_SecureLogin;
            UrlRequest.externalPlayerId = platformUser.game_user_id;
            UrlRequest.gameId = request.GameConfig["gameCode"];
            UrlRequest.language = lang ?? Model.Game.PP.PP.lang["en-US"];
            UrlRequest.platform = request.GameConfig["device"] == "DESKTOP" ? "WEB" : "MOBILE";//網頁板web:手機板app

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.lobbyURL = request.GameConfig["lobbyURL"];
            }

            try
            {
                var token_res = await _gameApiService._PPAPI.StartGameAsync(UrlRequest);
                return token_res.gameURL;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }
        /// <summary>
        /// 確認交易紀錄
        /// </summary>
        /// <param name="transfer_record"></param>
        /// <returns></returns>
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var PPReuslt = await _gameApiService._PPAPI.GetTransferStatusAsync(new GetTransferStatusRequest
            {
                secureLogin = Config.CompanyToken.PP_SecureLogin,
                externalPlayerId = Config.OneWalletAPI.Prefix_Key + transfer_record.Club_id,
                externalTransactionId = transfer_record.id.ToString()
            });
            if (PPReuslt.status == "Success" && PPReuslt.error == "0")
            {
                if (transfer_record.target == nameof(Platform.PP))//轉入PP直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.PP))
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = transfer_record.status = nameof(WalletTransferRecord.TransferStatus.success);
                transfer_record.success_datetime = DateTime.Now;
            }
            else if (PPReuslt.error == "0" && PPReuslt.status == "Not found")
            {
                if (transfer_record.target == nameof(Platform.PP))//轉入PP直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.PP))
                {
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = nameof(WalletTransferRecord.TransferStatus.fail);
                transfer_record.success_datetime = DateTime.Now;
                transfer_record.after_balance = transfer_record.before_balance;
            }
            CheckTransferRecordResponse.TRecord = transfer_record;
            return CheckTransferRecordResponse;
        }
        /// <summary>
        /// 取得遊戲第二層注單明細
        /// </summary>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var batRecords = new List<dynamic>();
            GetBetRecord res = new();
            var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            //新舊並存階段
            if (!partitions.Any()) //沒有對應為舊資料
            {
                var data = await _ppDBService.GetppRecordsBySummary(RecordReq);
                foreach (var item in data)
                {
                    batRecords.Add(item);
                }

            }
            else //有對應為新資料
            {
                var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
                if (summary == null)
                    return res;

                foreach (var createTime in partitions)
                {
                    var data = await _ppDBService.GetppRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                    foreach (var item in data)
                    {
                        batRecords.Add(item);
                    }
                }
            }
            res.Data = batRecords.OrderByDescending(x=>x.StartDate).ToList();
            return res;
        }
        /// <summary>
        /// 取得遊戲住單明細-轉跳
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var ppList = await _ppDBService.GetppRecord(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            if (ppList.Count == 0)
            {
                ppList = await _ppDBService.Getppv2Record(RecordDetailReq.record_id, RecordDetailReq.ReportTime.AddHours(-1));
            }
            var ppRecord = ppList.LastOrDefault();
            var PPResponseData = await _gameApiService._PPAPI.OpenHistoryAsync(new OpenHistoryRequest()
            {
                secureLogin = Config.CompanyToken.PP_SecureLogin,
                roundId = ppRecord.PlaySessionID,
                playerId = ppRecord.ExtPlayerID
            });

            if (PPResponseData.error != "0")
            {
                throw new Exception("no data");
            }
            return PPResponseData.url;
        }

        /// <summary>
        /// 補單-會總
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 10)
            {
                endTime = startTime.AddMinutes(10);
                RepairCount += await RepairPP(startTime, endTime);
                startTime = endTime;
            }
            RepairCount += await RepairPP(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._PPAPI.HealthCheckAsync();
        }
        #endregion


        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostPPRecord(List<GetRecordResponses> recordData)
        {
            var oldLogs = (await _ppDBService.GetppV2RecordsBytime(recordData.Min(l => l.StartDate.AddHours(8)), recordData.Max(l => l.StartDate.AddHours(8))))
            .Select(l => new { l.PlaySessionID, l.Status, l.StartDate })
            .ToHashSet();

            List<GetRecordResponses> newRecords = new List<GetRecordResponses>();

            foreach (var record in recordData)
            {
                if (oldLogs.Add(new { record.PlaySessionID, record.Status, StartDate=record.StartDate.AddHours(8)}))
                {
                    newRecords.Add(record);
                }
            }

            var postResult = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, GetRecordResponses>> linqRes = newRecords.GroupBy(x => x.ExtPlayerID);
            List<GetRecordResponses> betDetailData = new List<GetRecordResponses>();

            List<GetRecordResponses> betDetailDataRunning = new List<GetRecordResponses>();
            var dic = new Dictionary<string, HashSet<string>>();
            sw.Start();
            foreach (IGrouping<string, GetRecordResponses> group in linqRes)
            {

                try
                {
                    _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                    string club_id;
                    club_id = group.Key.Substring(3);
                    Wallet memberWalletData = await GetWalletCache(club_id);
                    if (memberWalletData == null || memberWalletData.Club_id == null)
                    {
                        throw new Exception("沒有會員id");
                    }

                    var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.PP);
                    if (gameUser == null || gameUser.game_user_id != group.Key)
                    {
                        throw new Exception("No PP user");
                    }
                    // 紀錄 reportTime 跟 playTime 的關聯
                    
                    var dt = DateTime.Now;
                    var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
              

                    foreach (GetRecordResponses item in group)
                    {
                        item.StartDate = item.StartDate.AddHours(8);
                        if (item.EndDate != null)
                        {
                            item.EndDate = item.EndDate.Value.AddHours(8);
                        }

                        item.report_time = reportTime;
                        await using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
                        {
                            await conn.OpenAsync();
                            await using var tran = await conn.BeginTransactionAsync();
                            await Calculate(conn, tran, item);

                        }
                        switch (item.Status)
                        {
                            case "I":
                                item.club_id = memberWalletData.Club_id;
                                item.franchiser_id = memberWalletData.Franchiser_id;
                                betDetailDataRunning.Add(item);
                                break;
                            case "C":
                                betDetailData.Add(item);
                                break;
                            default:
                                break;
                        }


                        var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                        if (!dic.ContainsKey(summaryTime))
                        {
                            dic.Add(summaryTime, new HashSet<string>());
                        }

                        dic[summaryTime].Add(item.StartDate.ToString("yyyy-MM-dd HH:mm"));

                    }

                    _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

                    _logger.LogError(ex, "Run PP record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                  
                }

            }
            try
            {
                foreach (var item in dic)
                {
                    foreach (var subItem in item.Value)
                    {
                        var key = $"{RedisCacheKeys.PpBetSummaryTime}:{item.Key}";
                        await _commonService._cacheDataService.ListPushAsync(key, subItem);
                    }
                }
                await using (var conn = new NpgsqlConnection(_betLogsDbConnectionStringManager.GetMasterConnectionString()))
                {
                    await conn.OpenAsync();

                    if (betDetailDataRunning.Count > 0)
                    {
                        await using var tran = await conn.BeginTransactionAsync();
                        int PostRunningRecordResult = await _ppDBService.PostppRunningRecord(conn, tran, betDetailDataRunning);
                        postResult += await _ppDBService.Postppv2Record(conn, tran, betDetailDataRunning);
                        await tran.CommitAsync();
                    }

                    foreach (var chunk in betDetailData.Chunk(10000))
                    {
                        await using var tran = await conn.BeginTransactionAsync();
                        postResult += await _ppDBService.Postppv2Record(conn, tran, betDetailData);
                        await tran.CommitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

                _logger.LogError(ex, "Run PP record  exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ",  ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return postResult;

        }
        /// <summary>
        /// 統計遊戲商
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }

                _logger.LogDebug("Create PP game provider report time {datetime}", reportTime);



                // 遊戲商(轉帳中心的欄位格式)
                var gameEmptyReport = new GameReport
                {
                    platform = nameof(Platform.PP),
                    report_datetime = reportTime,
                    report_type = (int)GameReport.e_report_type.FinancalReport,
                    total_bet = 0,
                    total_win = 0,
                    total_netwin = 0,
                    total_count = 0
                };

                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
                startDateTime = startDateTime.AddHours(1);


                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
                startDateTime = startDateTime.AddHours(1);
            }
            await Task.Delay(3000);

        }
        /// <summary>
        /// 統計W1
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
        {
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create PP game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _ppDBService.SumppBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.PP);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin;
                reportData.total_netwin = totalWin - totalBetValid;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }

        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, GetRecordResponses r)
        {
            r.pre_Bet = r.Bet;
            r.pre_Win = r.Win;
            var ppRecord = await _ppDBService.GetppRecordNew(r.PlaySessionID.ToString(), r.StartDate);
            ppRecord ??= new();
            if (ppRecord.Any(x => new { x.PlaySessionID, x.StartDate, x.Status }.Equals(new { r.PlaySessionID, r.StartDate, r.Status })))
            {
                r.Status = "F";
                return;
            }
            if (ppRecord.Any())
            {
                var lastpp = ppRecord.OrderByDescending(x => x.StartDate).First();

                r.Bet = r.Bet - lastpp.pre_Bet;
                r.Win = r.Win - lastpp.pre_Win;
            }

            await _ppDBService.DeleteppRunningRecord(conn, tran, r);

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

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairPP(DateTime startTime, DateTime endTime)
        {

            DateTimeOffset dto = new DateTimeOffset(startTime);
            var req = new GetRecordRequest
            {
                login = Config.CompanyToken.PP_SecureLogin,
                password = Config.CompanyToken.PP_Key,
                timepoint = dto.ToUnixTimeMilliseconds().ToString()
            };
            List<GetRecordResponses> res = new List<GetRecordResponses>();
            List<GetRecordResponses> repairList = new List<GetRecordResponses>();
            res = await _gameApiService._PPAPI.GetRecordAsync(req);

            foreach (var item in res.GroupBy(r => r.StartDate.Ticks / BATCH_OFFSET.Ticks * BATCH_OFFSET.Ticks))
            {
                var w1CenterList = await _ppDBService.GetppRecordsv1(item.Min(x => x.StartDate).AddHours(5), item.Max(x => x.StartDate).AddHours(8).AddMinutes(30));
                foreach (var data in item)
                {
                    var hasData = w1CenterList.Where(x => x.PlaySessionID == data.PlaySessionID && x.Status == data.Status).Any();
                    if (hasData == false)
                    {
                        repairList.Add(data);
                    }
                }
            }

            var postResult = 0;
            if (repairList.Count != 0)
            {
                foreach (var group in repairList.GroupBy(r => r.StartDate.Ticks / BATCH_OFFSET.Ticks * BATCH_OFFSET.Ticks))
                {
                    _logger.LogInformation("PostPPRecord Group Key:{key} Count:{count}", new DateTime(group.Key), group.Count());
                    try
                    {
                        postResult += await PostPPRecord(group.ToList());
                    }
                    catch (Exception ex)
                    {
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError(ex, "Run PP record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine} StartDate:{StartDate}", ex.GetType().FullName, ex.Message, errorFile, errorLine, new DateTime(group.Key));
                    }
                }
            }
            return postResult;
        }

        /// <summary>
        /// 查詢結算單
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> pp_results = await _ppDBService.GetppRunningRecord(RecordReq);
            pp_results = pp_results.OrderByDescending(e => e.StartDate);
            res.Data = pp_results.ToList();
            return res;

        }
        #endregion
        /// <summary>
        /// 五分會總
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="reportDatetime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _ppDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.PP);
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
    }
}
