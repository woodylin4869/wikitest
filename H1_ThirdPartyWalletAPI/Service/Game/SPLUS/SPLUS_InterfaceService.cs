using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.SPLUS;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Request;
using ThirdPartyWallet.Share.Model.Game.SPLUS.Response;
using static Google.Rpc.Context.AttributeContext.Types;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using SPLUSsetup = ThirdPartyWallet.Share.Model.Game.SPLUS.SPLUS;

namespace H1_ThirdPartyWalletAPI.Service.Game.SPLUS
{
    public interface ISPLUS_InterfaceService : IGameInterfaceService
    {
        Task<int> PostSPLUSRecord(List<BetlogResponse.Page_Info> recordData);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task<List<BetlogResponse.Page_Info>> Getbetlog(BetlogRequest req);

    }
    public class SPLUS_InterfaceService : ISPLUS_InterfaceService
    {
        private readonly ILogger<SPLUS_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly ISPLUSDBService _SPLUSDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly ISPLUSApiService _SPLUSApiService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        public SPLUS_InterfaceService(ILogger<SPLUS_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            ISPLUSDBService SPLUSDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            ISPLUSApiService SPLUSApiService)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _SPLUSDBService = SPLUSDBService;
            _SPLUSApiService = SPLUSApiService;
        }
        #region 進線
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
                var responseData = await _SPLUSApiService.Wallet(new WalletRequest
                {
                    account =Config.OneWalletAPI.Prefix_Key + platform_user.club_id.ToString()
                });

                if (responseData.status.code != "1")
                {
                    throw new Exception(SPLUSsetup.ErrorCode[responseData.status.code]);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.data.balance), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("SPLUS餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.SPLUS);

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
                var responseData = await _SPLUSApiService.Logout(new LogoutRequest
                {
                    account =Config.OneWalletAPI.Prefix_Key + platform_user.club_id.ToString()
                });
                if (responseData.status.code != "1")
                {
                    _logger.LogError("SPLUS KickUser: {Message}", SPLUSsetup.ErrorCode[responseData.status.code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SPLUS KickUser: {Message}", ex.Message);
            }
            return true;
        }

        /// <summary>
        /// 全館踢線
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platform_user"></param>
        /// <returns></returns>
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
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
            try
            {
                GamePlatformUser User = new GamePlatformUser();
                var responseData = await _SPLUSApiService.Player(new CreateRequest
                {
                    account = Config.OneWalletAPI.Prefix_Key + request.Club_id.ToString(),
                    nickname = Config.OneWalletAPI.Prefix_Key + request.Club_id.ToString(),
                    currency = SPLUSsetup.Currency[userData.Currency]
                });
                if (responseData.status.code == "1" || responseData.status.code == "2001")
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = Config.OneWalletAPI.Prefix_Key + request.Club_id;
                    gameUser.game_platform = Platform.SPLUS.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(SPLUSsetup.ErrorCode[responseData.status.code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SPLUS建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "SPLUS " + ex.Message.ToString());
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
                var responseData = await _SPLUSApiService.Deposit(new DepositRequest
                {
                    account = platform_user.game_user_id.ToString(),
                    amount = Math.Round(RecordData.amount,2),
                    transaction_id = RecordData.id.ToString()
                });

                if (responseData.status.code != "1")
                {
                    RecordData.status = nameof(TransferStatus.fail);
                    _logger.LogError("SPLUS Deposit: {Message}", SPLUSsetup.ErrorCode[responseData.status.code]);
                }
                else
                {
                    RecordData.status = nameof(TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("SPLUS TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("SPLUS Deposit: {Message}", ex.Message);
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
                var responseData = await _SPLUSApiService.Withdraw(new WithdrawRequest
                {
                    account = platform_user.game_user_id,
                    transaction_id = RecordData.id.ToString(),
                    amount = Math.Round(RecordData.amount, 2)
                });

                if (responseData.status.code != "1")
                {
                    RecordData.status = nameof(TransferStatus.fail);
                    _logger.LogError("SPLUS Withdraw : {ex}", SPLUSsetup.ErrorCode[responseData.status.code]);
                }
                else
                {
                    RecordData.status = nameof(TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("SPLUS TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(TransferStatus.pending);
                _logger.LogError("SPLUS Withdraw : {ex}", ex.Message);
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
            try
            {
                //沒有的語系就帶英文
                SPLUSsetup.lang.TryGetValue(request.GameConfig["lang"], out var langValue);
                GetlinkRequest req = new GetlinkRequest 
                {
                    account = platformUser.game_user_id,
                    gamecode = request.GameConfig["gameCode"],
                    returnurl = request.GameConfig["lobbyURL"],
                    lang = langValue ?? SPLUSsetup.lang["en-US"]
                };

                var res = await _SPLUSApiService.GameLink(req);
             
                if (res.status.code != "1")
                {
                    throw new Exception(SPLUSsetup.ErrorCode[res.status.code]);
                }
                return res.data.URL;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "SPLUS: " + ex.Message.ToString());
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

            var Reuslt = await _SPLUSApiService.Transaction(new TransferRequest
            {
                transaction_id = transfer_record.id.ToString()
            });
            if (Reuslt.status.code == "1")
            {
                if (transfer_record.target == nameof(Platform.SPLUS))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.SPLUS))
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = transfer_record.status = nameof(TransferStatus.success);
                transfer_record.success_datetime = DateTime.Now;
            }
            else
            {
                if (transfer_record.target == nameof(Platform.SPLUS))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.SPLUS))
                {
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = nameof(TransferStatus.fail);
                transfer_record.success_datetime = DateTime.Now;
                transfer_record.after_balance = transfer_record.before_balance;
            }
            CheckTransferRecordResponse.TRecord = transfer_record;
            return CheckTransferRecordResponse;
        }

        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Task HealthCheck(Platform platform)
        {
            WalletRequest req = new WalletRequest();
            return _SPLUSApiService.Wallet(req);
        }
        #endregion 進線

        #region 報表
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostSPLUSRecord(List<BetlogResponse.Page_Info> recordData)
        {
            var betRecords = recordData.Where(x => x.account.ToLower().StartsWith(Config.OneWalletAPI.Prefix_Key.ToLower())&& x.status =="1")
                                                  .OrderBy(x => x.bet_time).ToList();
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
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute / 5 * 5, 0);
                        List<BetlogResponse.Page_Info> betDetailData = new List<BetlogResponse.Page_Info>();

                        foreach (BetlogResponse.Page_Info item in group)
                        {
                            item.report_time = reportTime;
                            item.partition_time = item.bet_time;

                            betDetailData.Add(item);

                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }
                            dic[summaryTime].Add(item.bet_time.ToString("yyyy-MM-dd HH:mm"));
                        }

                        foreach (var item in dic)
                        {
                            foreach (var subItem in item.Value)
                            {
                                var key = nameof(Platform.SPLUS) + $"{RedisCacheKeys.BetSummaryTime}:{item.Key}";
                                await _commonService._cacheDataService.ListPushAsync(key, subItem);
                            }
                        }

                        if (betDetailData.Count > 0)
                        {
                            postResult += await _SPLUSDBService.PostSPLUSRecord(conn, tran, betDetailData);
                        }

                        tran.Commit();
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
        /// 補單-會總
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 30)
            {
                endTime = startTime.AddMinutes(30);
                RepairCount += await Repair(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await Repair(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
            await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }


        /// <summary>
        /// 取得遊戲第二層注單明細
        /// </summary>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var batRecords = new List<dynamic>();
            GetBetRecord res = new();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _SPLUSDBService.GetSPLUSRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                results = results.OrderByDescending(e => e.partition_time).ToList();
                batRecords.AddRange(results);  // 直接添加 BetRecord 列表
            }

            res.Data = batRecords.OrderByDescending(e => e.partition_time).Select(obj => (dynamic)new RespRecordLevel2_Electronic
            {
                RecordId = obj.bet_id.ToString(),
                BetTime = obj.bet_time,
                GameType = "slot",
                GameId = obj.gamecode,
                BetAmount = obj.bet_amount,
                NetWin = obj.pay_off_amount,
                Jackpot = obj.jp_win,
                BetStatus = obj.status,
                SettleTime = obj.pay_off_time,
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
            //沒有的語系就帶英文
            SPLUSsetup.lang.TryGetValue(RecordDetailReq.lang, out var langValue);
            var res = await _SPLUSApiService.Playcheck(new PlaycheckRequest
            {
                bet_id = RecordDetailReq.record_id,
                lang = langValue ?? SPLUSsetup.lang["en-US"]
            });

            if (res.status.code != "1")
            {
                throw new Exception(SPLUSsetup.ErrorCode[res.status.code]);
            }
            return res.data.result;

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
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create SPLUS game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _SPLUSApiService.Betlog_total(new TotalBetlogRequest()
                {
                    start_time = startDateTime.ToString("yyyy-MM-ddTHH"),
                    end_time = startDateTime.AddHours(1).ToString("yyyy-MM-ddTHH"),
                });
                var gameEmptyReport = new GameReport();
                if (req.data.Bet_quantity == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.SPLUS);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {
                    gameEmptyReport.platform = nameof(Platform.SPLUS);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = req.data.bet_amount;
                    gameEmptyReport.total_win = req.data.bet_amount + req.data.pay_off_amount;
                    gameEmptyReport.total_netwin = req.data.pay_off_amount + req.data.jp_win;
                    gameEmptyReport.total_count = req.data.Bet_quantity;
                }
                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
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
                _logger.LogDebug("Create SPLUS game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin, totalnetwin) = await _SPLUSDBService.SumSPLUSBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.SPLUS);
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
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            // 每日統計
            BetlogRequest req = new BetlogRequest()
            {
                start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                end_time = endTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                page = 1,
                page_size = 2000
            };
            var res = await Getbetlog(req);
            if (!res.Any())
            {
                return 0;
            }

            return await PostSPLUSRecord(res);
        }
        /// <summary>
        /// 到遊戲商拉取住單
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<List<BetlogResponse.Page_Info>> Getbetlog(BetlogRequest req)
        {
            var res = new List<BetlogResponse.Page_Info>();
            var Page = 1;
            req.time_type = 1;
            while (true)
            {
                req.page = Page;
                var betLogs = await _SPLUSApiService.Betlog(req);
                if (betLogs.data.total == 0)
                {
                    break;
                }
                res.AddRange(betLogs.data.page_info);
                if (Page >= betLogs.data.last_page)
                    break;
                Page++;
                await Task.Delay(1000);
            }
            return res;
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
            var summaryRecords = await _SPLUSDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.SPLUS);
                summaryData.Game_type = 3;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win) + summaryRecord.Sum(x => x.bet);
                summaryData.Netwin = summaryRecord.Sum(x => x.win);
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
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }
        #endregion



    }
}
