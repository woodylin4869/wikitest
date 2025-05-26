using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.VA;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.VA;
using ThirdPartyWallet.Share.Model.Game.VA.Request;
using ThirdPartyWallet.Share.Model.Game.VA.Response;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using VAsetup = ThirdPartyWallet.Share.Model.Game.VA.VA;


namespace H1_ThirdPartyWalletAPI.Service.Game.VA
{
    public interface IVAInterfaceService : IGameInterfaceService
    {
        Task<List<Betlog>> GetGameBetlogFunc(DateTime startTime, DateTime endTime);
        Task<int> PostVARecord(List<Betlog> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class VA_InterfaceService : IVAInterfaceService
    {
        private readonly ILogger<VA_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IVADBService _VADBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IOptions<VAConfig> _options;
        private readonly IVAApiService _VAApiService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        public VA_InterfaceService(ILogger<VA_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IVADBService VADBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IOptions<VAConfig> options,
            IVAApiService VAApiService,
            ICacheDataService cacheService)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _VADBService = VADBService;
            _options = options;
            _VAApiService = VAApiService;
            _cacheService = cacheService;
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
                var responseData = await _VAApiService.GetBalanceAsync(new GetBalanceRequest
                {
                    Account = platform_user.game_user_id
                });

                if (responseData.Status != null && responseData.Status.Code != 0)
                {
                    throw new Exception(VAsetup.ErrorCode[responseData.Status.Code]);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.Data.Balance), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("VA餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.VA);
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
                var responseData = await _VAApiService.KickUserAsync(new KickUserRequest
                {
                    Account = platform_user.game_user_id
                });
                if (responseData.Status != null && responseData.Status.Code != 0)
                {
                    throw new Exception(VAsetup.ErrorCode[responseData.Status.Code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("VA踢線失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "VA " + ex.Message.ToString());
            }
            return true;
        }
        /// <summary>
        /// 全站踢線
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public async Task<bool> KickAllUser(Platform platform)
        {
            return true;
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
            if (!VAsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreateRequest();
                req.Account = Config.OneWalletAPI.Prefix_Key + userData.Club_id;


                var response = await _VAApiService.CreateAsync(req);
                if (response.Status.Code == 0 || response.Status.Code == 202)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.Account;
                    gameUser.game_platform = Platform.VA.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(VAsetup.ErrorCode[response.Status.Code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("VA建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "VA " + ex.Message.ToString());
            }
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
                var responseData = await _VAApiService.DepositAsync(new DepositRequest
                {
                    Account = platform_user.game_user_id,
                    TransactionId = RecordData.id.ToString(),
                    Amount = Math.Round(RecordData.amount, 2)
                });

                if (responseData.Status != null && responseData.Status.Code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("VA Deposit: {Message}", VAsetup.ErrorCode[responseData.Status.Code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("VA TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("VA Deposit: {Message}", ex.Message);
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
                var responseData = await _VAApiService.WithdrawAsync(new WithdrawRequest
                {
                    Account = platform_user.game_user_id,
                    TransactionId = RecordData.id.ToString(),
                    Amount = Math.Round(RecordData.amount, 2)
                });

                if (responseData.Status != null && responseData.Status.Code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("VA Withdraw : {ex}", VAsetup.ErrorCode[responseData.Status.Code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("VA TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("VA Withdraw : {ex}", ex.Message);
            }
            return RecordData.status;
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
            GameLinkRequest UrlRequest = new GameLinkRequest
            {
                Account = platformUser.game_user_id,
                GameId = request.GameConfig["gameCode"],
                App = "N"
            };

            VAsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            // 沒帶值預設給英文
            UrlRequest.Lang = lang ?? VAsetup.lang["en-US"];
            //if (request.GameConfig.ContainsKey("clientIP"))
            //{
            //    UrlRequest.remoteip = request.GameConfig["clientIP"];
            //}
            if (request.GameConfig.ContainsKey("device"))
            {
                UrlRequest.GamePlat = VAsetup.Device[request.GameConfig["device"]];
            }
            else
            {
                UrlRequest.GamePlat = VAsetup.Device["MOBILE"];
            }

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.Url = request.GameConfig["lobbyURL"];
            }
            var game = request.GameConfig["gameCode"];


            try
            {
                var responseData = await _VAApiService.GameLinkAsync(UrlRequest);
                if (responseData.Status != null && responseData.Status.Code != 0)
                {
                    throw new Exception(VAsetup.ErrorCode[responseData.Status.Code]);
                }
                return responseData.Data.url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "VA: " + ex.Message.ToString());
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

            var responseData = await _VAApiService.TransactionDetailAsync(new TransactionDetailRequest
            {
                transactionId = transfer_record.id.ToString(),
            });
            if (responseData.Status.Code == 0 && responseData.Data.Status)
            {

                if (transfer_record.target == nameof(Platform.VA))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.VA))
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
            else
            {
                if (transfer_record.target == nameof(Platform.VA))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.VA))
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
            var batRecords = new List<Betlog>();  // 修改类型为 List<BetRecord>
            var res = new GetBetRecord();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _VADBService.GetVARecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                results = results.OrderByDescending(e => e.BetTime).ToList();
                batRecords.AddRange(results);  // 直接添加 BetRecord 列表
            }
            res.Data = batRecords.OrderByDescending(e => e.BetTime).Select(obj => (dynamic)new RespRecordLevel2_Electronic
            {
                RecordId = obj.BetId.ToString(),
                BetTime = obj.BetTime,
                GameType = obj.BetMode,
                GameId = obj.GameId.ToString(),
                BetAmount = obj.Bet,
                NetWin = obj.WinLose,
                Jackpot = obj.jackpotwin,
                BetStatus = obj.Status.ToString(), // 注單狀態 (0:未派彩, 1:已派彩)
                SettleTime = obj.SettleTime,
            }).ToList();
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
            BetlogDetailRequest request = new BetlogDetailRequest()
            {
                BetId = RecordDetailReq.record_id,
                Lang = RecordDetailReq.lang,
            };
            var responseData = await _VAApiService.BetlogDetailAsync(request);

            if (responseData.Status != null && responseData.Status.Code != 0)
            {
                throw new Exception(VAsetup.ErrorCode[responseData.Status.Code]);
            }
            return responseData.Data.Url;
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
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 15)
            {
                endTime = startTime.AddMinutes(15);
                RepairCount += await Repair(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await Repair(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            DateTime now = DateTime.Now.AddHours(-2);
            if (startTime <= now)
            {
                await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
                await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
            }
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostVARecord(List<Betlog> recordData)
        {
            var betRecords = recordData.Where(x => x.Account.ToLower().StartsWith(Config.OneWalletAPI.Prefix_Key.ToLower()))
                                                  .OrderBy(x => x.CreateTime)
                                                  .ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var postResult = 0;
            foreach (var group in betRecords.Chunk(20000))
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        sw.Stop();
                        _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                        sw.Restart();
                        // 紀錄 reportTime 跟 playTime 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                        List<Betlog> betDetailData = new List<Betlog>();

                        foreach (Betlog item in group)
                        {
                            item.report_time = reportTime;
                            item.partition_time = item.BetTime;

                            betDetailData.Add(item);

                            var formattedDateTime = item.CreateTime.ToString("yyyy-MM-dd HH:mm");
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }
                            dic[summaryTime].Add(formattedDateTime);
                        }


                        // 記錄到 Redis reportTime 跟 Bet_time(下注時間) 的關聯
                        foreach (var item in dic)
                        {
                            var key = $"{RedisCacheKeys.VABetSummaryTime}:{item.Key}";
                            await _commonService._cacheDataService.SortedSetAddAsync(key,
                                item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                        }

                        if (betDetailData.Count > 0)
                        {
                            postResult += await _VADBService.PostVARecord(conn, tran, betDetailData);
                        }
                        await tran.CommitAsync();

                        sw.Stop();
                        _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                        sw.Restart();
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return postResult;
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
                var reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create VA game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                // 每日統計
                var responseData = await _VAApiService.ReportCurrencyAsync(new ReportCurrencyRequest()
                {
                    StartTime = startDateTime.ToString("yyyy-MM-ddTHHzzz"),
                    EndTime = startDateTime.AddHours(1).ToString("yyyy-MM-ddTHHzzz"),
                });

                var gameReport = new GameReport
                {
                    platform = nameof(Platform.VA),
                    report_datetime = reportTime,
                    report_type = (int)GameReport.e_report_type.FinancalReport,
                    total_bet = 0,
                    total_win = 0,
                    total_netwin = 0,
                    total_count = 0
                };

                foreach (var record in responseData.Data.CurrencyList)
                {
                    gameReport.total_bet += (decimal)record.TotalBet;
                    gameReport.total_win += (decimal)record.TotalPayout;
                    gameReport.total_netwin += (decimal)record.TotalWinLose;
                    gameReport.total_count += (long)record.Count;
                }

                await _gameReportDBService.DeleteGameReport(gameReport);
                await _gameReportDBService.PostGameReport(gameReport);
                startDateTime = startDateTime.AddHours(1);

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
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create VA game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin, totalnetwin) = await _VADBService.SumVABetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.VA);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin;
                reportData.total_netwin = totalnetwin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }
        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> Repair(DateTime startTime, DateTime endTime)
        {
            if (DateTime.Compare(startTime, endTime) == 0)
            {
                return 0;
            }
            var w1data = await _VADBService.GetVARecordsBycreatetime(startTime, endTime);
            var w1DataHashSet = w1data.Select(x => new { x.BetId, x.CreateTime }).ToHashSet();

            List<Betlog> gameProviderBetRecords = await GetGameBetlogFunc(startTime, endTime);

            // Filter out game provider bet records that already exist in w1data.
            var uniqueGameProviderBetRecords = gameProviderBetRecords
                .Where(betlog => !w1DataHashSet.Contains(new { betlog.BetId, betlog.CreateTime }))
                .ToList();
            int repairCount = 0;
            if (uniqueGameProviderBetRecords.Count != 0)
            {
                repairCount = await PostVARecord(uniqueGameProviderBetRecords);
            }
            return repairCount;
        }

        public async Task<List<Betlog>> GetGameBetlogFunc(DateTime startTime, DateTime endTime)
        {

            // 取得注單delay時間(毫秒)
            int callAPIdelayMS = 1000;
            int pageIndex = 1;
            int pageSize = 5000;
            // 取得總頁數
            int? pageCount = null;

            List<Betlog> gameProviderBetRecords = new List<Betlog>();

            if (DateTime.Now.Subtract(startTime).TotalHours < 2)
            {
                BetlogListByTimeRequest req = new BetlogListByTimeRequest()
                {
                    StartTime = startTime,
                    EndTime = endTime.AddMilliseconds(-1),
                    PageSize = pageSize,
                };
                do
                {
                    // 設定頁碼
                    req.Page = pageIndex;

                    var responseData = await _VAApiService.BetlogListByTimeAsync(req);
                    if (responseData.Status.Code == 0 && responseData.Data.BetlogList.Count > 0)
                    {
                        foreach (var item in responseData.Data.BetlogList)
                        {
                            gameProviderBetRecords.Add(item);
                        }
                    }


                    pageCount = responseData.Data.LastPage;

                    pageIndex++;
                    await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
                } while (pageCount > pageIndex);
            }
            else
            {
                BetlogHistoryListByTimeRequest req = new BetlogHistoryListByTimeRequest()
                {
                    StartTime = startTime,
                    EndTime = endTime.AddMilliseconds(-1),
                    PageSize = pageSize,
                };
                do
                {
                    // 設定頁碼
                    req.Page = pageIndex;

                    var responseData = await _VAApiService.BetlogHistoryListByTimeAsync(req);
                    if (responseData.Status.Code == 0 && responseData.Data.BetlogList.Count > 0)
                    {
                        foreach (var item in responseData.Data.BetlogList)
                        {
                            gameProviderBetRecords.Add(item);
                        }
                    }
                    pageCount = responseData.Data.LastPage;

                    pageIndex++;
                    await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
                } while (pageCount > pageIndex);
            }

            return gameProviderBetRecords;
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
            var sw2 = System.Diagnostics.Stopwatch.StartNew();
            var sw1 = System.Diagnostics.Stopwatch.StartNew();

            // 將老虎機、魚機記錄好的 reporttime > playtime 取出
            var redisKey = $"{RedisCacheKeys.VABetSummaryTime}:{reportDatetime.ToString("yyyy-MM-dd HH:mm")}";

            // 取得匯總需要的起始和結束時間
            (DateTime StartTime, DateTime EndTime) = await GetRedisRecordSummaryDateTime(redisKey, reportDatetime);

            var summaryRecords = await _VADBService.SummaryGameRecord(reportDatetime, StartTime, EndTime);
            var GrouVAummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid.Substring(3)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();


            foreach (var summaryRecord in GrouVAummary)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key.Substring(3), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.VA);
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

            await _commonService._cacheDataService.KeyDelete(redisKey);
            sw2.Stop();
            _logger.LogInformation("VA summary record 寫入完成時間 {time}, 五分鐘匯總帳時間: {reporttime}, 開始時間: {starttime} 結束時間: {endtime}",
                 sw2.ElapsedMilliseconds,
                reportDatetime,
               StartTime.ToString("yyyy-MM-dd HH:mm"),
               EndTime.ToString("yyyy-MM-dd HH:mm"));
            return true;
        }


        /// <summary>
        /// 取得匯總需要的起始和結束時間
        /// </summary>
        /// <param name="redisKey"></param>
        /// <param name="reportTime">排程執行匯總時間</param>
        /// <returns>匯總需要的起始和結束時間</returns>
        private async Task<(DateTime StartTime, DateTime EndTime)> GetRedisRecordSummaryDateTime(string redisKey, DateTime reportTime)
        {
            DateTime? startTime = null;
            DateTime? endTime = null;



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
        #endregion
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Task HealthCheck(Platform platform)
        {
            healthcheckRequest req = new healthcheckRequest()
            {
            };
            return _VAApiService.healthcheckAsync(req);
        }
    }
}
