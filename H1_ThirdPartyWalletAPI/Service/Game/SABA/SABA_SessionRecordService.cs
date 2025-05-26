using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.W1API;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface ISabaSessionRecordService
    {
        public Task<ResCodeBase> PostSabaRecord(SABA_Game_Record recordData);
        public Task<ResCodeBase> PostGameReport(SABA_GetFinancialReportData sabaReportData);
        public Task<ResCodeBase> CreateGameReportFromBetRecord(DateTime reportDate);
    }
    public class SABA_SessionRecordService : ISabaSessionRecordService
    {
        private readonly ILogger<SABA_SessionRecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IWalletSessionService _walletSessionService;
        private readonly IWebHostEnvironment _env;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISabaDbService _sabaDbService;
        private readonly IGameReportDBService _gameReportDBService;


        public SABA_SessionRecordService(ILogger<SABA_SessionRecordService> logger
            , ICommonService commonService
            , IWebHostEnvironment env
            , IWalletSessionService walletSessionService
            , ITransferWalletService transferWalletService
            , ISabaDbService sabaDbService
            , IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _walletSessionService = walletSessionService;
            _transferWalletService = transferWalletService;
            _gameReportDBService = gameReportDBService;
            _env = env;
            _sabaDbService = sabaDbService;
        }
        public async Task<ResCodeBase> PostSabaRecord(SABA_Game_Record recordData)
        {
            ResCodeBase res = new ResCodeBase();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, SABA_BetDetails>> linqRes = recordData.BetDetails.GroupBy(x => x.vendor_member_id);
            foreach (IGrouping<string, SABA_BetDetails> group in linqRes)
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
                            if (_env.EnvironmentName != "PRD")
                            {
                                club_id = group.Key.Substring(3);
                            }
                            else
                            {
                                club_id = group.Key;
                            }
                            Wallet memberWalletData = await _transferWalletService.GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }
                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.SABA);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No saba user");
                            }
                            
                            //取得User Sesssion
                            var UserSessionList = new List<WalletSessionV2>();
                            var CurrectSession = await _walletSessionService.GetWalletSessionByClub(club_id);
                            if (CurrectSession != null)
                            {
                                CurrectSession.end_time = DateTime.Now; //因為還未結束, 結束時間先改成現在時間
                                UserSessionList.Add(CurrectSession);
                            }
                            List<short> status = new List<short>{
                              (short)WalletSessionV2.SessionStatus.UNSETTLE
                            };

                            //取得注單最小時間
                            var minTime = group.Min(record => record.transaction_time).GetValueOrDefault(DateTime.Now);
                            //搜尋時間最小時間-3天
                            var UnSettleSession = await _commonService._serviceDB.GetalletSessionV2History(status, club_id, minTime.AddDays(-3));
                            if (UnSettleSession.Any())
                            {
                                foreach (var session in UnSettleSession)
                                {
                                    UserSessionList.Add(session);
                                }
                            }
                            //彙總注單
                            Dictionary<string, BetRecordSession> summaryData = new Dictionary<string, BetRecordSession>();
                            //已結算注單
                            List<SABA_BetDetails> betDetailData = new List<SABA_BetDetails>();
                            //未結算注單
                            List<SABA_BetDetails> betDetailDataRunning = new List<SABA_BetDetails>();
                            foreach (SABA_BetDetails r in group)//loop club id bet detail
                            {
                                if (r.ticket_status == "running")
                                {
                                    IEnumerable<SABA_BetDetails> runningRecord = await _sabaDbService.GetSabaRunningRecord(conn, tran, r);
                                    if (runningRecord.Count() > 0) //取得重複Running注單
                                    {
                                        r.ticket_status = "duplicate";
                                    }
                                    r.club_id = memberWalletData.Club_id;
                                    r.franchiser_id = memberWalletData.Franchiser_id;
                                }
                                else if (r.ticket_status == "waiting")
                                {
                                    //waiting單不用管
                                }
                                else
                                {
                                    IEnumerable<SABA_BetDetails> runningRecord = await _sabaDbService.GetSabaRecord(r);
                                    if (runningRecord.Count() > 0) //取得重複Settle注單
                                    {
                                        r.ticket_status = "duplicate";
                                    }
                                }
                                BetRecordSession sumData = new BetRecordSession();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.SABA);


                                var Betsession = new List<WalletSessionV2>();
                                if (r.settlement_time != null)
                                {
                                    r.transaction_time = Convert.ToDateTime(r.transaction_time.ToString()).AddHours(12);
                                    r.settlement_time = Convert.ToDateTime(r.settlement_time.ToString()).AddHours(12);
                                    Betsession = UserSessionList.Where(x => x.start_time < r.settlement_time && x.end_time > r.settlement_time).ToList();
                                }
                                else
                                {
                                    r.transaction_time = Convert.ToDateTime(r.transaction_time.ToString()).AddHours(12);
                                    Betsession = UserSessionList.Where(x => x.start_time < r.transaction_time && x.end_time > r.transaction_time).ToList();
                                }                               
                                if (Betsession.Count() == 1)
                                {
                                    sumData.StartDatetime = Betsession.Single().start_time;
                                    sumData.EndDatetime = Betsession.Single().end_time;
                                    sumData.bet_session_id = Betsession.Single().session_id;
                                    sumData.Session_id = Betsession.Single().session_id;
                                    sumData.status = BetRecordSession.Recordstatus.InSession;

                                    //跨日要產生新的一筆Session record
                                    //if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    //{
                                    //    DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                    //    if (sumData.StartDatetime < ReportDateTime)
                                    //    {
                                    //        sumData.StartDatetime = ReportDateTime; //修改報表時間到當日12:00
                                    //    }
                                    //}
                                    //else 
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.StartDatetime < ReportDateTime)
                                        {
                                            sumData.StartDatetime = ReportDateTime; //修改報表時間到當日12:00
                                        }
                                    }
                                    else
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, 12, 00, 0);
                                        if (sumData.StartDatetime < ReportDateTime)
                                        {
                                            sumData.StartDatetime = ReportDateTime; //修改報表時間到當日12:00
                                        }
                                    }
                                }
                                else
                                {
                                    //找不到單先直接設定為投注時間
                                    sumData.StartDatetime = r.settlement_time;
                                    //找不到單先直接設定為結算時間
                                    sumData.EndDatetime = r.settlement_time;
                                    sumData.status = BetRecordSession.Recordstatus.SessionNotFound;
                                }
                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.StartDatetime.ToString()))
                                {
                                    sumData = summaryData[sumData.StartDatetime.ToString()];
                                    //合併處理
                                    sumData = await Calculate(conn, tran, sumData, r);
                                    summaryData[sumData.StartDatetime.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results = await _commonService._serviceDB.GetRecordSession(conn, tran, sumData);
                                    sw.Stop();
                                    _logger.LogDebug("get summary record ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                                    sw.Restart();
                                    if (results.Count() == 0) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = await Calculate(conn, tran, sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = await Calculate(conn, tran, sumData, r);
                                    }
                                    summaryData.Add(sumData.StartDatetime.ToString(), sumData);
                                }
                                r.last_version_key = recordData.last_version_key;
                                r.summary_id = sumData.id;

                                switch (r.ticket_status)
                                {
                                    case "waiting":
                                    case "duplicate":
                                        //waiting單不存
                                        break;
                                    case "running":
                                        betDetailDataRunning.Add(r);
                                        break;
                                    case "void":
                                    case "refund":
                                    case "reject":
                                    case "draw":
                                    case "lose":
                                    case "won":
                                    case "half won":
                                    case "half lose":
                                        betDetailData.Add(r);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            List<BetRecordSession> summaryList = new List<BetRecordSession>();
                            foreach (var s in summaryData)
                            {
                                if (s.Value.RecordCount > 0)
                                {
                                    summaryList.Add(s.Value);
                                }
                            }
                            int PostRecordSummaryReuslt = await _commonService._serviceDB.PostRecordSession(conn, tran, summaryList);
                            if (betDetailData.Count > 0)
                            {
                                int PostRecordResult = await _sabaDbService.PostSabaRecord(conn, tran, betDetailData);
                            }
                            if (betDetailDataRunning.Count > 0)
                            {
                                int PostRunningRecordResult = await _sabaDbService.PostSabaRunningRecord(conn, tran, betDetailDataRunning);
                                await _sabaDbService.PostSabaRecord(conn, tran, betDetailDataRunning);
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
                            if (_env.EnvironmentName != "PRD" && (ex.Message == "沒有會員id" || ex.Message == "No saba user"))
                            {
                                _logger.LogDebug("Run saba record group: {key} exception EX : {ex}  MSG : {Message} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString());
                            }
                            else
                            {
                                _logger.LogError("Run saba record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
                            }
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return res;
        }
        public async Task<ResCodeBase> PostGameReport(SABA_GetFinancialReportData sabaReportData)
        {
            ResCodeBase res = new ResCodeBase();
            GameReport reportData = new GameReport();
            reportData.platform = nameof(Platform.SABA);
            reportData.report_datetime = DateTime.Parse(sabaReportData.FinancialDate);
            reportData.report_type = (int)GameReport.e_report_type.FinancalReport;
            reportData.total_bet = sabaReportData.TotalBetAmount;
            reportData.total_win = sabaReportData.TotalWinAmount;
            reportData.total_netwin = sabaReportData.TotalWinAmount - sabaReportData.TotalBetAmount;
            reportData.total_count = sabaReportData.TotalBetCount;
            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);
            return res;
        }
        public async Task<ResCodeBase> CreateGameReportFromBetRecord(DateTime reportDate)
        {
            ResCodeBase res = new ResCodeBase();
            IEnumerable<dynamic> sabaReportData = await _sabaDbService.SumSabaBetRecordDaily(reportDate, "H1royal");
            var DailyReportData = sabaReportData.SingleOrDefault();
            GameReport reportData = new GameReport();
            reportData.report_datetime = reportDate;
            reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
            reportData.total_bet = DailyReportData.total_bet == null ? 0 : DailyReportData.total_bet;
            reportData.total_count = DailyReportData.total_count == null ? 0 : DailyReportData.total_count;
            reportData.total_netwin = DailyReportData.total_netwin == null ? 0 : DailyReportData.total_netwin;
            reportData.platform = nameof(Platform.SABA);
            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);
            return res;
        }
        private async Task<BetRecordSession> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSession SummaryData, SABA_BetDetails r)
        {
            //處理聯賽主客對資訊串關顯示內容
            if (r.ParlayData != null)
            {
                r.leaguename_en = "Parlay (" + r.ParlayData.Count + ")";
                r.hometeamname_en = " - ";
                r.awayteamname_en = " - ";
                r.sport_type = 0;
            }
            else
            {
                r.leaguename_en = (r.leaguename == null) ? null : r.leaguename.FirstOrDefault(x => x.lang == "en").name;
                r.hometeamname_en = (r.hometeamname == null) ? null : r.hometeamname.FirstOrDefault(x => x.lang == "en").name;
                r.awayteamname_en = (r.awayteamname == null) ? null : r.awayteamname.FirstOrDefault(x => x.lang == "en").name;
            }
            if (r.resettlementinfo != null)
            {
                return ReSettle(SummaryData, r);
            }
            else
            {
                return await Settle(conn, tran, SummaryData, r);
            }
        }
        private async Task<BetRecordSession> Settle(NpgsqlConnection conn, IDbTransaction tran, BetRecordSession SummaryData, SABA_BetDetails r)
        {
            r.stake = (r.stake == null) ? 0 : r.stake;
            r.winlost = (r.winlost == null) ? 0 : r.winlost;
            r.winlost_amount = (r.winlost_amount == null) ? 0 : r.winlost_amount;
            switch (r.ticket_status)
            {
                case "running":
                    SummaryData.RecordCount++;
                    SummaryData.Bet_amount += r.stake.GetValueOrDefault();
                    SummaryData.Turnover += 0;
                    SummaryData.Netwin += r.winlost_amount.GetValueOrDefault();
                    SummaryData.Win += 0;
                    if (r.winlost_datetime > DateTime.Now.AddDays(29))
                    {
                        r.winlost_datetime = DateTime.Now.Date.AddDays(1);
                    }
                    break;
                case "waiting":
                case "duplicate":
                    //waiting單暫不處理
                    break;
                case "void":
                case "refund":
                case "reject":
                case "draw":
                    IEnumerable<SABA_BetDetails> Refoundresults = await _sabaDbService.GetSabaRunningRecord(conn, tran, r);
                    Refoundresults = Refoundresults.ToList();
                    if (Refoundresults.Count() != 1) //沒有未結算單
                    {
                        r.pre_winlost_amount = r.winlost_amount;
                        r.pre_stake = r.stake;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += r.stake.GetValueOrDefault();
                        SummaryData.Turnover += 0;
                        SummaryData.Netwin += r.winlost_amount.GetValueOrDefault();
                        SummaryData.Win += (r.winlost_amount.GetValueOrDefault() + r.stake.GetValueOrDefault());
                    }
                    else
                    {
                        decimal? preWinLose = Refoundresults.SingleOrDefault().winlost_amount;
                        decimal? preBet = Refoundresults.SingleOrDefault().stake;
                        preWinLose = (preWinLose == null) ? 0 : preWinLose;
                        preBet = (preBet == null) ? 0 : preBet;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += (r.stake.GetValueOrDefault() - preBet.GetValueOrDefault());
                        SummaryData.Turnover += 0;
                        SummaryData.Netwin += (r.winlost_amount.GetValueOrDefault() - preWinLose.GetValueOrDefault());
                        SummaryData.Win += SummaryData.Netwin + SummaryData.Bet_amount;

                        //明細也要將未結算單預扣金額加回去
                        r.pre_winlost_amount = r.winlost_amount;
                        r.winlost_amount -= preWinLose;
                        r.pre_stake = r.stake;
                        r.stake -= preBet;
                        await _sabaDbService.DeleteSabaRunningRecord(conn, tran, r);
                    }
                    break;
                case "lose":
                case "won":
                case "half won":
                case "half lose":
                    IEnumerable<SABA_BetDetails> WinloseResults = await _sabaDbService.GetSabaRunningRecord(conn, tran, r);
                    if (WinloseResults.Count() != 1) //沒有未結算單
                    {
                        r.pre_winlost_amount = r.winlost_amount;
                        r.pre_stake = r.stake;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += r.stake.GetValueOrDefault();

                        if (r.odds < 0)
                        {
                            //馬來盤實投量要計算實際扣款額度
                            SummaryData.Turnover += Math.Abs((r.stake * r.odds).GetValueOrDefault());
                        }
                        else
                        {
                            SummaryData.Turnover += r.stake.GetValueOrDefault();
                        }

                        SummaryData.Netwin += r.winlost_amount.GetValueOrDefault();
                        SummaryData.Win += (r.winlost_amount.GetValueOrDefault() + r.stake.GetValueOrDefault());
                    }
                    else
                    {
                        decimal preWinLose = WinloseResults.SingleOrDefault().winlost_amount.GetValueOrDefault();
                        decimal? preBet = WinloseResults.SingleOrDefault().stake;
                        preBet = (preBet == null) ? 0 : preBet;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += (r.stake.GetValueOrDefault() - preBet.GetValueOrDefault());
                        //馬來盤實投量要計算實際扣款額度
                        SummaryData.Turnover += Math.Abs(preWinLose);
                        SummaryData.Netwin += (r.winlost_amount.GetValueOrDefault() - preWinLose);
                        SummaryData.Win += SummaryData.Netwin + SummaryData.Bet_amount;
                        //明細也要將未結算單預扣金額加回去
                        r.pre_winlost_amount = r.winlost_amount;
                        r.winlost_amount -= preWinLose;
                        r.pre_stake = r.stake;
                        r.stake -= preBet;
                        await _sabaDbService.DeleteSabaRunningRecord(conn, tran, r);
                    }
                    break;
            }
            SummaryData.UpdateDatetime = DateTime.Now;
            return SummaryData;
        }
        private BetRecordSession ReSettle(BetRecordSession SummaryData, SABA_BetDetails r)
        {
            _logger.LogWarning("saba bet record resettle info :{info}", r);
            SummaryData.UpdateDatetime = DateTime.Now;
            foreach (SABA_ResettlementInfo rdata in r.resettlementinfo)
            {
                if (rdata.balancechange)
                {
                    r.winlost = (rdata.winlost == null) ? 0 : rdata.winlost;
                    r.winlost_amount = (r.winlost_amount == null) ? 0 : r.winlost_amount;
                    switch (r.ticket_status)
                    {
                        case "running":
                        case "waiting":
                            break;
                        case "void":
                        case "refund":
                        case "reject":
                        case "draw":
                        case "lose":
                        case "won":
                        case "half won":
                        case "half lose":
                            decimal? adjustAmount = r.winlost_amount - r.winlost;
                            SummaryData.Netwin += adjustAmount.GetValueOrDefault();
                            SummaryData.Win += adjustAmount.GetValueOrDefault();
                            _logger.LogWarning("saba bet record summary adjust amount : {amount} user_id : {user_id}", adjustAmount, r.vendor_member_id);
                            break;
                    }
                    return SummaryData;
                }
            }
            return SummaryData;
        }
    }

}
