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
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.W1API;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IStreamerSessionRecordService
    {
        public Task<ResCodeBase> PostRcgRecord(RCG_GetBetRecordList_Res rcgBetRecord);
        public Task<ResCodeBase> SummaryHourlyRecord(DateTime reportDateTime);
    }
    public class StreamerSessionRecordService : IStreamerSessionRecordService
    {
        private readonly ILogger<StreamerSessionRecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IWalletSessionService _walletSessionService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly IRcgDBService _rcgDBService;
        private readonly IGameReportDBService _gameReportDBService;

        public StreamerSessionRecordService(ILogger<StreamerSessionRecordService> logger
            , ICommonService commonService
            , IWalletSessionService walletSessionService,
            ITransferWalletService transferWalletService,
            IRcgDBService rcgDbService, 
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _walletSessionService = walletSessionService;
            _transferWalletService = transferWalletService;
            _rcgDBService = rcgDbService;
            _gameReportDBService = gameReportDBService;
        }

        public async Task<ResCodeBase> PostRcgRecord(RCG_GetBetRecordList_Res rcgBetRecord)
        {
            ResCodeBase res = new ResCodeBase();
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                IEnumerable<IGrouping<string, BetRecord>> linqRes = rcgBetRecord.dataList.GroupBy(x => x.memberAccount);
                foreach (IGrouping<string, BetRecord> group in linqRes)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            string club_id = group.Key;
                            Wallet memberWalletData = await _transferWalletService.GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }
                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.STREAMER);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No rcg user");
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
                            var minTime = group.Min(record => record.reportDT);
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
                            List<BetRecord> betDetailData = new List<BetRecord>();
                            foreach (BetRecord r in group)//loop club id bet detail
                            {
                                BetRecordSession sumData = new BetRecordSession();
                                sumData.Club_id = memberWalletData.Club_id;
                                if (Config.OneWalletAPI.RCGMode == "W2")
                                {
                                    sumData.Game_id = nameof(Platform.STREAMER);
                                }
                                else
                                {
                                    sumData.Game_id = nameof(Platform.RCG);
                                }
                                //注單寫入Web,system
                                r.systemCode = rcgBetRecord.systemCode;
                                r.webId = rcgBetRecord.webId;
                                DateTime tempDateTime = r.reportDT;

                                //找到報表時間Session
                                var session = UserSessionList.Where(x => x.start_time < tempDateTime && x.end_time > tempDateTime).ToList();
                                if (session.Count() == 1)
                                {
                                    sumData.StartDatetime = session.Single().start_time;
                                    sumData.EndDatetime = session.Single().end_time;
                                    sumData.Session_id = session.Single().session_id;
                                    sumData.status = BetRecordSession.Recordstatus.InSession;
                                    //跨日要產生新的一筆Session record
                                    if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.StartDatetime < ReportDateTime)
                                        {
                                            sumData.StartDatetime = ReportDateTime; //修改報表時間到當日12:00
                                        }
                                    }
                                    else if (DateTime.Now.Hour >= 12) //換日線
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
                                    //todo 找不到單先設定成當天時間12:00:00
                                    sumData.StartDatetime = DateTime.Now;
                                    sumData.EndDatetime = DateTime.Now;
                                    sumData.Session_id = Guid.NewGuid();
                                    sumData.status = BetRecordSession.Recordstatus.SessionNotFound;
                                }
                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.StartDatetime.ToString()))
                                {
                                    sumData = summaryData[sumData.StartDatetime.ToString()];
                                    //合併處理
                                    sumData = Calculate(sumData, r);
                                    summaryData[sumData.StartDatetime.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results = await _commonService._serviceDB.GetRecordSession(conn, tran, sumData);
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
                                    summaryData.Add(sumData.StartDatetime.ToString(), sumData);
                                }
                                r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }
                            List<BetRecordSession> summaryList = new List<BetRecordSession>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }
                            int PostRecordSummaryReuslt = await _commonService._serviceDB.PostRecordSession(conn, tran, summaryList);
                            int PostRecordResult = await _rcgDBService.PostRcgRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run rcg record group: {key} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile , errorFile);
                            await tran.RollbackAsync();
                        }

                    }
                }
                await conn.CloseAsync();
            }

            return res;
        }
        private BetRecordSession Calculate(BetRecordSession SummaryData, BetRecord r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.bet;
            SummaryData.Turnover += r.available;
            SummaryData.Reward += (1 - r.waterRate / 100) * r.available;
            //RCG NetWin 要再加上退水
            SummaryData.Netwin += (r.winLose + (1 - r.waterRate / 100) * r.available);           
            SummaryData.Win += r.winLose;
            SummaryData.UpdateDatetime = DateTime.Now;
            return SummaryData;
        }
        public async Task<ResCodeBase> SummaryHourlyRecord(DateTime reportDateTime)
        {
            var reportData = await _rcgDBService.SumRcgBetRecordHourly(reportDateTime);
            reportData.platform = nameof(Platform.RCG);
            reportData.report_datetime = reportDateTime;
            reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
            reportData.total_win = reportData.total_netwin + reportData.total_bet;
            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);
            return new ResCodeBase();
        }
    }

}
