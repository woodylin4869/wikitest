using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Request;
using H1_ThirdPartyWalletAPI.Model.Game.SEXY.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static H1_ThirdPartyWalletAPI.Model.Game.SEXY.SEXY;

namespace H1_ThirdPartyWalletAPI.Service.Game.SEXY
{
    public interface ISEXYInterfaceService : IGameInterfaceService
    {
        Task PostSexyRecord(List<Record> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class SEXY_RecordService : ISEXYInterfaceService
    {
        private readonly ILogger<SEXY_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly ISEXYDBService _sexyDBService;
        private readonly ICacheDataService _cacheService;
        private readonly ISEXYApiService _apiService;
        private readonly IGameReportDBService _gameReportDBService;

        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public SEXY_RecordService(ILogger<SEXY_RecordService> logger,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IGameApiService gameaApiService,
            IDBService dbService,
            ISEXYDBService sexyDBService, 
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _apiService = gameaApiService._SexyApi;
            _dbService = dbService;
            _summaryDBService = summaryDBService;
            _sexyDBService = sexyDBService;
            _cacheService = commonService._cacheDataService;
            _gameReportDBService = gameReportDBService;
        }
        #region GameInterfaceService

        public async Task PostSexyRecord(List<Record> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();


                var oldLogs = (await _sexyDBService.GetsexyRecordsBytime(recordData.Min(l => l.betTime), recordData.Max(l => l.betTime)))
          .Select(l => new { l.platformTxId, l.betTime, l.updateTime }).ToHashSet();

                //去除同批內重複注單，並以updateTime較大的為主
                recordData = recordData.OrderByDescending(r => r.updateTime)
                                       .DistinctBy(r => new { r.platformTxId, r.betTime, r.updateTime })
                                       .Reverse()
                                       .ToList();
                var linqRes = recordData.GroupBy(x => x.userId);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            if (Config.OneWalletAPI.Prefix_Key != "PRD")
                            {
                                if (!group.Key.Contains(Config.OneWalletAPI.Prefix_Key))
                                {
                                    continue;
                                }
                            }

                            string club_id;
                            club_id = group.Key.Substring(3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.SEXY);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No SEXY user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData =
                                new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<Record> betDetailData = new List<Record>();

                            foreach (Record r in group) //loop club id bet detail
                            {
                                //跳過重複注單
                                if (oldLogs.Contains(new { r.platformTxId, r.betTime, r.updateTime }))
                                    continue;

                                r.pre_betAmount = r.betAmount;
                                r.pre_realBetAmount = r.realBetAmount;
                                r.pre_realWinAmount = r.realWinAmount;
                                r.pre_turnover = r.turnover;

                                int Game_type = 0;
                                LiveGameMap.CodeToId.TryGetValue(r.gameCode, out Game_type);

                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.SEXY);
                                sumData.Game_type = Game_type;
                                DateTime tempDateTime = r.betTime;
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
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.platformTxId);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.platformTxId);
                                        }
                                    }
                                }

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                    //合併處理
                                    sumData = await CalculateLIVE(sumData, r, tran);
                                    summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results =
                                        await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    results = results.Where(x => x.Game_type == sumData.Game_type).ToList();
                                    if (!results.Any()) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = await CalculateLIVE(sumData, r, tran);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = await CalculateLIVE(sumData, r, tran);
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

                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            int PostRecordResult = await _sexyDBService.PostSEXYRecord(conn, tran, betDetailData);
                            _logger.LogDebug("insert SEXY record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (Record r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.platformTxId, r.updateTime);

                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run SEXY record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }

                await conn.CloseAsync();
            }
        }


        /// <summary>
        /// 廠商小時帳
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }

                _logger.LogDebug("Create SEXY game provider report time {datetime}", reportTime);
                // 每小時投注匯總
                GetSummaryByTxTimeHourRequest req = new GetSummaryByTxTimeHourRequest()
                {
                    cert = Config.CompanyToken.SEXY_Cert,
                    agentId = Config.CompanyToken.SEXY_Agent,
                    startTime = reportTime.ToString("yyyy-MM-ddTHHzzz", CultureInfo.InvariantCulture),
                    endTime = reportTime.AddHours(1).ToString("yyyy-MM-ddTHHzzz", CultureInfo.InvariantCulture),
                    platform = "SEXYBCRT"
                };

                //取得這小時
                GetSummaryByTxTimeHourResponse SexyCenterList = await _apiService.GetSummaryByTxTimeHour(req);
                if (SexyCenterList.transactions.Count == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.SEXY),
                        report_datetime = reportTime,
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
                    decimal total_bet = 0;
                    decimal total_win = 0;
                    decimal total_netwin = 0;
                    int total_count = 0;
                    foreach (var item in SexyCenterList.transactions)
                    {
                        total_bet += item.betAmount;
                        total_win += item.realWinAmount;
                        total_netwin += (item.realWinAmount - item.betAmount);
                        total_count += item.betCount;
                    }
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.SEXY),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = total_bet,
                        total_win = total_win,
                        total_netwin = total_netwin,
                        total_count = total_count,
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddHours(1);
                }
                await Task.Delay(20000);
            }
        }

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
                _logger.LogDebug("Create SEXY game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _sexyDBService.SumsexyBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.SEXY);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin;
                reportData.total_netwin = totalWin - totalBetValid;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            GetBalanceRequest req = new GetBalanceRequest();
            req.userIds = platform_user.game_user_id;
            req.isFilterBalance = 0;
            req.alluser = 0;
            req.cert = Config.CompanyToken.SEXY_Cert;
            req.agentId = Config.CompanyToken.SEXY_Agent;

            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _apiService.GetBalance(req);
                Balance.Amount = Convert.ToDecimal(res.results[0].balance);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("SEXY餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.SEXY);
            return Balance;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {

                var responseData = await _apiService.GameLogout(new GameLogoutRequest()
                {
                    cert = Config.CompanyToken.SEXY_Cert,
                    agentId = Config.CompanyToken.SEXY_Agent,
                    userIds = platform_user.game_user_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出SEXY使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {

                var responseData = await _apiService.Deposit(new DepositRequest
                {
                    cert = Config.CompanyToken.SEXY_Cert,
                    agentId = Config.CompanyToken.SEXY_Agent,
                    transferAmount = Math.Round(transfer_amount, 4),
                    txCode = RecordData.id.ToString(),
                    userId = platform_user.game_user_id
                }); ;

                if (responseData.status != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage(responseData.status, responseData.desc);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("SEXY TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("SEXY TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInSexyFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }

        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var responseData = await _apiService.Withdraw(new WithdrawRequest
                {
                    cert = Config.CompanyToken.SEXY_Cert,
                    agentId = Config.CompanyToken.SEXY_Agent,
                    transferAmount = Math.Round(transfer_amount, 4),
                    txCode = RecordData.id.ToString(),
                    userId = platform_user.game_user_id,
                });

                if (responseData.status != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage(responseData.status, responseData.desc);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("SEXY TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("SEXY TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInSexyFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }



        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.SEXY.SEXY.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            string Account = Config.OneWalletAPI.Prefix_Key + userData.Club_id;
            BetLimitClass betLimitClass = new BetLimitClass();
            Model.Game.SEXY.SEXY.LIVE lIVE = new LIVE();
            List<int> BetLimit = new List<int>();
            BetLimit.Add(260901);
            BetLimit.Add(260902);
            BetLimit.Add(260903);
            lIVE.limitId = BetLimit;
            Model.Game.SEXY.SEXY.SEXYBCRT sEXYBCRT = new SEXYBCRT();
            sEXYBCRT.LIVE = lIVE;
            betLimitClass.SEXYBCRT = sEXYBCRT;
            var req = new CreateMemberRequest()
            {
                cert = Config.CompanyToken.SEXY_Cert,
                agentId = Config.CompanyToken.SEXY_Agent,
                userId = Account,
                currency = Model.Game.SEXY.SEXY.Currency[userData.Currency],
                betLimit = JsonConvert.SerializeObject(betLimitClass),
                language = "th",
                userName = Account,
            };
            //創建帳號
            try
            {
                var response = await _apiService.CreateMember(req);

                if (response.status == (int)ErrorCodeEnum.Success || response.status == (int)ErrorCodeEnum.Account_existed)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = Account;
                    gameUser.game_platform = request.Platform;
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.desc);
                }

            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message.ToString());
            }


        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 4 Get Game URL
            var requestData = new DoLoginAndLaunchGameRequest()
            {
                cert = Config.CompanyToken.SEXY_Cert,
                agentId = Config.CompanyToken.SEXY_Agent,
                userId = platformUser.game_user_id,
            };
            requestData.hall = "SEXY";
            requestData.platform = "SEXYBCRT";
            requestData.gameType = "LIVE";

            requestData.gameCode = "MX-LIVE-001";

            //if (request.GameConfig.ContainsKey("gameCode"))
            //{
            //    requestData.gameCode = request.GameConfig["gameCode"];
            //}
            List<int> BetLimit = new List<int>();
            if (request.GameConfig.ContainsKey("betLimitGroup"))
            {
                string betLimitGroup = "";
                betLimitGroup = request.GameConfig["betLimitGroup"];

                if (!string.IsNullOrEmpty(betLimitGroup))
                {
                    List<string> betLimitStrList = new List<string>();
                    betLimitStrList = betLimitGroup.Split(',').ToList();
                    foreach (string item in Model.Game.SEXY.SEXY.SexyBetLimitList.Intersect(betLimitStrList))
                    {
                        BetLimit.Add(Convert.ToInt32(item));
                    }
                }
            }
            if (BetLimit.Count == 0)
            {
                BetLimit.Add(260901);
                BetLimit.Add(260902);
                BetLimit.Add(260903);
            }
            BetLimitClass betLimitClass = new BetLimitClass();
            Model.Game.SEXY.SEXY.LIVE lIVE = new LIVE();


            lIVE.limitId = BetLimit;
            Model.Game.SEXY.SEXY.SEXYBCRT sEXYBCRT = new SEXYBCRT();
            sEXYBCRT.LIVE = lIVE;
            betLimitClass.SEXYBCRT = sEXYBCRT;

            requestData.betLimit = JsonConvert.SerializeObject(betLimitClass);

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                requestData.externalURL = request.GameConfig["lobbyURL"];
            }


            if (request.GameConfig.ContainsKey("lang") && Model.Game.SEXY.SEXY.lang.ContainsKey(request.GameConfig["lang"]))
            {
                requestData.language = Model.Game.SEXY.SEXY.lang[request.GameConfig["lang"]];
            }
            else
            {
                requestData.language = Model.Game.SEXY.SEXY.lang["en-US"];
            }

            try
            {
                var response = await _apiService.DoLoginAndLaunchGame(requestData);
                if (response.status != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail]);
                }
                return response.url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var SexyReuslt = await _apiService.CheckTransferOperation(new CheckTransferOperationRequest
            {
                cert = Config.CompanyToken.SEXY_Cert,
                agentId = Config.CompanyToken.SEXY_Agent,
                txCode = transfer_record.id.ToString()
            });
            if (SexyReuslt.txStatus == 1)
            {
                if (transfer_record.target == nameof(Platform.SEXY))//轉入SEXY直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.SEXY))
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
            else if (SexyReuslt.txStatus == 0 || SexyReuslt.status != (int)ErrorCodeEnum.Success)
            {
                if (transfer_record.target == nameof(Platform.SEXY))//轉入SEXY直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.SEXY))
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

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var sexyRecord = await _sexyDBService.GetsexyRecord(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            if (sexyRecord == null)
            {
                throw new Exception("no data");
            }


            GetTransactionHistoryResultRequest source = new GetTransactionHistoryResultRequest();
            source.cert = Config.CompanyToken.SEXY_Cert;
            source.agentId = Config.CompanyToken.SEXY_Agent;
            source.userId = sexyRecord.userid;
            source.platform = sexyRecord.platform;
            source.platformTxId = sexyRecord.platformtxid;
            source.roundId = sexyRecord.roundid;

            var response = await _apiService.GetTransactionHistoryResult(source);
            if (response.status != (int)ErrorCodeEnum.Success)
            {
                throw new Exception(response.desc);
            }

            if (Model.Game.SEXY.SEXY.lang.ContainsKey(RecordDetailReq.lang))
            {
                response.txnUrl = response.txnUrl.Replace("&lang=en", "&lang=" + Model.Game.SEXY.SEXY.lang[RecordDetailReq.lang]);
            }


            return response.txnUrl;

        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalHours > 1)
            {
                endTime = startTime.AddHours(1);
                RepairCount += await RepairSexy(startTime, endTime);
                startTime = endTime;
                //api建議20~30秒爬一次
                await Task.Delay(20000);
            }
            RepairCount += await RepairSexy(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Live;
        }

        public Task HealthCheck(Platform platform)
        {
            return _apiService.GetBalance(new()
            {
                userIds = "HealthCheck",
                isFilterBalance = 0,
                alluser = 0,
                cert = Config.CompanyToken.SEXY_Cert,
                agentId = Config.CompanyToken.SEXY_Agent
            });
        }
        #endregion
        /// <summary>
        /// 計算彙總
        /// </summary>
        /// <param name="SummaryData"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private BetRecordSummary Calculate(BetRecordSummary SummaryData, Record r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Math.Abs(r.betAmount);
            SummaryData.Turnover += Math.Abs(r.turnover);
            SummaryData.Netwin += r.realWinAmount - r.realBetAmount;
            SummaryData.Win += r.realWinAmount;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }

        private async Task<BetRecordSummary> CalculateLIVE(BetRecordSummary SummaryData, Record r, IDbTransaction tran)
        {
            var oldRecords = await _sexyDBService.GetsexyRecord(tran, r.platformTxId, r.betTime);
            oldRecords ??= new();

            //重複單則跳開
            if (oldRecords.Any(oldr => new { oldr.platformTxId, oldr.betTime, oldr.updateTime }.Equals(new { r.platformTxId, r.betTime, r.updateTime })))
                return SummaryData;

            //沖銷掉原注單
            if (oldRecords.Any())
            {
                Record lastRecord = oldRecords.OrderByDescending(r => r.updateTime).First(); //沖銷最後一筆即可
                r.betAmount = r.betAmount - lastRecord.pre_betAmount;
                r.realBetAmount = r.realBetAmount - lastRecord.pre_realBetAmount;
                r.realWinAmount = r.realWinAmount - lastRecord.pre_realWinAmount;
                r.turnover = r.turnover - lastRecord.turnover;
            }
            return Calculate(SummaryData, r);
            throw new ExceptionMessage(ResponseCode.Fail, $"PostSEXYLiveRecord Fail! | UnSupported platformTxId: {r.platformTxId} Record:{System.Text.Json.JsonSerializer.Serialize(r)}");

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
        /// SEXY 帳務比對
        /// 1. 比對轉帳中心與遊戲商的下注明細是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairSexy(DateTime startTime, DateTime endTime)
        {
            var req = new GetTransactionByTxTimeRequest
            {
                cert = Config.CompanyToken.SEXY_Cert,
                agentId = Config.CompanyToken.SEXY_Agent,
                startTime = startTime,
                endTime = endTime,
                platform = "SEXYBCRT"
            };

            GetTransactionByUpdateDateResponse res = new GetTransactionByUpdateDateResponse()
            {
                transactions = new List<Record>()
            };
            while (true)
            {
                var betLogs = await _apiService.GetTransactionByTxTime(req);

                if (betLogs.transactions.Count == 0)
                {
                    break;
                }
                res.transactions.AddRange(betLogs.transactions);

                if (betLogs.transactions.Count > 20000)
                {
                    req.startTime = betLogs.transactions.Max(x => x.betTime);
                }
                else
                {
                    break;
                }
                //api建議20~30秒爬一次
                await Task.Delay(20000);
            }

            var w1CenterList = await _sexyDBService.GetsexyRecordsBytime(startTime, endTime);

            res.transactions = res.transactions.Except(w1CenterList, new SEXYBetRecordComparer()).ToList();

            if (res.transactions.Count > 0)
            {
                res.transactions = res.transactions.Distinct().ToList();
                await PostSexyRecord(res.transactions);
            }

            return res.transactions.Count;
        }

        /// <summary>
        /// 設定差異比對參數
        /// </summary>
        public class SEXYBetRecordComparer : IEqualityComparer<Record>
        {
            public bool Equals(Record x, Record y)
            {
                //確認兩個物件的資料是否相同
                if (Object.ReferenceEquals(x, y)) return true;

                //確認兩個物件是否有任何資料為空值
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //這邊就依照個人需求比對各個屬性的值
                return x.userId == y.userId
                    && x.platformTxId == y.platformTxId
                    && x.gameCode == y.gameCode;
            }

            public int GetHashCode(Record e)
            {
                //確認物件是否為空值
                if (Object.ReferenceEquals(e, null)) return 0;

                int parentBetId = e.userId == null ? 0 : e.userId.GetHashCode();
                int betId = e.platformTxId == null ? 0 : e.platformTxId.GetHashCode();

                //計算HashCode，因為是XOR所以要全部都是1才會回傳1，否則都會回傳0
                return parentBetId ^ betId;
            }
        }


        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            IEnumerable<dynamic> sexy_results = await _sexyDBService.GetsexyRecordsBySummary(RecordReq);
            sexy_results = sexy_results.OrderByDescending(e => e.betTime);
            res.Data = sexy_results.ToList();
            return res;
        }

    }
}
