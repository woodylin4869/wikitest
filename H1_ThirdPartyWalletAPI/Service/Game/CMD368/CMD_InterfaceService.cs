using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Request;
using H1_ThirdPartyWalletAPI.Model.Game.CMD368.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Worker.Game.CMD368;
using H1_ThirdPartyWalletAPI.Worker.Game.NEXTSPIN;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using CMDConfig = H1_ThirdPartyWalletAPI.Model.Game.CMD368.CMD368;


namespace H1_ThirdPartyWalletAPI.Service.Game.CMD368;

public interface ICMDInterfaceService : IGameInterfaceService
{
    Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    Task<List<BetRecordResponse.Daet>> GetCMDRecord(long version);
    Task<int> PostCMDRecord(IEnumerable<BetRecordResponse.Daet> betInfos);
    Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
}

public class CMD_InterfaceService : ICMDInterfaceService
{
    private readonly ILogger<CMD_InterfaceService> _logger;
    private readonly ICMDApiService _CMDApiService;
    private readonly ICommonService _commonService;
    private readonly IDBService _dbService;
    private readonly ICMDDBService _cmdDBService;
    private readonly ISummaryDBService _summaryDBService;
    private readonly ICacheDataService _cacheService;
    private readonly IMemoryCache _memoryCache;
    private readonly IGamePlatformUserService _gamePlatformUserService;
    private readonly IGameReportDBService _gameReportDBService;
    private readonly ISystemParameterDbService _systemParameterDbService;

    private const int _cacheSeconds = 600;
    private const int _cacheFranchiserUser = 1800;

    private readonly string _prefixKey;
    private readonly string _partnerkey;



    public CMD_InterfaceService(ILogger<CMD_InterfaceService> logger,
                                ICMDApiService CMDApiService,
                                IDBService dbService,
                                ICMDDBService cmdDBService,
                                ICacheDataService cacheService,
                                IMemoryCache memoryCache,
                                IGamePlatformUserService gamePlatformUserService,
                                ISummaryDBService summaryDBService,
                                IGameReportDBService gameReportDBService,
                                ISystemParameterDbService systemParameterDbService,
                                ICommonService commonService)
    {
        _logger = logger;
        _CMDApiService = CMDApiService;
        _dbService = dbService;
        _cmdDBService = cmdDBService;
        _cacheService = cacheService;
        _summaryDBService = summaryDBService;
        _memoryCache = memoryCache;
        _gameReportDBService = gameReportDBService;
        _prefixKey = Config.OneWalletAPI.Prefix_Key;
        _partnerkey = Config.CompanyToken.CMD368_MerchantCode;
        _gamePlatformUserService = gamePlatformUserService;
        _systemParameterDbService = systemParameterDbService;
        _commonService = commonService;
    }

    public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
    {
        var checkTransferRecordResponse = new CheckTransferRecordResponse();

        var gameUser = await _gamePlatformUserService.GetSingleGamePlatformUserAsync(transfer_record.Club_id, Platform.CMD368);

        var response = new GetWDTResponse();
        var req = new GetWDTRequest()
        {
            PartnerKey = _partnerkey,
            UserName = gameUser.game_user_id,
            TicketNo = transfer_record.id.ToString()
        };

        response = await _CMDApiService.GetWDTAsync(req);

        var successed = false;
        successed = response.Data.Any(d => d.TicketNo == transfer_record.id.ToString() && d.Status == 1);

        if (successed)
        {
            if (transfer_record.target == nameof(Platform.CMD368))//轉入CMD直接改訂單狀態為成功
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
        else
        {
            if (transfer_record.target == nameof(Platform.CMD368))//轉入CMD直接改訂單狀態為失敗
            {
                checkTransferRecordResponse.CreditChange = transfer_record.amount;
                checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;

            }
            else if (transfer_record.source == nameof(Platform.CMD368))
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
        if (!CMDConfig.Currency.ContainsKey(userData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        var req = new GetCreateUserRequest()
        {
            Username = _prefixKey + request.Club_id,
            Partnerkey = _partnerkey,
            CurrencyCode = CMDConfig.Currency[userData.Currency]
        };

        await _CMDApiService.RegisterAsync(req);

        var gameUser = new GamePlatformUser();
        gameUser.club_id = userData.Club_id;
        gameUser.game_user_id = req.Username;
        gameUser.game_platform = Platform.CMD368.ToString();
        return gameUser;
    }

    public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!CMDConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new DepositRequest()
            {
                PaymentType = 1,
                TicketNo = RecordData.id.ToString(),
                PartnerKey = _partnerkey,
                Money = Math.Round(RecordData.amount, 2),
                UserName = platform_user.game_user_id
            };

            var response = await _CMDApiService.DepositAsync(req);

            RecordData.status = nameof(TransferStatus.success);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Deposit fail ex : {ex}", Platform.CMD368, ex);
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
        var batRecords = new List<dynamic>();
        foreach (var betTime in bettimePair)
        {
            var cmdRecords = await _cmdDBService.GetCMDRecordsBySummary(new()
            {
                summary_id = RecordReq.summary_id,
                ReportTime = betTime
            });

            foreach (var record in cmdRecords)
            {
                if (record.TransType == "PAR")
                {
                    var parlayBetRecord = await _CMDApiService.ParlayBetRecordAsync(new()
                    {
                        PartnerKey = _partnerkey,
                        SocTransID = (int)record.SocTransId,
                    });

                    record.LeagueEn = $"Parlay({parlayBetRecord.Data.Length})";
                    record.HomeTeamEn = " - ";
                    record.AwayTeamEn = " - ";
                }
                else
                {
                    record.LeagueEn = await GetCMDLeagueEnAsync(record.LeagueId);
                    record.HomeTeamEn = await GetCMDTeamEnAsync(record.HomeTeamId);
                    record.AwayTeamEn = await GetCMDTeamEnAsync(record.AwayTeamId);
                }

                if (record.WinLoseStatus == "P") record.StateUpdateTs = null;
            }

            batRecords.AddRange(cmdRecords);
        }

        batRecords = batRecords.OrderByDescending(e => e.TransDate).ToList();
        res.Data = batRecords.ToList();
        return res;
    }

    public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
    {
        GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
        var pme_results = await _cmdDBService.GetCMDRunningRecord(RecordReq);
        foreach (var record in pme_results)
        {
            if (record.TransType == "PAR")
            {
                var parlayBetRecord = await _CMDApiService.ParlayBetRecordAsync(new()
                {
                    PartnerKey = _partnerkey,
                    SocTransID = (int)record.SocTransId,
                });

                record.LeagueEn = $"Parlay({parlayBetRecord.Data.Length})";
                record.HomeTeamEn = " - ";
                record.AwayTeamEn = " - ";
            }
            else
            {
                record.LeagueEn = await GetCMDLeagueEnAsync(record.LeagueId);
                record.HomeTeamEn = await GetCMDTeamEnAsync(record.HomeTeamId);
                record.AwayTeamEn = await GetCMDTeamEnAsync(record.AwayTeamId);
            }

            if (record.WinLoseStatus == "P") record.StateUpdateTs = null;
        }
        pme_results = pme_results.OrderByDescending(e => e.StateUpdateTs).ToList();
        res.Data = new List<dynamic>(pme_results);
        return res;
    }

    public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
    {
        var memberBalance = new MemberBalance();
        try
        {
            var req = new BalanceRequest()
            {
                UserName = platform_user.game_user_id,
                PartnerKey = _partnerkey,
            };

            var result = await _CMDApiService.BalanceAsync(req);

            memberBalance.Amount = result.Data.First().BetAmount;
        }
        catch (Exception ex)
        {
            memberBalance.Amount = 0;
            memberBalance.code = (int)ResponseCode.Fail;
            memberBalance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
            _logger.LogError(ex, "{platform} 餘額取得失敗 Msg: {Message}", Platform.CMD368, ex.Message);
        }

        memberBalance.Wallet = nameof(Platform.CMD368);
        return memberBalance;
    }

    public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
    {
        try
        {
            var result = await _CMDApiService.KickAsync(new KickRequest()
            {
                PartnerKey = _partnerkey,
                UserName = platform_user.game_user_id
            });
        }
        catch (Exception ex)
        {
            _logger.LogInformation("KickUser 踢出CMD使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
        }
        return true;
    }

    public async Task<bool> KickAllUser(Platform platform)
    {
        try
        {
            var result = await _CMDApiService.KickAllAsync(new KickAllRequest()
            {
                PartnerKey = _partnerkey,
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("KickAllUser CMD user fail MSG : {Message}", ex.Message);
            throw new ExceptionMessage((int)ResponseCode.KickUserFail, MessageCode.Message[(int)ResponseCode.KickUserFail]);
        }
    }
    public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
    {
        if (!CMDConfig.Currency.ContainsKey(userData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            CMDConfig.lang.TryGetValue(request.GameConfig["lang"], out var lang);
            lang ??= CMDConfig.lang["en-US"];

            var token = (Guid.NewGuid().ToString());

            var storeTokenTask = _cacheService.StringSetAsync($"{RedisCacheKeys.LoginToken}:{Platform.CMD368}:{token}", platformUser.game_user_id, (int)TimeSpan.FromMinutes(15).TotalSeconds);
            var domain = Config.GameAPI.CMD368_FORWARDGAME_URL;

            await storeTokenTask;

            return $"{domain}/auth.aspx?lang={lang}&user={platformUser.game_user_id}&token={token}&currency={CMDConfig.Currency[userData.Currency]}";
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.GetGameURLFail, ex.Message.ToString());
        }
    }

    public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!CMDConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new WithdrawRequest()
            {
                PaymentType = 0,
                TicketNo = RecordData.id.ToString(),
                PartnerKey = _partnerkey,
                Money = Math.Round(RecordData.amount, 2),
                UserName = platform_user.game_user_id
            };

            var response = await _CMDApiService.WithdrawAsync(req);

            RecordData.status = nameof(TransferStatus.success);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Withdraw fail ex : {ex}", Platform.CMD368, ex);
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

            var betInfos = await GetCMDRecord(start, end);

            if (betInfos.Any())
            {
                foreach (var group in betInfos.GroupBy(b => b.TransDate / TimeSpan.FromHours(3).Ticks))
                {
                    postResult += await PostCMDRecord(group);
                }
            }


            start = end;
        }
        #endregion
        await Task.Delay(TimeSpan.FromSeconds(1));
        #region 重產匯總帳
        var ReportScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(CMDReportSchedule.SYSTEM_PARAMETERS_KEY)).value);


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
        return $"Game: {Platform.CMD368} 新增資料筆數: {postResult}";
        throw new NotImplementedException();
    }
    public async Task<ResCodeBase> SetLimit(SetLimitReq request, GamePlatformUser gameUser, Wallet memberWalletData)
    {
        if (gameUser == null)
        {
            gameUser = await CreateGameUser(
                new ForwardGameReq
                {
                    Platform = request.Platform, 
                    Club_id = memberWalletData.Club_id
                }, memberWalletData);
            await _gamePlatformUserService.PostGamePlatformUserAsync(gameUser);
        }

        LimitRequest CMDEditLimit = new LimitRequest
        {
            PartnerKey = Config.CompanyToken.CMD368_MerchantCode,
            UserName = gameUser.game_user_id,
            TemplateName = request.bet_setting.ToString(),
        };
        var res = new ResCodeBase();

        var CMDreq = await _CMDApiService.LimitAsync(CMDEditLimit);
        if (CMDreq.Code != (int)Model.Game.CMD368.CMD368.error_code.successed)
        {
            res.code = (int)ResponseCode.SetLimitFail;
            res.Message = CMDreq.Message;
            return res;
        }
        res.code = (int)ResponseCode.Success;
        res.Message = MessageCode.Message[(int)ResponseCode.Success];
        return res;
    }

    public PlatformType GetPlatformType(Platform platform)
    {
        return PlatformType.Sport;
    }

    public async Task<int> PostCMDRecord(IEnumerable<BetRecordResponse.Daet> betInfos)
    {
        if (!betInfos.Any()) return 0;

        //排除舊資料
        var existsPK = (await _cmdDBService.GetCMDRecordsPKByBetTime(betInfos.Min(b => b.TransDateFormatted), betInfos.Max(b => b.TransDateFormatted)))
            .Select(b => new { b.ReferenceNo, b.WinLoseStatus, b.TransDate, b.StateUpdateTs })
            .ToHashSet();

        await using var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
        await conn.OpenAsync();
        var postResult = 0;
        foreach (var logGroup in betInfos.GroupBy(b => b.SourceName))
        {
            using var tran = await conn.BeginTransactionAsync();
            try
            {
                var clubId = logGroup.Key[_prefixKey.Length..];
                Wallet memberWalletData = await GetWalletCache(clubId);
               
                //已結算注單
                List<BetRecordResponse.Daet> betLogs = new();

                // 紀錄 reportTime 跟 adddate(下注時間) 的關聯
                var dic = new Dictionary<string, HashSet<string>>();
                var dt = DateTime.Now;
                var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                var acceptedStatus = new HashSet<string>() { "N", "A", "C", "R" };
                //C:已取消 R:已拒絕
                var cancelStatus = new HashSet<string>() { "C", "R" };
                var bets = logGroup
                    .Where(b => acceptedStatus.Contains(b.DangerStatus))
                    .OrderBy(b => b.StateUpdateTs)
                    .Select(b =>
                    {
                        b.club_id = memberWalletData.Club_id;
                        b.franchiser_id = memberWalletData.Franchiser_id;

                        //取消及撤銷單下注金額調整為0
                        if (cancelStatus.Contains(b.DangerStatus))
                            b.BetAmount = decimal.Zero;

                        //買單將狀態改為自訂CashOut
                        if (b.IsCashOut)
                            b.WinLoseStatus = "CO";

                        //拒絕單狀態改為自訂Reject
                        if (b.DangerStatus == "R")
                            b.WinLoseStatus = "RJ";

                        //因CMD368WinAmount為淨輸贏，因應H1特規，未結算淨輸贏需扣掉下注金額
                        if (b.WinLoseStatus == "P") 
                        {
                            if (b.Odds < 0)
                            {
                                if (b.OddsType == "US")
                                {
                                    b.WinAmount = Convert.ToDecimal(b.Odds) / 100 * (b.BetAmount);
                                }
                                else
                                {
                                    b.WinAmount = Convert.ToDecimal(b.Odds) * (b.BetAmount);
                                }
                            }
                            else
                            {
                                b.WinAmount -= b.BetAmount;
                            }
                        }

                        if (b.Odds < 0)
                        {
                            if (b.OddsType == "US")
                            {
                                b.validbet += Convert.ToDecimal(Math.Abs(b.Odds)) / 100 * b.BetAmount;
                            }
                            else
                                //馬來盤實投量要計算實際扣款額度
                                b.validbet += Convert.ToDecimal(Math.Abs(b.Odds)) * b.BetAmount;
                        }
                        else
                        {
                            b.validbet += b.BetAmount;
                        }


                        b.pre_betamount = b.BetAmount;
                        b.pre_winamount = b.WinAmount;
                        return b;
                    });
              
                foreach (var r in bets)
                {
                    //排除重複
                    if (!existsPK.Add(new { r.ReferenceNo, r.WinLoseStatus, TransDate = r.TransDateFormatted, StateUpdateTs = r.StateUpdateTsFormatted })) continue;

                    r.partition_time = r.TransDateFormatted;
                    r.report_time = reportTime;

                    await Calculate(tran, conn, r);

                    r.club_id = memberWalletData.Club_id;
                    r.franchiser_id = memberWalletData.Franchiser_id;
                    betLogs.Add(r);

                    var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                    if (!dic.ContainsKey(summaryTime))
                    {
                        dic.Add(summaryTime, new HashSet<string>());
                    }

                    dic[summaryTime].Add(r.TransDateFormatted.ToString("yyyy-MM-dd HH:mm"));
                }
                // 記錄到 Redis reportTime 跟 adddate(下注時間) 的關聯
                foreach (var item in dic)
                {
                    foreach (var subItem in item.Value)
                    {
                        var key = nameof(Platform.CMD368) + $"{RedisCacheKeys.BetSummaryTime}:{item.Key}";
                        await _commonService._cacheDataService.ListPushAsync(key, subItem);
                    }
                }
                //寫入未結算單
                if (betLogs.Any(b => b.WinLoseStatus == "P"))
                    await _cmdDBService.PostCMDRecordRunning(conn,tran, betLogs.Where(b => b.WinLoseStatus == "P"));

                //寫入明細帳
                postResult += await _cmdDBService.PostCMDRecord(conn,tran, betLogs);

                //刪除已結算之未結算單
                foreach (var settleRecord in betLogs.Where(b => b.WinLoseStatus != "P").DistinctBy(b => b.ReferenceNo))
                    await _cmdDBService.DeleteCMDRecordRunning(tran, settleRecord.ReferenceNo, settleRecord.TransDateFormatted);

                _logger.LogDebug("insert CMD368 record member: {group}, count: {count}", logGroup.Key,
                    betLogs.Count);

                await tran.CommitAsync();

            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();

                foreach (var r in logGroup) //loop club id bet detail
                {
                    _logger.LogError("record referenceno : {referenceno}, time: {time}", r.ReferenceNo, r.TransDateFormatted);
                }
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "Run {platform} record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                    Platform.CMD368, logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
            }
        }

        return postResult;
    }

    //public async Task<int> PostCMDRecordback(IEnumerable<BetRecordResponse.Daet> betInfos)
    //{
    //    if (!betInfos.Any()) return 0;

    //    var existsPK = (await _cmdDBService.GetCMDRecordsPKByBetTime(betInfos.Min(b => b.TransDateFormatted), betInfos.Max(b => b.TransDateFormatted)))
    //        .Select(b => new { b.ReferenceNo, b.WinLoseStatus, b.TransDate, b.StateUpdateTs })
    //        .ToHashSet();

    //    await using var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
    //    await conn.OpenAsync();
    //    var postResult = 0;
    //    foreach (var logGroup in betInfos.GroupBy(b => b.SourceName))
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

    //            var gameUser = await _gamePlatformUserService.GetSingleGamePlatformUserAsync(clubId, Platform.CMD368);
    //            if (gameUser == null || gameUser.game_user_id != logGroup.Key)
    //            {
    //                throw new Exception("No cmd user");
    //            }

    //            //彙總注單
    //            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();

    //            //已結算注單
    //            List<BetRecordResponse.Daet> betLogs = new();

    //            var summaryBetRecordMappings = new HashSet<t_summary_bet_record_mapping>();

    //            var acceptedStatus = new HashSet<string>() { "N", "A", "C", "R" };
    //            //C:已取消 R:已拒絕
    //            var cancelStatus = new HashSet<string>() { "C", "R" };
    //            var bets = logGroup
    //                .Where(b => acceptedStatus.Contains(b.DangerStatus))
    //                .OrderBy(b => b.StateUpdateTs)
    //                .Select(b =>
    //                {
    //                    b.club_id = memberWalletData.Club_id;
    //                    b.franchiser_id = memberWalletData.Franchiser_id;

    //                    //取消及撤銷單下注金額調整為0
    //                    if (cancelStatus.Contains(b.DangerStatus))
    //                        b.BetAmount = decimal.Zero;

    //                    //買單將狀態改為自訂CashOut
    //                    if (b.IsCashOut)
    //                        b.WinLoseStatus = "CO";

    //                    //拒絕單狀態改為自訂Reject
    //                    if (b.DangerStatus == "R")
    //                        b.WinLoseStatus = "RJ";

    //                    //因CMD368WinAmount為淨輸贏，因應H1特規，未結算淨輸贏需扣掉下注金額
    //                    if (b.WinLoseStatus == "P")
    //                    {
    //                        if (b.Odds < 0)
    //                        {
    //                            if (b.OddsType == "US")
    //                            {
    //                                b.WinAmount = Convert.ToDecimal(b.Odds) / 100 * (b.BetAmount);
    //                            }
    //                            else
    //                            {
    //                                b.WinAmount = Convert.ToDecimal(b.Odds) * (b.BetAmount);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            b.WinAmount -= b.BetAmount;
    //                        }
    //                    }

    //                    b.pre_betamount = b.BetAmount;
    //                    b.pre_winamount = b.WinAmount;
    //                    return b;
    //                });

    //            foreach (var r in bets)
    //            {
    //                //排除重複
    //                if (!existsPK.Add(new { r.ReferenceNo, r.WinLoseStatus, TransDate = r.TransDateFormatted, StateUpdateTs = r.StateUpdateTsFormatted })) continue;

    //                BetRecordSummary sumData = new();
    //                sumData.Club_id = memberWalletData.Club_id;
    //                sumData.Game_id = nameof(Platform.CMD368);
    //                sumData.Game_type = 0;
    //                DateTime tempDateTime = DateTime.Now.ToLocalTime();
    //                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
    //                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
    //                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
    //                sumData.ReportDatetime = tempDateTime;

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

    //                var mapping = new t_summary_bet_record_mapping()
    //                {
    //                    summary_id = sumData.id,
    //                    report_time = sumData.ReportDatetime.Value,
    //                    partition_time = r.TransDateFormatted.Date
    //                };
    //                summaryBetRecordMappings.Add(mapping);
    //            }

    //            //寫入匯總帳
    //            List<BetRecordSummary> summaryList = summaryData.Values.ToList();
    //            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
    //            //寫入匯總帳對應
    //            await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMappings);

    //            //寫入未結算單
    //            if (betLogs.Any(b => b.WinLoseStatus == "P"))
    //                await _cmdDBService.PostCMDRecordRunning(tran, betLogs.Where(b => b.WinLoseStatus == "P"));

    //            //寫入明細帳
    //            postResult += await _cmdDBService.PostCMDRecord(tran, betLogs);

    //            //刪除已結算之未結算單
    //            foreach (var settleRecord in betLogs.Where(b => b.WinLoseStatus != "P").DistinctBy(b => b.ReferenceNo))
    //                await _cmdDBService.DeleteCMDRecordRunning(tran, settleRecord.ReferenceNo, settleRecord.TransDateFormatted);

    //            _logger.LogDebug("insert CMD368 record member: {group}, count: {count}", logGroup.Key,
    //                betLogs.Count);

    //            await tran.CommitAsync();
    //        }
    //        catch (Exception ex)
    //        {
    //            await tran.RollbackAsync();

    //            foreach (var r in logGroup) //loop club id bet detail
    //            {
    //                _logger.LogError("record referenceno : {referenceno}, time: {time}", r.ReferenceNo, r.TransDateFormatted);
    //            }
    //            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
    //            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
    //            _logger.LogError(ex, "Run {platform} record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
    //                Platform.CMD368, logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
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

            _logger.LogDebug("Create CMD368 game W1 report time {datetime}", start);

            var (totalCount, totalBetValid, totalNetWin) = await _cmdDBService.SumCMDBetRecordByBetTime(start, end);

            GameReport reportData = new();
            reportData.platform = nameof(Platform.CMD368);
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


    public async Task<List<BetRecordResponse.Daet>> GetCMDRecord(DateTime startDateTime, DateTime endDateTime)
    {
        var betInfos = new List<BetRecordResponse.Daet>();

        var req = new BetRecordByDateRequest()
        {
            PartnerKey = _partnerkey,
            StartDate = startDateTime,
            EndDate = endDateTime,
            TimeType = 1,
            Version = 0
        };

        while (true)
        {

            var res = await _CMDApiService.BetRecordByDateAsync(req);

            if (res.Data is not null)
            {
                betInfos.AddRange(res.Data);
            }

            if (res.Data.Length < 1000) break; //末頁跳出

            req.Version = res.Data.Max(d => d.Id);
        }

        return betInfos
                    .Where(l => l.SourceName.StartsWith(_prefixKey))
                    .ToList();
    }

    public async Task<List<BetRecordResponse.Daet>> GetCMDRecord(long version)
    {
        var betInfos = new List<BetRecordResponse.Daet>();

        var req = new BetRecordRequest()
        {
            PartnerKey = _partnerkey,
            Version = version
        };

        while (true)
        {

            var res = await _CMDApiService.BetRecordAsync(req);
            if (res.Data is not null)
            {
                betInfos.AddRange(res.Data);
            }

            if (res.Data.Length < 1000) break; //末頁跳出

            req.Version = res.Data.Max(d => d.Id);
        }

        return betInfos
                    .Where(l => l.SourceName.StartsWith(_prefixKey))
                    .ToList();
    }

    public Task HealthCheck(Platform platform)
    {
        return _CMDApiService.BalanceAsync(new()
        {
            UserName = "HealthCheck",
            PartnerKey = _partnerkey,
        });
    }

    /// <summary>
    /// 計算彙總
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="SummaryData"></param>
    /// <param name="r"></param>
    /// <returns></returns> 
    private async Task Calculate(IDbTransaction tran, NpgsqlConnection conn, BetRecordResponse.Daet r)
    {
        #region back
        //var oldRecords = await _cmdDBService.GetCMDRecordsPreAmountById(tran, r.ReferenceNo, r.TransDateFormatted);
        //if (oldRecords.Any())
        //{
        //    var lastRecord = oldRecords.OrderByDescending(r => r.StateUpdateTs).First();

        //    r.WinAmount -= lastRecord.pre_winamount;
        //    r.BetAmount -= lastRecord.pre_betamount;
        //}

        //SummaryData.RecordCount++;
        //SummaryData.Bet_amount += Math.Max(decimal.Zero, r.BetAmount);
        //if (r.Odds < 0 )
        //{
        //    if (r.OddsType == "US")
        //    {
        //        SummaryData.Turnover += Convert.ToDecimal(Math.Abs(r.Odds)) / 100 * r.BetAmount;
        //        SummaryData.Win += r.WinAmount + Convert.ToDecimal(r.Odds) /100 * r.BetAmount;
        //    }
        //    else
        //    //馬來盤實投量要計算實際扣款額度
        //    SummaryData.Turnover += Convert.ToDecimal(Math.Abs(r.Odds)) * r.BetAmount;
        //    SummaryData.Win += r.WinAmount + Convert.ToDecimal(r.Odds)* r.BetAmount;
        //}
        //else
        //{
        //    SummaryData.Turnover += r.BetAmount;
        //    SummaryData.Win += r.WinAmount + r.BetAmount;
        //} 
        //    SummaryData.Netwin += r.WinAmount;
        //SummaryData.updatedatetime = DateTime.Now;

        //return SummaryData;
        #endregion

        var Records = await _cmdDBService.GetCMDRecordsV2PreAmountById(tran, r.ReferenceNo, r.TransDateFormatted);
        if (!Records.Any())
            Records = await _cmdDBService.GetCMDRecordsPreAmountById(tran, r.ReferenceNo, r.TransDateFormatted);
        if (Records.Any())
        {
            var lastRecord = Records.OrderByDescending(r => r.StateUpdateTs).First();

            r.WinAmount -= lastRecord.pre_winamount;
            r.BetAmount -= lastRecord.pre_betamount;
        }
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

         var summaryRecords = await _cmdDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
        var Groupsummary = summaryRecords.GroupBy(x => x.userid);
        sw1.Stop();
        _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

        var userlist = summaryRecords.Select(x => ConvertGamePlatformUserToClubInfo(x.userid)).Distinct().ToList();
        var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
        var summaryRecordList = new List<BetRecordSummary>();
        var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();

        foreach (var summaryRecord in Groupsummary)
        {
            if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key), out var userWallet)) continue;

            var summaryData = new BetRecordSummary();
            summaryData.Turnover = summaryRecord.Sum(x => x.betValidBet);
            summaryData.ReportDatetime = reportDatetime;
            summaryData.Currency = userWallet.Currency;
            summaryData.Club_id = userWallet.Club_id;
            summaryData.Franchiser_id = userWallet.Franchiser_id;
            summaryData.RecordCount = summaryRecord.Sum(x => x.count);
            summaryData.Game_id = nameof(Platform.CMD368);
            summaryData.Game_type = 0;
            summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
            summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
            summaryData.Win = summaryRecord.Sum(x => (x.netWin + x.betValidBet));
            summaryData.Netwin = summaryRecord.Sum(x => x.netWin);
            summaryRecordList.Add(summaryData);

            foreach (var item in summaryRecord)
            {
                var mapping = new t_summary_bet_record_mapping()
                {
                    summary_id = summaryData.id,
                    report_time = reportDatetime,
                    partition_time = item.partitionTime
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

    private string ConvertGamePlatformUserToClubInfo(string propertyValue)
    {
        string result = "";
        //依照環境變數調整Prefix
        int prefixLength = Config.OneWalletAPI.Prefix_Key.Length;
        result = propertyValue.Substring(prefixLength);
        return result.ToUpper();
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

    private Task<string> GetCMDLeagueEnAsync(long LeagueId)
    {
        return _memoryCache.GetOrCreateAsync($"CMD/LeagueId/{LeagueId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8);
            return (await _CMDApiService.LanguageInfoAsync(new()
            {
                PartnerKey = _partnerkey,
                Type = 1,
                ID = LeagueId.ToString()
            })).Data[CMDConfig.lang["en-US"]];
        });
    }

    private Task<string> GetCMDTeamEnAsync(long TeamId)
    {
        return _memoryCache.GetOrCreateAsync($"CMD/TeamId/{TeamId}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8);
            return (await _CMDApiService.LanguageInfoAsync(new()
            {
                PartnerKey = _partnerkey,
                Type = 0,
                ID = TeamId.ToString()
            })).Data[CMDConfig.lang["en-US"]];
        });
    }
}
