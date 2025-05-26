using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Response;
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
using JokerConfig = H1_ThirdPartyWalletAPI.Model.Game.JOKER.JOKER;

namespace H1_ThirdPartyWalletAPI.Service.Game.JOKER;

public interface IJOKER_InterfaceService : IGameInterfaceService
{
    /// <summary>
    /// 新增 5 分鐘匯總帳
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    Task<int> PostJokerRecordDetail(List<t_joker_bet_record> source);

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
    /// 取得注單明細
    /// </summary>
    /// <returns></returns>
    Task<GetBetDetailResponse> GetJokerRecords(GetBetDetailRequest source);
}

public class JOKER_InterfaceService : IJOKER_InterfaceService
{
    private readonly IGameApiService _gameApiService;
    private readonly ICommonService _commonService;
    private readonly ILogger<JOKER_InterfaceService> _logger;
    const int _cacheSeconds = 600;
    private readonly IDBService _dbService;
    private readonly ISummaryDBService _summaryDBService;
    private readonly IJokerDBService _jokerDBService;
    private readonly ICacheDataService _cacheService;
    private readonly IGameReportDBService _gameReportDBService;
    private readonly string _prefixKey;
    private const int _cacheFranchiserUser = 1800;

    public JOKER_InterfaceService(IGameApiService gameApiService,
                                  ICommonService commonService,
                                  ILogger<JOKER_InterfaceService> logger,
                                  ISummaryDBService summaryDBService,
                                  IJokerDBService jokerDBService,
                                  IGameReportDBService gameReportDBService)
    {
        _gameApiService = gameApiService;
        _commonService = commonService;
        _logger = logger;
        _dbService = commonService._serviceDB;
        _summaryDBService = summaryDBService;
        _cacheService = commonService._cacheDataService;
        _gameReportDBService = gameReportDBService;
        _prefixKey = Config.OneWalletAPI.Prefix_Key;
        _jokerDBService = jokerDBService;
    }

    /// <summary>
    /// 取得遊戲額度
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platform_user"></param>
    /// <returns></returns>
    public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
    {
        var Balance = new MemberBalance();

        try
        {
            var result = await _gameApiService._JokerApi.GetCreditAsync(new GetCreditRequest()
            {
                Username = platform_user.game_user_id
            });

            Balance.Amount = result.Credit;
        }
        catch (Exception ex)
        {
            Balance.Amount = 0;
            Balance.code = (int)ResponseCode.Fail;
            Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
            _logger.LogError("Joker餘額取得失敗 Msg: {Message}", ex.Message);
        }
        Balance.Wallet = nameof(Platform.JOKER);
        return Balance;
    }

    /// <summary>
    /// 玩家踢線
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="platform_user"></param>
    /// <returns></returns>
    public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
    {
        try
        {
            var result = await _gameApiService._JokerApi.KickPlayerAsync(new KickPlayerRequest()
            {
                Username = platform_user.game_user_id
            });

            if (result.Status != "OK")
            {
                throw new Exception(result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("踢出Joker使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
        }
        return true;
    }

    /// <summary>
    /// 全部玩家踢線
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> KickAllUser(Platform platform)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 轉入額度
    /// </summary>
    /// <param name="platform_user"></param>
    /// <param name="walletData"></param>
    /// <param name="RecordData"></param>
    /// <returns></returns>
    public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        try
        {
            var result = await _gameApiService._JokerApi.TransferCreditAsync(new TransferCreditRequest()
            {
                Username = platform_user.game_user_id,
                Amount = RecordData.amount.ToString(),
                RequestID = RecordData.id.ToString()
            });

            // W1 傳小寫帳號遊戲會回傳大寫帳號，故全部用小寫比對
            if (result.Username.ToLower() == platform_user.game_user_id.ToLower() && result.Credit == RecordData.amount)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            else
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
            }

        }
        catch (TaskCanceledException ex)
        {
            RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
            _logger.LogError("Joker TransferIn Timeout ex : {ex}", ex);
        }
        catch (ExceptionMessage ex)
        {
            RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
            _logger.LogError("Joker TransferIn Fail ex : {ex}", ex.Message);
        }
        catch (Exception ex)
        {
            // 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
            if (ex.Message == HttpStatusCode.NotFound.ToString())
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("Joker TransferIn Fail ex : {ex}", ex.Message);
            }
            else
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Joker TransferIn Fail ex : {ex}", ex.Message);
            }
        }

        return RecordData.status;
    }

    /// <summary>
    /// 轉出額度
    /// </summary>
    /// <param name="platform_user"></param>
    /// <param name="walletData"></param>
    /// <param name="RecordData"></param>
    /// <returns></returns>
    public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        try
        {
            // 對 Joker 轉出要用負數
            decimal amount = 0;
            if (RecordData.amount > 0)
            {
                amount = RecordData.amount * -1;
            }

            var result = await _gameApiService._JokerApi.TransferCreditAsync(new TransferCreditRequest()
            {
                Username = platform_user.game_user_id,
                Amount = amount.ToString(),
                RequestID = RecordData.id.ToString()
            });


            // W1 傳小寫帳號遊戲會回傳大寫帳號，故全部用小寫比對
            if (result.Username.ToLower() == platform_user.game_user_id.ToLower() && result.BeforeCredit == RecordData.amount)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            else
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
            }
        }
        catch (TaskCanceledException ex)
        {
            RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
            _logger.LogError("Joker TransferOut Timeout ex : {ex}", ex);
        }
        catch (ExceptionMessage ex)
        {
            RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
            _logger.LogError("Joker TransferOut Fail ex : {ex}", ex.Message);
        }
        catch (Exception ex)
        {
            // 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
            if (ex.Message == HttpStatusCode.NotFound.ToString())
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("Joker TransferOut Fail ex : {ex}", ex.Message);

            }
            else
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Joker TransferOut Fail ex : {ex}", ex.Message);
            }
        }
        return RecordData.status;
    }

    /// <summary>
    /// 建立使用者
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    /// <exception cref="ExceptionMessage"></exception>
    public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
    {
        if (!JokerConfig.Currency.ContainsKey(userData.Currency))
        {
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);
        }

        var userName = string.Empty;

        //遊戲明細是大寫 UATC250000611117
        userName = (_prefixKey + userData.Club_id).ToLower();

        try
        {
            var result = await _gameApiService._JokerApi.CreatePlayerAsync(new CreatePlayerRequest()
            {
                Username = userName
            });

            if (result.Status != "Created" && result.Status != "OK")
            {
                throw new ExceptionMessage((int)ResponseCode.CheckJokerUserFail, MessageCode.Message[(int)ResponseCode.CheckJokerUserFail] + "|" + " joker create user fail");
            }
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage((int)ResponseCode.CheckJokerUserFail, MessageCode.Message[(int)ResponseCode.CheckJokerUserFail] + "|" + ex.Message.ToString());
        }


        return new GamePlatformUser
        {
            club_id = userData.Club_id,
            game_user_id = userName,
            game_platform = request.Platform
        };
    }

    /// <summary>
    /// 進入遊戲
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userData"></param>
    /// <param name="platformUser"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    /// <exception cref="ExceptionMessage"></exception>
    public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
    {
        var url = string.Empty;
        var lang = string.Empty;

        if (!request.GameConfig.ContainsKey("gameCode"))
        {
            throw new Exception("game code not found");
        }

        if (request.GameConfig.ContainsKey("lobbyURL"))
        {
            url = request.GameConfig["lobbyURL"];
        }

        if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && Model.Game.JOKER.JOKER.lang.ContainsKey(request.GameConfig["lang"]))
        {
            lang = Model.Game.JOKER.JOKER.lang[request.GameConfig["lang"]];
        }
        else
        {
            lang = Model.Game.JOKER.JOKER.lang["en-US"];
        }

        try
        {
            var getToke = await _gameApiService._JokerApi.GetGameTokenAsync(new GetGameTokenRequest()
            {
                Username = platformUser.game_user_id
            });

            return _gameApiService._JokerApi.GetGameUrl(new GetGameUrlRequest()
            {
                Token = getToke.Token,
                GameCode = request.GameConfig["gameCode"],
                RedirectUrl = url,
                Mobile = true,
                Lang = lang
            });
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
        }
    }

    /// <summary>
    /// 檢查交易紀錄
    /// </summary>
    /// <param name="transfer_record"></param>
    /// <returns></returns>
    public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
    {
        var CheckTransferRecordResponse = new CheckTransferRecordResponse();

        try
        {
            // 验证转移信用
            var result = await _gameApiService._JokerApi.ValidTransferCreditAsync(new ValidTransferCreditRequest()
            {
                RequestID = transfer_record.id.ToString()
            });

            // 找不到單號會回傳 Http Status 404
            // 找到會回傳相同的單號
            if (result.RequestID == transfer_record.id.ToString())
            {
                if (transfer_record.target == nameof(Platform.JOKER))//轉入Joker直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.JOKER))
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    if (transfer_record.status != nameof(WalletTransferRecord.TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = transfer_record.status = nameof(WalletTransferRecord.TransferStatus.success);
                transfer_record.success_datetime = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            // 响应-失败：HTTP/1.1 404 Not Found 表示 requestId 不存在
            if (ex.Message == HttpStatusCode.NotFound.ToString())
            {
                if (transfer_record.target == nameof(Platform.JOKER))//轉入Joker直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.JOKER))
                {
                    if (transfer_record.status != nameof(WalletTransferRecord.TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = nameof(WalletTransferRecord.TransferStatus.fail);
                transfer_record.success_datetime = DateTime.Now;
                transfer_record.after_balance = transfer_record.before_balance;
            }
            else
            {
                throw;
            }
        }

        CheckTransferRecordResponse.TRecord = transfer_record;
        return CheckTransferRecordResponse;
    }

    /// <summary>
    /// GetBetRecords
    /// </summary>
    /// <param name="RecordReq"></param>
    /// <returns></returns>
    public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
    {
        #region backup
        //GetBetRecord res = new GetBetRecord();
        //IEnumerable<dynamic> joker_results = await _jokerDBService.GetJokerRecordsBySummary(RecordReq);
        //joker_results = joker_results.OrderByDescending(e => e.Time);
        //joker_results = joker_results.Select(x =>
        //{
        //    x.Result = x.Result - x.Amount;
        //    return x;
        //});

        //res.Data = joker_results.ToList();
        #endregion
        var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);

        // Game_type = 3為電子注單，其餘為真人注單
        if (summary.Game_type != 3)
        {
            return new GetBetRecord();
        }

        var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();

        var res = new GetBetRecord();
        var userName = string.Empty;

        userName = (_prefixKey + summary.Club_id).ToUpper();

        var jokerResults = new List<dynamic>();

        foreach (var partition in partitions)
        {
            jokerResults.AddRange(await _jokerDBService.GetJokerRecordByReportTime(summary, partition, partition.AddDays(1), userName));
        }

        res.Data = jokerResults.OrderByDescending(e => e.Time).Select(x =>
        {
            x.Result = x.Result - x.Amount;
            return x;
        }).ToList();

        return res;
    }

    /// <summary>
    /// 遊戲商開牌紀錄
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<string> GameDetailURL(GetBetDetailReq request)
    {
        var result = await _gameApiService._JokerApi.GetGameHistoryUrlAsync(new GetGameHistoryUrlRequest()
        {
            OCode = request.record_id,
            Language = Model.Game.JOKER.JOKER.GameDetaliLang.ContainsKey(request.lang) ? Model.Game.JOKER.JOKER.GameDetaliLang[request.lang] : "en",
            Type = "Game"
        });

        return result.Url;
    }

    /// <summary>
    /// 補單
    /// </summary>
    /// <param name="RepairReq"></param>
    /// <returns></returns>
    public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
    {
        #region 補注單
        var start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
        var maxEnd = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, RepairReq.EndTime.Second);

        var offSet = TimeSpan.FromMinutes(30);
        var postResult = 0;

        // 批次請求查詢注單
        while (start < maxEnd)
        {
            var end = start.Add(offSet);
            if (end > maxEnd)
            {
                end = maxEnd;
            }

            var betLogs = new List<t_joker_bet_record>();
            var nextId = string.Empty;

            while (true)
            {
                var result = await GetJokerRecords(new GetBetDetailRequest()
                {
                    StartDate = start.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndDate = end.ToString("yyyy-MM-dd HH:mm:ss"),
                    NextId = nextId
                });

                // NextId 不為空就用一樣的時間 + NextId 重新請求
                // NextId 為空代表已經沒資料了
                if (string.IsNullOrEmpty(result.nextId)) break;

                if (result.data.Game != null && result.data.Game.Any())
                {
                    // 遊戲注單
                    betLogs.AddRange(result.data.Game.Select(x => new t_joker_bet_record()
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
                        BetType = BetTypeEnum.Game
                    }).ToList());
                }


                if (result.data.Jackpot != null && result.data.Jackpot.Any())
                {
                    // 彩金注單
                    betLogs.AddRange(result.data.Jackpot.Select(x => new t_joker_bet_record()
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
                        BetType = BetTypeEnum.Jackpot
                    }).ToList());
                }


                if (result.data.Competition != null && result.data.Competition.Any())
                {
                    // 競賽注單
                    betLogs.AddRange(result.data.Competition.Select(x => new t_joker_bet_record()
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
                        BetType = BetTypeEnum.Competition
                    }).ToList());
                }

                nextId = result.nextId;
            }


            if (betLogs.Any())
            {
                // 五分鐘匯總帳裡面會判斷是否是重複單
                postResult += await PostJokerRecordDetail(betLogs);
            }

            start = end;
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
        return $"Game: {Platform.JOKER} 新增資料筆數: {postResult}";
    }

    /// <summary>
    /// 取得注單明細
    /// </summary>
    /// <returns></returns>
    public async Task<GetBetDetailResponse> GetJokerRecords(GetBetDetailRequest source)
    {
        return await _gameApiService._JokerApi.GetBetDetailAsync(source);
    }

    /// <summary>
    /// 新增 5 分鐘匯總帳和注單
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    //public async Task<int> PostJokerRecord_backup(List<t_joker_bet_record> source)
    //{
    //    // 取得現有的注單，用來比對重複單
    //    var startTime = source.Min(l => l.Time);
    //    var endTime = source.Max(l => l.Time);
    //    var oldBetList = await _jokerDBService.GetJokerRecordsByBetTime(startTime, endTime);
    //    var oldBetIds = oldBetList.Select(x => x.Ocode).ToHashSet();

    //    // 新增比數
    //    var postResult = 0;

    //    var logGroups = source.GroupBy(b => b.Username);
    //    await using var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
    //    await conn.OpenAsync();

    //    foreach (var logGroup in logGroups)
    //    {
    //        using var tran = await conn.BeginTransactionAsync();
    //        try
    //        {
    //            var club_id = logGroup.Key.Substring(3);
    //            Wallet memberWalletData = await GetWalletCache(club_id);
    //            if (memberWalletData == null || memberWalletData.Club_id == null)
    //            {
    //                throw new Exception("沒有會員id");
    //            }

    //            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.JOKER);
    //            if (gameUser == null || gameUser.game_user_id != logGroup.Key.ToLower())
    //            {
    //                throw new Exception("No joker user");
    //            }

    //            //彙總注單
    //            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();

    //            //已結算注單
    //            List<t_joker_bet_record> betLogs = new();
    //            foreach (var r in logGroup)
    //            {
    //                //跳過重複注單
    //                if (!oldBetIds.Add(r.Ocode)) continue;

    //                BetRecordSummary sumData = new BetRecordSummary();
    //                sumData.Club_id = memberWalletData.Club_id;
    //                sumData.Game_id = nameof(Platform.JOKER);
    //                sumData.Game_type = 3; //電子遊戲 = 3
    //                DateTime tempDateTime = r.Time;
    //                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
    //                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
    //                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
    //                sumData.ReportDatetime = tempDateTime;

    //                //確認是否已經超過搬帳時間 For H1 only
    //                if (Config.OneWalletAPI.RCGMode == "H1")
    //                {
    //                    if (DateTime.Now.Hour >= 12) //換日線
    //                    {
    //                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
    //                            DateTime.Now.Day, 12, 00, 0);
    //                        if (sumData.ReportDatetime < ReportDateTime)
    //                        {
    //                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
    //                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.Ocode);
    //                        }
    //                    }
    //                    else
    //                    {
    //                        var lastday = DateTime.Now.AddDays(-1);
    //                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
    //                        if (sumData.ReportDatetime < ReportDateTime)
    //                        {
    //                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
    //                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.Ocode);
    //                        }
    //                    }
    //                }

    //                //先確認有沒有符合的匯總單
    //                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
    //                {
    //                    sumData = summaryData[sumData.ReportDatetime.ToString()];
    //                    //合併處理
    //                    sumData = Calculate(sumData, r);
    //                    summaryData[sumData.ReportDatetime.ToString()] = sumData;
    //                }
    //                else
    //                {
    //                    //用Club_id與ReportDatetime DB取得彙總注單
    //                    IEnumerable<dynamic> results =
    //                        await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
    //                    if (!results.Any()) //沒資料就建立新的
    //                    {
    //                        //建立新的Summary
    //                        sumData.Currency = memberWalletData.Currency;
    //                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

    //                        //合併處理
    //                        sumData = Calculate(sumData, r);
    //                    }
    //                    else //有資料就更新
    //                    {
    //                        sumData = results.SingleOrDefault();
    //                        //合併處理
    //                        sumData = Calculate(sumData, r);
    //                    }

    //                    summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
    //                }

    //                //r.Summary_id = sumData.id;
    //                betLogs.Add(r);
    //            }

    //            List<BetRecordSummary> summaryList = summaryData.Select(s => s.Value).ToList();

    //            await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
    //            var postJokerRecord = await _jokerDBService.PostJokerRecord(conn, tran, betLogs);

    //            _logger.LogDebug("insert Joker record member: {group}, count: {count}", logGroup.Key, betLogs.Count);
    //            await tran.CommitAsync();
    //            postResult += postJokerRecord;
    //        }
    //        catch (Exception ex)
    //        {
    //            foreach (var r in logGroup) //loop club id bet detail
    //            {
    //                _logger.LogError("record id : {id}, time: {time}", r.Ocode, r.Time);
    //            }

    //            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
    //            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
    //            _logger.LogError("Run Joker record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}", logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);

    //            await tran.RollbackAsync();
    //        }
    //    }

    //    return postResult;
    //}


    /// <summary>
    /// Whereprefixkey
    /// </summary>
    /// <returns></returns>
    private bool Whereprefixkey(string Username)
    {
        return Username.StartsWith(_prefixKey.ToUpper());
    }

    /// <summary>
    /// 寫入注單明細 後匯總
    /// </summary>
    /// <param name="jokerBetRecord"></param>
    /// <returns></returns>
    public async Task<int> PostJokerRecordDetail(List<t_joker_bet_record> jokerBetRecord)
    {
        if (jokerBetRecord is null)
        {
            throw new ArgumentNullException(nameof(jokerBetRecord));
        }

        var uniqueJokerBetRecords = jokerBetRecord.Where(x => Whereprefixkey(x.Username)).DistinctBy(x => new { x.Ocode, x.Time }).ToList();

        if (!uniqueJokerBetRecords.Any())
        {
            return 0;
        }

        // 新增比數
        var postResult = 0;

        await using var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
        await conn.OpenAsync();
        foreach (IEnumerable<t_joker_bet_record> group in uniqueJokerBetRecords.Chunk(20000))
        {
            using var tran = await conn.BeginTransactionAsync();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var betDetailData = new List<t_joker_bet_record>();
            var dt = DateTime.Now;
            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

            // 紀錄 reportTime 跟 Time 的關聯
            var dic = new Dictionary<string, HashSet<string>>();
            foreach (var r in group)//loop club id bet detail
            {
                r.report_time = reportTime;
                r.Partition_time = r.Time;
                //todo 彩金要測試
                // 彩金、競賽都歸類在彩金
                if (r.BetType == BetTypeEnum.Jackpot || r.BetType == BetTypeEnum.Competition)
                {
                    //彩金獨立欄位紀錄方便SUM統計
                    r.JackpotWin = r.Result;
                    r.Result = 0;
                }
                else   // 一般注單
                {
                    r.JackpotWin = 0;
                }

                betDetailData.Add(r);

                // 紀錄 reportTime 跟 Time 的關聯
                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                if (!dic.ContainsKey(summaryTime))
                {
                    dic.Add(summaryTime, new HashSet<string>());
                }
                dic[summaryTime].Add(r.Time.ToString("yyyy-MM-dd HH:mm"));
            }

            postResult += await _jokerDBService.PostJokerRecord(conn, tran, betDetailData);
            await tran.CommitAsync();

            // 記錄到 Redis reportTime 跟 Time 的關聯
            foreach (var item in dic)
            {
                var key = $"{RedisCacheKeys.JokerBetSummaryTime}:{item.Key}";
                var value = item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks);
                await _commonService._cacheDataService.SortedSetAddAsync(key, value);
            }

            dic.Clear();
            sw.Stop();
            _logger.LogDebug("JokerRecordSchedule 寫入{count}筆資料時間 : {time} MS", postResult, sw.ElapsedMilliseconds);
        }

        return postResult;
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
        var summaryRecords = await _jokerDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
        sw1.Stop();
        _logger.LogInformation("{Platform}_SummaryGameRecord count:{count}, cost:{cost}", Platform.JOKER.ToString(), summaryRecords.Count(), sw1.ElapsedMilliseconds);

        // 取得遊戲內帳號轉為為Club_id集合

        var userSummaries = summaryRecords.GroupBy(s => s.userid);
        var userlist = userSummaries.Select(x => ConvertGamePlatformUserToClubInfo(x.Key)).Distinct().ToList();

        // 批次處理，每次1000筆
        var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
        {
            return (await _commonService._serviceDB.GetWallet(betch));
        }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);

        var summaryRecordList = new List<(BetRecordSummary summay, HashSet<t_summary_bet_record_mapping> mappings)>();
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
            summaryData.Game_id = nameof(Platform.JOKER);
            summaryData.Game_type = 3;
            summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
            summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
            summaryData.Win = summaryRecord.Sum(x => x.win);
            summaryData.Netwin = summaryRecord.Sum(x => x.win) - summaryRecord.Sum(x => x.bet);


            var mapping = new HashSet<t_summary_bet_record_mapping>();
            foreach (var tickDateTime in summaryRecord.Select(s => s.bettime))
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
                await _summaryDBService.BulkInsertSummaryBetRecordMapping(tran, group.SelectMany(c => c.mappings));
                await _summaryDBService.BatchInsertRecordSummaryAsync(conn, group.Select(c => c.summay).ToList());
                await tran.CommitAsync();
            }
            await conn.CloseAsync();
            sw.Stop();
            _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", group.Count(), sw.ElapsedMilliseconds);
        }
        return true;
    }

    /// <summary>
    /// W1 t_jdb_bet_record GamePlatformUser 轉換 Club Info 屬性規則
    /// 使用情境：後彙總排程從遊戲明細查詢使用者遊戲帳號 轉換 為H1的Club_Id 提供 wallet 查詢使用到
    /// </summary>
    /// <param name="propertyValue"></param>
    /// <returns></returns>
    /// <exception cref="ExceptionMessage"></exception>
    private string ConvertGamePlatformUserToClubInfo(string propertyValue)
    {
        string result = "";
        //依照環境變數調整Prefix
        int prefixLength = _prefixKey.Length;
        result = propertyValue.Substring(prefixLength);
        return result;
    }

    /// <summary>
    /// 新增 遊戲商小時匯總帳
    /// </summary>
    /// <returns></returns>
    public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
    {
        // 每小時投注資料
        var list = new List<t_joker_bet_record>();
        var nextId = string.Empty;
        decimal jp = 0;
        while (true)
        {
            //var result = await _gameApiService._JokerApi.GetBetDetailAsync(new GetBetDetailRequest()
            //{
            //    StartDate = startDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            //    EndDate = endDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            //    NextId = nextId
            //});
            var result = await _gameApiService._JokerApi.GethourBetAsync(new GethourBetRequest()
            {
                StartDate = startDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = endDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                NextId = nextId
            });

            if (string.IsNullOrEmpty(result.nextId)) break;

            if (result.data.Game != null && result.data.Game.Any())
            {
                // 遊戲注單
                list.AddRange(result.data.Game.Select(x => new t_joker_bet_record()
                {
                    Ocode = x.OCode,
                    Username = x.Username,
                    Gamecode = x.GameCode,
                    Description = x.Description,
                    Type = x.Type,
                    Amount = x.Amount,
                    Result = x.Result,
                    Time = x.Time,
                }).ToList());
            }


            if (result.data.Jackpot != null && result.data.Jackpot.Any())
            {
                // 彩金注單
                list.AddRange(result.data.Jackpot.Select(x => new t_joker_bet_record()
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

                }).ToList());
                jp += result.data.Jackpot.Sum(x => x.Result);
            }


            if (result.data.Competition != null && result.data.Competition.Any())
            {
                // 競賽注單
                list.AddRange(result.data.Competition.Select(x => new t_joker_bet_record()
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
                    Transactionocode = x.TransactionOCode
                }).ToList());
                jp += result.data.Competition.Sum(x => x.Result);
            }

            nextId = result.nextId;
        }

        // 從遊戲商取到的注單檢查會員前綴
        list = list
            .Where(x => x.Username.Substring(0, _prefixKey.Length).ToLower() == _prefixKey.ToLower())
            .ToList();

        // 沒有資料寫入空的匯總帳就結束排程
        if (!list.Any())
        {
            // 遊戲商(轉帳中心的欄位格式)
            //var gameEmptyReport = new t_joker_game_report
            //{
            //    Time = startDateTime,
            //    Amount = 0,
            //    Result = 0,
            //    Count = 0
            //};

            //await _jokerDBService.DeleteJokerReport(gameEmptyReport);
            //await _jokerDBService.PostJokerReport(gameEmptyReport);

            // 轉帳中心(轉帳中心的欄位格式)
            var w1CenterEmptyReport = new GameReport
            {
                platform = nameof(Platform.JOKER),
                report_datetime = startDateTime,
                report_type = (int)GameReport.e_report_type.FinancalReport,
                total_bet = 0,
                total_win = 0,
                total_netwin = 0,
                total_count = 0
            };

            await _gameReportDBService.DeleteGameReport(w1CenterEmptyReport);
            await _gameReportDBService.PostGameReport(w1CenterEmptyReport);

            return;
        }
        // 遊戲商的每小時鐘匯總報表(遊戲商的欄位格式)
        //var jokerSummaryReport = new t_joker_game_report()
        //{
        //    Time = startDateTime,
        //    Amount = list.Sum(x => x.Amount),
        //    Result = list.Sum(x => x.Result),
        //    Count = list.Count
        //};

        //await _jokerDBService.DeleteJokerReport(jokerSummaryReport);
        //await _jokerDBService.PostJokerReport(jokerSummaryReport);

        // 遊戲商的每小時匯總報表(轉帳中心的欄位格式)
        var summaryReport = new GameReport
        {
            platform = nameof(Platform.JOKER),
            report_datetime = startDateTime,
            report_type = (int)GameReport.e_report_type.FinancalReport,
            total_bet = list.Sum(x => x.Amount),
            //Result已經包含JP在內要再另外扣除
            total_win = list.Sum(x => x.Amount) + list.Sum(x => x.Result) - jp,
            total_netwin = list.Sum(x => x.Result),
            total_count = list.Count
        };
        await _gameReportDBService.DeleteGameReport(summaryReport);
        await _gameReportDBService.PostGameReport(summaryReport);
    }

    /// <summary>
    /// 新增 W1小時匯總帳
    /// </summary>
    /// <returns></returns>
    public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
    {
        // 每小時投注資料
        var result = await _jokerDBService.SumJokerBetRecordByBetTime(startDateTime, endDateTime);


        GameReport reportData = new();
        reportData.platform = nameof(Platform.JOKER);
        reportData.report_datetime = startDateTime;
        reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
        reportData.total_bet = result.totalBetValid;
        reportData.total_win = result.totalBetValid + result.totalNetWin;
        reportData.total_netwin = result.totalNetWin + result.Jackpotwin;
        reportData.total_count = result.totalCount;

        await _gameReportDBService.DeleteGameReport(reportData);
        await _gameReportDBService.PostGameReport(reportData);
    }

    /// <summary>
    /// GetPlatformType
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public PlatformType GetPlatformType(Platform platform)
    {
        return PlatformType.Electronic;
    }

    /// <summary>
    /// HealthCheck
    /// </summary>
    /// <param name="platform"></param>
    /// <returns></returns>
    public Task HealthCheck(Platform platform)
    {
        return _gameApiService._JokerApi.GetGameListAsync();
    }
}