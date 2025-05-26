using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.NEXTSPIN;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Request;
using H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Utility;
using H1_ThirdPartyWalletAPI.Worker.Game.NEXTSPIN;

using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using NextSpinConfig = H1_ThirdPartyWalletAPI.Model.Game.NEXTSPIN.NEXTSPIN;

namespace H1_ThirdPartyWalletAPI.Service.Game.NEXTSPIN;

public interface INEXTSPIN_InterfaceService : IGameInterfaceService
{
    Task<List<GetBetHistoryResponse.BetInfo>> GetNextSpinRecord(DateTime startDateTime, DateTime endDateTime);
    Task<int> PostNextSpinRecord(IEnumerable<GetBetHistoryResponse.BetInfo> betInfos);
    Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    Task<int> PostNextSpinRecordV2(List<GetBetHistoryResponse.BetInfo> betInfos);
}

public class NEXTSPIN_InterfaceService : INEXTSPIN_InterfaceService
{
    private readonly ILogger<NEXTSPIN_InterfaceService> _logger;
    private readonly INEXTSPINApiService _apiService;
    private readonly INextSpinDBService _nextSpinDBService;
    private readonly ISummaryDBService _summaryDBService;
    private readonly IDBService _dbService;
    private readonly ICacheDataService _cacheService;
    private readonly IGamePlatformUserService _gamePlatformUserService;
    private readonly IGameReportDBService _gameReportDBService;
    private readonly IBetLogsDbConnectionStringManager _betLogsDbConnectionStringManager;
    private readonly ISystemParameterDbService _systemParameterDbService;

    private const int _cacheSeconds = 600;
    private const int _cacheFranchiserUser = 1800;

    private readonly string _prefixKey;

    public NEXTSPIN_InterfaceService(ILogger<NEXTSPIN_InterfaceService> logger,
                                     INEXTSPINApiService apiService,
                                     IDBService dbService,
                                     ICacheDataService cacheService,
                                     IGamePlatformUserService gamePlatformUserService,
                                     INextSpinDBService nextSpinDBService,
                                     ISummaryDBService summaryDBService,
                                     IGameReportDBService gameReportDBService,
                                     IBetLogsDbConnectionStringManager betLogsDbConnectionStringManager,
                                     ISystemParameterDbService systemParameterDbService)
    {
        _logger = logger;
        _apiService = apiService;
        _dbService = dbService;
        _cacheService = cacheService;
        _gamePlatformUserService = gamePlatformUserService;
        _prefixKey = Config.OneWalletAPI.Prefix_Key;
        _nextSpinDBService = nextSpinDBService;
        _summaryDBService = summaryDBService;
        _gameReportDBService = gameReportDBService;
        _betLogsDbConnectionStringManager = betLogsDbConnectionStringManager;
        _systemParameterDbService = systemParameterDbService;
    }

    public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
    {
        var checkTransferRecordResponse = new CheckTransferRecordResponse();

        var response = new CheckTransferResponse();
        try
        {
            var req = new CheckTransferRequest()
            {
                serialNo = CutGuidTo20Characters(transfer_record.id)
            };

            response = await _apiService.CheckTransferAsync(req);
        }
        catch (ExceptionMessage ex) when (ex.MsgId == (int)NextSpinConfig.ErrorCode.Record_Id_Not_Found)
        {
            response.status = 0;
        }

        if (response.status == 1)
        {
            if (transfer_record.target == nameof(Platform.NEXTSPIN))//轉入NEXTSPIN直接改訂單狀態為成功
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
            transfer_record.status = transfer_record.status = nameof(TransferStatus.success);
            transfer_record.success_datetime = DateTime.Now;
        }
        else if (response.status == 0)
        {
            if (transfer_record.target == nameof(Platform.NEXTSPIN))//轉入NEXTSPIN直接改訂單狀態為失敗
            {
                checkTransferRecordResponse.CreditChange = transfer_record.amount;
                checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;

            }
            else if (transfer_record.source == nameof(Platform.NEXTSPIN))
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
        checkTransferRecordResponse.TRecord = transfer_record;
        return checkTransferRecordResponse;
    }

    public Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
    {
        if (!NextSpinConfig.Currency.ContainsKey(userData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        //創建帳號 NEXTSPIN存款時自動建立帳號
        var gameUser = new GamePlatformUser();
        gameUser.club_id = userData.Club_id;
        gameUser.game_user_id = _prefixKey + request.Club_id;
        gameUser.game_platform = Platform.NEXTSPIN.ToString();
        return Task.FromResult(gameUser);
    }

    public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!NextSpinConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new DepositRequest()
            {
                acctId = platform_user.game_user_id,
                amount = RecordData.amount,
                currency = walletData.Currency,
                serialNo = CutGuidTo20Characters(RecordData.id),
            };

            var response = await _apiService.DepositAsync(req);

            RecordData.status = nameof(TransferStatus.success);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Deposit fail ex : {ex}", Platform.NEXTSPIN, ex);
        }

        return RecordData.status;
    }

    public async Task<string> GameDetailURL(GetBetDetailReq request)
    {
        using var md5 = MD5.Create();
        byte[] plainByteArray = md5.ComputeHash(Encoding.UTF8.GetBytes($"{request.record_id}|{Config.CompanyToken.NEXTSPIN_SecretKey}"));
        string cipherText = Convert.ToHexString(plainByteArray).ToLower();

        string domain = Config.GameAPI.NEXTSPIN_DETAIL_URL;
        if (string.IsNullOrWhiteSpace(domain))
        {
            domain = await GetDomainAsync(); ;
            domain = domain.Replace("lobby.", "gameapi.");
        }
        return $"{domain}/betDetails?merchantId={Config.CompanyToken.NEXTSPIN_MerchantCode}&token={cipherText}&lang={request.lang}&ticketId={request.record_id}";
    }

    public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
    {
        GetBetRecord res = new();
        var records = new List<dynamic>();
        var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();

        var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
        if (summary == null)
            return res;
        //更新判斷新舊表日期
        DateTime targetDate = new DateTime(2024, 7, 29, 11, 55, 00);
        if (summary.ReportDatetime < targetDate)
        {
            foreach (var partition in partitions)
            {
                records.AddRange(await _nextSpinDBService.GetNextSpinRecordsBySummary(RecordReq.summary_id, partition, partition.AddDays(1).AddMilliseconds(-1)));
            }
        }
        else
        {

            foreach (var partition in partitions)
            {
                records.AddRange(await _nextSpinDBService.GetNextSpinRecordV2sBySummary(summary.Club_id
                       , summary.ReportDatetime.Value
                       , partition
                       , partition.AddDays(1)));
            }
        }


        //新舊並存階段
        //if (!partitions.Any()) //沒有對應為舊資料
        //{
        //    records.AddRange(await _nextSpinDBService.GetNextSpinRecordsBySummary(RecordReq.summary_id, RecordReq.ReportTime.AddDays(-3), RecordReq.ReportTime.AddDays(1)));
        //}
        //else //有對應為新資料
        //{
        //    var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
        //    if (summary == null)
        //        return res;

        //    foreach (var partition in partitions)
        //    {
        //        records.AddRange(await _nextSpinDBService.GetNextSpinRecordV2sBySummary(summary.Club_id
        //            , summary.ReportDatetime.Value
        //            , partition
        //            , partition.AddDays(1)));
        //    }
        //}

        res.Data = records.OrderByDescending(r => r.ticketTime).ToList();
        return res;
    }

    public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
    {
        var memberBalance = new MemberBalance();
        try
        {
            var req = new GetAcctInfoRequest()
            {
                acctId = platform_user.game_user_id,
                serialNo = CutGuidTo20Characters(Guid.NewGuid()),
            };

            var result = await _apiService.GetAcctInfoAsync(req);

            if (result.list.Any())
                memberBalance.Amount = result.list.Single().balance;
            else
                memberBalance.Amount = decimal.Zero;
        }
        catch (Exception ex)
        {
            memberBalance.Amount = 0;
            memberBalance.code = (int)ResponseCode.Fail;
            memberBalance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
            _logger.LogError("{platform} 餘額取得失敗 Msg: {Message}", Platform.NEXTSPIN, ex.Message);
        }

        memberBalance.Wallet = nameof(Platform.NEXTSPIN);
        return memberBalance;
    }

    public async Task<bool> KickAllUser(Platform platform)
    {
        try
        {
            var req = new KickAcctRequest()
            {
                acctId = string.Empty,
                serialNo = CutGuidTo20Characters(Guid.NewGuid())
            };

            await _apiService.KickAcctAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{platform} KickAllUser Exception {error}", Platform.NEXTSPIN, ex.ToString());
        }

        return true;
    }

    public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
    {
        try
        {
            var req = new KickAcctRequest()
            {
                acctId = platform_user.game_user_id,
                serialNo = CutGuidTo20Characters(Guid.NewGuid())
            };

            await _apiService.KickAcctAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{platform} KickUser Exception {error}", Platform.NEXTSPIN, ex.ToString());
        }

        return true;
    }

    public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
    {
        NextSpinConfig.Lang.TryGetValue(request.GameConfig["lang"], out var lang);

        var token = CutGuidTo20Characters(Guid.NewGuid());
        lang ??= NextSpinConfig.Lang["en-US"];
        var game = request.GameConfig["gameCode"];
        var acctId = platformUser.game_user_id;

        var storeTokenTask = _cacheService.StringSetAsync($"{RedisCacheKeys.LoginToken}:{Platform.NEXTSPIN}:{userData.Club_id}", token, (int)TimeSpan.FromMinutes(15).TotalSeconds);
        var storeGamePlatformUser = _cacheService.StringSetAsync($"{RedisCacheKeys.LoginToken}:{Platform.NEXTSPIN}:GamePlatformUser:{platformUser.game_user_id}", platformUser, (int)TimeSpan.FromMinutes(15).TotalSeconds);

        var domain = await GetDomainAsync();

        await storeTokenTask;
        await storeGamePlatformUser;

        return $"{domain}/{Config.CompanyToken.NEXTSPIN_MerchantCode}/auth/?acctId={acctId}&token={token}&language={lang}&game={game}";
    }
    public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!NextSpinConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new WithdrawRequest()
            {
                acctId = platform_user.game_user_id,
                amount = RecordData.amount,
                currency = walletData.Currency,
                serialNo = CutGuidTo20Characters(RecordData.id),
            };

            var response = await _apiService.WithdrawAsync(req);

            RecordData.status = nameof(TransferStatus.success);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{platform} Withdraw fail ex : {ex}", Platform.NEXTSPIN, ex);
        }

        return RecordData.status;
    }

    public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
    {
        #region 補注單
        var start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
        var maxEnd = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, 0);

        var offSet = TimeSpan.FromHours(1);

        var postResult = 0;

        while (start < maxEnd)
        {
            var end = start.Add(offSet);
            if (end > maxEnd)
            {
                end = maxEnd;
            }

            var betInfos = await GetNextSpinRecord(start, end);

            postResult += await PostNextSpinRecordV2(betInfos);

            start = end;
        }
        #endregion
        #region 重產匯總帳
        var ReportScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(NextSpinReportSchedule.SYSTEM_PARAMETERS_KEY)).value);


        start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0);
        offSet = TimeSpan.FromHours(1);
        while (start < maxEnd)
        {
            if (start > ReportScheduleTime)
            {
                break;
            }

            var end = start.Add(offSet);

            await SummaryW1Report(start, end);

            start = end;
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        #endregion
        return $"Game: {Platform.NEXTSPIN} 新增資料筆數: {postResult}";
    }

    public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
    {
        var key = $"{RedisCacheKeys.NextSpinBetSummaryTime}:{reportDatetime:yyyy-MM-dd HH:mm}";

        var start = reportDatetime.AddDays(-3);
        var end = reportDatetime.AddMinutes(30);

        try
        {
            var min = await _cacheService.SortedSetPopMinAsync(key);
            var max = await _cacheService.SortedSetPopMaxAsync(key);

            if (min.element != default)
                start = DateTime.Parse(min.element).AddMinutes(-5);

            if (max.element != default)
                end = DateTime.Parse(max.element).AddMinutes(5);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "{action} {message} {reportTime}", nameof(RecordSummary), ex.Message, reportDatetime);
        }

        var summaries = await _nextSpinDBService.NextSpinBetRecordV2Summary(reportDatetime, start, end);
        _logger.LogInformation("{action} 彙總時間: {reportTime} 查詢範圍 {start} ~ {end}", nameof(RecordSummary), reportDatetime, start, end);

        //以userid重新GroupBy
        var userSummaries = summaries.GroupBy(s => s.userid);

        #region 取得會員wallet。 批次處理，每次1000筆
        // 提取用戶列表
        var userList = userSummaries.Select(g => g.Key).ToList();

        // 將用戶列表分批處理
        var batchedUserLists = userList.Chunk(1000);

        // 非同步獲取每一批用戶的錢包信息
        var semaphore = new SemaphoreSlim(3); // 最多允許3個同時執行的任務
        var walletTasks = batchedUserLists.Select(async batch =>
        {
            await semaphore.WaitAsync(); // 等待獲取許可
            try
            {
                return await _dbService.GetWallet(batch);
            }
            finally
            {
                semaphore.Release(); // 釋放許可
            }
        });
        var userWallets = await Task.WhenAll(walletTasks);

        // 將結果轉換為字典
        var userWalletDict = userWallets.SelectMany(wallets => wallets)
            .ToDictionary(wallet => wallet.Club_id, wallet => wallet);
        #endregion

        var summaryRecordList = new List<(BetRecordSummary summay, HashSet<t_summary_bet_record_mapping> mappings)>();
        foreach (var userSummariesGroup in userSummaries)
        {
            if (!userWalletDict.TryGetValue(userSummariesGroup.Key, out var userWallet)) continue;

            var summaryData = new BetRecordSummary
            {
                Turnover = userSummariesGroup.Sum(s => s.bet),
                ReportDatetime = reportDatetime,
                Currency = userWallet.Currency,
                Club_id = userWallet.Club_id,
                Franchiser_id = userWallet.Franchiser_id,
                RecordCount = userSummariesGroup.Sum(s => s.count),
                Game_id = nameof(Platform.NEXTSPIN),
                Game_type = 3, //電子遊戲
                JackpotWin = decimal.Zero,
                Bet_amount = userSummariesGroup.Sum(s => s.bet),
                Win = userSummariesGroup.Sum(s => s.win),
                Netwin = userSummariesGroup.Sum(s => s.netwin)
            };

            var mapping = new HashSet<t_summary_bet_record_mapping>();
            foreach (var tickDateTime in userSummariesGroup.Select(s => s.ticketTimeDate))
            {
                mapping.Add(new()
                {
                    summary_id = summaryData.id,
                    report_time = summaryData.ReportDatetime.Value,
                    partition_time = tickDateTime
                });
            }

            summaryRecordList.Add((summaryData, mapping));
        }

        await using NpgsqlConnection conn = new(_betLogsDbConnectionStringManager.GetMasterConnectionString());
        await conn.OpenAsync();
        foreach (var chunk in summaryRecordList.Chunk(10000))
        {
            await using var tran = await conn.BeginTransactionAsync();
            var sw = Stopwatch.StartNew();
            await _summaryDBService.BatchInsertRecordSummaryAsync(conn, chunk.Select(c => c.summay).ToList());
            await _summaryDBService.BulkInsertSummaryBetRecordMapping(tran, chunk.SelectMany(c => c.mappings));
            sw.Stop();
            _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", chunk.Count(), sw.ElapsedMilliseconds);
            await tran.CommitAsync();
        }
        await conn.CloseAsync();

        await _cacheService.KeyDelete(key);

        return true;
    }

    public async Task<int> PostNextSpinRecord(IEnumerable<GetBetHistoryResponse.BetInfo> betInfos)
    {
        if (!betInfos.Any()) return 0;

        var existsTicketIds =
            (await GetExistsTicketIds(betInfos.Min(b => b.TicketTimeFormatted)
                , betInfos.Max(b => b.TicketTimeFormatted)))
            .ToHashSet();

        await using var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
        await conn.OpenAsync();
        var postResult = 0;
        foreach (var logGroup in betInfos.GroupBy(b => b.acctId))
        {
            using var tran = await conn.BeginTransactionAsync();
            try
            {
                Wallet memberWalletData = await GetWalletCache(logGroup.Key);
                if (memberWalletData == null || memberWalletData.Club_id == null)
                {
                    throw new Exception("沒有會員id");
                }

                var gameUser = await _gamePlatformUserService.GetSingleGamePlatformUserAsync(logGroup.Key, Platform.NEXTSPIN);

                if (gameUser == null || gameUser.game_user_id.ToLower()[_prefixKey.Length..] != logGroup.Key.ToLower())
                {
                    throw new Exception("No nextspin user");
                }

                //彙總注單
                Dictionary<string, BetRecordSummary> summaryData = new();
                var summaryBetRecordMappings = new HashSet<t_summary_bet_record_mapping>();

                //已結算注單
                List<GetBetHistoryResponse.BetInfo> betLogs = new();

                foreach (var r in logGroup)
                {
                    if (!existsTicketIds.Add(r.ticketId)) continue;

                    BetRecordSummary sumData = new();
                    sumData.Club_id = memberWalletData.Club_id;
                    sumData.Game_id = nameof(Platform.NEXTSPIN);
                    sumData.Game_type = 3; //電子遊戲 = 3
                    DateTime tempDateTime = r.TicketTimeFormatted;
                    tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                    tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                    tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                    sumData.ReportDatetime = tempDateTime;
                    //確認是否已經超過搬帳時間 For H1 only
                    if (Config.OneWalletAPI.RCGMode == "H1")
                    {
                        if (DateTime.Now.Hour >= 12) //換日線
                        {
                            DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                DateTime.Now.Day, 12, 00, 0);
                            if (sumData.ReportDatetime < ReportDateTime)
                            {
                                sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.ticketId);
                            }
                        }
                        else
                        {
                            var lastday = DateTime.Now.AddDays(-1);
                            DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                            if (sumData.ReportDatetime < ReportDateTime)
                            {
                                sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.ticketId);
                            }
                        }
                    }

                    //先確認有沒有符合的匯總單
                    if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                    {
                        sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                        //合併處理
                        sumData = Calculate(sumData, r);
                        summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                    }
                    else
                    {
                        //用Club_id與ReportDatetime DB取得彙總注單
                        IEnumerable<dynamic> results =
                            await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                        if (!results.Any()) //沒資料就建立新的
                        {
                            //建立新的Summary
                            sumData.Currency = memberWalletData.Currency;
                            sumData.Franchiser_id = memberWalletData.Franchiser_id;

                            //合併處理
                            sumData = Calculate(sumData, r);
                        }
                        else //有資料就更新
                        {
                            sumData = results.SingleOrDefault();
                            //合併處理
                            sumData = Calculate(sumData, r);
                        }

                        summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
                    }

                    r.summary_id = sumData.id;
                    betLogs.Add(r);

                    summaryBetRecordMappings.Add(new()
                    {
                        summary_id = sumData.id,
                        report_time = sumData.ReportDatetime.Value,
                        partition_time = r.TicketTimeFormatted.Date
                    });
                }

                List<BetRecordSummary> summaryList = summaryData.Select(s => s.Value).ToList();

                int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMappings);
                int PostRecordResult = await _nextSpinDBService.PostNextSpinRecord(tran, betLogs);
                _logger.LogDebug("insert NEXTSPIN record member: {group}, count: {count}", logGroup.Key,
                    betLogs.Count);

                await tran.CommitAsync();

                postResult += PostRecordResult;

                #region PK寫入Redis(停用)
                //try
                //{
                //    var sw = Stopwatch.StartNew();
                //    await Task.WhenAll(
                //        _cacheService.BatchStringSetAsync(
                //            betLogs
                //                .Where(b => (DateTime.Now - b.TicketTimeFormatted) < TimeSpan.FromDays(8))//超過8天注單不寫入redis，用不到
                //                .ToDictionary(b => $"{RedisCacheKeys.RecordPrimaryKey}:{Platform.NEXTSPIN}:{b.ticketId}", b => b.ticketId.ToString())
                //            , TimeSpan.FromDays(7))
                //        );
                //    sw.Stop();
                //    _logger.LogInformation("{platform} write {count} records pk to redis cost {cost}ms!", Platform.NEXTSPIN, betLogs.Count, sw.ElapsedMilliseconds);
                //}
                //catch (Exception ex) //Redis 寫入異常不進行db rollback
                //{
                //    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                //    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                //    _logger.LogError("Run {platform} record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                //        Platform.NEXTSPIN, logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                //}
                #endregion
            }
            catch (Exception ex)
            {
                foreach (var r in logGroup) //loop club id bet detail
                {
                    _logger.LogError("record id : {id}, time: {time}", r.ticketId, r.TicketTimeFormatted);
                }
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run {platform} record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                    Platform.NEXTSPIN, logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                await tran.RollbackAsync();
            }
        }

        return postResult;
    }

    public async Task<int> PostNextSpinRecordV2(List<GetBetHistoryResponse.BetInfo> betInfos)
    {
        if (!betInfos.Any()) return 0;

        var existsTicketIds =
            (await GetExistsTicketIds(betInfos.Min(b => b.TicketTimeFormatted)
                , betInfos.Max(b => b.TicketTimeFormatted)))
            .ToHashSet();

        var now = DateTime.Now;
        var reportTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, (now.Minute / 5) * 5, 0);

        var tickTimes = new HashSet<string>();

        var newBetInfos =
            betInfos
            .Where(b => !existsTicketIds.Contains(b.ticketId))
            .Select(b =>
            {
                b.ReportTime = reportTime;

                tickTimes.Add(b.TicketTimeFormatted.ToString("yyyy-MM-dd HH:mm"));

                return b;
            })
            .ToList();

        if (!newBetInfos.Any()) return 0;

        var result = 0;

        await _cacheService.SortedSetAddAsync($"{RedisCacheKeys.NextSpinBetSummaryTime}:{reportTime:yyyy-MM-dd HH:mm}",
            tickTimes.ToDictionary(t => t, t => (double)DateTime.Parse(t).Ticks));
        await using (var conn = new NpgsqlConnection(_betLogsDbConnectionStringManager.GetMasterConnectionString()))
        {
            await conn.OpenAsync();

            foreach (var chunk in newBetInfos.Chunk(10000))
            {
                await using var tran = await conn.BeginTransactionAsync();
                result += await _nextSpinDBService.PostNextSpinRecordV2(conn, tran, chunk.ToList());
                await tran.CommitAsync();
            }

        }


        return result;
    }

    public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
    {
        var start = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
        var maxEnd = new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, 0, 0);
        while (start < maxEnd)
        {
            var end = start.AddHours(1);

            if (end > maxEnd)
            {
                end = maxEnd;
            }

            _logger.LogDebug("Create NEXTSPIN game W1 report time {datetime}", start);

            var (totalCount, totalBetValid, totalNetWin) = await _nextSpinDBService.SumNextSpinBetRecordV2ByTicketTime(start, end.AddMilliseconds(-1));

            GameReport reportData = new();
            reportData.platform = nameof(Platform.NEXTSPIN);
            reportData.report_datetime = start;
            reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
            reportData.total_bet = totalBetValid;
            reportData.total_win = totalBetValid + totalNetWin;
            reportData.total_netwin = totalNetWin;
            reportData.total_count = totalCount;

            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);

            start = end;
            await Task.Delay(3000);
        }
    }
    /// <summary>
    /// 統計遊戲商
    /// </summary>
    /// <param name = "startDateTime" ></ param>
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
            _logger.LogDebug("Create Ps game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            // 每日統計
            var req = await _apiService.GetReportAsync(new GetReportRequest()
            {
                beginDate = startDateTime.ToString("yyyyMMdd'T'HHmmss"),
                endDate = startDateTime.AddHours(1).AddMilliseconds(-1).ToString("yyyyMMdd'T'HHmmss"),
                pageIndex = 1,
                merchantCode = Config.CompanyToken.NEXTSPIN_MerchantCode

            });
            int count = 0;
            var gameEmptyReport = new GameReport();
            if (req.resultCount == 0)
            {
                gameEmptyReport.platform = nameof(Platform.NEXTSPIN);
                gameEmptyReport.report_datetime = reportTime;
                gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                gameEmptyReport.total_bet = 0;
                gameEmptyReport.total_win = 0;
                gameEmptyReport.total_netwin = 0;
                gameEmptyReport.total_count = 0;

            }
            else
            {
                foreach (var dateEntry in req.list)
                {
                    gameEmptyReport.platform = nameof(Platform.NEXTSPIN);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet += (decimal)dateEntry.betAmount;
                    gameEmptyReport.total_win += (decimal)dateEntry.totalWL + (decimal)dateEntry.betAmount;
                    gameEmptyReport.total_netwin += (decimal)dateEntry.totalWL;
                    gameEmptyReport.total_count+= dateEntry.betCount;
                }
            }

            await _gameReportDBService.DeleteGameReport(gameEmptyReport);
            await _gameReportDBService.PostGameReport(gameEmptyReport);
            startDateTime = startDateTime.AddHours(1);

            await Task.Delay(3000);
        }
    }
    public PlatformType GetPlatformType(Platform platform)
    {
        return PlatformType.Electronic;
    }

    public async Task<List<GetBetHistoryResponse.BetInfo>> GetNextSpinRecord(DateTime startDateTime, DateTime endDateTime)
    {
        var betInfos = new List<GetBetHistoryResponse.BetInfo>();

        var page = 1;
        var req = new GetBetHistoryRequest()
        {
            beginDate = startDateTime.ToString("yyyyMMddTHHmmss"),
            endDate = endDateTime.AddSeconds(-1).ToString("yyyyMMddTHHmmss"),
        };

        while (true)
        {
            req.pageIndex = page;
            req.serialNo = CutGuidTo20Characters(Guid.NewGuid());

            GetBetHistoryResponse res;

            res = await _apiService.GetBetHistoryAsync(req);

            if (res.list is not null)
                betInfos.AddRange(res.list);

            if (page >= res.pageCount)
                break;

            page++;
        }

        return betInfos
            .Where(l => l.acctId.ToLower().StartsWith(_prefixKey.ToLower()))
            .Select(l =>
            {
                l.acctId = l.acctId[_prefixKey.Length..]; //去除PrfixKey
                return l;
            })
            .ToList();
    }

    public Task HealthCheck(Platform platform)
    {
        return _apiService.GetDomainListAsync(new()
        {
            serialNo = CutGuidTo20Characters(Guid.NewGuid()),
        });
    }

    /// <summary>
    /// 計算彙總
    /// </summary>
    /// <param name="SummaryData"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    private BetRecordSummary Calculate(BetRecordSummary SummaryData, GetBetHistoryResponse.BetInfo r)
    {
        SummaryData.RecordCount++;
        SummaryData.Bet_amount += r.betAmount;
        SummaryData.Turnover += r.betAmount;
        SummaryData.Netwin += r.winLoss;
        SummaryData.Win += r.betAmount + r.winLoss;
        SummaryData.updatedatetime = DateTime.Now;
        return SummaryData;
    }

    private async Task<Wallet> GetWalletCache(string Club_id)
    {
        Wallet walletData = await _cacheService.GetOrSetValueAsync($"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}",
        async () =>
        {
            try
            {
                IEnumerable<Wallet> result = await _dbService.GetWallet(Club_id);
                if (result.Count() != 1)
                {
                    throw new ExceptionMessage(ResponseCode.UserNotFound);
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
    /// 縮短GUID長度
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    private static string CutGuidTo20Characters(Guid guid)
    {
        return guid.ToString("N")[12..];
    }
    private async Task<string> GetDomainAsync()
    {
        return await _cacheService.GetOrSetValueAsync($"{RedisCacheKeys.DomainCache}:{Platform.NEXTSPIN}"
            , async () =>
            {
                var req = new GetDomainListRequest()
                {
                    serialNo = CutGuidTo20Characters(Guid.NewGuid())
                };

                var response = await _apiService.GetDomainListAsync(req);

                return response.domains.First();
            }
            , (int)TimeSpan.FromMinutes(5).TotalSeconds);
    }

    private async Task<List<long>> GetExistsTicketIds(DateTime start, DateTime end)
    {
        var tasks = new List<Task<List<NextSpinPrimaryKey>>>
        {
            _nextSpinDBService.GetNextSpinRecordsByTicketTime(start, end), //讀取舊表
            _nextSpinDBService.GetNextSpinRecordV2sByTicketTime(start, end) //讀取新表
        };
        var result = await Task.WhenAll(tasks);
        return result
            .SelectMany(r => r)
            .Select(r => r.ticketId)
            .ToList();
    }
}
