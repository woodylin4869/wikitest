using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Request;
using H1_ThirdPartyWalletAPI.Model.Game.PME.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Worker.Game.PME;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using PMEConfig = H1_ThirdPartyWalletAPI.Model.Game.PME.PME;

namespace H1_ThirdPartyWalletAPI.Service.Game.PME;

public interface IPMEInterfaceService : IGameInterfaceService
{
    Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    Task<List<QueryScrollResponse.Bet>> GetPMERecord(DateTime startDateTime, DateTime endDateTime);
    Task<int> PostPMERecord(IEnumerable<QueryScrollResponse.Bet> betInfos);
    Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
}

public class PME_InterfaceService : IPMEInterfaceService
{
    private readonly ILogger<PME_InterfaceService> _logger;
    private readonly IPMEApiService _apiService;
    private readonly IDBService _dbService;
    private readonly ISummaryDBService _summaryDBService;
    private readonly ICacheDataService _cacheService;
    private readonly IGamePlatformUserService _gamePlatformUserService;
    private readonly IPMEDBService _pmedbService;
    private readonly IGameReportDBService _gameReportDBService;
    private readonly ISystemParameterDbService _systemParameterDbService;
    private readonly ICommonService _commonService;
    private const int _cacheSeconds = 600;
    private const int _cacheFranchiserUser = 1800;

    private readonly string _prefixKey;

    public PME_InterfaceService(ILogger<PME_InterfaceService> logger,
                                ICommonService commonService,
                                IPMEApiService apiService,
                                IDBService dbService,
                                ICacheDataService cacheService,
                                IGamePlatformUserService gamePlatformUserService,
                                ISummaryDBService summaryDBService,
                                IPMEDBService pmedbService,
                                IGameReportDBService gameReportDBService,
                                ISystemParameterDbService systemParameterDbService)
    {
        _logger = logger;
        _commonService = commonService;
        _apiService = apiService;
        _dbService = dbService;
        _summaryDBService = summaryDBService;
        _cacheService = cacheService;
        _gamePlatformUserService = gamePlatformUserService;
        _gameReportDBService = gameReportDBService;
        _prefixKey = Config.OneWalletAPI.Prefix_Key;
        _pmedbService = pmedbService;
        _systemParameterDbService = systemParameterDbService;
    }

    public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
    {
        var checkTransferRecordResponse = new CheckTransferRecordResponse();

        var response = new TransferQueryResponse();
        try
        {
            var req = new TransferQueryRequest()
            {
                merOrderId = ConvertToNumericUuid(transfer_record.id)
            };

            response = await _apiService.TransferQueryAsync(req);
        }
        catch (ExceptionMessage ex) when (ex.Message.Contains("merOrderId not found"))
        {
            response.data = "2";
        }

        if (response.data == "3")
        {
            if (transfer_record.target == nameof(Platform.PME))//轉入PME直接改訂單狀態為成功
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
        else if (response.data == "2")
        {
            if (transfer_record.target == nameof(Platform.PME))//轉入PME直接改訂單狀態為失敗
            {
                checkTransferRecordResponse.CreditChange = transfer_record.amount;
                checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;

            }
            else if (transfer_record.source == nameof(Platform.PME))
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

    public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
    {
        if (!PMEConfig.Currency.ContainsKey(userData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);
        try
        {
            var req = new RegisterRequest()
            {
                username = _prefixKey + request.Club_id,
                password = MakePassword(_prefixKey + request.Club_id),
                tester = 0,
                currency_code = PMEConfig.Currency[userData.Currency]
            };

            var response = await _apiService.RegisterAsync(req);

            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = _prefixKey + request.Club_id;
            gameUser.game_platform = Platform.PME.ToString();
            return gameUser;
        }
        catch (ExceptionMessage ex) when (ex.Message.Contains("the username is already registered"))
        {
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = _prefixKey + request.Club_id;
            gameUser.game_platform = Platform.PME.ToString();
            return gameUser;
        }
    }

    public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!PMEConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new TransferRequest()
            {
                currency_code = PMEConfig.Currency[walletData.Currency],
                merOrderId = ConvertToNumericUuid(RecordData.id),
                amount = RecordData.amount,
                type = (int)Model.Game.PME.TransferType.Deposit,
                username = platform_user.game_user_id
            };

            var response = await _apiService.TransferAsync(req);

            RecordData.status = nameof(TransferStatus.success);
        }
        catch (ExceptionMessage ex) when (ex.Message.Contains("illegal merOrderId") || ex.Message.Contains("merOrderId duplicate"))
        {
            RecordData.status = nameof(TransferStatus.fail);
            _logger.LogError(ex, "{paltform} Deposit fail ex : {ex}", Platform.PME, ex);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Deposit fail ex : {ex}", Platform.PME, ex);
        }

        return RecordData.status;
    }

    public async Task<string> GameDetailURL(GetBetDetailReq request)
    {
        throw new NotImplementedException();
    }

    public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
    {
        GetBetRecord res = new();
        var bettimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
        var batRecords = new GetBetRecord();
        List<dynamic> pme_results = new List<dynamic>();
        foreach (var createTime in bettimePair)
        {
            var results = await _pmedbService.GetPmeRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
            results = results.OrderByDescending(e => e.bet_time).ToList();
            pme_results.AddRange(results);
        }
        if (!pme_results.Any())
        {
            pme_results.AddRange(await _pmedbService.GetPMERecordsBySummary(RecordReq));
        }

        res.Data = pme_results.OrderByDescending(e => e.bet_time).Select(x => x).ToList();
        return res;
    }

    public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
    {
        GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
        IEnumerable<dynamic> pme_results = await _pmedbService.GetPMERunningRecord(RecordReq);
        pme_results = pme_results.OrderByDescending(e => e.update_time);
        res.Data = pme_results.ToList();
        return res;
    }

    public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
    {
        var memberBalance = new MemberBalance();
        try
        {
            var req = new GetBalanceRequest()
            {
                username = platform_user.game_user_id
            };

            var result = await _apiService.GetBalanceAsync(req);

            memberBalance.Amount = decimal.Parse(result.data);
        }
        catch (Exception ex)
        {
            memberBalance.Amount = 0;
            memberBalance.code = (int)ResponseCode.Fail;
            memberBalance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
            _logger.LogError(ex, "{platform} 餘額取得失敗 Msg: {Message}", Platform.PME, ex.Message);
        }

        memberBalance.Wallet = nameof(Platform.PME);
        return memberBalance;
    }

    public async Task<bool> KickAllUser(Platform platform)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
    {
        try
        {
            var req = new KickRequest()
            {
                username = platform_user.game_user_id
            };
            var rep = await _apiService.KickAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError("PME踢線失敗 Msg: {Message}", ex.Message);
            throw new ExceptionMessage(ResponseCode.Fail, "PME " + ex.Message.ToString());
        }
        return true;
    }

    public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
    {
        try
        {
            PMEConfig.Lang.TryGetValue(request.GameConfig["lang"], out var lang);
            lang ??= PMEConfig.Lang["en-US"];

            var req = new LoginRequest()
            {
                username = platformUser.game_user_id,
                password = MakePassword(platformUser.game_user_id),
                client_ip = ConvertClientIPToLong(request.GameConfig["clientIP"])
            };

            var result = await _apiService.LoginAsync(req);

            var url = request.GameConfig["device"] == "DESKTOP" ? result.data.pc : result.data.h5;
            var langIndex = url.IndexOf("lang=");
            var langEndIndex = langIndex + url[langIndex..].IndexOf("&");
            url = url[..langIndex] + $"lang={lang}" + url[langEndIndex..];

            return url;
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.GetGameURLFail, ex.Message.ToString());
        }
    }

    public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!PMEConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new TransferRequest()
            {
                currency_code = PMEConfig.Currency[walletData.Currency],
                merOrderId = ConvertToNumericUuid(RecordData.id),
                amount = RecordData.amount,
                type = (int)Model.Game.PME.TransferType.Withdraw,
                username = platform_user.game_user_id
            };

            var response = await _apiService.TransferAsync(req);

            RecordData.status = nameof(TransferStatus.success);
        }
        catch (ExceptionMessage ex) when (ex.Message.Contains("illegal merOrderId") || ex.Message.Contains("merOrderId duplicate"))
        {
            RecordData.status = nameof(TransferStatus.fail);
            _logger.LogError(ex, "{paltform} Deposit fail ex : {ex}", Platform.PME, ex);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Withdraw fail ex : {ex}", Platform.PME, ex);
        }

        return RecordData.status;
    }

    public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
    {
        #region 補注單
        var start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
        var maxEnd = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, 0);

        var offSet = TimeSpan.FromMinutes(30);

        var postResult = 0;

        while (start < maxEnd)
        {
            var end = start.Add(offSet);
            if (end > maxEnd)
            {
                end = maxEnd;
            }

            var betInfos = await GetPMERecord(start, end);

            if (betInfos.Any())
            {
                foreach (var group in betInfos.GroupBy(b => b.bet_time / TimeSpan.FromHours(3).TotalMilliseconds))
                {
                    postResult += await PostPMERecord(group);
                }
            }


            start = end;
        }
        #endregion
        await Task.Delay(TimeSpan.FromSeconds(1));
        #region 重產匯總帳
        var ReportScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(PMEReportSchedule.SYSTEM_PARAMETERS_KEY)).value);

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
        return $"Game: {Platform.PME} 新增資料筆數: {postResult}";
        throw new NotImplementedException();
    }

    public PlatformType GetPlatformType(Platform platform)
    {
        return PlatformType.ESport;
    }
    /// <summary>
    /// 計算彙總
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="SummaryData"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    private async Task Calculate(IDbTransaction tran, NpgsqlConnection conn, QueryScrollResponse.Bet r)
    {
        #region oldcode
        //var oldRecords = await _pmedbService.GetPMERecordsPreAmountById(tran, r.id, r.BetTimeFormatted);
        //if (oldRecords.Any())
        //{
        //    var lastRecord = oldRecords.OrderByDescending(r => r.update_time).First();

        //    r.win_amount -= lastRecord.pre_win_amount;
        //    r.bet_amount -= lastRecord.pre_bet_amount;
        //}

        //SummaryData.RecordCount++;
        //SummaryData.Bet_amount += Math.Max(decimal.Zero, r.bet_amount);
        //SummaryData.Turnover += r.bet_amount;
        //SummaryData.Netwin += r.win_amount - r.bet_amount;
        //SummaryData.Win += r.win_amount;
        //SummaryData.updatedatetime = DateTime.Now;

        //return SummaryData;
        #endregion oldcode

        var Records = await _pmedbService.GetPMEV2RecordsPreAmountById(tran, r.id, r.BetTimeFormatted);
        if (!Records.Any())
            Records = await _pmedbService.GetPMERecordsPreAmountById(tran, r.id, r.BetTimeFormatted);
        if (Records.Any())
        {
            var lastRecord = Records.OrderByDescending(r => r.update_time).First();
            r.win_amount -= lastRecord.pre_win_amount;
            r.bet_amount -= lastRecord.pre_bet_amount;
        }
    }

    public async Task<int> PostPMERecord(IEnumerable<QueryScrollResponse.Bet> betInfos)
    {
        if (!betInfos.Any()) return 0;

        var existsPK = (await _pmedbService.GetPMERecordsPKByBetTime(betInfos.Min(b => b.BetTimeFormatted), betInfos.Max(b => b.BetTimeFormatted)))
            .Select(b => new { b.id, b.bet_status, b.bet_time, b.update_time })
            .ToHashSet();

        IEnumerable<IGrouping<string, QueryScrollResponse.Bet>> linqRes = betInfos.OrderBy(x => x.update_time).GroupBy(x => x.member_account);

        var postResult = 0;
        foreach (IGrouping<string, QueryScrollResponse.Bet> logGroup in linqRes)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                using (var tran = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        var clubId = logGroup.Key[_prefixKey.Length..];
                        Wallet memberWalletData = await GetWalletCache(clubId);
                        //彙總注單
                        var dic = new Dictionary<string, HashSet<string>>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                        //4:已取消,7:已撤銷
                        var cancelStatus = new HashSet<short>() { 4, 7 };
                        List<QueryScrollResponse.Bet> betDetailData = new List<QueryScrollResponse.Bet>();

                        foreach (QueryScrollResponse.Bet r in logGroup)
                        {
                            //排除重複
                            if (!existsPK.Add(new { r.id, r.bet_status, bet_time = r.BetTimeFormatted, update_time = r.UpdateTimeFormatted })) continue;

                            r.pre_bet_amount = r.bet_amount;
                            r.pre_win_amount = r.win_amount;
                            //取消及撤銷單下注金額調整為0
                            if (cancelStatus.Contains(r.bet_status))
                                r.bet_amount = decimal.Zero;

                            r.report_time = reportTime;
                            r.partition_time = r.BetTimeFormatted;
                            await Calculate(tran, conn, r);
                            switch (r.bet_status)
                            {
                                case <= 3:
                                    r.club_id = memberWalletData.Club_id;
                                    r.franchiser_id = memberWalletData.Franchiser_id;
                                    break;
                                default:
                                    break;
                            }
                            betDetailData.Add(r);
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }

                            dic[summaryTime].Add(r.BetTimeFormatted.ToString("yyyy-MM-dd HH:mm"));
                        }

                        // 記錄到 Redis reportTime 跟 adddate(下注時間) 的關聯
                        foreach (var item in dic)
                        {
                            foreach (var subItem in item.Value)
                            {
                                var key = $"{RedisCacheKeys.PMEBetSummaryTime}:{item.Key}";
                                await _commonService._cacheDataService.ListPushAsync(key, subItem);
                            }
                        }

                        //寫入未結算單
                        if (betDetailData.Any(b => b.bet_status == 3))
                            await _pmedbService.PostPMERecordRunning(conn, tran, betDetailData.Where(b => b.bet_status == 3));

                        //寫入明細帳
                        postResult += await _pmedbService.PostPMERecord_V2(conn, tran, betDetailData);

                        //刪除已結算之未結算單
                        foreach (var settleRecord in betDetailData.Where(b => b.bet_status > 3))
                            await _pmedbService.DeletePMERecordRunning(tran, settleRecord.id, settleRecord.BetTimeFormatted);


                        await tran.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync();
                    }
                }
            }
        }
        return postResult;
    }

    //public async Task<int> PostPMERecord(IEnumerable<QueryScrollResponse.Bet> betInfos)
    //{
    //    if (!betInfos.Any()) return 0;

    //    var existsPK = (await _pmedbService.GetPMERecordsPKByBetTime(betInfos.Min(b => b.BetTimeFormatted), betInfos.Max(b => b.BetTimeFormatted)))
    //        .Select(b => new { b.id, b.bet_status, b.bet_time, b.update_time })
    //        .ToHashSet();

    //    await using var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
    //    await conn.OpenAsync();
    //    var postResult = 0;
    //    foreach (var logGroup in betInfos.GroupBy(b => b.member_account))
    //    {
    //        using var tran = await conn.BeginTransactionAsync();
    //        try
    //        {
    //            var clubId = logGroup.Key[_prefixKey.Length..];
    //            Wallet memberWalletData = await GetWalletCache(clubId);
    //            if (memberWalletData == null || memberWalletData.Club_id == null)
    //            {
    //                throw new Exception("沒有會員id");
    //            }

    //            var gameUser = await _gamePlatformUserService.GetSingleGamePlatformUserAsync(clubId, Platform.PME);
    //            if (gameUser == null || gameUser.game_user_id != logGroup.Key)
    //            {
    //                throw new Exception("No pme user");
    //            }

    //            //彙總注單
    //            Dictionary<string, BetRecordSummary> summaryData = new();

    //            //已結算注單
    //            List<QueryScrollResponse.Bet> betLogs = new();

    //            var summaryBetRecordMappings = new HashSet<t_summary_bet_record_mapping>();

    //            //4:已取消,7:已撤銷
    //            var cancelStatus = new HashSet<short>() { 4, 7 };
    //            var bets = logGroup
    //                .OrderBy(b => b.update_time)
    //                .Select(b =>
    //                {
    //                    b.club_id = memberWalletData.Club_id;
    //                    b.franchiser_id = memberWalletData.Franchiser_id;
    //                    b.pre_bet_amount = b.bet_amount;
    //                    b.pre_win_amount = b.win_amount;

    //                    //取消及撤銷單下注金額調整為0
    //                    if (cancelStatus.Contains(b.bet_status))
    //                        b.bet_amount = decimal.Zero;

    //                    return b;
    //                });

    //            foreach (var r in bets)
    //            {
    //                //排除重複
    //                if (!existsPK.Add(new { r.id, r.bet_status, bet_time = r.BetTimeFormatted, update_time = r.UpdateTimeFormatted })) continue;

    //                BetRecordSummary sumData = new();
    //                sumData.Club_id = memberWalletData.Club_id;
    //                sumData.Game_id = nameof(Platform.PME);
    //                sumData.Game_type = 0;
    //                DateTime tempDateTime = DateTime.Now;
    //                sumData.ReportDatetime = tempDateTime.AddTicks(-(tempDateTime.Ticks % TimeSpan.FromMinutes(5).Ticks));

    //                //先確認有沒有符合的匯總單
    //                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
    //                {
    //                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
    //                    //合併處理
    //                    sumData = await Calculate(tran, sumData, r);
    //                    summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
    //                }
    //                else
    //                {
    //                    //用Club_id與ReportDatetime DB取得彙總注單
    //                    IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
    //                    if (!results.Any()) //沒資料就建立新的
    //                    {
    //                        //建立新的Summary
    //                        sumData.Currency = memberWalletData.Currency;
    //                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

    //                        //合併處理
    //                        sumData = await Calculate(tran, sumData, r);
    //                    }
    //                    else //有資料就更新
    //                    {
    //                        sumData = results.SingleOrDefault();
    //                        //合併處理
    //                        sumData = await Calculate(tran, sumData, r);
    //                    }

    //                    summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
    //                }

    //                r.summary_id = sumData.id;
    //                betLogs.Add(r);

    //                summaryBetRecordMappings.Add(new()
    //                {
    //                    summary_id = sumData.id,
    //                    report_time = sumData.ReportDatetime.Value,
    //                    partition_time = r.BetTimeFormatted.Date
    //                });
    //            }

    //            //寫入匯總帳
    //            List<BetRecordSummary> summaryList = summaryData.Values.ToList();
    //            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
    //            //寫入匯總帳對應
    //            await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMappings);

    //            //寫入未結算單
    //            if (betLogs.Any(b => b.bet_status == 3))
    //                await _pmedbService.PostPMERecordRunning(tran, betLogs.Where(b => b.bet_status == 3));

    //            //寫入明細帳
    //            postResult += await _pmedbService.PostPMERecord(tran, betLogs);

    //            //刪除已結算之未結算單
    //            foreach (var settleRecord in betLogs.Where(b => b.bet_status > 3))
    //                await _pmedbService.DeletePMERecordRunning(tran, settleRecord.id, settleRecord.BetTimeFormatted);

    //            _logger.LogDebug("insert PME record member: {group}, count: {count}", logGroup.Key,
    //                betLogs.Count);

    //            await tran.CommitAsync();
    //        }
    //        catch (Exception ex)
    //        {
    //            await tran.RollbackAsync();

    //            foreach (var r in logGroup) //loop club id bet detail
    //            {
    //                _logger.LogError("record id : {id}, time: {time}", r.id, r.BetTimeFormatted);
    //            }
    //            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
    //            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
    //            _logger.LogError(ex, "Run {platform} record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
    //                Platform.PME, logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
    //        }
    //    }

    //    return postResult;
    //}

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

            _logger.LogDebug("Create PME game W1 report time {datetime}", start);

            var (totalCount, totalBetValid, totalNetWin) = await _pmedbService.SumPMEBetRecordByBetTime(start, end.AddMilliseconds(-1));

            GameReport reportData = new();
            reportData.platform = nameof(Platform.PME);
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

    public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
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

            _logger.LogDebug("Create PME game provider report time {datetime}", start);

            GameReport reportData = new();
            reportData.platform = nameof(Platform.PME);
            reportData.report_datetime = start;
            reportData.report_type = (int)GameReport.e_report_type.FinancalReport;
            reportData.total_bet = decimal.Zero;
            reportData.total_win = decimal.Zero;
            reportData.total_netwin = decimal.Zero;
            reportData.total_count = 0L;

            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);

            start = end;
        }
    }

    /// <summary>
    /// 遊戲商注單明細表 GamePlatformUser 轉換 Club Info 屬性規則
    /// 使用情境：後彙總排程從遊戲明細查詢使用者遊戲帳號 轉換 為H1的Club_Id 提供 wallet 查詢使用到
    /// </summary>
    /// <param name="propertyValue"></param>
    /// <returns></returns>
    /// <exception cref="ExceptionMessage"></exception>
    private string ConvertGamePlatformUserToClubInfo(string propertyValue)
    {
        string result = "";
        //依照環境變數調整Prefix
        int prefixLength = Config.OneWalletAPI.Prefix_Key.Length;
        result = propertyValue.Substring(prefixLength);
        return result.ToUpper();
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
        var redisKey = $"{RedisCacheKeys.PMEBetSummaryTime}:{reportDatetime.ToString("yyyy-MM-dd HH:mm")}";

        //redisKey 空值的話會變 
        var timeStringList = await _commonService._cacheDataService.ListGetAsync<string>(redisKey);

        // 使用 LINQ 提取不重複的日期
        var uniqueDates = timeStringList?.Select(date => Convert.ToDateTime(date).Date).Distinct().ToList();
        List<(int count, decimal win, decimal bet, decimal jackpot, string userid, decimal netwin, DateTime bettime)> summaryRecords = new List<(int, decimal, decimal, decimal, string, decimal, DateTime)>();

        if (uniqueDates != null && uniqueDates.Any())
        {
            foreach (DateTime item in uniqueDates)
            {
                var _summaryRecords = await _pmedbService.SummaryGameRecord(reportDatetime, item, item.AddDays(1).AddMilliseconds(-1));
                summaryRecords.AddRange(_summaryRecords);
            }
        }
        else
        {
            summaryRecords.AddRange(await _pmedbService.SummaryGameRecord(reportDatetime, reportDatetime.AddDays(-2), reportDatetime.AddHours(1).AddMilliseconds(-1)));
        }
        sw1.Stop();
        _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);
        // 取得遊戲內帳號轉為為Club_id集合
        var userSummaries = summaryRecords.GroupBy(s => s.userid);
        var userlist = userSummaries.Select(x => x.Key[_prefixKey.Length..]).Distinct().ToList();
        // 批次處理，每次1000筆
        var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
        {
            return (await _commonService._serviceDB.GetWallet(betch));
        }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);
        var summaryRecordList = new List<BetRecordSummary>();
        var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();

        foreach (var summaryRecord in userSummaries)
        {
            if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key), out var userWallet)) continue;

            var summaryData = new BetRecordSummary();
            summaryData.Turnover = summaryRecord.Sum(x => x.bet);
            summaryData.ReportDatetime = reportDatetime;
            summaryData.Currency = userWallet.Currency;
            summaryData.Club_id = userWallet.Club_id;
            summaryData.Franchiser_id = userWallet.Franchiser_id;
            summaryData.RecordCount = summaryRecord.Sum(x => x.count);
            summaryData.Game_id = nameof(Platform.PME);
            summaryData.Game_type = 0;
            summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
            summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
            summaryData.Win = summaryRecord.Sum(x => x.win);
            summaryData.Netwin = summaryRecord.Sum(x => x.netwin);
            summaryRecordList.Add(summaryData);

            foreach (var item in summaryRecord)
            {
                var mapping = new t_summary_bet_record_mapping()
                {
                    summary_id = summaryData.id,
                    report_time = reportDatetime,
                    partition_time = item.bettime
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

    public async Task<List<QueryScrollResponse.Bet>> GetPMERecord(DateTime startDateTime, DateTime endDateTime)
    {
        var betInfos = new List<QueryScrollResponse.Bet>();

        var req = new QueryScrollRequest()
        {
            start_time = DateTimeToUnixTimeSeconds(startDateTime),
            end_time = DateTimeToUnixTimeSeconds(endDateTime),
            agency = true,
            page_size = 10000
        };

        var ticketReq = new TicketOrderQueryRequest()
        {
            start_time = req.start_time,
            end_time = req.end_time,
            agency = req.agency,
            page_size = req.page_size
        };

        foreach (var currency in PMEConfig.Currency.Values)
        {
            req.currency_code = currency;
            ticketReq.currency_code = currency;

            var last_order_id = 0L;
            while (true)
            {
                req.last_order_id = last_order_id;

                var res = await _apiService.QueryScrollAsync(req);

                if (res.bet is not null)
                {
                    foreach (var bet in res.bet)
                    {
                        if (res.tournament.TryGetValue(bet.tournament_id, out var tournament))
                            bet.tournament = tournament;

                        if (res.detail.TryGetValue(bet.id, out var details))
                        {
                            bet.details = details;
                            bet.tournament = "Parlay (" + bet.details.Length + ")";
                            bet.team_en_names = " - , - ";
                        }
                    }

                    betInfos.AddRange(res.bet);
                }

                if (res.lastOrderID == 0) break; //末頁跳出

                last_order_id = res.lastOrderID;
            }

            #region 英雄召喚注單
            last_order_id = 0;
            while (true)
            {
                ticketReq.last_order_id = last_order_id;

                var res = await _apiService.TicketOrderQueryAsync(ticketReq);

                if (res.ticketOrder is not null)
                {
                    var bets = res.ticketOrder
                        .Where(b => b.bet_status > 3)//排除待結算
                        .Select(ParseTicketOrderToBet)
                        .ToList();
                    betInfos.AddRange(bets);
                }

                if (res.lastOrderID == 0) break; //末頁跳出

                last_order_id = res.lastOrderID;
            }
            #endregion
        }

        return betInfos
            .Where(l => l.member_account.ToLower().StartsWith(_prefixKey.ToLower()))
            .ToList();
    }

    public Task HealthCheck(Platform platform)
    {
        return _apiService.GetBalanceAsync(new()
        {
            username = "HealthCheck"
        });
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
    /// GUID 轉為 數字字串
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    private static string ConvertToNumericUuid(Guid uuid)
    {
        byte[] hashBytes = Sha256Encrypt(uuid.ToByteArray());
        string hexString = BitConverter.ToString(hashBytes).Replace("-", "");

        // Convert the hexadecimal string to a BigInteger
        BigInteger number = BigInteger.Parse(hexString, System.Globalization.NumberStyles.HexNumber);

        // Limit the number to the desired digit count range (30 to 32 bits)
        BigInteger minNumber = BigInteger.Pow(10, 30);
        BigInteger maxNumber = BigInteger.Pow(10, 32);
        BigInteger numericUuid = BigInteger.Remainder(number, maxNumber - minNumber) + minNumber;
        numericUuid = BigInteger.Abs(numericUuid);

        return numericUuid.ToString();
    }

    public static string MakePassword(string username)
    {
        var originStr = $"{username}&8+8#8&8%8!";
        var hashBytes = Sha256Encrypt(Encoding.UTF8.GetBytes(originStr));
        return BitConverter.ToString(hashBytes).Replace("-", "")[..30];
    }

    private static byte[] Sha256Encrypt(byte[] data)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(data);
        return hashBytes;
    }

    private static long DateTimeToUnixTimeSeconds(DateTime dt)
    {
        var unixSeconds = new DateTimeOffset(dt).ToUnixTimeSeconds();
        return unixSeconds;
    }

    private static QueryScrollResponse.Bet ParseTicketOrderToBet(TicketOrderQueryResponse.TicketOrder ticket)
    {
        var result = new QueryScrollResponse.Bet()
        {
            id = ticket.order_id,
            member_account = ticket.member_account,
            member_id = ticket.member_id,
            merchant_id = ticket.merchant_id,
            merchant_account = ticket.merchant_account,
            parent_merchant_account = ticket.parent_merchant_account,
            parent_merchant_id = ticket.parent_merchant_id,
            tester = ticket.tester,
            order_type = ticket.chase_order + 1000, //1000-英雄召喚普通注單(自訂)、1001-英雄召喚追號注單(自訂)
            parley_type = 1,
            game_id = ticket.game_id,
            tournament_id = ticket.game_id,
            tournament = ticket.ticket_name_en,
            match_type = 1001,  // 英雄召喚-1001(自訂)
            market_cn_name = ticket.play_name_cn,
            team_en_names = " - , - ",
            odd_name = ticket.bet_content_en,
            odd = ticket.odd,
            bet_amount = ticket.bet_amount,
            win_amount = ticket.win_amount,
            bet_status = ticket.bet_status,
            bet_time = ticket.bet_time,
            settle_time = ticket.settle_time,
            match_start_time = ticket.plan_sales_start_time,
            update_time = ticket.update_time,
            settle_count = 1,
            device = ticket.device,
            currency_code = ticket.currency_code,
            exchange_rate = ticket.exchange_rate
        };

        return result;
    }



    #region ConvertClientIPTolong
    public static long ConvertClientIPToLong(string clientIP)
    {
        long longValue = 0;
        BigInteger bigInteger = new BigInteger(-1);
        if (IsIPv6(clientIP))
        {
            bigInteger = ConvertIPv6ToBigInteger(clientIP);

            // Check if BigInteger is within the range of a long
            if (bigInteger >= long.MinValue && bigInteger <= long.MaxValue)
            {
                return longValue = (long)bigInteger;
                Console.WriteLine($"Converted BigInteger to long: {longValue}");
            }
            else
            {
                Console.WriteLine("BigInteger is out of the range of a long.");
            }
        }
        else if (IsIPv4(clientIP))
        {
            return long.Parse(clientIP.Replace(".", string.Empty));
        }
        else
        {
            //throw new ArgumentException("Invalid IP address format");
        }
        return longValue;
    }

    public static bool IsIPv6(string ip)
    {
        return IPAddress.TryParse(ip, out IPAddress address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
    }

    public static bool IsIPv4(string ip)
    {
        return IPAddress.TryParse(ip, out IPAddress address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    public static BigInteger ConvertIPv4ToBigInteger(string ipv4Address)
    {
        IPAddress ipAddress = IPAddress.Parse(ipv4Address);
        byte[] bytes = ipAddress.GetAddressBytes();

        // Convert the 4-byte array to a BigInteger (IPv4 is 32 bits)
        // Pad the 4-byte IPv4 address to make it fit into 128 bits by adding 0s in front (IPv6)
        byte[] paddedBytes = new byte[16];  // 128 bits for IPv6
        Array.Copy(bytes, 0, paddedBytes, 12, bytes.Length);  // Place IPv4 bytes in the last 4 bytes of the 128-bit array

        return new BigInteger(paddedBytes);
    }

    public static BigInteger ConvertIPv6ToBigInteger(string ipv6Address)
    {
        IPAddress ipAddress = IPAddress.Parse(ipv6Address);
        byte[] bytes = ipAddress.GetAddressBytes();

        // IPv6 is already 128 bits, so just return the BigInteger directly
        return new BigInteger(bytes);
    }
    #endregion

}
