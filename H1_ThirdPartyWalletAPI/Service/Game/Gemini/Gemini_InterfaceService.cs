using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using Npgsql;
using System.Data;
using System.Linq;
using Geminisetup = ThirdPartyWallet.Share.Model.Game.Gemini.Gemini;
using H1_ThirdPartyWalletAPI.Model.Config;
using ThirdPartyWallet.Share.Model.Game.Gemini.Request;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using ThirdPartyWallet.Share.Model.Game.Gemini.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.Gemini
{
    public interface IGeminiInterfaceService : IGameInterfaceService
    {
        Task<int> PostGeminiRecord(List<BetlistResponse.Datalist> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class Gemini_InterfaceService : IGeminiInterfaceService
    {
        private readonly ILogger<Gemini_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IGeminiDBService _geminiDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IGeminiApiService _geminiApiService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        public Gemini_InterfaceService(ILogger<Gemini_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IGeminiDBService geminiDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IGeminiApiService geminiApiService)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _geminiDBService = geminiDBService;
            _geminiApiService = geminiApiService;
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
                var responseData = await _geminiApiService.GetBalanceAsync(new GetBalanceRequest
                {
                    username = platform_user.game_user_id,
                });

                if (responseData.code != 0)
                {
                    throw new Exception(Geminisetup.ErrorCode[responseData.code]);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.data.thb), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("gemini餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.GEMINI);
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
            return true;
        }
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
            if (!Geminisetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreateplayerRequest()
                {
                    username = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    default_currency = userData.Currency
                };


                var response = await _geminiApiService.CreateplayerAsync(req);
                if (response.code == 0 || response.code == 12005)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.username;
                    gameUser.game_platform = Platform.GEMINI.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(Geminisetup.ErrorCode[response.code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GEMINI建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "GEMINI " + ex.Message.ToString());
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
                var responseData = await _geminiApiService.TransferinAsync(new TransferinRequest
                {
                    username = platform_user.game_user_id,
                    transfer_id = RecordData.id.ToString(),
                    amount = RecordData.amount.ToString(),
                    currency = "THB"
                });

                if (responseData.code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("Gemini Deposit: {Message}", Geminisetup.ErrorCode[responseData.code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Gemini TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Gemini Deposit: {Message}", ex.Message);
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
                var responseData = await _geminiApiService.TransferoutAsync(new TransferoutRequest
                {
                    username = platform_user.game_user_id,
                    transfer_id = RecordData.id.ToString(),
                    amount = RecordData.amount.ToString(),
                    currency = "THB"
                });

                if (responseData.code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("Gemini Withdraw : {ex}", Geminisetup.ErrorCode[responseData.code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Gemini TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Gemini Withdraw : {ex}", ex.Message);
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
            Geminisetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            LaunchRequest UrlRequest = new LaunchRequest
            {
                username = platformUser.game_user_id,
                gametype = request.GameConfig["gameCode"],
                lang = lang ?? Geminisetup.lang["en-US"]
            };
            //if (request.GameConfig.ContainsKey("lobbyURL"))
            //{
            //    UrlRequest.returnurl = request.GameConfig["lobbyURL"];
            //}

            try
            {
                var res = await _geminiApiService.LaunchAsync(UrlRequest);
                if (res.code != 0)
                {
                    throw new Exception(Geminisetup.ErrorCode[res.code]);
                }
                return res.data.url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "Gemini: " + ex.Message.ToString());
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

            var Reuslt = await _geminiApiService.QueryorderAsync(new QueryorderRequest
            {
                transfer_id = transfer_record.id.ToString()
            });
            if (Reuslt.code == 0)
            {
                if (transfer_record.target == nameof(Platform.GEMINI))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.GEMINI))
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
                if (transfer_record.target == nameof(Platform.GEMINI))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.GEMINI))
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
            //var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _geminiDBService.GetGeminiRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                results = results.OrderByDescending(e => e.createtime).ToList();
                foreach (var result in results)
                {
                    batRecords.Add(result);

                }
            }

            res.Data = batRecords.OrderByDescending(e => e.createtime).ToList();
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
            var data = await _geminiDBService.GetGeminiRecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            var Settled = data.Where(x => x.billstatus == "Settled").First();
            if (Settled == null)
            {
                throw new Exception("未結算無法轉跳廠商URL");
            }

            GamedetailRequest request = new GamedetailRequest()
            {
                username = Settled.username,
                gamecode = Settled.gamecode,
            };
            var url = await _geminiApiService.GamedetailAsync(request);

            return url.data;
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
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostGeminiRecord(List<BetlistResponse.Datalist> recordData)
        {
            List<string> NOT_Status = new List<string>();
            NOT_Status.Add("Abnormal");
            NOT_Status.Add("Rollback");
            recordData = recordData.Where(x => !NOT_Status.Contains(x.billstatus)).ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, BetlistResponse.Datalist>> linqRes = recordData.GroupBy(x => x.username);

            var postResult = 0;
            foreach (IGrouping<string, BetlistResponse.Datalist> group in linqRes)
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            sw.Stop();
                            _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                            sw.Restart();
                            string club_id;
                            club_id = group.Key.Substring(3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.GEMINI);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No GEMINI user");
                            }
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            List<BetlistResponse.Datalist> betDetailData = new List<BetlistResponse.Datalist>();

                            List<BetlistResponse.Datalist> betDetailDataRunning = new List<BetlistResponse.Datalist>();
                            foreach (BetlistResponse.Datalist item in group)
                            {
                                item.report_time = reportTime;

                                await Calculate(conn, tran, item);


                                switch (item.billstatus)
                                {
                                    case "Unsettlement":
                                        item.club_id = memberWalletData.Club_id;
                                        item.franchiser_id = memberWalletData.Franchiser_id;
                                        betDetailDataRunning.Add(item);
                                        break;
                                    case "F":
                                    case "Abnormal":
                                    case "Rollback":
                                        break;
                                    default:
                                        betDetailData.Add(item);
                                        break;
                                }

                                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }

                                dic[summaryTime].Add(DateTimeOffset.FromUnixTimeMilliseconds(item.createtime).ToOffset(TimeSpan.FromHours(8)).DateTime.ToString("yyyy-MM-dd HH:mm"));
                            }


                            if (betDetailDataRunning.Count > 0)
                            {
                                await _geminiDBService.PostGeminiRunningRecord(conn, tran, betDetailDataRunning);
                                postResult += await _geminiDBService.PostGeminiRecord(conn, tran, betDetailDataRunning);
                            }

                            if (betDetailData.Count > 0)
                            {
                                postResult += await _geminiDBService.PostGeminiRecord(conn, tran, betDetailData);
                            }
                            tran.Commit();

                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = $"{RedisCacheKeys.GeminiBetSummaryTime}:{item.Key}";
                                    await _commonService._cacheDataService.ListPushAsync(key, subItem);
                                }
                            }

                            sw.Stop();
                            _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                            sw.Restart();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

                            _logger.LogError("Run Gemini record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                        }
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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create GEMINI game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _geminiApiService.BetlistAsync(new BetlistRequest()
                {
                    timetype = "Create",
                    begintime = (long)(startDateTime - unixEpoch).TotalMilliseconds,
                    endtime = (long)(startDateTime.AddHours(1).AddMilliseconds(-1) - unixEpoch).TotalMilliseconds,
                    page = 0,
                    num = 1
                });
                var gameEmptyReport = new GameReport();
                if (req.data.total == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.GEMINI);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {

                    gameEmptyReport.platform = nameof(Platform.GEMINI);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = (decimal)req.data.totalbet;
                    gameEmptyReport.total_win = (decimal)(req.data.totalbet + req.data.totalwin);
                    gameEmptyReport.total_netwin = (decimal)req.data.totalwin;
                    gameEmptyReport.total_count = req.data.total;
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
                _logger.LogDebug("Create GEMINI game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin, totalnetwin) = await _geminiDBService.SumGeminiBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.GEMINI);
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
        /// 統計5分鐘
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="SummaryData"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, BetlistResponse.Datalist r)
        {
            if (r.billstatus == "Unsettlement")
            {
                r.pre_betamount = r.betamount;
                r.pre_turnover = r.betamount;
                r.pre_wonamount = r.wonamount;
                r.pre_winLose = r.wonamount - r.betamount;

                r.turnover = r.betamount;
                r.wonamount = 0;
                r.winLose = r.wonamount - r.betamount;
            }
            else
            {
                r.pre_betamount = r.betamount;
                r.pre_wonamount = r.wonamount;
                r.pre_turnover = r.turnover;
                r.pre_winLose = r.winLose;
            }



            var Record = await _geminiDBService.GetGeminiRecords(r.billNo, DateTimeOffset.FromUnixTimeMilliseconds(r.createtime).ToOffset(TimeSpan.FromHours(8)).DateTime);

            Record ??= new();
            if (Record.Any(x => new { x.billNo, x.createtime, x.billstatus }.Equals(new { r.billNo, createtime = DateTimeOffset.FromUnixTimeMilliseconds(r.createtime).ToOffset(TimeSpan.FromHours(8)).DateTime, r.billstatus })))
            {
                r.billstatus = "F";
                return;
            }
            if (Record.Any())
            {
                var lastpp = Record.OrderByDescending(x => x.reckontime).First();

                r.betamount = r.betamount - lastpp.pre_betamount;
                r.wonamount = r.wonamount - lastpp.pre_wonamount;
                r.turnover = r.turnover - lastpp.pre_turnover;
                r.winLose = r.winLose - lastpp.pre_winLose;
            }
            await _geminiDBService.DeleteGeminiRunningRecord(conn, tran, r);
        }

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> Repair(DateTime startTime, DateTime endTime)
        {

            var w1data = await _geminiDBService.GetGeminiRecordsBycreatetime(startTime, endTime);

            var resPK = w1data.Select(x => new { x.billNo, x.billstatus }).ToHashSet();


            var res = new List<BetlistResponse.Datalist>();
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            // 每日統計
            BetlistRequest req = new BetlistRequest()
            {
                timetype = "Create",
                begintime = (long)(startTime - unixEpoch).TotalMilliseconds,
                endtime = (long)(endTime - unixEpoch).TotalMilliseconds,
                page = 0,
                num = 1000
            };
            var Page = 0;

            while (true)
            {
                req.page = Page;
                var betLogs = await _geminiApiService.BetlistAsync(req);

                if (betLogs.data.total == 0)
                {
                    break;
                }
                foreach (var itme in betLogs.data.datalist)
                {
                    if (resPK.Add(new { itme.billNo, itme.billstatus }))
                    {
                        res.Add(itme);
                    }
                }

                if (Page > betLogs.data.total / 1000)
                    break;

                Page++;

                await Task.Delay(1000);
            }

            if (res.Count == 0)
            {
                return 0;
            }

            return await PostGeminiRecord(res);
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
            var summaryRecords = await _geminiDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.GEMINI);
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

        #endregion
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
            return _geminiApiService.healthcheckAsync();
        }
        /// <summary>
        /// 取得位結算
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> bti_results = await _geminiDBService.GetGeminiRunningRecord(RecordReq);
            bti_results = bti_results.OrderByDescending(e => e.createtime);
            res.Data = bti_results.ToList();
            return res;
        }
    }
}
