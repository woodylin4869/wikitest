using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MP.Response;
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
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using MPsetup = H1_ThirdPartyWalletAPI.Model.Game.MP.MP;

namespace H1_ThirdPartyWalletAPI.Service.Game.MP
{

    public interface IMPInterfaceService : IGameInterfaceService
    {
        Task<ResCodeBase> PostMPRecord(List<MPData> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    }
    public class MP_InterfaceService : IMPInterfaceService
    {
        private readonly ILogger<MP_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IMPDBService _mpDBService;
        private readonly IGameReportDBService _gameReportDBService;

        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public MP_InterfaceService(ILogger<MP_InterfaceService> logger,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IMPDBService mpDBService, 
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _dbService = dbService;
            _mpDBService = mpDBService;
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
                var responseData = await _gameApiService._MPAPI.LnquiryScoreStatusAsync(new LnquiryScoreStatusParam
                {
                    account = platform_user.game_user_id,
                });

                if (responseData.d.code != 0)
                {
                    throw new Exception(responseData.d.code.ToString());
                }
                Balance.Amount = decimal.Parse(responseData.d.freeMoney);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("MP餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.MP);
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
                var responseData = await _gameApiService._MPAPI.KickPlayerOfflineAsync(new KickPlayerOfflineParam()
                {
                    account = platform_user.game_user_id,
                });

                if (responseData.d.code != 0)
                {
                    throw new Exception(responseData.d.code.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出MP使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
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

            if (!MPsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new LoginToPlatformParam()
                {
                    account = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    money = "0",
                    ip = request.GameConfig["clientIP"],
                    KindID = request.GameConfig["gameCode"]
                };


                var response = await _gameApiService._MPAPI.LoginToPlatformAsync(req);
                if (response.d.code == 0)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.account;
                    gameUser.game_platform = Platform.MP.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.d.code.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("MP建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "MP " + ex.Message.ToString());
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
                var responseData = await _gameApiService._MPAPI.FundInAsync(new KFundInParam
                {
                    account = platform_user.game_user_id,
                    orderid = Config.CompanyToken.MP_Id + RecordData.create_datetime.ToString("yyyyMMddHHmmsss") + platform_user.game_user_id,
                    money = RecordData.amount.ToString(),
                });

                if (responseData.d.code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("MP Deposit: {Message}", responseData.d.code.ToString());
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MP TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MP Deposit: {Message}", ex.Message);
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
                var responseData = await _gameApiService._MPAPI.FundOutAsync(new KFundOutParam
                {
                    account = platform_user.game_user_id,
                    orderid = Config.CompanyToken.MP_Id + RecordData.create_datetime.ToString("yyyyMMddHHmmsss") + platform_user.game_user_id,
                    money = RecordData.amount.ToString(),
                });

                if (responseData.d.code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("MP Withdraw : {ex}", responseData.d.code.ToString());
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MP TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MP Withdraw : {ex}", ex.Message);
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
            MPsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            LoginToPlatformParam UrlRequest = new LoginToPlatformParam
            {
                account = platformUser.game_user_id,
                money = "0",
                ip = request.GameConfig["clientIP"],
                KindID = request.GameConfig["gameCode"]
            };

            //if (request.GameConfig.ContainsKey("lobbyURL"))
            //{
            //    UrlRequest.returnurl = request.GameConfig["lobbyURL"];
            //}

            try
            {
                var token_res = await _gameApiService._MPAPI.LoginToPlatformAsync(UrlRequest);
                if (token_res.d.code != 0)
                {
                    throw new Exception(token_res.d.code.ToString());
                }
                lang = lang ?? MPsetup.lang["en-US"];
                var loginurl = token_res.d.url.ToString();

                var langIndex = loginurl.IndexOf("lang=");
                var langEndIndex = langIndex + loginurl[langIndex..].IndexOf("&");
                loginurl = loginurl[..langIndex] + $"lang={lang}" + loginurl[langEndIndex..];
                loginurl += "&returnType=1"; //關閉返回大廳按鈕
                loginurl += $"&returnUrl={request.GameConfig["lobbyURL"]}";

                return loginurl;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "MP: " + ex.Message.ToString());
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

            var Reuslt = await _gameApiService._MPAPI.InquiryaboutOrderStatusAsync(new InquiryaboutOrderparam
            {
                orderid = Config.CompanyToken.MP_Id + transfer_record.create_datetime.ToString("yyyyMMddHHmmsss") + Config.OneWalletAPI.Prefix_Key + transfer_record.Club_id,
            });
            if (Reuslt.d.code == 0 && Reuslt.d.status == 0)
            {
                if (transfer_record.target == nameof(Platform.MP))//轉入MP直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.MP))
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
            else if (Reuslt.d.code == 0 && (Reuslt.d.status == -1 || Reuslt.d.status == 2))
            {
                if (transfer_record.target == nameof(Platform.MP))//轉入MP直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.MP))
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
            IEnumerable<dynamic> results = await _mpDBService.GetMPRecordsBySummary(RecordReq);

            results = results.OrderByDescending(e => e.GameStartTime);

            res.Data = results.ToList();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// 取得自訂第三層
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RCGRowData> GameRowData(GetRowDataReq RecordDetailReq)
        {
            RCGRowData rCGRowData = new RCGRowData();

            List<MPData> results = await _mpDBService.GetMPRecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            if (results.Count == 0)
            {
                throw new Exception("no data");
            }
            List<object> res = new List<object>();
            var data = results.OrderByDescending(x => x.GameStartTime).First();

            res.Add(data);

            rCGRowData.dataList = res;
            return rCGRowData;
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
                RepairCount += await RepairMP(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await RepairMP(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0));
            await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._MPAPI.GetPlatformStatus(new());
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<ResCodeBase> PostMPRecord(List<MPData> recordData)
        {
            ResCodeBase res = new ResCodeBase();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, MPData>> linqRes = recordData.GroupBy(x => x.Accounts);

            foreach (IGrouping<string, MPData> group in linqRes)
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
                            club_id = group.Key.Substring(Config.CompanyToken.MP_Id.Length + Config.OneWalletAPI.Prefix_Key.Length + 1);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.MP);
                            if (gameUser == null || gameUser.game_user_id != group.Key.Substring(Config.CompanyToken.MP_Id.Length + 1))
                            {
                                throw new Exception("No MP user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<MPData> betDetailData = new List<MPData>();
                            foreach (MPData item in group)
                            {
                                DateTime drawtime = DateTime.Now;

                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.MP);
                                sumData.Game_type = 5;
                                sumData.ReportDatetime = new DateTime(drawtime.Year, drawtime.Month, drawtime.Day, drawtime.Hour, (drawtime.Minute / 5) * 5, 0);

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
                                betDetailData.Add(item);
                            }
                            List<BetRecordSummary> summaryList = new();
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
                                int PostRecordResult = await _mpDBService.PostMPRecord(conn, tran, betDetailData);
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

                            _logger.LogError("Run MP record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create MP game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計
                var req = await _gameApiService._MPAPI.CheckSummaryAsync(new CheckSummaryparam()
                {
                    startTime = reportTime.ToString("yyyy-MM-dd HH:00:00"),
                    endTime = reportTime.AddHours(1).ToString("yyyy-MM-dd HH:00:00"),
                });
                var gameEmptyReport = new GameReport();


                if (req.d.Transactions.Count == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.MP);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;
                }
                else
                {
                    var data = req.d.Transactions[0];


                    gameEmptyReport.platform = nameof(Platform.MP);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = data.totalBetAmount;
                    gameEmptyReport.total_win = data.playerPL + data.totalBetAmount;
                    gameEmptyReport.total_netwin = data.playerPL;
                    gameEmptyReport.total_count = data.betCount;
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
                _logger.LogDebug("Create MP game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _mpDBService.SumMPBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.MP);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin + totalBetValid;
                reportData.total_netwin = totalWin;
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
        private async Task<BetRecordSummary> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, MPData r)
        {

            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Convert.ToDecimal(r.AllBet);
            SummaryData.Turnover += Convert.ToDecimal(r.CellScore);
            SummaryData.Netwin += Convert.ToDecimal(r.Profit);
            SummaryData.Win += Convert.ToDecimal(r.Profit) + Convert.ToDecimal(r.AllBet);
            SummaryData.updatedatetime = DateTime.Now;

            SummaryData.JackpotWin = 0;
            return SummaryData;
        }

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairMP(DateTime startTime, DateTime endTime)
        {
            var req = new PullGameBettingSlipParam
            {
                startTime = new DateTimeOffset(startTime).ToUnixTimeMilliseconds().ToString(),
                endTime = new DateTimeOffset(endTime).ToUnixTimeMilliseconds().ToString(),
            };


            var res = await _gameApiService._MPAPI.PullGameBettingSlipAsync(req);


            List<MPData> repairList = new List<MPData>();

            if (res.Count == 0)
            {
                return repairList.Count;
            }

            var w1CenterList = await _mpDBService.GetMPRecordsBytime(startTime.AddMinutes(-10), endTime.AddMinutes(10));

            foreach (var item in res)
            {
                var hasData = w1CenterList.Where(x => x.GameID == item.GameID).Any();
                if (hasData == false)
                {
                    repairList.Add(item);
                }
            }
            if (repairList.Count != 0)
            {
                await PostMPRecord(repairList);
            }
            return repairList.Count;
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
            return PlatformType.Chess;
        }


    }
}
