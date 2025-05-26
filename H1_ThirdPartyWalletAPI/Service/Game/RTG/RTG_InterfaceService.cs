using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RTG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IRtgInterfaceService : IGameInterfaceService
    {
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task PostRtgRecord(List<Record> recordData);
    }
    public class RTG_RecordService : IRtgInterfaceService
    {
        private readonly ILogger<RTG_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly IRtgDBService _rtgDBService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        const int pageLimit = 100;
        const int getDelayMS = 200;
        public RTG_RecordService(ILogger<RTG_RecordService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IRtgDBService rtgDBService,
            ISummaryDBService summaryDBService, 
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameaApiService;
            _rtgDBService = rtgDBService;
            _summaryDBService = summaryDBService;
            _gameReportDBService = gameReportDBService;
        }
        #region GameInterfaceService
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                var walletCache = await GetWalletCache(platform_user.club_id);
                var responseData = await _gameApiService._RtgAPI.GetUser(new GetUserRequest()
                {
                    SystemCode = Config.CompanyToken.RTG_SystemCode,
                    WebId = Config.CompanyToken.RTG_WebID,
                    UserId = platform_user.game_user_id,
                }); ;

                if (responseData.MsgID != (int)ErrorCodeEnum.Success)
                {
                    throw new Exception(responseData.Message);
                }
                Balance.Amount = responseData.Data.UserBalance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Rtg餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.RTG);
            return Balance;
        }
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var responseData = await _gameApiService._RtgAPI.Deposit(new DepositRequest
                {
                    SystemCode = Config.CompanyToken.RTG_SystemCode,
                    WebId = Config.CompanyToken.RTG_WebID,
                    UserId = platform_user.game_user_id,
                    TransactionId = RecordData.id.ToString().Replace("-", "").Substring(0, 20),
                    Balance = Math.Round(transfer_amount, 2)
                });

                if (responseData.MsgID != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage(responseData.MsgID, responseData.Message);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RTG TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("RTG TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInRtgFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var game_balance = RecordData.amount;
            Platform platform = (Platform)Enum.Parse(typeof(Platform), RecordData.type, true);
            try
            {
                var responseData = await _gameApiService._RtgAPI.Withdraw(new WithdrawRequest()
                {
                    SystemCode = Config.CompanyToken.RTG_SystemCode,
                    WebId = Config.CompanyToken.RTG_WebID,
                    UserId = platform_user.game_user_id,
                    TransactionId = RecordData.id.ToString().Replace("-", "").Substring(0, 20),
                    Balance = Math.Round(game_balance, 2)
                });

                if (responseData.MsgID != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage(responseData.MsgID, responseData.Message);
                }

                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RTG TransferOut Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("RTG TransferOut Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RTG TransferOut Fail ex : {ex}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var responseData = await _gameApiService._RtgAPI.KickUser(new KickUserRequest()
                {
                    SystemCode = Config.CompanyToken.RTG_SystemCode,
                    WebId = Config.CompanyToken.RTG_WebID,
                    UserId = platform_user.game_user_id,
                });
                if (responseData.MsgID != (int)ErrorCodeEnum.Success)
                {
                    _logger.LogInformation("踢出RTG使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, responseData.Message);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("KickRtgUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            return true;
        }
        public async Task<bool> KickAllUser(Platform platform)
        {

            var responseData = await _gameApiService._RtgAPI.KickAll(new KickAllRequest()
            {
                SystemCode = Config.CompanyToken.RTG_SystemCode,
                WebId = Config.CompanyToken.RTG_WebID,
            });
            return true;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.RTG.RTG.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //Step 1 Create Member
            var requestData = new CreateUpdateMemberRequest();
            requestData.SystemCode = Config.CompanyToken.RTG_SystemCode;
            requestData.WebId = Config.CompanyToken.RTG_WebID;
            requestData.UserId = Config.OneWalletAPI.Prefix_Key + userData.Club_id;
            requestData.UserName = Config.OneWalletAPI.Prefix_Key + userData.Club_Ename;
            requestData.Currency = Model.Game.RTG.RTG.Currency[userData.Currency];
            try
            {
                var result = await _gameApiService._RtgAPI.CreateUpdateMember(requestData);
                if (result.MsgID != (int)ErrorCodeEnum.Success)
                {
                    throw new Exception(result.Message);
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message.ToString());
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = requestData.UserId;
            gameUser.game_platform = request.Platform;
            gameUser.agent_id = requestData.WebId;
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 4 Get Game URL
            var requestData = new GetGameUrlRequest();
            requestData.SystemCode = Config.CompanyToken.RTG_SystemCode;
            requestData.WebId = Config.CompanyToken.RTG_WebID;
            requestData.UserId = Config.OneWalletAPI.Prefix_Key + userData.Club_id;

            if (!request.GameConfig.ContainsKey("gameCode"))
            {
                throw new Exception("game code not found");
            }

            requestData.GameId = Convert.ToInt32(request.GameConfig["gameCode"]);

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                requestData.BackUrl = request.GameConfig["lobbyURL"];
            }
            if (request.GameConfig.ContainsKey("lang") && Model.Game.RTG.RTG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                requestData.Language = Model.Game.RTG.RTG.lang[request.GameConfig["lang"]];
            }
            else
            {
                requestData.Language = Model.Game.RTG.RTG.lang["en-US"];
            }

            var token_res = await _gameApiService._RtgAPI.GetGameUrl(requestData);
            if (token_res.MsgID != (int)ErrorCodeEnum.Success)
            {
                throw new ExceptionMessage(token_res.MsgID, token_res.Message);
            }
            return token_res.Data.GameUrl;
        }
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            var requestData = new SingleTransactionRequest();
            requestData.SystemCode = Config.CompanyToken.RTG_SystemCode;
            requestData.WebId = Config.CompanyToken.RTG_WebID;
            requestData.TransactionId = transfer_record.id.ToString().Replace("-", "").Substring(0, 20);
            var RsgReuslt = await _gameApiService._RtgAPI.SingleTransaction(requestData);
            if (RsgReuslt.MsgID == (int)ErrorCodeEnum.Success)
            {
                if (transfer_record.target == nameof(Platform.RTG))//轉入RTG直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RTG))
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
            else if (RsgReuslt.MsgID == (int)ErrorCodeEnum.No_such_transaction)
            {
                if (transfer_record.target == nameof(Platform.RTG))//轉入RTG直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RTG))
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
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            IEnumerable<dynamic> rtg_results = await _rtgDBService.GetRtgRecordBySummary(RecordReq);
            rtg_results = rtg_results.OrderByDescending(e => e.settlementtime);
            res.Data = rtg_results.ToList();
            return res;
        }
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var GetBetRecordReq = new GetVideoLinkRequest();
            GetBetRecordReq.SystemCode = Config.CompanyToken.RTG_SystemCode;
            GetBetRecordReq.WebId = Config.CompanyToken.RTG_WebID;
            GetBetRecordReq.RecordId = long.Parse(RecordDetailReq.record_id);

            Model.Game.RTG.RTG.lang.TryGetValue(RecordDetailReq.lang, out var lang);
            GetBetRecordReq.Language = lang ?? Model.Game.RTG.RTG.lang["en-US"];

            var RsgresponseData = await _gameApiService._RtgAPI.GetVideoLink(GetBetRecordReq);
            return RsgresponseData.Data.VideoUrl;
        }
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 60)
            {
                endTime = startTime.AddMinutes(60);
                _logger.LogDebug("Repair RTG record start Time : {startTime} end Time : {endTime}", startTime, endTime);
                RepairCount += await RepairRtg(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            _logger.LogDebug("Repair RTG record start Time : {startTime} end Time : {endTime}", startTime, RepairReq.EndTime);
            RepairCount += await RepairRtg(startTime, RepairReq.EndTime);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region GameRecordService
        public async Task PostRtgRecord(List<Record> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                var linqRes = recordData.GroupBy(x => x.UserId);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {

                            string club_id;
                            club_id = group.Key.Substring(3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.RTG);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No rtg user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData =
                                new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<Record> betDetailData = new List<Record>();

                            foreach (Record r in group) //loop club id bet detail
                            {
                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.RTG);
                                sumData.Game_type = r.game_id;
                                DateTime tempDateTime = r.SettlementTime;
                                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                sumData.ReportDatetime = tempDateTime;
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.RecordId);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.RecordId);
                                        }
                                    }
                                }

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                    //合併處理
                                    sumData = Calculate(sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results =
                                        await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    results = results.ToList().Where(x => x.Game_type == sumData.Game_type);
                                    if (results.Count() == 0) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                    }

                                    summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
                                }

                                r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }

                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }

                            int PostRecordResult = await _rtgDBService.PostRtgRecord(conn, tran, betDetailData);
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            _logger.LogDebug("insert RTG record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (Record r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.RecordId, r.SettlementTime);

                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run rtg record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }

                await conn.CloseAsync();
            }
        }
        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                var rtgGameReportByGame = new List<Gamereport>();
                var gamelist = Model.Game.RTG.RTG.GameList.Keys.ToList();
                foreach (var game in gamelist)
                {
                    var betReport = await _gameApiService._RtgAPI.GetGameDailyRecord(new GetGameDailyRecordRequest()
                    {
                        SystemCode = Config.CompanyToken.RTG_SystemCode,
                        WebId = Config.CompanyToken.RTG_WebID,
                        GameId = game,
                        Date = reportTime.ToString("yyyy-MM-dd")
                    });
                    foreach (var report in betReport.Data.GameReport)
                    {
                        rtgGameReportByGame.Add(report);
                    }
                }
                // 遊戲商的每小時匯總報表(轉帳中心的欄位格式)
                var rtgSummaryReport = new GameReport
                {
                    platform = nameof(Platform.RTG),
                    report_datetime = Convert.ToDateTime(reportTime),
                    report_type = (int)GameReport.e_report_type.FinancalReport,
                    total_bet = rtgGameReportByGame.Sum(x => x.Bet),
                    total_win = rtgGameReportByGame.Sum(x => x.WinLose + x.Bet),
                    total_netwin = rtgGameReportByGame.Sum(x => x.WinLose),
                    total_count = rtgGameReportByGame.Sum(x => x.PlayCount)
                };
                await _gameReportDBService.DeleteGameReport(rtgSummaryReport);
                await _gameReportDBService.PostGameReport(rtgSummaryReport);
                startDateTime = startDateTime.AddDays(1);
            }

        }
        public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
        {
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create RTG game W1 report time {datetime}", reportTime);
                IEnumerable<dynamic> dailyReport = await _rtgDBService.SumRtgBetRecordDaily(reportTime);
                var HourlylyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.RTG);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = HourlylyReportData.total_bet == null ? 0 : Math.Abs(HourlylyReportData.total_bet);
                reportData.total_win = HourlylyReportData.total_win == null ? 0 : HourlylyReportData.total_win;
                reportData.total_netwin = reportData.total_win - reportData.total_bet;
                reportData.total_count = HourlylyReportData.total_cont == null ? 0 : HourlylyReportData.total_cont;
                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);
                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }


        #endregion
        private BetRecordSummary Calculate(BetRecordSummary SummaryData, Record r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Math.Abs(r.Bet);
            SummaryData.Turnover += Math.Abs(r.Bet);
            SummaryData.Netwin += r.WinLose;
            SummaryData.Win += r.Bet + r.WinLose;
            SummaryData.updatedatetime = DateTime.Now;
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
        /// RTG 帳務比對
        /// 1. 比對轉帳中心與遊戲商的匯總帳是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairRtg(DateTime startTime, DateTime endTime)
        {
            List<Record> rtgBetRecord = new List<Record>();
            var gamelist = Model.Game.RTG.RTG.GameList.Keys.ToList();
            foreach (var game in gamelist)
            {
                // 遊戲商的歷史下注紀錄
                var rtgBetRecordByGame = new List<Record>();
                var isEnable = true;
                var pageIndex = 1;
                while (isEnable)
                {
                    var betRecord = await _gameApiService._RtgAPI.GameSettlementRecord(new GameSettlementRecordRequest()
                    {
                        SystemCode = Config.CompanyToken.RTG_SystemCode,
                        WebId = Config.CompanyToken.RTG_WebID,
                        GameId = game,
                        StartTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        EndTime = endTime.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                        Page = pageIndex,
                        Rows = pageLimit,
                    });
                    foreach (var record in betRecord.Data.Record)
                    {
                        record.game_id = game;
                        rtgBetRecordByGame.Add(record);
                    }
                    if (rtgBetRecordByGame.Count >= Convert.ToInt32(betRecord.Data.TotalCount))
                    {
                        isEnable = false;
                    }
                    else
                    {
                        pageIndex++;
                        await Task.Delay(getDelayMS);
                    }
                }
                rtgBetRecord.AddRange(rtgBetRecordByGame);
            }

            // 轉帳中心的歷史下注紀錄
            var w1CenterList = await _rtgDBService.GetRtgRecordByTime(startTime, endTime.AddHours(1));

            var repairList = new List<Record>();
            foreach (var record in rtgBetRecord)
            {
                var hasData = w1CenterList.Where(x => x.RecordId == record.RecordId).Any();
                if (hasData == false)
                {
                    repairList.Add(record);
                }
            }
            if (repairList.Count > 0)
            {
                await PostRtgRecord(repairList);
            }
            return repairList.Count;
        }
    }
}
