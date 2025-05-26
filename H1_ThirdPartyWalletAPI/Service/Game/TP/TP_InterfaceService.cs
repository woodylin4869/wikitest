using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.TP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.TP.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using NETCore.Encrypt.Shared;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;
using TPConfig = H1_ThirdPartyWalletAPI.Model.Game.TP.TP;
using TPErrorCode = H1_ThirdPartyWalletAPI.Model.Game.TP.error_code;

namespace H1_ThirdPartyWalletAPI.Service.Game.TP;


public interface ITPInterfaceService : IGameInterfaceService
{
    /// <summary>
    /// 新增 5 分鐘匯總帳
    /// </summary>
    /// <param name="recordData"></param>
    /// <returns></returns>
    Task<int> PostTpRecord(List<BetLogResponse.BetLog> recordData);
    /// <summary>
    /// 目前沒串接 新增真人注單及 5 分鐘匯總帳
    /// </summary>
    /// <param name="recordData"></param>
    /// <returns></returns>
    //Task<int> PostTpLiveRecord(List<BetLogResponse.BetLog> recordData);
    /// <summary>
    /// 新增 遊戲商小時匯總帳
    /// </summary>
    /// <returns></returns>
    Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    /// <summary>
    /// 新增 W1小時匯總帳
    /// </summary>
    /// <returns></returns>
    Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    /// <summary>
    /// 取得時間內注單
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <param name="byBetTime"></param>
    /// <returns></returns>
    Task<List<BetLogResponse.BetLog>> GetTpRecords(DateTime startDateTime, DateTime endDateTime, bool byBetTime = false);
}

public class TP_InterfaceService : ITPInterfaceService
{
    private readonly ILogger<TP_InterfaceService> _logger;
    private readonly ITPApiService _apiService;
    private readonly IDBService _dbService;
    private readonly ISummaryDBService _summaryDBService;
    private readonly ICacheDataService _cacheService;
    private readonly IGamePlatformUserService _gamePlatformUserService;
    private readonly ITpDBService _tpDbService;
    private readonly IGameReportDBService _gameReportDBService;
    private readonly ICommonService _commonService;
    private readonly string _gamehall;

    private const int _cacheSeconds = 600;
    private const int _cacheFranchiserUser = 1800;

    private readonly string _prefixKey;

    public TP_InterfaceService(ILogger<TP_InterfaceService> logger,
                               ITPApiService tpapiservice,
                               ICommonService commonService,
                               ISummaryDBService summaryDBService,
                               ITpDBService tpDbService,
                               IGameReportDBService gameReportDBService
                               )
    {
        _logger = logger;
        _apiService = tpapiservice;
        _dbService = commonService._serviceDB;
        _summaryDBService = summaryDBService;
        _cacheService = commonService._cacheDataService;
        _gamePlatformUserService = commonService._gamePlatformUserService;
        _gameReportDBService = gameReportDBService;
        _prefixKey = Config.OneWalletAPI.Prefix_Key;
        _gamehall = Config.CompanyToken.TP_Gamehall;
        _tpDbService = tpDbService;
        _commonService = commonService;
    }

    public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
    {
        var checkTransferRecordResponse = new CheckTransferRecordResponse();

        var req = new SingleTransactionRequest()
        {
            transaction_id = CutGuidTo30Characters(transfer_record.id)
        };

        var result = new TpResponse<SingleTransactionResponse>();
        try
        {
            result = await _apiService.SingleTransaction(req);
        }
        catch (ExceptionMessage ex) when (ex.MsgId == (int)TPErrorCode.Transaction_ID_not_exist)//轉帳ID不存在視為轉帳失敗
        {
            _logger.LogError("Tp CheckTransferRecord Error! | id:{id} Error:{msg}", transfer_record.id.ToString(), ex.Message);

            result.status = new()
            {
                code = 0,
                message = "success",
                timestamp = DateTime.Now.ToLocalTime().Ticks
            };

            result.data = new()
            {
                status = "failed"
            };
        }

        if (result.data.status.ToLower() == "success")
        {
            if (transfer_record.target == nameof(Platform.TP))//轉入TP直接改訂單狀態為成功
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
        else if (result.data.status.ToLower() == "failed")
        {
            if (transfer_record.target == nameof(Platform.TP))//轉入TP直接改訂單狀態為失敗
            {
                checkTransferRecordResponse.CreditChange = transfer_record.amount;
                checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;

            }
            else if (transfer_record.source == nameof(Platform.TP))
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
        if (!TPConfig.Currency.ContainsKey(userData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);
        string prefixKey = _prefixKey;
        if (request.Club_id.Length > 10)
        {
            switch (_prefixKey)
            {
                case "Local":
                    prefixKey = "d";
                    break;
                case "dev":
                    prefixKey = "d";
                    break;
                case "uat":
                    prefixKey = "u";
                    break;
                case "prd":
                    prefixKey = "p";
                    break;
            }
        }
        //創建帳號
        try
        {
            var req = new PlayerRequest()
            {
                account = prefixKey + request.Club_id.ToLower(),
                password = prefixKey + request.Club_id.ToLower(),
                nickname = userData.Club_Ename
            };

            var response = await _apiService.Player(req);
            var tpId = response.data.account;
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = tpId;
            gameUser.game_platform = Platform.TP.ToString();
            return gameUser;
        }
        catch (ExceptionMessage ex) when (ex.MsgId == (int)TPErrorCode.Player_account_duplicated)
        {
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = prefixKey + userData.Club_id.ToLower();
            gameUser.game_platform = request.Platform;
            return gameUser;
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.CreateTpUserFail, ex.Message.ToString());
        }
    }

    public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {

        #region STEP1: 檢查帳號是否存在
        try
        {
            var req = new CheckPlayerRequest()
            {
                account = platform_user.game_user_id
            };

            var response = await _apiService.CheckPlayer(req);

            if (!response.data)
                throw new Exception("Account:Tp User Not Found!");
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.CheckTpUserFail, ex.Message.ToString());
        }
        #endregion

        #region STEP2: 發起存款

        var transfer_amount = RecordData.amount;
        var currency = walletData.Currency;

        //檢查幣別
        if (!TPConfig.Currency.ContainsKey(currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new DepositRequest()
            {
                gamehall = _gamehall,
                account = platform_user.game_user_id,
                transaction_id = CutGuidTo30Characters(RecordData.id),
                amount = transfer_amount
            };

            var response = await _apiService.Deposit(req);

            //確認結果是否異常
            RecordData.status = nameof(TransferStatus.pending);

            if (response.data.status.ToLower() == "failed")
                RecordData.status = nameof(TransferStatus.fail);

            if (response.data.status.ToLower() == "success")
                RecordData.status = nameof(TransferStatus.success);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError("TP Deposit fail ex : {ex}", ex);
        }

        #endregion

        return RecordData.status;
    }
    public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
    {
        var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
        var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
        GetBetRecord res = new GetBetRecord();
        if (summary.Game_type == 3) //Game_type = 3為電子注單，其餘為真人注單
        {
                List<dynamic> tp_results = new List<dynamic>();
                // 第二層明細舊表
                tp_results.AddRange(await _tpDbService.GetTpRecordsBySummary(RecordReq));
                if (tp_results.Count == 0)
                {
                    foreach (var partition in partitions)
                    {
                        tp_results.AddRange( await _tpDbService.GetTpRecordByReportTime(summary, partition, partition.AddDays(1)));
                    }
                }
            res.Data = tp_results.OrderByDescending(e => e.bettime).Select(x => x).ToList();
        }
        return res;
    }
    public async Task<string> GameDetailURL(GetBetDetailReq request)
    {
        TPConfig.lang.TryGetValue(request.lang, out var lang);
        var req = new PlayCheckResquest()
        {
            gamehall = _gamehall,
            betID = request.record_id,
            bet_time = request.ReportTime.AddHours(-12).AddSeconds(-1), //GMT+8 => GMT-4
            lang = lang ?? TPConfig.lang["en-US"],
        };

        var result = await _apiService.PlayCheck(req);
        return result.data.playCheck;
    }
    public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
    {
        var memberBalance = new MemberBalance();

        try
        {
            var req = new PlayerWalletRequest()
            {
                gamehall = _gamehall,
                account = platform_user.game_user_id,
            };

            var result = await _apiService.PlayerWallet(req);

            memberBalance.Amount = decimal.Parse(result.data.balance);
            memberBalance.Amount = decimal.Round(memberBalance.Amount, 2, MidpointRounding.ToZero);//取至小數點後2位，無條件捨去。TP小數點第2位後不提供提款

        }
        catch (Exception ex)
        {
            memberBalance.Amount = 0;
            memberBalance.code = (int)ResponseCode.Fail;
            memberBalance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
            _logger.LogError("TP餘額取得失敗 Msg: {Message}", ex.Message);
        }

        memberBalance.Wallet = nameof(Platform.TP);
        return memberBalance;
    }
    public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
    {
        try
        {
            var req = new PlayerLogoutRequest()
            {
                gamehall = _gamehall,
                account = platform_user.game_user_id,
            };

            var res = await _apiService.PlayerLogout(req);
        }
        catch (Exception ex)
        {
            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
            _logger.LogError("KickTpUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
        }

        return true;
    }

    public async Task<bool> KickAllUser(Platform platform)
    {
        var req = new KickAllRequest()
        {
            gamehall = _gamehall
        };

        await _apiService.KickAll(req);
        return true;
    }

    public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
    {
        try
        {
            TPConfig.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            var req = new GameLinkRequest()
            {
                gamehall = _gamehall,
                //真人遊戲gamecode需Mapping
                gamecode = TPConfig.LiveGameMap.IdToCode.TryGetValue(request.GameConfig["gameCode"], out var liveGameCode) ? liveGameCode : request.GameConfig["gameCode"],
                account = platformUser.game_user_id,
                lang = lang ?? TPConfig.lang["en-US"],
                platform = request.GameConfig["device"] == "DESKTOP" ? "web" : "mobile",
                return_url = request.GameConfig["lobbyURL"],
            };

            var result = await _apiService.GameLink(req);

            return result.data.url;
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.GetGameURLFail, ex.Message.ToString());
        }
    }

    public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
    {
        #region 補注單
        var start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
        var maxEnd = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, 0);

        var offSet = TimeSpan.FromMinutes(30);
        if (RepairReq.SearchType == 1)
            offSet = TimeSpan.FromMinutes(5);

        var postResult = 0;

        while (start < maxEnd)
        {
            var end = start.Add(offSet);
            if (end > maxEnd)
            {
                end = maxEnd;
            }

            var betLogs = await GetTpRecords(start, end, RepairReq.SearchType == 1);
            var res = new List<BetLog>();
            if (betLogs.Count != 0)
            {
                var oldLogs = (await _tpDbService.GetTpRecordsByBetTime(betLogs.Min(l => l.bettime), betLogs.Max(l => l.bettime)))
                     .Select(l => new { l.rowid, l.status, l.bettime })
                     .ToHashSet();
                foreach (var itme in betLogs)
                {
                    if (oldLogs.Add(new { itme.rowid, itme.status, itme.bettime }))
                    {
                        res.Add(itme);
                    }
                }
                //真人目前沒串接
                //if (res.Any(b => b.isLiveRecord))
                //    postResult += await PostTpLiveRecord(res.Where(b => b.isLiveRecord).ToList());
                if (res.Any(b => b.isElectronicRecord))
                    postResult += await PostTpRecord(res.Where(b => b.isElectronicRecord).ToList());
            }
            start = end;
            //await Task.Delay(20 * 1000);
        }


        #endregion
        #region 重產匯總帳
        start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0);
        offSet = TimeSpan.FromHours(1);
        while (start < maxEnd)
        {
            var end = start.Add(offSet);

            await SummaryW1Report(start, end);
            await SummaryGameProviderReport(start, end);

            start = end;
            await Task.Delay(100);
        }
        #endregion
        return $"Game: {Platform.TP} 新增資料筆數: {postResult}";
    }

    public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {

        #region STEP1: 檢查帳號是否存在
        try
        {
            var req = new CheckPlayerRequest()
            {
                account = platform_user.game_user_id
            };

            var response = await _apiService.CheckPlayer(req);

            if (!response.data)
                throw new Exception("Account:{tpTp} Not Found!");
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.CheckTpUserFail, ex.Message.ToString());
        }
        #endregion

        #region STEP2: 發起提款

        var transfer_amount = RecordData.amount;
        var currency = walletData.Currency;

        //檢查幣別
        if (!TPConfig.Currency.ContainsKey(currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            var req = new WithdrawRequest()
            {
                gamehall = _gamehall,
                account = platform_user.game_user_id,
                transaction_id = CutGuidTo30Characters(RecordData.id),
                amount = transfer_amount
            };

            var response = await _apiService.Withdraw(req);

            //確認結果是否異常
            RecordData.status = nameof(TransferStatus.pending);

            if (response.data.status.ToLower() == "failed")
                RecordData.status = nameof(TransferStatus.fail);

            if (response.data.status.ToLower() == "success")
                RecordData.status = nameof(TransferStatus.success);
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError("TP Withdraw fail ex : {ex}", ex);
        }

        #endregion

        return RecordData.status;
    }

    public async Task<int> PostTpRecord(List<BetLogResponse.BetLog> recordData)
    { 
        // TP 的 API 回傳不會排除 DEV 與 UAT 環境的資料，所以要依據目前環境排除其他環境的資料 有兩個規則會員帳號會有兩種前贅詞dev 是dev及d UAT是 uat及u
        var betRecords = recordData.OrderBy(x => x.bettime).ToList();
        if (!betRecords.Any()) return 0;
        //新表
        var newLogs = (await _tpDbService.GetTpRecordV2ByBetTime(betRecords.Min(l => l.bettime), betRecords.Max(l => l.bettime)))
               .Select(l => new { l.rowid, l.status, l.bettime })
               .ToHashSet();
        var postResult = 0;
        using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
        {
            await conn.OpenAsync();
            var Chucklist = betRecords.Chunk(20000);

            foreach (IEnumerable<BetLogResponse.BetLog> group in Chucklist)
            {
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var betDetailData = new List<BetLogResponse.BetLog>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                        // 紀錄 reportTime 跟 playTime 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();
                        foreach (BetLogResponse.BetLog r in group) //loop club id bet detail
                        {
                            // 跳過重複注單
                            if (newLogs.Add(new { r.rowid, r.status, r.bettime }) == false)
                                continue;

                            r.db_report_time = reportTime;
                            r.partition_time = r.bettime;
                            betDetailData.Add(r);
                            // 紀錄 reportTime 跟 partition column bdate 的關聯
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }

                            dic[summaryTime].Add(r.bettime.ToString("yyyy-MM-dd HH:mm"));
                        }
                        int PostRecordResult = await _tpDbService.PostTpRecord(conn, tran, betDetailData);
                        await tran.CommitAsync();

                        postResult += PostRecordResult;
                        // 記錄到 Redis reportTime 跟 playTime 的關聯
                        foreach (var item in dic)
                        {
                            var key = $"{RedisCacheKeys.TpBetSummaryTime}:{item.Key}";
                            await _commonService._cacheDataService.SortedSetAddAsync(key,
                                item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                        }
                        dic.Clear();
                        sw.Stop();
                        _logger.LogDebug("insert TP record member 寫入{count}筆資料時間 : {time} MS", betRecords.Count, sw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        foreach (var r in group) //loop club id bet detail
                        {
                            _logger.LogError("record id : {id}, time: {time}", r.rowid, r.reporttime);
                        }
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError("Run tp record  exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                             ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                        await tran.RollbackAsync();
                    }
                }
            }
        }
        return postResult;
    }

    /// <summary>
    /// 真人目前沒串接
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <returns></returns>
    //public async Task<int> PostTpLiveRecord(List<BetLogResponse.BetLog> recordData)
    //{
    //    // TP 的 API 回傳不會排除 DEV 與 UAT 環境的資料，所以要依據目前環境排除其他環境的資料 有兩個規則會員帳號會有兩種前贅詞dev 是dev及d UAT是 uat及u
    //    var betRecords = recordData.Where(x => x.casino_account.ToLower().StartsWith(_prefixKey.Substring(0, 1).ToLower()))
    //                                    .OrderBy(x => x.bettime)
    //                                    .ToList();
    //    var newLogs =(await _tpDbService.GetTpLiveRecordsV2ByBetTime(betRecords.Min(l => l.bettime), betRecords.Max(l => l.bettime)))
    //                        .Select(l => new { l.rowid, l.status, l.bettime })
    //                        .ToHashSet();
    //    //去除同批內重複注單，並以reporttime較大的為主
    //    recordData = betRecords.Where(r => r.status != "0")//忽略未派彩
    //                           .OrderByDescending(r => r.reporttime)
    //                           .DistinctBy(r => new { r.rowid, r.status, r.bettime })
    //                           .Reverse()
    //                           .ToList();

    //    var logGroups = recordData.GroupBy(b => b.casino_account);
    //    var postResult = 0;
    //    using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
    //    {
    //        await conn.OpenAsync();
    //        var Chucklist = recordData.Chunk(20000);
    //        foreach (var logGroup in Chucklist)
    //        {
    //            using (var tran = conn.BeginTransaction())
    //            {
    //                try
    //                {
    //                    //已結算注單
    //                    List<BetLogResponse.BetLog> betLogs = new();
    //                    var sw = System.Diagnostics.Stopwatch.StartNew();
    //                    var betDetailData = new List<BetLogResponse.BetLog>();
    //                    var dt = DateTime.Now;
    //                    var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
    //                    // 紀錄 reportTime 跟 playTime 的關聯
    //                    var dic = new Dictionary<string, HashSet<string>>();

    //                    //真人注單有改牌邏輯，需先儲存原始資料
    //                    var records = logGroup.Select(r =>
    //                    {
    //                        r.partition_time = r.bettime;
    //                        r.pre_betamount = r.betamount;
    //                        r.pre_betresult = r.betresult;
    //                        r.pre_betvalid = r.betvalid;
    //                        return r;
    //                    });
    //                    foreach (BetLogResponse.BetLog r in records) //loop club id bet detail
    //                    {
    //                        // 跳過重複注單
    //                        if (newLogs.Add(new { r.rowid, r.status, r.bettime }) == false)
    //                            continue;

    //                        r.db_report_time = reportTime;
    //                        betDetailData.Add(r);
    //                        // 紀錄 reportTime 跟 partition column bdate 的關聯
    //                        var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
    //                        if (!dic.ContainsKey(summaryTime))
    //                        {
    //                            dic.Add(summaryTime, new HashSet<string>());
    //                        }

    //                        dic[summaryTime].Add(r.bettime.ToString("yyyy-MM-dd HH:mm"));
    //                    }
    //                    int PostRecordResult = await _tpDbService.PostTpLiveRecord(conn, tran, betDetailData);
    //                    await tran.CommitAsync();
    //                    postResult += PostRecordResult;

    //                    foreach (var item in dic)
    //                    {
    //                        var key = $"{RedisCacheKeys.TpBetSummaryTime}:{item.Key}";
    //                        await _commonService._cacheDataService.SortedSetAddAsync(key,
    //                            item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
    //                    }

    //                    dic.Clear();
    //                    sw.Stop();
    //                    _logger.LogDebug("insert TP record member 寫入{count}筆資料時間 : {time} MS", betRecords.Count, sw.ElapsedMilliseconds);
    //                }
    //                catch (Exception ex)
    //                {
    //                    foreach (var r in logGroup) //loop club id bet detail
    //                    {
    //                        _logger.LogError("record id : {id}, time: {time}", r.rowid, r.reporttime);
    //                    }
    //                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
    //                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
    //                    _logger.LogError("Run tp record  exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
    //                         ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);

    //                    await tran.RollbackAsync();
    //                }
    //            }
    //        }
    //    }

    //    return postResult;
    //}

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
            _logger.LogDebug("Create TP game provider report time {datetime}", start);
            // 每小時投注匯總
            var result = await GetAllStatisicsByGameAsync(start, end);

            // 沒有資料寫入空的匯總帳就結束排程
            if (!result.Any())
            {
                //遊戲商(轉帳中心的欄位格式)
                var gameEmptyReport = new GameReport
                {
                    platform = nameof(Platform.TP),
                    report_datetime = start,
                    report_type = (int)GameReport.e_report_type.FinancalReport,
                    total_bet = 0,
                    total_win = 0,
                    total_netwin = 0,
                    total_count = 0
                };

                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);

                return;
            }

            // 遊戲商的每小時鐘匯總報表(遊戲商的欄位格式)
            await _tpDbService.DeleteTpReport(result);
            await _tpDbService.PostTpReport(result);

            // 遊戲商的每小時匯總報表(轉帳中心的欄位格式)
            var tpSummaryReport = new GameReport
            {
                platform = nameof(Platform.TP),
                report_datetime = start,
                report_type = (int)GameReport.e_report_type.FinancalReport,
                total_bet = result.Sum(s => decimal.Parse(s.bet_value)),
                total_win = result.Sum(s => decimal.Parse(s.bet_result) + decimal.Parse(s.bet_value)),
                total_netwin = result.Sum(s => decimal.Parse(s.bet_result)),
                total_count = result.Sum(s => long.Parse(s.bet_count))
            };
            await _gameReportDBService.DeleteGameReport(tpSummaryReport);
            await _gameReportDBService.PostGameReport(tpSummaryReport);

            start = end;
            await Task.Delay(3000);
        }
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

        // 將老虎機、魚機記錄好的 reporttime > playtime 取出
        var redisKey = $"{RedisCacheKeys.TpBetSummaryTime}:{reportTime.ToString("yyyy-MM-dd HH:mm")}";

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
    public async Task<bool> RecordSummary(Platform platform, DateTime reportTime, DateTime startTime, DateTime endTime)
    {
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var summaryRecords = await _tpDbService.SummaryGameRecord(reportTime, startTime, endTime);
        sw1.Stop();
        _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

        var userSummaries = summaryRecords.GroupBy(s => s.userid);
        var userlist = userSummaries.Select(x => x.Key).Distinct().ToList();
        // 批次處理，每次1000筆
        var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
        {
            return (await _commonService._serviceDB.GetWallet(betch));
        }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);

        var summaryRecordList = new List<(BetRecordSummary summay, HashSet<t_summary_bet_record_mapping> mappings)>();
        foreach (var summaryRecord in userSummaries)
        {
            if (!userWalletList.TryGetValue(summaryRecord.Key, out var userWallet)) continue;

            var summaryData = new BetRecordSummary();
            summaryData.Turnover += summaryRecord.Sum(x=>x.turnover);
            summaryData.ReportDatetime = reportTime;
            summaryData.Currency = userWallet.Currency;
            summaryData.Club_id = userWallet.Club_id;
            summaryData.Franchiser_id = userWallet.Franchiser_id;
            summaryData.RecordCount = summaryRecord.Sum(x => x.RecordCount);
            summaryData.Game_id = nameof(Platform.TP);
            summaryData.Game_type = 3; //電子遊戲 = 3
            summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
            summaryData.Win = summaryRecord.Sum(x => x.turnover) + summaryRecord.Sum(x => x.netwin);
            summaryData.Netwin = summaryRecord.Sum(x => x.netwin);
            summaryData.updatedatetime = DateTime.Now;


            var mapping = new HashSet<t_summary_bet_record_mapping>();
            foreach (var tickDateTime in summaryRecord.Select(s => s.betTime))
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

        var Chucklist = summaryRecordList.Chunk(10000);
        foreach (var group in Chucklist)
        {
            await using NpgsqlConnection conn = new(Config.OneWalletAPI.DBConnection.BetLog.Master);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await conn.OpenAsync();
            await using (var tran = await conn.BeginTransactionAsync())
            {
                await _tpDbService.BatchInsertRecordSummaryAsync(conn, group.Select(c => c.summay).ToList());
                await _tpDbService.BulkInsertSummaryBetRecordMapping(tran, group.SelectMany(c => c.mappings));
                await tran.CommitAsync();
            }
            await conn.CloseAsync();
            sw.Stop();
            _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", group.Count(), sw.ElapsedMilliseconds);
        }
        return true;
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

            _logger.LogDebug("Create TP game W1 report time {datetime}", start);

            var (totalCount, totalBetValid, totalNetWin) = await _tpDbService.SumTpBetRecordByBetTime(start, end.AddMilliseconds(-1));
            //真人目前為串接
            //var liveTotal = await _tpDbService.SumTpLiveBetRecordByBetTime(start, end.AddMilliseconds(-1));
            //totalCount += liveTotal.totalCount;
            //totalBetValid += liveTotal.totalBetValid;
            //totalNetWin += liveTotal.totalNetWin;

            GameReport reportData = new();
            reportData.platform = nameof(Platform.TP);
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
    /// 取得時間區間內的所有注單
    /// 
    /// 依注單產生時間搜尋(byBetTime=false)搜尋區間不得大於30分鐘
    /// 依下注時間搜尋(byBetTime=true)搜尋區間不得大於5分鐘
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <param name="byBetTime"></param>
    /// <returns></returns>
    public async Task<List<BetLogResponse.BetLog>> GetTpRecords(DateTime startDateTime, DateTime endDateTime, bool byBetTime = false)
    {
        var betLogs = new List<BetLogResponse.BetLog>();

        var page = 1;
        var req = new BetLogRequest()
        {
            gamehall = _gamehall,
            start_time = startDateTime.AddHours(-12),
            end_time = endDateTime.AddHours(-12).AddSeconds(-1),
            page = page,
            page_size = 20000
        };

        while (true)
        {
            req.page = page;

            TpResponse<BetLogResponse> res;
            if (byBetTime)
                res = await _apiService.BetLogByBetTime(req);
            else
                res = await _apiService.BetLog(req);

            betLogs.AddRange(res.data.page_result);

            if (page >= res.data.last_page)
                break;

            page++;

            //api建議20~30秒爬一次
            //await Task.Delay(20 * 1000);
        }

        //*****************
        return betLogs
         .Where(l => Whereprefixkey(l.casino_account) || l.casino_account.Length > 13)
         .Select(l =>
         {
             l.casino_account = l.casino_account.Length == 13 ? l.casino_account[_prefixKey.Length..].ToUpper() : l.casino_account[1..].ToUpper(); //去除PrfixKey
             l.reporttime = l.reporttime.AddHours(12); //-4 => +8
                l.bettime = l.bettime.AddHours(12); //-4 => +8
                l.payout_time = l.payout_time.AddHours(12) ; //-4 => +8
                return l;
            }).ToList();
    }

    private bool Whereprefixkey(string account)
    {

        return account.StartsWith(_prefixKey.Substring(0, 1));
    }

    public Task HealthCheck(Platform platform)
    {
        return _apiService.GameList(new()
        {
            gamehall = _gamehall,
        });
    }

    /// <summary>
    /// 計算彙總
    /// </summary>
    /// <param name="SummaryData"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    private BetRecordSummary Calculate(BetRecordSummary SummaryData, BetLogResponse.BetLog r)
    {
        SummaryData.RecordCount++;
        SummaryData.Bet_amount += decimal.Parse(r.betamount);
        SummaryData.Turnover += decimal.Parse(r.betvalid);
        SummaryData.Netwin += decimal.Parse(r.betresult);
        SummaryData.Win += decimal.Parse(r.betvalid) + decimal.Parse(r.betresult);
        SummaryData.updatedatetime = DateTime.Now;
        return SummaryData;
    }

    /// <summary>
    /// 計算Live彙總
    /// </summary>
    /// <param name="SummaryData"></param>
    /// <param name="r"></param>
    /// <param name="tran"></param>
    /// <returns></returns>
    //private async Task<(bool calculated,BetRecordSummary summary)> CalculateLive(BetRecordSummary SummaryData, BetLogResponse.BetLog r, IDbTransaction tran)
    //{
    //    var oldRecords = await _tpDbService.GetTpLiveRecordsByRowId(tran, r.rowid, r.bettime);
    //    oldRecords ??= new();

    //    //重複單則跳開
    //    if (oldRecords.Any(oldr => new { oldr.rowid, oldr.status, oldr.bettime, oldr.reporttime }.Equals(new { r.rowid, r.status, r.bettime, r.reporttime })))
    //        return (false, SummaryData);

    //    //沖銷掉原注單
    //    if (oldRecords.Any())
    //    {
    //        var lastRecord = oldRecords.OrderByDescending(r => r.reporttime).First(); //沖銷最後一筆即可

    //        //比對betamount、betvalid、betresult相同則不計算該單
    //        if (decimal.Parse(r.betamount) == lastRecord.pre_betamount && decimal.Parse(r.betvalid) == lastRecord.pre_betvalid && decimal.Parse(r.betresult) == lastRecord.pre_betresult)
    //            return (false, SummaryData);

    //        r.betamount = (decimal.Parse(r.betamount) - lastRecord.pre_betamount).ToString();
    //        r.betvalid = (decimal.Parse(r.betvalid) - lastRecord.pre_betvalid).ToString();
    //        r.betresult = (decimal.Parse(r.betresult) - lastRecord.pre_betresult).ToString();
    //    }

    //    return (true, Calculate(SummaryData, r));

    //    throw new ExceptionMessage(ResponseCode.Fail, $"PostTpLiveRecord Fail! | UnSupported Status: {r.status} Record:{System.Text.Json.JsonSerializer.Serialize(r)}");
    //}

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
    private static string CutGuidTo30Characters(Guid guid)
    {
        return guid.ToString("N")[2..];
    }
    private async Task<List<StatisticsByGameResponse.Statistics>> GetAllStatisicsByGameAsync(DateTime startTime, DateTime endTime)
    {
        var start = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0).AddHours(-12);
        var end = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, 0, 0).AddHours(-12);

        var statistics = new List<StatisticsByGameResponse.Statistics>();

        var page = 1;
        var req = new StatisticsByGameRequest()
        {
            start_time = start,
            end_time = end.AddSeconds(-1),
            page_size = 500,
            page = page,
        };

        while (true)
        {
            req.page = page;

            var res = await _apiService.StatisticsByGame(req);

            statistics.AddRange(res.data.data);

            if (page == res.data.last_page)
                break;

            page++;
        }

        return statistics.Select(s =>
        {
            s.report_time = start.AddHours(12); //-4 => +8
            return s;
        }).ToList();
    }

    public PlatformType GetPlatformType(Platform platform)
    {
        return PlatformType.Electronic | PlatformType.Live;
    }
}

