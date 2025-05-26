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
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IStreamerInterfaceService : IGameInterfaceService
    {
        public Task<ResCodeBase> PostRcgRecord(RCG_GetBetRecordList_Res rcgBetRecord);
        public Task<ResCodeBase> SummaryHourlyRecord(DateTime reportDateTime);
    }
    public class STREAMER_InterfaceService : IStreamerInterfaceService
    {
        private readonly ILogger<STREAMER_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameApiService _gameaApiService;
        private readonly IRcgDBService _rcgDBService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        public STREAMER_InterfaceService(ILogger<STREAMER_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IRcgDBService rcgDbService,
            ISummaryDBService summaryDBService, 
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameaApiService = gameaApiService;
            _summaryDBService = summaryDBService;
            _rcgDBService = rcgDbService;
            _gameReportDBService = gameReportDBService;
        }
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            var req = new RCG_GetBalance();
            var Balance = new MemberBalance();
            try
            {
                var StreamerData = await _commonService._serviceDB.GetStreamerToken(platform_user.club_id);
                if (StreamerData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                req.memberAccount = platform_user.game_user_id;
                req.systemCode = StreamerData.system_code;
                req.webId = StreamerData.web_id;

                RCG_ResBase<RCG_GetBalance_Res> getStreamerBalanceResult = await _gameaApiService._StreamerApi.GetBalance(req);
                if (getStreamerBalanceResult.msgId == (int)STREAMER.msgId.Success)
                {
                    Balance.Amount = getStreamerBalanceResult.data.balance;
                }
                else
                {
                    Balance.Amount = 0;
                    Balance.code = getStreamerBalanceResult.msgId;
                    Balance.Message = getStreamerBalanceResult.message;
                    _logger.LogError("Streamer餘額取得失敗 Code:{errorCode} Msg: {Message}", getStreamerBalanceResult.msgId, getStreamerBalanceResult.message);
                }
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Streamer Credit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }


            Balance.Wallet = nameof(Platform.STREAMER);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            if (platform_user.game_user_id != null)
            {
                RCG_ResBase<RCG_KickOut_Res> res = new RCG_ResBase<RCG_KickOut_Res>();
                try
                {
                    var result = await _commonService._serviceDB.GetStreamerToken(platform_user.club_id);
                    if (result == null)
                    {
                        throw new Exception("沒有使用者Streamer token");
                    }
                    RCG_KickOut req = new RCG_KickOut();
                    req.memberAccount = platform_user.club_id;
                    req.webId = result.web_id;
                    req.systemCode = result.system_code;
                    res = await _gameaApiService._StreamerApi.KickOut(req);
                    if (res.msgId != (int)STREAMER.msgId.Success)
                    {
                        _logger.LogInformation("踢出Streamer使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, res.message);
                    }
                }
                catch (Exception ex)
                {
                    res.msgId = (int)RCG.msgId.Fail;
                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                    _logger.LogError("KickRcgUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
            }
            return true;
        }
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var streamerData = await _commonService._serviceDB.GetStreamerToken(platform_user.club_id);
                if (streamerData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                RCG_Deposit StreamerTransferData = new RCG_Deposit();
                StreamerTransferData.systemCode = streamerData.system_code;
                StreamerTransferData.webId = streamerData.web_id;
                StreamerTransferData.transactionId = RecordData.id.ToString();
                StreamerTransferData.transctionAmount = transfer_amount;
                StreamerTransferData.memberAccount = platform_user.game_user_id;
                RCG_ResBase<RCG_Deposit_Res> StreamerTransferResult = await _gameaApiService._StreamerApi.Deposit(StreamerTransferData);
                if (StreamerTransferResult.msgId == (int)STREAMER.msgId.Success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (StreamerTransferResult.msgId == (int)RCG.msgId.TimeOut)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("FundTransferInRcgFail Msg: {Message}", StreamerTransferResult.message);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("streamer TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInstreamerFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var game_balance = RecordData.amount;
            var currency = walletData.Currency;
            Platform platform = (Platform)Enum.Parse(typeof(Platform), RecordData.type, true);
            try
            {
                var streamerData = await _commonService._serviceDB.GetStreamerToken(platform_user.club_id);
                if (streamerData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                RCG_Withdraw StreamerTransferData = new RCG_Withdraw();
                StreamerTransferData.systemCode = streamerData.system_code;
                StreamerTransferData.webId = streamerData.web_id;
                StreamerTransferData.transactionId = RecordData.id.ToString();
                StreamerTransferData.transctionAmount = game_balance;
                StreamerTransferData.memberAccount = platform_user.game_user_id;
                RCG_ResBase<RCG_Withdraw_Res> StreamerTransferResult = await _gameaApiService._StreamerApi.Withdraw(StreamerTransferData);
                if (StreamerTransferResult.msgId == (int)RCG.msgId.Success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (StreamerTransferResult.msgId == (int)RCG.msgId.TimeOut)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("STREAMER TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("STREAMER TransferOut Fail ex : {ex}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            ForwardGame res = new ForwardGame();
            string systemcode = Config.CompanyToken.STREAMER_System;
            string webid = request.GameConfig["webid"];
            //step 1 建立Streamer遊戲商資料
            var CreateReq = new STREAMER_CreateOrSetUser();
            CreateReq.clubId = userData.Club_id;
            CreateReq.displayName = userData.Club_Ename;
            CreateReq.systemCode = systemcode;
            CreateReq.webId = webid;
            CreateReq.pictureUrl = "";
            CreateReq.clubEname = userData.Club_Ename;
            if (!await _gameaApiService._StreamerApi.STREAMER_CreateOrSetUser(CreateReq))
            {
                throw new ExceptionMessage((int)ResponseCode.CreateRcgUserTokenFail, MessageCode.Message[(int)ResponseCode.CreateRcgUserTokenFail]);
            }
            if (!STREAMER.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //step 2 建立RCG遊戲館資料 (H1特規官網自行Create User)
            if (Config.OneWalletAPI.RCGMode != "H1")
            {
                RCG_CreateOrSetUser req = new RCG_CreateOrSetUser();

                req.memberAccount = userData.Club_id;
                req.memberName = userData.Club_Ename;
                req.webId = webid;
                req.systemCode = systemcode;
                req.currency = STREAMER.Currency[userData.Currency];
                var lang = "en-US";
                if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && STREAMER.lang.ContainsKey(request.GameConfig["lang"]))
                {
                    lang = request.GameConfig["lang"];
                }
                req.language = STREAMER.lang[lang];
                if (request.GameConfig.ContainsKey("betLimitGroup"))
                {
                    req.betLimitGroup = request.GameConfig["betLimitGroup"];
                }
                if (request.GameConfig.ContainsKey("openGameList"))
                {
                    req.openGameList = request.GameConfig["openGameList"];
                }
                RCG_ResBase<RCG_CreateOrSetUser_Res> result = await _gameaApiService._StreamerApi.CreateOrSetUser(req);
                if (result.msgId != (int)RCG.msgId.Success)
                {
                    throw new Exception("Rcg CreateOrSetUser fail");
                }
            }
            var results = await _commonService._serviceDB.GetStreamerToken(userData.Club_id);
            if (results == null)
            {

                StreamerToken steramerData = new StreamerToken();
                steramerData.club_id = userData.Club_id;
                steramerData.system_code = systemcode;
                steramerData.web_id = webid;
                if (await _commonService._serviceDB.PostStreamerToken(steramerData) != 1)
                {
                    throw new ExceptionMessage((int)ResponseCode.CreateRcgUserTokenFail, MessageCode.Message[(int)ResponseCode.CreateRcgUserTokenFail]);
                }
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = userData.Club_id;
            gameUser.game_platform = request.Platform;
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            ForwardGame res = new ForwardGame();
            string systemcode = request.GameConfig["systemcode"];
            string webid = request.GameConfig["webid"];
            //step 4. 登入RCG取得遊戲
            RCG_Login StreamerloninReq = new RCG_Login();
            StreamerloninReq.systemCode = systemcode;
            StreamerloninReq.webId = webid;
            StreamerloninReq.memberAccount = request.Club_id;
            if (Config.OneWalletAPI.WalletMode == "SingleWallet")
            {
                StreamerloninReq.welletMode = 4; //單一錢包
            }
            else
            {
                if (Config.OneWalletAPI.RCGMode == "H1")
                {
                    StreamerloninReq.welletMode = 5; //H1轉帳模式
                }
                else
                {
                    StreamerloninReq.welletMode = 1;
                }
            }
            RCG_ResBase<RCG_Login_Res> login_result = await _gameaApiService._StreamerApi.Login(StreamerloninReq);
            if (login_result.msgId != (int)RCG.msgId.Success)
            {
                res.code = (int)ResponseCode.GetGameURLFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameURLFail] + " | " + login_result.message;
                throw new ExceptionMessage(res.code, res.Message);
            }
            //step 5. 登入STREAMER遊戲取得連結
            var LoginReq = new STREAMER_Login();
            LoginReq.clubId = platformUser.game_user_id;
            LoginReq.systemCode = systemcode;
            LoginReq.webid = webid;
            LoginReq.loginUrl = login_result.data.url;
            var LoginRes = await _gameaApiService._StreamerApi.STREAMER_ForwardGameURL(LoginReq);
            res.Url = LoginRes;
            return res.Url;
        }
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            return new CheckTransferRecordResponse();
        }
        /// <summary>
        /// 取得遊戲第二層注單明細
        /// </summary>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            return res;
        }
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            return null;
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var returnString = "";
            return returnString;
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
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.STREAMER);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No rcg user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<BetRecord> betDetailData = new List<BetRecord>();
                            foreach (BetRecord r in group)//loop club id bet detail
                            {
                                BetRecordSummary sumData = new BetRecordSummary();
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
                                sumData.ReportDatetime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day, tempDateTime.Hour, (tempDateTime.Minute / 5) * 5, 0);
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                    else if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                }
                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString()];
                                    //合併處理
                                    sumData = Calculate(sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString()] = sumData;
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
                                        sumData = Calculate(sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                    }
                                    summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
                                }
                                r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }
                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            int PostRecordResult = await _rcgDBService.PostRcgRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run rcg record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }
                await conn.CloseAsync();
            }

            return res;
        }
        private BetRecordSummary Calculate(BetRecordSummary SummaryData, BetRecord r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.bet;
            SummaryData.Turnover += r.available;
            //RCG NetWin 要再加上退水
            SummaryData.Netwin += (r.winLose + (1 - r.waterRate / 100) * r.available);
            SummaryData.Win += r.winLose;
            SummaryData.updatedatetime = DateTime.Now;
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
            return PlatformType.Live;
        }

        public Task HealthCheck(Platform platform)
        {
            throw new NotImplementedException();
        }
    }

}
