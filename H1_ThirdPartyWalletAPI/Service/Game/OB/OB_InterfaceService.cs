using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using OBsetup = H1_ThirdPartyWalletAPI.Model.Game.OB.OB;

namespace H1_ThirdPartyWalletAPI.Service.Game.OB
{

    public interface IOBInterfaceService : IGameInterfaceService
    {
        Task<ResCodeBase> PostOBRecord(List<BetHistoryRecordResponse.Record> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class OB_RecordService : IOBInterfaceService
    {
        private readonly ILogger<OB_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IOBDBService _obDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public OB_RecordService(ILogger<OB_RecordService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            ISummaryDBService summaryDBService,
            IDBService dbService,
            IOBDBService obDBService,
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _dbService = dbService;
            _obDBService=obDBService;
            _gameReportDBService = gameReportDBService;
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
                var responseData = await _gameApiService._OBApi.GetbalanceAsync(new GetbalanceReqserver()
                {
                    loginName = platform_user.game_user_id,
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                });

                if (responseData.code != "200")
                {
                    throw new Exception(responseData.message);
                }
                Balance.Amount = responseData.data.balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("OB餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.OB);
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
            if (!OBsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            OBsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);
            try
            {
                var req = new Model.Game.OB.Reqserver.CreateMemberRequest()
                {
                    loginName = Config.CompanyToken.OB_MerchantCode.ToLower() + Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    loginPassword = "!8@8#8",
                    lang = lang == 0 ? OBsetup.lang["en-US"] : lang,
                    oddType = OBsetup.oddType[request.GameConfig["oddType"]],
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                };
                var response = await _gameApiService._OBApi.CreateMemberAsync(req);
                if (response.code == "200" || response.code == "20000")
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.agent_id = Config.CompanyToken.OB_MerchantCode;
                    gameUser.game_user_id = req.loginName;
                    gameUser.game_platform = Platform.OB.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("OB建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "OB " + ex.Message.ToString());
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
                var responseData = await _gameApiService._OBApi.DepositAsync(new depositReqserver
                {
                    loginName = platform_user.game_user_id,
                    amount = RecordData.amount,
                    transferNo = RecordData.id.ToString(),
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                });

                if (responseData.code != "200")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("OB Deposit: {Message}", responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("OB TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("OB Deposit: {Message}", ex.Message);
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
                var responseData = await _gameApiService._OBApi.WithdrawAsync(new WithdrawReqserver
                {
                    loginName = platform_user.game_user_id,
                    amount = RecordData.amount,
                    transferNo = RecordData.id.ToString(),
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                });

                if (responseData.code != "200")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("OB Withdraw : {ex}", responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("OB TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("OB Withdraw : {ex}", ex.Message);
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
            OBsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);
            //Step 3 Get Game URL
            FastGameReqserver UrlRequest = new FastGameReqserver();
            UrlRequest.loginName = platformUser.game_user_id;
            UrlRequest.loginPassword = "!8@8#8";
            UrlRequest.lang = lang == 0 ? OBsetup.lang["en-US"] : lang;
            UrlRequest.oddType = OBsetup.oddType[request.GameConfig["oddType"]];
            UrlRequest.deviceType = request.GameConfig["device"] == "DESKTOP" ? 1 : 2;//網頁板web:手機板app
            UrlRequest.showExit = 1;
            UrlRequest.gameTypeId = request.GameConfig["gameCode"] == "-1" ? null : request.GameConfig["gameCode"];
            UrlRequest.timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.backurl = request.GameConfig["lobbyURL"];
            }

            try
            {
                var token_res = await _gameApiService._OBApi.FastGameAsync(UrlRequest);
                return token_res.data.url;
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

            var OBReuslt = await _gameApiService._OBApi.TransferAsync(new TransferReqserver
            {
                transferNo = transfer_record.id.ToString(),
                loginName = Config.CompanyToken.OB_MerchantCode.ToLower() + Config.OneWalletAPI.Prefix_Key + transfer_record.Club_id,
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()

            });
            if (OBReuslt.code == "200")
            {
                if (transfer_record.target == nameof(Platform.OB))//轉入OB直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.OB))
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
                if (transfer_record.target == nameof(Platform.OB))//轉入OB直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.OB))
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
            IEnumerable<dynamic> ob_results = await _obDBService.GetOBRecordsBySummary(RecordReq);

            ob_results = ob_results.OrderByDescending(e => e.netAt);

            res.Data = ob_results.ToList();
            return res;
        }
        /// <summary>
        /// 取得遊戲住單明細-轉跳
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 補單-會總
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            RepairReq.EndTime = RepairReq.EndTime.AddDays(1);
            RepairReq.StartTime = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, 0, 0, 0);
            RepairReq.EndTime = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, 0, 0, 0);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 30)
            {
                endTime = startTime.AddMinutes(30);
                RepairCount += await RepairOB(startTime, endTime);
                startTime = endTime;
                await Task.Delay(4000);
            }
            RepairCount += await RepairOB(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Live;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._OBApi.GetbalanceAsync(new GetbalanceReqserver()
            {
                loginName = "HealthCheck",
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            });
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<ResCodeBase> PostOBRecord(List<BetHistoryRecordResponse.Record> recordData)
        {
            ResCodeBase res = new ResCodeBase();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, BetHistoryRecordResponse.Record>> linqRes = recordData.GroupBy(x => x.playerName);

            foreach (IGrouping<string, BetHistoryRecordResponse.Record> group in linqRes)
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
                            club_id = group.Key.Substring(Config.CompanyToken.OB_MerchantCode.Length + 3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.OB);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No OB user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<BetHistoryRecordResponse.Record> betDetailData = new List<BetHistoryRecordResponse.Record>();
                            foreach (BetHistoryRecordResponse.Record item in group)
                            {

                                DateTime drawtime = DateTimeOffset.FromUnixTimeMilliseconds(item.netAt).ToLocalTime().DateTime;


                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.OB);
                                sumData.Game_type = item.gameTypeId;
                                sumData.ReportDatetime = new DateTime(drawtime.Year, drawtime.Month, drawtime.Day, drawtime.Hour, (drawtime.Minute / 5) * 5, 0);

                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                        }
                                    }
                                }
                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                    //合併處理
                                    sumData = await Calculate(conn, tran, sumData, item);
                                    summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    sw.Stop();
                                    _logger.LogDebug("get summary record ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                                    sw.Restart();
                                    if (results.Count() == 0) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = await Calculate(conn, tran, sumData, item);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = await Calculate(conn, tran, sumData, item);
                                    }
                                    summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
                                }
                                item.summary_id = sumData.id;

                                switch (item.betFlag)
                                {

                                    case 0:
                                    case 1:
                                    case 2:
                                    case 4:
                                        betDetailData.Add(item);
                                        break;
                                    default:
                                        break;
                                }

                            }
                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                if (s.Value.RecordCount > 0)
                                {
                                    summaryList.Add(s.Value);
                                }
                            }
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            if (betDetailData.Count > 0)
                            {
                                int PostRecordResult = await _obDBService.PostOBRecord(conn, tran, betDetailData);
                            }
                            tran.Commit();
                            sw.Stop();
                            _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                            sw.Restart();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

                            _logger.LogError("Run OB record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return res;
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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create OB game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計
                ReportAgentReqserver req = new ReportAgentReqserver()
                {
                    startDate = startDateTime.ToString("yyyyMMdd"),
                    endDate = startDateTime.ToString("yyyyMMdd"),
                    pageIndex = 1,
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
                };

                //取得這小時
                ReportAgentResponse OBCenterList = await _gameApiService._OBApi.ReportAgentAsync(req);
                if (OBCenterList.data.record.Length == 0|| OBCenterList.code=="92222")
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.OB),
                        report_datetime = DateTime.Parse(reportTime.ToString("yyyy-MM-dd")),
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
                        platform = nameof(Platform.OB),
                        report_datetime = DateTime.Parse(reportTime.ToString("yyyy-MM-dd")),
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = (OBCenterList.data.record[0].betAmount) * 5,
                        total_win = (OBCenterList.data.record[0].betAmount + OBCenterList.data.record[0].netAmount) * 5,
                        total_netwin = (OBCenterList.data.record[0].netAmount) * 5,
                        total_count = OBCenterList.data.record[0].betCount,
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddDays(1);
                }
                await Task.Delay(65000);
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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create OB game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _obDBService.SumOBBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.OB);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalBetValid + totalWin;
                reportData.total_netwin = totalWin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }
        private async Task<BetRecordSummary> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, BetHistoryRecordResponse.Record r)
        {
            r.pre_betAmount = r.betAmount;
            r.pre_netAmount = r.netAmount;
            r.pre_validBetAmount = r.validBetAmount;
            r.pre_payAmount = r.payAmount;

            switch (r.betFlag)
            {
                case 1:
                case 2:
                case 4:
                    var oldRecords = await _obDBService.GetOBRecords(r.id);
                    oldRecords ??= new();
                    if (oldRecords.Any(oldr => new { oldr.id, oldr.netAt, oldr.updatedAt }.Equals(new { r.id, netAt = DateTimeOffset.FromUnixTimeMilliseconds(r.netAt).LocalDateTime, updatedAt = DateTimeOffset.FromUnixTimeMilliseconds(r.updatedAt).LocalDateTime })))
                    {
                        r.betFlag = -1;
                        return SummaryData;
                    }

                    if (oldRecords.Any())
                    {
                        var lastRecord = oldRecords.OrderByDescending(r => r.updatedAt).First(); //僅需沖銷最後一筆即可
                        r.betAmount = r.betAmount - lastRecord.pre_betAmount;
                        r.netAmount = r.netAmount - lastRecord.pre_netAmount;
                        r.validBetAmount = r.validBetAmount - lastRecord.pre_validBetAmount;
                        r.payAmount = r.payAmount - lastRecord.pre_payAmount;

                    }
                    break;
            }
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.betAmount;
            SummaryData.Turnover += r.validBetAmount;
            SummaryData.Netwin += r.netAmount;
            SummaryData.Win += r.payAmount;
            SummaryData.updatedatetime = DateTime.Now;
            SummaryData.JackpotWin = 0;
            Translate(r);
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
        private async Task<int> RepairOB(DateTime startTime, DateTime endTime)
        {

            var Page = 1;
            var req = new BetHistoryRecordReqserver
            {
                startTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = endTime.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                pageIndex = Page,
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            };

            List<BetHistoryRecordResponse.Record> res = new List<BetHistoryRecordResponse.Record>();
            while (true)
            {
                req.pageIndex = Page;
                var betLogs = await _gameApiService._OBApi.BetHistoryRecordAsync(req);

                if (betLogs.data.pageSize == 0)
                {
                    break;
                }
                res.AddRange(betLogs.data.record);

                Page++;
                if (Page > betLogs.data.totalPage)
                    break;
                //api建議3秒爬一次
                await Task.Delay(4000);
            }
            res = res.ToList();
            var w1CenterList = await _obDBService.GetOBRecordsBytime(startTime, endTime);
            List<BetHistoryRecordResponse.Record> repairList = new List<BetHistoryRecordResponse.Record>();

            foreach (var item in res)
            {
                var hasData = w1CenterList.Where(x => x.id == item.id).Any();
                if (hasData == false)
                {
                    repairList.Add(item);
                }

            }
            if (repairList.Count != 0)
            {
                await PostOBRecord(repairList);
            }


            return repairList.Count;
        }

        #endregion
        private void Translate(BetHistoryRecordResponse.Record r)
        {
            try
            {
                string[] Array = r.addstr2.Split(';');
                if (r.gameTypeName.Contains("百家乐"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {

                        Array[i] = Array[i].Replace("庄", OBsetup.Bacc["庄"]);


                        Array[i] = Array[i].Replace("闲", OBsetup.Bacc["闲"]);

                    }
                    r.betPointName = OBsetup.Bacc[r.betPointName];
                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("龙虎"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {

                        Array[i] = Array[i].Replace("龙", OBsetup.LongHu["龙"]);

                        Array[i] = Array[i].Replace("虎", OBsetup.LongHu["虎"]);

                    }
                    r.betPointName = OBsetup.LongHu[r.betPointName];
                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("轮盘"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("单", OBsetup.LunPan["单"]);
                        Array[i] = Array[i].Replace("双", OBsetup.LunPan["双"]);
                        Array[i] = Array[i].Replace("大", OBsetup.LunPan["大"]);
                        Array[i] = Array[i].Replace("小", OBsetup.LunPan["小"]);
                        Array[i] = Array[i].Replace("红", OBsetup.LunPan["红"]);
                        Array[i] = Array[i].Replace("黑", OBsetup.LunPan["黑"]);
                        Array[i] = Array[i].Replace("第一打", OBsetup.LunPan["第一打"]);
                        Array[i] = Array[i].Replace("第二打", OBsetup.LunPan["第二打"]);
                        Array[i] = Array[i].Replace("第三打", OBsetup.LunPan["第三打"]);
                    }
                    r.betPointName = r.betPointName.Replace("单", OBsetup.LunPan["单"]);
                    r.betPointName = r.betPointName.Replace("双", OBsetup.LunPan["双"]);
                    r.betPointName = r.betPointName.Replace("大", OBsetup.LunPan["大"]);
                    r.betPointName = r.betPointName.Replace("小", OBsetup.LunPan["小"]);
                    r.betPointName = r.betPointName.Replace("红", OBsetup.LunPan["红"]);
                    r.betPointName = r.betPointName.Replace("黑", OBsetup.LunPan["黑"]);
                    r.betPointName = r.betPointName.Replace("第一打", OBsetup.LunPan["第一打"]);
                    r.betPointName = r.betPointName.Replace("第二打", OBsetup.LunPan["第二打"]);
                    r.betPointName = r.betPointName.Replace("第三打", OBsetup.LunPan["第三打"]);
                    r.betPointName = r.betPointName.Replace("打一", OBsetup.LunPan["打一"]);
                    r.betPointName = r.betPointName.Replace("打二", OBsetup.LunPan["打二"]);
                    r.betPointName = r.betPointName.Replace("打三", OBsetup.LunPan["打三"]);
                    r.betPointName = r.betPointName.Replace("直注", OBsetup.LunPan["直注"]);
                    r.betPointName = r.betPointName.Replace("分注", OBsetup.LunPan["分注"]);
                    r.betPointName = r.betPointName.Replace("街注", OBsetup.LunPan["街注"]);
                    r.betPointName = r.betPointName.Replace("三数", OBsetup.LunPan["三数"]);
                    r.betPointName = r.betPointName.Replace("四个号码", OBsetup.LunPan["四个号码"]);
                    r.betPointName = r.betPointName.Replace("角注", OBsetup.LunPan["角注"]);
                    r.betPointName = r.betPointName.Replace("线注", OBsetup.LunPan["线注"]);
                    r.betPointName = r.betPointName.Replace("列一", OBsetup.LunPan["列一"]);
                    r.betPointName = r.betPointName.Replace("列二", OBsetup.LunPan["列二"]);
                    r.betPointName = r.betPointName.Replace("列三", OBsetup.LunPan["列三"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("21点"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("庄", OBsetup.Blackjack["庄"]);

                        Array[i] = Array[i].Replace("闲", OBsetup.Blackjack["闲"]);
                    }


                    r.betPointName = r.betPointName.Replace("庄", OBsetup.Blackjack["庄"]);
                    r.betPointName = r.betPointName.Replace("闲", OBsetup.Blackjack["闲"]);
                    r.betPointName = r.betPointName.Replace("21+3", OBsetup.Blackjack["21+3"]);
                    r.betPointName = r.betPointName.Replace("完美对子", OBsetup.Blackjack["完美对子"]);
                    r.betPointName = r.betPointName.Replace("保险", OBsetup.Blackjack["保险"]);
                    r.betPointName = r.betPointName.Replace("旁注", OBsetup.Blackjack["旁注"]);
                    r.betPointName = r.betPointName.Replace("底注", OBsetup.Blackjack["底注"]);
                    r.betPointName = r.betPointName.Replace("分牌", OBsetup.Blackjack["分牌"]);
                    r.betPointName = r.betPointName.Replace("加倍", OBsetup.Blackjack["加倍"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("牌九"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("顺门", OBsetup.PaiGowPok["顺门"]);
                        Array[i] = Array[i].Replace("出门", OBsetup.PaiGowPok["出门"]);
                        Array[i] = Array[i].Replace("到门", OBsetup.PaiGowPok["到门"]);
                        Array[i] = Array[i].Replace("庄门", OBsetup.PaiGowPok["庄门"]);
                    }
                    r.betPointName = r.betPointName.Replace("顺门赢", OBsetup.PaiGowPok["顺门赢"]);
                    r.betPointName = r.betPointName.Replace("顺门输", OBsetup.PaiGowPok["顺门输"]);
                    r.betPointName = r.betPointName.Replace("出门赢", OBsetup.PaiGowPok["出门赢"]);
                    r.betPointName = r.betPointName.Replace("出门输", OBsetup.PaiGowPok["出门输"]);
                    r.betPointName = r.betPointName.Replace("到门赢", OBsetup.PaiGowPok["到门赢"]);
                    r.betPointName = r.betPointName.Replace("到门输", OBsetup.PaiGowPok["到门输"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("骰宝"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("和值", OBsetup.ShaiZi["和值"]);
                        Array[i] = Array[i].Replace("大", OBsetup.ShaiZi["大"]);
                        Array[i] = Array[i].Replace("小", OBsetup.ShaiZi["小"]);
                        Array[i] = Array[i].Replace("单", OBsetup.ShaiZi["单"]);
                        Array[i] = Array[i].Replace("双", OBsetup.ShaiZi["双"]);
                    }

                    r.betPointName = r.betPointName.Replace("和值", OBsetup.ShaiZi["和值"]);
                    r.betPointName = r.betPointName.Replace("大", OBsetup.ShaiZi["大"]);
                    r.betPointName = r.betPointName.Replace("小", OBsetup.ShaiZi["小"]);
                    r.betPointName = r.betPointName.Replace("单", OBsetup.ShaiZi["单"]);
                    r.betPointName = r.betPointName.Replace("双", OBsetup.ShaiZi["双"]);
                    r.betPointName = r.betPointName.Replace("围骰", OBsetup.ShaiZi["围骰"]);
                    r.betPointName = r.betPointName.Replace("全围", OBsetup.ShaiZi["全围"]);
                    r.betPointName = r.betPointName.Replace("单点", OBsetup.ShaiZi["单点"]);
                    r.betPointName = r.betPointName.Replace("对子", OBsetup.ShaiZi["对子"]);
                    r.betPointName = r.betPointName.Replace("牌九式", OBsetup.ShaiZi["牌九式"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("三公"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("庄", OBsetup.ThreeTrumps["庄"]);
                        Array[i] = Array[i].Replace("闲", OBsetup.ThreeTrumps["闲"]);
                        Array[i] = Array[i].Replace("赢", OBsetup.ThreeTrumps["赢"]);
                        Array[i] = Array[i].Replace("输", OBsetup.ThreeTrumps["输"]);
                        Array[i] = Array[i].Replace("单公", OBsetup.ThreeTrumps["单公"]);
                        Array[i] = Array[i].Replace("双公", OBsetup.ThreeTrumps["双公"]);
                        Array[i] = Array[i].Replace("三公", OBsetup.ThreeTrumps["三公"]);
                    }

                    r.betPointName = r.betPointName.Replace("闲1对牌以上", OBsetup.ThreeTrumps["闲1对牌以上"]);
                    r.betPointName = r.betPointName.Replace("闲2对牌以上", OBsetup.ThreeTrumps["闲2对牌以上"]);
                    r.betPointName = r.betPointName.Replace("闲3对牌以上", OBsetup.ThreeTrumps["闲3对牌以上"]);
                    r.betPointName = r.betPointName.Replace("庄", OBsetup.ThreeTrumps["庄"]);
                    r.betPointName = r.betPointName.Replace("闲", OBsetup.ThreeTrumps["闲"]);
                    r.betPointName = r.betPointName.Replace("赢", OBsetup.ThreeTrumps["赢"]);
                    r.betPointName = r.betPointName.Replace("输", OBsetup.ThreeTrumps["输"]);
                    r.betPointName = r.betPointName.Replace("单公", OBsetup.ThreeTrumps["单公"]);
                    r.betPointName = r.betPointName.Replace("双公", OBsetup.ThreeTrumps["双公"]);
                    r.betPointName = r.betPointName.Replace("三公", OBsetup.ThreeTrumps["三公"]);
                    r.betPointName = r.betPointName.Replace("和", OBsetup.ThreeTrumps["和"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("牛牛"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("庄", OBsetup.Niuniu["庄"]);
                        Array[i] = Array[i].Replace("闲", OBsetup.Niuniu["闲"]);
                        Array[i] = Array[i].Replace("牛", OBsetup.Niuniu["牛"]);
                        Array[i] = Array[i].Replace("无", OBsetup.Niuniu["无"]);
                        Array[i] = Array[i].Replace("一", OBsetup.Niuniu["一"]);
                        Array[i] = Array[i].Replace("二", OBsetup.Niuniu["二"]);
                        Array[i] = Array[i].Replace("三", OBsetup.Niuniu["三"]);
                        Array[i] = Array[i].Replace("四", OBsetup.Niuniu["四"]);
                        Array[i] = Array[i].Replace("五", OBsetup.Niuniu["五"]);
                        Array[i] = Array[i].Replace("六", OBsetup.Niuniu["六"]);
                        Array[i] = Array[i].Replace("七", OBsetup.Niuniu["七"]);
                        Array[i] = Array[i].Replace("八", OBsetup.Niuniu["八"]);
                        Array[i] = Array[i].Replace("九", OBsetup.Niuniu["九"]);
                    }


                    r.betPointName = r.betPointName.Replace("庄", OBsetup.Niuniu["庄"]);
                    r.betPointName = r.betPointName.Replace("闲", OBsetup.Niuniu["闲"]);
                    r.betPointName = r.betPointName.Replace("平倍", OBsetup.Niuniu["平倍"]);
                    r.betPointName = r.betPointName.Replace("翻倍", OBsetup.Niuniu["翻倍"]);


                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("番摊"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("和值", OBsetup.FanTan["和值"]);
                        Array[i] = Array[i].Replace("单", OBsetup.FanTan["单"]);
                        Array[i] = Array[i].Replace("双", OBsetup.FanTan["双"]);

                    }


                    r.betPointName = r.betPointName.Replace("和值", OBsetup.FanTan["和值"]);
                    r.betPointName = r.betPointName.Replace("单", OBsetup.FanTan["单"]);
                    r.betPointName = r.betPointName.Replace("双", OBsetup.FanTan["双"]);
                    r.betPointName = r.betPointName.Replace("番", OBsetup.FanTan["番"]);
                    r.betPointName = r.betPointName.Replace("念", OBsetup.FanTan["念"]);
                    r.betPointName = r.betPointName.Replace("角", OBsetup.FanTan["角"]);
                    r.betPointName = r.betPointName.Replace("四通", OBsetup.FanTan["四通"]);
                    r.betPointName = r.betPointName.Replace("三通", OBsetup.FanTan["三通"]);
                    r.betPointName = r.betPointName.Replace("二通", OBsetup.FanTan["二通"]);
                    r.betPointName = r.betPointName.Replace("一通", OBsetup.FanTan["一通"]);
                    r.betPointName = r.betPointName.Replace("三门", OBsetup.FanTan["三门"]);


                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("德州扑克"))
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("庄", OBsetup.Texas["庄"]);
                        Array[i] = Array[i].Replace("闲", OBsetup.Texas["闲"]);

                    }

                    r.betPointName = r.betPointName.Replace("底注", OBsetup.Texas["底注"]);
                    r.betPointName = r.betPointName.Replace("跟注", OBsetup.Texas["跟注"]);
                    r.betPointName = r.betPointName.Replace("AA边注", OBsetup.Texas["AA边注"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("安达巴哈"))
                {

                    r.betPointName = r.betPointName.Replace("安达", OBsetup.AndarBahar["安达"]);
                    r.betPointName = r.betPointName.Replace("巴哈", OBsetup.AndarBahar["巴哈"]);

                    for (int i = 0; i < Array.Length; i++)
                    {

                        Array[i] = OBsetup.CardType[Array[i]];

                    }

                    r.addstr2 = String.Join(";", Array);

                }

                if (r.gameTypeName.Contains("色碟"))
                {

                    r.betPointName = r.betPointName.Replace("大", OBsetup.SeDie["大"]);
                    r.betPointName = r.betPointName.Replace("小", OBsetup.SeDie["小"]);
                    r.betPointName = r.betPointName.Replace("单", OBsetup.SeDie["单"]);
                    r.betPointName = r.betPointName.Replace("双", OBsetup.SeDie["双"]);

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName.Contains("印度炸金花"))
                {

                    r.betPointName = OBsetup.TeenPatti[r.betPointName];

                    r.addstr2 = String.Join(";", Array);
                }

                if (r.gameTypeName == "炸金花")
                {
                    for (int i = 0; i < Array.Length; i++)
                    {
                        Array[i] = Array[i].Replace("龙", OBsetup.Winthreecards["龙"]);
                        Array[i] = Array[i].Replace("凤", OBsetup.Winthreecards["凤"]);
                        Array[i] = Array[i].Replace("对子", OBsetup.Winthreecards["对子"]);
                        Array[i] = Array[i].Replace("散牌", OBsetup.Winthreecards["散牌"]);

                    }
                    r.betPointName = OBsetup.Winthreecards[r.betPointName];

                    r.addstr2 = String.Join(";", Array);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
