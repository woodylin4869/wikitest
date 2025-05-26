using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JILI;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses;
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
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static H1_ThirdPartyWalletAPI.Model.Game.JILI.Responses.GetBetRecordByTimeResponse;


namespace H1_ThirdPartyWalletAPI.Service.Game.JILI
{
    public interface IJILIInterfaceService : IGameInterfaceService
    {
        Task<int> PostJiliRecord(GetBetRecordByTimeData recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class JILI_RecordService : IJILIInterfaceService
    {
        private readonly ILogger<JILI_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly IDBService _dbService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameApiService _gameApiService;
        private readonly IJILIDBService _jilidbService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IBetLogsDbConnectionStringManager _betLogsDbConnectionStringManager;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public JILI_RecordService(ILogger<JILI_RecordService> logger,
            ICommonService commonService,
            IWebHostEnvironment env,
            IGameApiService gameaApiService,
            IDBService dbService,
            IJILIDBService jilidbService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IBetLogsDbConnectionStringManager betLogsDbConnectionStringManager
        )
        {
            _logger = logger;
            _commonService = commonService;
            _env = env;
            _gameApiService = gameaApiService;
            _dbService = dbService;
            _summaryDBService = summaryDBService;
            _jilidbService = jilidbService;
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
                var responseData = await _gameApiService._JiliApi.GetMemberInfoAsync(new GetMemberInfoRequest()
                {
                    Accounts = platform_user.game_user_id
                });
                //未取得遊戲帳號
                if (responseData.ErrorCode == 0 && responseData.Data[0].Status == 3)
                {
                    //重新建立遊戲帳號
                    var req = new CreateMemberRequest()
                    {
                        Account = Config.OneWalletAPI.Prefix_Key + platform_user.club_id
                    };
                    var response = await _gameApiService._JiliApi.CreateMemberAsync(req);
                    if (response.ErrorCode == 0)
                    {
                        responseData = await _gameApiService._JiliApi.GetMemberInfoAsync(new GetMemberInfoRequest()
                        {
                            Accounts = platform_user.game_user_id
                        });
                    }
                    else
                    {
                        throw new Exception(response.Message);
                    }
                }
                if (responseData.ErrorCode != 0)
                {
                    throw new Exception(responseData.Message);
                }
                Balance.Amount = responseData.Data[0].Balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("JILI餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.JILI);
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
                var responseData = await _gameApiService._JiliApi.KickMemberAsync(new KickMemberRequest()
                {
                    Account = platform_user.game_user_id
                });

                if (responseData.ErrorCode != 0)
                {
                    throw new Exception(responseData.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出JILI使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
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
                var responseData = await _gameApiService._JiliApi.ExchangeTransferByAgentIdAsync(new ExchangeTransferByAgentIdRequest
                {
                    Account = platform_user.game_user_id,
                    Amount = RecordData.amount,
                    TransactionId = RecordData.id.ToString(),
                    TransferType = 2,
                    Time = DateTime.UtcNow.AddHours(-4).AddMinutes(1)
                });
                //未取得遊戲帳號
                if (responseData.ErrorCode == 101)
                {
                    //重新建立遊戲帳號
                    var req = new CreateMemberRequest()
                    {
                        Account = Config.OneWalletAPI.Prefix_Key + platform_user.club_id
                    };
                    var response = await _gameApiService._JiliApi.CreateMemberAsync(req);
                    if (response.ErrorCode == 0)
                    {
                        responseData = await _gameApiService._JiliApi.ExchangeTransferByAgentIdAsync(new ExchangeTransferByAgentIdRequest
                        {
                            Account = platform_user.game_user_id,
                            Amount = RecordData.amount,
                            TransactionId = RecordData.id.ToString(),
                            TransferType = 2,
                            Time = DateTime.UtcNow.AddHours(-4).AddMinutes(1)
                        });
                    }
                    else
                        RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                }
                if (responseData.ErrorCode != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("JILI Deposit: {Message}", responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("JILI TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("JILI Deposit: {Message}", ex.Message);
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
                var responseData = await _gameApiService._JiliApi.ExchangeTransferByAgentIdAsync(new ExchangeTransferByAgentIdRequest
                {
                    Account = platform_user.game_user_id,
                    Amount = RecordData.amount,
                    TransactionId = RecordData.id.ToString(),
                    TransferType = 3,
                    Time = DateTime.UtcNow.AddHours(-4).AddMinutes(1)
                });

                if (responseData.ErrorCode != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("JILI Withdraw : {ex}", responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("JILI TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("JILI Withdraw : {ex}", ex.Message);
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
            if (!Model.Game.JILI.JILI.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }

            //創建帳號
            try
            {
                var req = new CreateMemberRequest()
                {
                    Account = Config.OneWalletAPI.Prefix_Key + userData.Club_id
                };
                var response = await _gameApiService._JiliApi.CreateMemberAsync(req);
                if (response.ErrorCode == 0 || response.ErrorCode == 101)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.Account;
                    gameUser.game_platform = Platform.JILI.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("JILI建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "JILI " + ex.Message.ToString());
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
            Model.Game.JILI.JILI.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            //Step 3 Get Game URL
            LoginWithoutRedirectRequest UrlRequest = new LoginWithoutRedirectRequest();
            UrlRequest.Account = platformUser.game_user_id;
            UrlRequest.GameId = int.Parse(request.GameConfig["gameCode"]);
            UrlRequest.Lang = lang ?? Model.Game.JILI.JILI.lang["en-US"];
            UrlRequest.platform = request.GameConfig["device"] == "DESKTOP" ? "web" : "app";//網頁板web:手機板app

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.HomeUrl = request.GameConfig["lobbyURL"];
            }

            try
            {
                var token_res = await _gameApiService._JiliApi.LoginWithoutRedirectAsync(UrlRequest);
                if (token_res.ErrorCode == 14)
                {
                    var req = new CreateMemberRequest()
                    {
                        Account = Config.OneWalletAPI.Prefix_Key + platformUser.club_id
                    };
                    var response = await _gameApiService._JiliApi.CreateMemberAsync(req);
                    if (response.ErrorCode == 0)
                    {
                        token_res = await _gameApiService._JiliApi.LoginWithoutRedirectAsync(UrlRequest);
                    }
                    else
                        throw new Exception(response.Message);
                }

                return token_res.Data;
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

            var JiliReuslt = await _gameApiService._JiliApi.CheckTransferByTransactionIdAsync(new CheckTransferByTransactionIdRequest
            {
                TransactionId = transfer_record.id.ToString()
            });
            if (JiliReuslt.ErrorCode == 0 && JiliReuslt.Data.Status == 1)
            {
                if (transfer_record.target == nameof(Platform.JILI))//轉入JILI直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.JILI))
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
            else if (JiliReuslt.ErrorCode == 101 || JiliReuslt.Data.Status == 2)
            {
                if (transfer_record.target == nameof(Platform.JILI))//轉入JILI直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.JILI))
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
            GetBetRecord res = new GetBetRecord();
            List<dynamic> jili_results = new List<dynamic>();
            var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();

            foreach (var partition in partitions)
            {
                jili_results.AddRange(await _jilidbService.GetjiliRecordsBySummary(RecordReq.summary_id, partition, partition.AddDays(1)));
            }

            if (jili_results.Count == 0)
            {
                foreach (var partition in partitions)
                {
                    jili_results.AddRange(await _jilidbService.GetjiliRecordsByreporttime(Config.OneWalletAPI.Prefix_Key + RecordReq.ClubId, RecordReq.ReportTime, partition, partition.AddDays(1)));
                }

            }

            jili_results = jili_results
                .OrderByDescending(e => e.WagersTime)
                .Select(x =>
                {
                    x.PayoffAmount += x.BetAmount;
                    return x;
                })
                .ToList();
            res.Data = jili_results;
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
            Model.Game.JILI.JILI.lang.TryGetValue(RecordDetailReq.lang, out var lang);
            lang ??= Model.Game.JILI.JILI.lang["en-US"];
            var JILIResponseData = await _gameApiService._JiliApi.GetGameDetailUrlAsync(new GetGameDetailUrlRequest()
            {
                WagersId = Int64.Parse(RecordDetailReq.record_id),
                Lang = lang,
            });

            if (JILIResponseData != null && JILIResponseData.ErrorCode != 0)
            {
                throw new Exception("no data");
            }
            return JILIResponseData.Data.Url;
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
                RepairCount += await RepairJili(startTime, endTime);
                startTime = endTime;
            }
            RepairCount += await RepairJili(startTime, RepairReq.EndTime);
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
            return _gameApiService._JiliApi.GetGameListAsync();
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostJiliRecord(GetBetRecordByTimeData recordData)
        {
            var postResult = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var betRecords = recordData.Result.Where(x => x.Account.ToLower().StartsWith(Config.OneWalletAPI.Prefix_Key.ToLower()))
                                                  .OrderBy(x => x.WagersTime)
                                                  .ToList();
            List<Result> betDetailData = new List<Result>();
            var dic = new Dictionary<string, HashSet<string>>();
            sw.Restart();
            foreach (Result item in betRecords)
            {
                //彙總注單
                // 紀錄 reportTime 跟 WagersTime 的關聯
                var dt = DateTime.Now;
                var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                //已結算注單

                item.report_time = reportTime;

                betDetailData.Add(item);
                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                if (!dic.ContainsKey(summaryTime))
                {
                    dic.Add(summaryTime, new HashSet<string>());
                }
                dic[summaryTime].Add(item.WagersTime.ToString("yyyy-MM-dd HH:mm"));
            }
            try
            {
                foreach (var item in dic)
                {
                    foreach (var subItem in item.Value)
                    {
                        var key = $"{RedisCacheKeys.JiliSummaryTime}:{item.Key}";
                        await _commonService._cacheDataService.ListPushAsync(key, subItem);
                    }
                }
                if (betDetailData.Count > 0)
                {
                    await using (var conn = new NpgsqlConnection(_betLogsDbConnectionStringManager.GetMasterConnectionString()))
                    {
                        await conn.OpenAsync();
                        _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                        foreach (var chunk in betDetailData.Chunk(10000))
                        {

                            await using var tran = await conn.BeginTransactionAsync();
                            postResult += await _jilidbService.PostjiliRecord(conn, tran, chunk.ToList());
                            await tran.CommitAsync();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

                _logger.LogError("Run JILI record  exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
            }
            sw.Stop();
            _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
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

                _logger.LogDebug("Create JILI game provider report time {datetime}", reportTime);
                // 每小時投注匯總
                // UTC-4
                GetBetRecordSummaryRequest req = new GetBetRecordSummaryRequest()
                {
                    StartTime = reportTime.AddHours(-12),
                    EndTime = reportTime.AddHours(1).AddHours(-12),
                    FilterAgent = 1
                };

                //取得這小時
                GetBetRecordSummaryResponse JiliCenterList = await _gameApiService._JiliApi.GetBetRecordSummaryAsync(req);
                if (JiliCenterList.Data.Length == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.JILI),
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
                }
                else
                {
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.JILI),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = -1 * (JiliCenterList.Data[0].BetAmount),
                        total_win = JiliCenterList.Data[0].PayoffAmount,
                        total_netwin = JiliCenterList.Data[0].PayoffAmount - (-1 * (JiliCenterList.Data[0].BetAmount)),
                        total_count = JiliCenterList.Data[0].WagersCount,
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddHours(1);
                }
                await Task.Delay(3000);
            }
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
                _logger.LogDebug("Create JILI game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _jilidbService.SumjiliBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.JILI);
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

        #endregion
        private async Task<BetRecordSummary> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, Result r)
        {
            switch (r.Status)
            {
                //1: 贏 2: 輸 3: 平局
                case 1:
                case 2:
                case 3:
                    SummaryData.RecordCount++;
                    SummaryData.Bet_amount += -1 * r.BetAmount;
                    SummaryData.Turnover += r.Turnover;
                    SummaryData.Netwin += r.PayoffAmount - (-1 * r.BetAmount);
                    SummaryData.Win += r.PayoffAmount;
                    break;
                //不處理 重複單
                case 4:
                    //waiting單暫不處理   
                    break;
            }
            SummaryData.updatedatetime = DateTime.Now;
            SummaryData.JackpotWin = 0;
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

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairJili(DateTime startTime, DateTime endTime)
        {
            var Page = 1;
            var req = new GetBetRecordByTimeRequest
            {
                StartTime = startTime.AddHours(-12),
                EndTime = endTime.AddHours(-12).AddSeconds(-1),
                Page = Page,
                PageLimit = 10000,
                FilterAgent = 1
            };

            GetBetRecordByTimeData res = new GetBetRecordByTimeData()
            {
                Result = new List<Result>()
            };
            while (true)
            {
                req.Page = Page;
                var betLogs = await _gameApiService._JiliApi.GetBetRecordByTimeAsync(req);

                if (betLogs.Data.Result.Count == 0)
                {
                    break;
                }
                res.Result.AddRange(betLogs.Data.Result);

                Page++;
                if (Page > betLogs.Data.Pagination.TotalPages)
                    break;
                //api建議20~30秒爬一次
                await Task.Delay(10000);
            }

            if (!res.Result.Any())
                return 0;

            var minWagersTime = res.Result.Min(r => r.WagersTime);
            var maxWagersTime = res.Result.Max(r => r.WagersTime);

            var exsitsPK = (await _jilidbService.GetjiliRecordsBytime(minWagersTime, maxWagersTime.AddSeconds(1)))
                .ToHashSet(new JILIBetRecordComparer());

            res.Result =
                res.Result
                .Where(r => exsitsPK.Add(new()
                {
                    Account = r.Account,
                    WagersId = r.WagersId,
                    WagersTime = r.WagersTime
                }))
                .ToList();

            if (!res.Result.Any())
                return 0;

            await PostJiliRecord(res);

            return res.Result.Count;
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
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _jilidbService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Turnover = -1 * summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.JILI);
                summaryData.Game_type = 3;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = -1 * summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.win) - (-1 * summaryRecord.Sum(x => x.bet));
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
        /// 設定差異比對參數
        /// </summary>
        public class JILIBetRecordComparer : IEqualityComparer<JILIRecordPrimaryKey>
        {
            bool IEqualityComparer<JILIRecordPrimaryKey>.Equals(JILIRecordPrimaryKey x, JILIRecordPrimaryKey y)
            {
                //確認兩個物件的資料是否相同
                if (ReferenceEquals(x, y)) return true;

                //確認兩個物件是否有任何資料為空值
                if (x is null || y is null)
                    return false;

                //這邊就依照個人需求比對各個屬性的值
                return x.Account == y.Account
                    && x.WagersId == y.WagersId
                    && x.WagersTime == y.WagersTime;
            }

            int IEqualityComparer<JILIRecordPrimaryKey>.GetHashCode(JILIRecordPrimaryKey e)
            {
                //確認物件是否為空值
                if (e is null) return 0;

                int parentBetId = e.Account == null ? 0 : e.Account.GetHashCode();

                //計算HashCode，因為是XOR所以要全部都是1才會回傳1，否則都會回傳0
                return parentBetId ^ e.WagersId.GetHashCode() ^ e.WagersTime.GetHashCode();
            }
        }
    }
}
