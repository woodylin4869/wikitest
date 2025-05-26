using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Request;
using H1_ThirdPartyWalletAPI.Model.Game.BTI.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using BTIConfig = H1_ThirdPartyWalletAPI.Model.Game.BTI.BTI;

namespace H1_ThirdPartyWalletAPI.Service.Game.BTI;

public interface IBTIInterfaceService : IGameInterfaceService
{
    Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    Task<List<Bets>> CallBTIRecord(DateTime startDateTime, DateTime endDateTime);
    Task<int> PostBTIRecord(List<Bets> betInfos);
    Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
}

public class BTI_InterfaceService : IBTIInterfaceService
{
    private readonly ILogger<BTI_InterfaceService> _logger;
    private readonly IBTIApiService _apiService;
    private readonly IBTIDBService _btiDBService;
    private readonly IDBService _dbService;
    private readonly ISummaryDBService _summaryDBService;
    private readonly ICacheDataService _cacheService;
    private readonly IGamePlatformUserService _gamePlatformUserService;
    private readonly IGameReportDBService _gameReportDBService;
    private const int _cacheSeconds = 600;
    private const int _cacheFranchiserUser = 1800;
    private const int _page = 0;
    private const int _rowperpage = 1000;
    private readonly string _prefixKey;
    private readonly string _urlGame;

    public BTI_InterfaceService(ILogger<BTI_InterfaceService> logger,
                                IBTIApiService apiService,
                                IDBService dbService,
                                ICacheDataService cacheService,
                                IGamePlatformUserService gamePlatformUserService,
                                IBTIDBService btiDBService,
                                ISummaryDBService summaryDBService,
                                IGameReportDBService gameReportDBService)
    {
        _logger = logger;
        _apiService = apiService;
        _btiDBService = btiDBService;
        _dbService = dbService;
        _summaryDBService = summaryDBService;
        _cacheService = cacheService;
        _gamePlatformUserService = gamePlatformUserService;
        _gameReportDBService = gameReportDBService;
        _urlGame = Config.GameAPI.BTI_GAME_URL;
        Config.CompanyToken.BTI_ForTestAccount ??= "false";
        if (Config.CompanyToken.BTI_ForTestAccount.ToLower() == "true")
        {
            // 因廠商無測試環境，僅提供正式環境，所以我方測試站介接的會員ID請幫帶入前綴test 以方便區分是測試帳號
            _prefixKey = "test_" + Config.OneWalletAPI.Prefix_Key;
        }
        else
        {
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
        }
    }

    public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
    {
        var checkTransferRecordResponse = new CheckTransferRecordResponse();

        var req = new CheckTransactionRequest()
        {
            RefTransactionCode = transfer_record.id.ToString() //ConvertToNumericUuid(transfer_record.id)
        };

        var response = await _apiService.CheckTransactionAsync(req);

        if (response.ErrorCode == BTIConfig.WalletErrorCode["NoError"])
        {
            if (transfer_record.target == nameof(Platform.BTI))//轉入BTI直接改訂單狀態為成功
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
        else if (response.ErrorCode == BTIConfig.WalletErrorCode["TransactionCodeNotFound"])
        {
            if (transfer_record.target == nameof(Platform.BTI))//轉入BTI直接改訂單狀態為失敗
            {
                checkTransferRecordResponse.CreditChange = transfer_record.amount;
                checkTransferRecordResponse.LockCreditChange = -transfer_record.amount;

            }
            else if (transfer_record.source == nameof(Platform.BTI))
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
        // 確認幣別
        if (!BTIConfig.Currency.ContainsKey(userData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        // 請求建帳號 確保 MerchantCustomerCode 跟 LoginName 都要帶一樣
        string userId = _prefixKey + request.Club_id;
        var req = new CreateUserNewRequest()
        {
            MerchantCustomerCode = userId,
            LoginName = userId,
            CurrencyCode = BTIConfig.Currency[userData.Currency],
            CountryCode = "TH",                                     // 国家必须英文 ISO 3166-1 标准. 如 CN 
            City = "city",                                          // 城市，可以中文
            FirstName = userData.Club_Ename,                        // 姓，可以中文
            LastName = userData.Club_Ename,                         // 名，可以中文
            Group1ID = (int)BTIConfig.Group1ID.NewUser,
            CustomerMoreInfo = "",                                  // 请带参数然后留空
            CustomerDefaultLanguage = BTIConfig.Lang["en-US"],      // 玩家语言 ISO 639-1. 标准如 zh
            DomainID = "",                                          // 请带参数然后留空
            DateOfBirth = ""                                        // 日/月/年份– 生日日期来确定年龄过 18, 可留空              
        };

        try
        {
            var response = await _apiService.CreateUserNewAsync(req);
            if (response.ErrorCode == BTIConfig.WalletErrorCode["NoError"] ||
                response.ErrorCode == BTIConfig.WalletErrorCode["DuplicateMerchantCustomerCode"])
            {
                var gameUser = new GamePlatformUser();
                gameUser.club_id = userData.Club_id;
                gameUser.game_user_id = _prefixKey + request.Club_id;
                gameUser.game_platform = Platform.BTI.ToString();
                return gameUser;
            }
            else
            {
                throw new ExceptionMessage(ResponseCode.CreateWalletFail, response.ErrorCode);
            }
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.CreateWalletFail, ex.Message.ToString());
        }
    }

    public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
    {
        try
        {
            // 進遊戲語系
            BTIConfig.Lang.TryGetValue(request.GameConfig["lang"], out var lang);
            lang ??= BTIConfig.Lang["en-US"];

            // 請求token
            var req = new GetCustomerAuthTokenRequest()
            {
                MerchantCustomerCode = platformUser.game_user_id,
            };

            var response = await _apiService.GetCustomerAuthTokenAsync(req);
            if (response.ErrorCode != BTIConfig.WalletErrorCode["NoError"])
                throw new ExceptionMessage(ResponseCode.GetGameURLFail, response.ErrorCode);

            // https://prod20279-122711001.442hattrick.com/zh/sports?operatorToken=
            var url = _urlGame + "/" + lang + "/sports?operatorToken=" + response.AuthToken;
            return url;
        }
        catch (Exception ex)
        {
            throw new ExceptionMessage(ResponseCode.GetGameURLFail, ex.Message.ToString());
        }
    }

    public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!BTIConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            // todo: 小數點?位
            var req = new TransferToWHLRequest()
            {
                MerchantCustomerCode = platform_user.game_user_id,
                RefTransactionCode = RecordData.id.ToString(),
                Amount = RecordData.amount,
                BonusCode = "" // Not use, please pass empty string. 
            };

            var response = await _apiService.TransferToWHLAsync(req);

            if (response.ErrorCode == BTIConfig.WalletErrorCode["NoError"])
            {
                RecordData.status = nameof(TransferStatus.success);
            }
            else // 廠商只有成功 其他一律是Exception 交易單號重複也是回Exception 留給pending機制再去查
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("{paltform} Deposit fail: game_user_id = {user}, id = {id}, response = {code}", Platform.BTI, req.MerchantCustomerCode, req.RefTransactionCode, response.ErrorCode);
            }
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Deposit exception: {ex}", Platform.BTI, ex);
        }

        return RecordData.status;
    }

    public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
    {
        if (!BTIConfig.Currency.ContainsKey(walletData.Currency))
            throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

        try
        {
            // todo: 小數點?位
            var req = new TransferFromWHLRequest()
            {
                MerchantCustomerCode = platform_user.game_user_id,
                RefTransactionCode = RecordData.id.ToString(),
                Amount = RecordData.amount
            };

            var response = await _apiService.TransferFromWHLAsync(req);

            if (response.ErrorCode == BTIConfig.WalletErrorCode["NoError"])
            {
                RecordData.status = nameof(TransferStatus.success);
            }
            else // 廠商只有成功 其他一律是Exception 交易單號重複也是回Exception 留給pending機制再去查
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("{paltform} Withdraw fail: game_user_id = {user}, id = {id}, response = {code}", Platform.BTI, req.MerchantCustomerCode, req.RefTransactionCode, response.ErrorCode);
            }
        }
        catch (Exception ex)
        {
            RecordData.status = nameof(TransferStatus.pending);
            _logger.LogError(ex, "{paltform} Withdraw exception: {ex}", Platform.BTI, ex);
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
            batRecords.AddRange(await _btiDBService.GetBTIRecordsBySummary(new()
            {
                summary_id = RecordReq.summary_id,
                ReportTime = betTime
            }));
        }

        batRecords = batRecords.OrderByDescending(e => e.UpdateDate).ToList();
        res.Data = batRecords.Select(b =>
        {
            // 比如串關沒遊戲ID時 前端要null不要0
            b.BranchID = (b.BranchID == 0 ? null : b.BranchID);
            // 第三層輸出的結算時間UpdateDate為null時 表示為未結算
            b.UpdateDate = (b.BetStatus == nameof(BTIConfig.BetStatus.Opened) ? null : b.UpdateDate);
            return b;
        }).ToList();
        return res;
    }

    public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
    {
        GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
        IEnumerable<dynamic> bti_results = await _btiDBService.GetBTIRunningRecord(RecordReq);
        bti_results = bti_results.OrderByDescending(e => e.UpdateDate);
        res.Data = bti_results.ToList();
        return res;
    }

    public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
    {
        var memberBalance = new MemberBalance();
        try
        {
            var req = new GetBalanceRequest()
            {
                MerchantCustomerCode = platform_user.game_user_id
            };

            var result = await _apiService.GetBalanceAsync(req);

            memberBalance.Amount = result.Balance;
        }
        catch (Exception ex)
        {
            memberBalance.Amount = 0;
            memberBalance.code = (int)ResponseCode.Fail;
            memberBalance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
            _logger.LogError(ex, "{platform} 餘額取得失敗 Msg: {Message}", Platform.BTI, ex.Message);
        }

        memberBalance.Wallet = nameof(Platform.BTI);
        return memberBalance;
    }

    public async Task<bool> KickAllUser(Platform platform)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
    {
        return true;
    }

    public async Task<List<Bets>> CallBTIRecord(DateTime startDateTime, DateTime endDateTime)
    {
        var betInfos = new List<Bets>();

        // todo : +redis
        // 請求拉單時token
        var requestToken = await _apiService.AuthorizeV2DefaultAsync();
        string token = requestToken.token;

        // 請求已結算單 request
        var requestHistory = new GetHistoryBetsPagingRequest()
        {
            From = startDateTime.AddHours(-8), //要系統時間後在-8
            To = endDateTime.AddHours(-8),
            Pagination = new Pagination()
            {
                page = _page,
                rowperpage = _rowperpage
            }
        };

        // 請求未結算單 request
        var requestOpen = new GetOpenBetsPagingRequest()
        {
            From = startDateTime.AddHours(-8), //要系統時間後在-8
            To = endDateTime.AddHours(-8),
            Pagination = new Pagination()
            {
                page = _page,
                rowperpage = _rowperpage
            }
        };

        // 請求已結算
        #region 請求已結算
        while (true)
        {
            var responseHistory = await _apiService.GetHistoryBetsPagingAsync(requestHistory, token);

            if (responseHistory.Bets.Count > 0)
            {
                betInfos.AddRange(responseHistory.Bets);
            }

            // 沒資料了跳出
            if (responseHistory.Bets.Count == 0) break;

            // 下一頁
            requestHistory.Pagination.page++;
        }

        // 本次撈取到廠商已結算的單號
        // var existsPurchaseID = betInfos.Select(b => new { b.PurchaseID }).ToHashSet();
        #endregion

        // 請求未結算
        #region 請求未結算
        while (true)
        {
            var responseOpen = await _apiService.GetOpenBetsPagingAsync(requestOpen, token);

            if (responseOpen.Bets.Count > 0)
            {
                betInfos.AddRange(
                    responseOpen.Bets
                // 若撈到未結算...依已撈到已結算為主... 但不能這樣做 Partial Cash Out 部分提前兑现 會有相同PurchaseID同時出現在結算跟未結算
                // ActionType – 结算类型 (freebet 免费下注, RiskFreebet 无风险下注, Real 下注, Cashout 提前兑现, Partial Cash Out 部分提前兑现)
                // .Where(x => !existsPurchaseID.Contains(new { x.PurchaseID })).ToList()
                );
            }

            // 沒資料了跳出
            if (responseOpen.Bets.Count == 0) break;

            // 下一頁
            requestOpen.Pagination.page++;
        }
        #endregion

        return betInfos
            .Where(l => l.MerchantCustomerID.ToLower().StartsWith(_prefixKey.ToLower()))
            .ToList();
    }

    public async Task<int> PostBTIRecord(List<Bets> betInfos)
    {
        if (!betInfos.Any()) return 0;

        // 跟BTI請求起訖時間 拉單回的資料是SearchDateTime範圍內的 所以w1也要查同SearchDateTime範圍內的已存在單!? 目前先沒用..有遇到SearchDateTime不一樣但PK一樣重複
        // 廠商是+0 轉+8
        var existsHistoryPK = (await _btiDBService.GetBTIHistoryBetPKByBetTime(betInfos.Min(b => b.CreationDate.AddHours(8)), betInfos.Max(b => b.CreationDate.AddHours(8))))
            .Select(b => new { b.PurchaseID, b.BetStatus, b.CreationDate, b.UpdateDate })
            .ToHashSet();

        _logger.LogDebug("PostBTIRecord -> betInfos {betInfos_count}: {betInfos}, existsHistoryPK {existsHistoryPK_count}: {existsHistoryPK}", betInfos.Count, betInfos, existsHistoryPK.Count, existsHistoryPK);

        var postResult = 0;
        using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
        {
            await conn.OpenAsync();
            foreach (var logGroup in betInfos
                .Where(x => x.MerchantCustomerID.StartsWith(_prefixKey))
                .GroupBy(x => x.MerchantCustomerID)
                )
            {
                using (var tran = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // 移除環境別前贅字
                        var club_id = logGroup.Key[_prefixKey.Length..];
                        Wallet memberWalletData = await GetWalletCache(club_id);
                        if (memberWalletData == null || memberWalletData.Club_id == null)
                        {
                            throw new Exception("沒有會員id");
                        }

                        var gameUser = await _gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.BTI);
                        if (gameUser == null || gameUser.game_user_id != logGroup.Key)
                        {
                            throw new Exception("No BTI user");
                        }

                        //寫入 匯總
                        Dictionary<string, BetRecordSummary> summaryData = new();

                        //寫入 單筆注單
                        List<Bets> betLogs = new();

                        // t_summary_bet_record_mapping
                        var summaryBetRecordMappings = new HashSet<t_summary_bet_record_mapping>();

                        // 轉換注單內容
                        var bets = logGroup
                        .OrderBy(b => b.UpdateDate)
                        .Select(b =>
                        {
                            b.club_id = memberWalletData.Club_id;
                            b.franchiser_id = memberWalletData.Franchiser_id;
                            // 特別處理未結算狀態時統一欄位值為 -> ex: 下注10, 有效10, 輸贏0, 淨輸贏-10
                            // BTI未結算時欄位原始值 -> TotalStake = 10, ValidStake = 0, PL = 0
                            // BTI未結算時欄位破壞值 -> TotalStake = 10, ValidStake = 10, PL = -10
                            b.ValidStake = (b.BetStatus == nameof(BTIConfig.BetStatus.Opened)) ? b.TotalStake : b.ValidStake;
                            b.PL = (b.BetStatus == nameof(BTIConfig.BetStatus.Opened)) ? -b.TotalStake : b.PL;
                            // 體育注單有未結算跟改牌邏輯，需先儲存原始資料
                            b.pre_TotalStake = b.TotalStake;
                            b.pre_ValidStake = b.ValidStake;
                            b.pre_PL = b.PL;
                            b.pre_Return = b.Return;
                            b.pre_BetStatus = b.BetStatus;
                            // 廠商是+0 轉+8 
                            b.BetSettledDate = b.BetSettledDate.AddHours(8);
                            b.CreationDate = b.CreationDate.AddHours(8);
                            b.UpdateDate = b.UpdateDate.AddHours(8);
                            b.SearchDateTime = b.SearchDateTime.AddHours(8);
                            return b;
                        });

                        foreach (Bets r in bets) //loop club id bet detail
                        {
                            // 已結算 排除重複
                            if (!existsHistoryPK.Add(new { r.PurchaseID, r.BetStatus, r.CreationDate, r.UpdateDate })) continue;

                            // 取非串關字串寫入 遊戲名/聯賽名/主隊名/客隊名/下注名
                            // 若 Selections 不只一組 兩組以上就不寫入... 多P也不知取誰為主
                            if (r.Selections.Count == 1)
                            {
                                var selection = r.Selections.FirstOrDefault();
                                r.BranchID = selection.BranchID;
                                r.BranchName = selection.BranchName;
                                r.LeagueName = selection.LeagueName;
                                r.HomeTeam = selection.HomeTeam;
                                r.AwayTeam = selection.AwayTeam;
                                r.YourBet = selection.YourBet;
                            }
                            else // 串關
                            {
                                r.BranchID = 0;
                                r.BranchName = " - ";
                                r.LeagueName = "Parlay (" + r.Selections.Count + ")";
                                r.HomeTeam = " - ";
                                r.AwayTeam = " - ";
                                r.YourBet = " - ";
                            }

                            BetRecordSummary sumData = new BetRecordSummary();
                            sumData.Club_id = memberWalletData.Club_id;
                            sumData.Game_id = nameof(Platform.BTI);
                            sumData.Game_type = 0;
                            DateTime tempDateTime = DateTime.Now;
                            sumData.ReportDatetime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day, tempDateTime.Hour, (tempDateTime.Minute / 5) * 5, 0);
                            //先確認有沒有符合的匯總單
                            if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                            {
                                sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                //合併處理
                                sumData = await Calculate(tran, sumData, r);
                                summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                            }
                            else
                            {
                                //用Club_id與ReportDatetime DB取得彙總注單
                                IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                if (results.Count() == 0) //沒資料就建立新的
                                {
                                    //建立新的Summary
                                    sumData.Currency = memberWalletData.Currency;
                                    sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                    //合併處理
                                    sumData = await Calculate(tran, sumData, r);
                                }
                                else //有資料就更新
                                {
                                    sumData = results.SingleOrDefault();
                                    //合併處理
                                    sumData = await Calculate(tran, sumData, r);
                                }
                                summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
                            }
                            r.summary_id = sumData.id;

                            betLogs.Add(r);

                            var mapping = new t_summary_bet_record_mapping()
                            {
                                summary_id = sumData.id,
                                report_time = sumData.ReportDatetime.Value,
                                partition_time = r.CreationDate.Date
                            };
                            summaryBetRecordMappings.Add(mapping);
                        }

                        //寫入匯總帳
                        if (summaryData.Any())
                        {
                            List<BetRecordSummary> summaryList = summaryData.Values.ToList();
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);

                            //寫入匯總帳對應
                            await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMappings);
                        }

                        //寫入未結算單
                        if (betLogs.Any(b => b.BetStatus == nameof(BTIConfig.BetStatus.Opened)))
                            await _btiDBService.PostBTIRecordRunning(tran, betLogs.Where(b => b.BetStatus == nameof(BTIConfig.BetStatus.Opened)));

                        //寫入明細帳
                        if (betLogs.Any())
                            postResult += await _btiDBService.PostBTIRecord(tran, betLogs);

                        //刪除已結算之未結算單
                        foreach (var settleRecord in betLogs.Where(b => b.BetStatus != nameof(BTIConfig.BetStatus.Opened)))
                            await _btiDBService.DeleteBTIRecordRunning(tran, settleRecord.PurchaseID, settleRecord.CreationDate);

                        _logger.LogDebug("insert {platform} record member: {group}, count: {count}", Platform.BTI, logGroup.Key,
                            betLogs.Count);
                        await tran.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync();

                        foreach (Bets r in logGroup) //loop club id bet detail
                        {
                            _logger.LogError("PurchaseID : {PurchaseID}, BetStatus: {BetStatus}, CreationDate: {CreationDate}, UpdateDate: {UpdateDate}", r.PurchaseID, r.BetStatus, r.CreationDate, r.UpdateDate);
                        }
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError(ex, "Run {platform} record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                            Platform.BTI, logGroup.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                    }
                }
            }

            await conn.CloseAsync();
        }

        return postResult;
    }

    /// <summary>
    /// 計算彙總
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="SummaryData"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    private async Task<BetRecordSummary> Calculate(IDbTransaction tran, BetRecordSummary SummaryData, Bets r)
    {
        var oldRecords = await _btiDBService.GetBTIRecordsPreAmountByPurchaseId(tran, r.PurchaseID, r.CreationDate);
        if (oldRecords.Any())
        {
            var lastRecord = oldRecords.OrderByDescending(r => r.UpdateDate).First(); //僅需沖銷最後一筆即可
            // r.BetStatus = lastRecord.pre_BetStatus; // 此欄屬於PK 不可被替換
            r.TotalStake -= lastRecord.pre_TotalStake;
            r.ValidStake -= lastRecord.pre_ValidStake;
            r.PL -= lastRecord.pre_PL;
            r.Return -= lastRecord.pre_Return;
        }

        SummaryData.RecordCount++;
        SummaryData.Bet_amount += r.TotalStake;
        SummaryData.Turnover += r.ValidStake;
        SummaryData.Netwin += r.PL;
        SummaryData.Win += r.Return;
        SummaryData.updatedatetime = DateTime.Now;
        return SummaryData;
    }

    /// <summary>
    /// 補單 廠商沒限制起訖時間範圍 就不loop切時間拉 直接給廠商分頁查
    /// </summary>
    /// <param name="RepairReq"></param>
    /// <returns></returns>
    public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
    {
        #region 補注單

        var postResult = 0;

        // 調用廠商拉帳 BTI 是 ( 起始時間 <= 要查的範圍 <= 結束時間)
        var betInfos = await CallBTIRecord(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));

        if (betInfos.Any())
        {
            postResult = await PostBTIRecord(betInfos);
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
        #endregion

        #region 重產匯總帳
        // 有新補單才需要重產匯總帳
        if (postResult > 0)
        {
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
        }
        #endregion

        return $"Game: {Platform.BTI} 新增資料筆數: {postResult}";
    }

    /// <summary>
    /// 補單 loop 每30分鐘
    /// </summary>
    /// <param name="RepairReq"></param>
    /// <returns></returns>
    public async Task<string> RepairGameRecordLoop30(RepairBetSummaryReq RepairReq)
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

            // 調用廠商拉帳 BTI 是 ( 起始時間 <= 要查的範圍 <= 結束時間)
            var betInfos = await CallBTIRecord(start, end.AddSeconds(-1));

            if (betInfos.Any())
            {
                postResult += await PostBTIRecord(betInfos);
            }

            start = end;
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
        #endregion

        #region 重產匯總帳
        await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
        #endregion

        return $"Game: {Platform.BTI} 新增資料筆數: {postResult}";
    }

    /// <summary>
    /// 小時帳 遊戲廠商
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <returns></returns>
    public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
    {
        // nothing
        await Task.Delay(1);
    }

    /// <summary>
    /// 小時帳 w1
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
            _logger.LogDebug("Create {platform} SummaryW1Report {datetime}", Platform.BTI, reportTime);
            var (totalCount, totalBetValid, totalWin) = await _btiDBService.SumBTIBetRecordByCreationdate(reportTime, reportTime.AddHours(1));

            GameReport reportData = new();
            reportData.platform = nameof(Platform.BTI);
            reportData.report_datetime = reportTime;
            reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
            reportData.total_bet = totalBetValid;
            reportData.total_win = totalWin + totalBetValid;
            reportData.total_netwin = totalWin;
            reportData.total_count = totalCount;

            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);

            startDateTime = startDateTime.AddHours(1);
            await Task.Delay(1000);
        }
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

    public PlatformType GetPlatformType(Platform platform)
    {
        return PlatformType.Sport;
    }

    public Task HealthCheck(Platform platform)
    {
        return _apiService.GetBalanceAsync(new()
        {
            MerchantCustomerCode = "HealthCheck"
        });
    }
}
