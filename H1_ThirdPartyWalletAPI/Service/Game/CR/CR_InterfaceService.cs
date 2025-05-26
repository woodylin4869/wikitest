using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Worker.Game.CR;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.CR.Enum;
using ThirdPartyWallet.Share.Model.Game.CR.Request;
using ThirdPartyWallet.Share.Model.Game.CR.Response;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using CRsetup = ThirdPartyWallet.Share.Model.Game.CR.CR;

namespace H1_ThirdPartyWalletAPI.Service.Game.CR
{
    public interface ICRInterfaceService : IGameInterfaceService
    {
        Task<int> PostCRRecord(List<Wager_Data> recordData);

        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);

        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class CR_InterfaceService : ICRInterfaceService
    {
        private readonly ILogger<CR_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;

        private readonly ICRDBService _CRDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly ICRApiService _CRApiService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;

        public CR_InterfaceService(ILogger<CR_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            ICRDBService CRDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            ICRApiService CRApiService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _CRDBService = CRDBService;
            _CRApiService = CRApiService;
            _systemParameterDbService = systemParameterDbService;
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
                var responseData = await _CRApiService.chkMemberBalanceAsync(new chkMemberBalanceRequest
                {
                    memname = platform_user.game_user_id,
                });

                if (responseData.RespCode != "0000")
                {
                    throw new Exception(responseData.Error);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.balance), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("CR餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.CR);
            return Balance;
        }

        /// <summary>
        /// 踢線(RG富遊 不支援踢線)
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platform_user"></param>
        /// <returns></returns>
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var responseData = await _CRApiService.KickOutMemAsync(new KickOutMemRequest
                {
                    memname = platform_user.game_user_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出CR使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }

        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
            if (!CRsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreateMemberRequest()
                {
                    memname = ConvertClubInfoToGamePlatformUser(userData.Club_id),
                    currency = CRsetup.Currency[userData.Currency]
                };

                var responseData = await _CRApiService.CreateMemberAsync(req);
                if (responseData.RespCode == "0000" || responseData.RespCode == "0008")
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.memname;
                    gameUser.game_platform = Platform.CR.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(responseData.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CR建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "CR " + ex.Message.ToString());
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
                var responseData = await _CRApiService.DepositAsync(new DepositRequest
                {
                    memname = platform_user.game_user_id,
                    payno = RecordData.id.ToString(),
                    amount = RecordData.amount,
                });

                if (responseData.RespCode != "0000")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("CR Deposit: {Message}", responseData.Error);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("CR TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("CR Deposit: {Message}", ex.Message);
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
                decimal transfer_amount = Math.Floor(RecordData.amount * 100) / 100;
                var responseData = await _CRApiService.WithdrawAsync(new WithdrawRequest
                {
                    memname = platform_user.game_user_id,
                    payno = RecordData.id.ToString(),
                    amount = transfer_amount,
                });

                if (responseData.RespCode != "0000")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("CR Withdraw : {ex}", responseData.Error);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("CR TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("CR Withdraw : {ex}", ex.Message);
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
            LaunchGameRequest UrlRequest = new LaunchGameRequest
            {
                memname = platformUser.game_user_id,
                isSSL = "Y",
                currency = CRsetup.Currency["THB"],
            };

            CRsetup.Lang.TryGetValue(request.GameConfig["lang"], out var lang);

            // 沒帶值預設給英文
            UrlRequest.langx = lang ?? CRsetup.Lang["en-US"];
            UrlRequest.remoteip = "127.0.0.1";
            if (request.GameConfig.ContainsKey("clientIP"))
            {
                UrlRequest.remoteip = request.GameConfig["clientIP"];
            }
            if (request.GameConfig.ContainsKey("device"))
            {
                UrlRequest.machine = CRsetup.Device[request.GameConfig["device"]];
            }
            else
            {
                UrlRequest.machine = CRsetup.Device["DESKTOP"];
            }

            try
            {
                var responseData = await _CRApiService.LaunchGameAsync(UrlRequest);
                if (responseData.RespCode != "0000")
                {
                    throw new Exception(responseData.Error);
                }
                return responseData.launchgameurl;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "CR: " + ex.Message.ToString());
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

            var responseData = await _CRApiService.ChkTransInfoAsync(new ChkTransInfoRequest
            {
                transidtype = "1",
                memname = ConvertClubInfoToGamePlatformUser(transfer_record.Club_id),
                transid = transfer_record.id.ToString()
            });
            if (responseData.RespCode == "0000")
            {
                if (transfer_record.target == nameof(Platform.CR))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.CR))
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
            else if (responseData.RespCode == "0003")
            {
                if (transfer_record.target == nameof(Platform.CR))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.CR))
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
            // var batRecords = new List<dynamic>();
            var betRecords = new List<Wager_Data>();

            GetBetRecord res = new();
            //var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);

            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _CRDBService.GetCRRecordsBytime(createTime, RecordReq.ReportTime, ConvertClubInfoToGamePlatformUser(RecordReq.ClubId));
                results = results.OrderByDescending(e => e.adddate).ToList();
                foreach (var result in results)
                {
                    betRecords.Add(result);
                }
            }

            // 統一輸出格式為 RespRecordLevel2_Electronic
            res.Data = betRecords.OrderByDescending(e => e.adddate).Select(obj => new RespRecordLevel2_Sport
            {
                RecordId = obj.id,
                BetTime = obj.adddate,
                GameId = obj.gtype,
                BetAmount = obj.gold,
                NetWin = obj.wingold - obj.degold,
                SettlementTime = (obj.result == "0" ? null : obj.resultdate),
                LeagueName = obj.league,
                HomeTeamName = obj.tname_home,
                AwayTeamName = obj.tname_away,
                BetTeam = obj.order,
                Odds = obj.ioratio ?? 0,
                OddsType = obj.odds
            }).Cast<dynamic>().ToList();
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
        /// 補單-會總
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            #region 檢查拉單進度(補單不可先於拉單，否則會有重複單)

            var recordScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(CRRecordSchedule.SYSTEM_PARAMETERS_KEY)).value);
            if (RepairReq.StartTime > recordScheduleTime || RepairReq.EndTime > recordScheduleTime)
                throw new ExceptionMessage(ResponseCode.Fail, "CR補單不可先於拉單!");

            #endregion 檢查拉單進度(補單不可先於拉單，否則會有重複單)

            var RepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 30)
            {
                endTime = startTime.AddMinutes(30);
                RepairCount += await Repair(startTime, endTime, (SearchMode)RepairReq.SearchType);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await Repair(startTime, RepairReq.EndTime, (SearchMode)RepairReq.SearchType);
            await Task.Delay(1000);
            #region 重新匯總帳
            var ReportScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(CRReportSchedule.SYSTEM_PARAMETERS_KEY)).value);

            var start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
            var maxEnd = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, 0);
            var offSet = TimeSpan.FromHours(1);
            while (start < maxEnd)
            {
                if (start > ReportScheduleTime)
                {
                    break;
                }

                var end = start.Add(offSet);


                await SummaryW1Report(start, end);
                await SummaryGameProviderReport(start, end);

                start = end;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            #endregion
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        #endregion GameInterfaceService

        #region GameRecordService

        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostCRRecord(List<Wager_Data> recordData)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, Wager_Data>> linqRes = recordData.OrderBy(x => x.result).GroupBy(x => x.username);

            var postResult = 0;
            foreach (IGrouping<string, Wager_Data> group in linqRes)
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
                            club_id = ConvertGamePlatformUserToClubInfo(group.Key);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.CR);
                            if (gameUser == null || !string.Equals(gameUser.game_user_id, group.Key, StringComparison.InvariantCultureIgnoreCase))
                            {
                                throw new Exception("No CR user");
                            }
                            // 紀錄 reportTime 跟 adddate(下注時間) 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            List<Wager_Data> betDetailData = new List<Wager_Data>();

                            List<Wager_Data> betDetailDataRunning = new List<Wager_Data>();
                            foreach (Wager_Data item in group)
                            {
                                item.Create_time = dt;
                                item.Report_time = reportTime;
                                // 把Partition col(bet_time) binding 到 col Partition_time
                                //item.adddate = item.adddate;

                                await Calculate(conn, tran, item);

                                //“0” 未有結果 “L”輸 “Ｗ”贏 “Ｐ”合 “D” 取消 “Ａ”還原   F 重複
                                switch (item.result)
                                {
                                    case "0":
                                        item.Club_id = memberWalletData.Club_id;
                                        item.Franchiser_id = memberWalletData.Franchiser_id;
                                        betDetailDataRunning.Add(item);
                                        break;
                                    case "F":
                                        continue;
                                    default:
                                        betDetailData.Add(item);
                                        break;
                                }

                                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }

                                dic[summaryTime].Add(item.adddate.ToString("yyyy-MM-dd HH:mm"));
                            }

                            if (betDetailDataRunning.Count > 0)
                            {
                                await _CRDBService.PostCRRunningRecord(conn, tran, betDetailDataRunning);
                                postResult += await _CRDBService.PostCRRecord(conn, tran, betDetailDataRunning);
                            }

                            if (betDetailData.Count > 0)
                            {
                                postResult += await _CRDBService.PostCRRecord(conn, tran, betDetailData);
                            }
                            await tran.CommitAsync();

                            // 記錄到 Redis reportTime 跟 adddate(下注時間) 的關聯
                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = $"{RedisCacheKeys.CRBetSummaryTime}:{item.Key}";
                                    await _commonService._cacheDataService.ListPushAsync(key, subItem);
                                }
                            }

                            // 清除資源
                            dic.Clear();

                            sw.Stop();
                            _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                            sw.Restart();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

                            _logger.LogError("Run CR record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
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

            //while (true)
            //{
            //    DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
            //    if (reportTime > endDateTime)
            //    {
            //        //超過結束時間離開迴圈
            //        break;
            //    }
            //    _logger.LogDebug("Create CR game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

            //    // 每日統計
            //    var resp = await _CRApiService.ReportHourAsync(new ReportHourRequest()
            //    {
            //        Hour = reportTime
            //    });
            //    var gameEmptyReport = new GameReport();
            //    if (resp.Data.Bet_total == 0)
            //    {
            //        gameEmptyReport.platform = nameof(Platform.CR);
            //        gameEmptyReport.report_datetime = reportTime;
            //        gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
            //        gameEmptyReport.total_bet = 0;
            //        gameEmptyReport.total_win = 0;
            //        gameEmptyReport.total_netwin = 0;
            //        gameEmptyReport.total_count = 0;
            //    }
            //    else
            //    {
            //        gameEmptyReport.platform = nameof(Platform.CR);
            //        gameEmptyReport.report_datetime = reportTime;
            //        gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
            //        gameEmptyReport.total_bet = (decimal)resp.Data.Bet_total;
            //        gameEmptyReport.total_win = (decimal)resp.Data.Payoff + resp.Data.Bet_real;
            //        gameEmptyReport.total_netwin = (decimal)resp.Data.Payoff;
            //        gameEmptyReport.total_count = resp.Data.Bet_count;
            //    }
            //    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
            //    await _gameReportDBService.PostGameReport(gameEmptyReport);
            //    startDateTime = startDateTime.AddHours(1);

            //    await Task.Delay(3000);
            //}
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
                _logger.LogDebug("Create CR game W1 report time {datetime}", reportTime);
                var (TotalCount, TotalBetValid, TotalNetWin) = await _CRDBService.SumCRBetRecordByPartitionTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.CR);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = TotalBetValid;
                reportData.total_win = TotalNetWin + TotalBetValid;
                reportData.total_netwin = TotalNetWin;
                reportData.total_count = TotalCount;

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
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, Wager_Data r)
        {
            //處理聯賽主客對資訊串關顯示內容
            if (r.parlaysub != null)
            {
                decimal combinedIoratio = 1;
                try
                {
                    foreach (var parlay in r.parlaysub.Values)
                    {
                        if (decimal.TryParse(parlay.ioratio, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ioratio))
                        {
                            combinedIoratio *= ioratio;
                        }
                    }
                }
                catch
                {
                }
                r.ioratio = Math.Round(combinedIoratio, 2); ;
                r.league = "Parlay (" + r.parlaynum + ")";
                r.tname_home = " - ";
                r.tname_away = " - ";
                r.order = " - ";
            }

            //“0” 未有結果 “L”輸 “Ｗ”贏 “Ｐ”合 “D” 取消 “Ａ”還原 F 重複
            if (r.result == "D")
            {
                // 取消單 結果為 10/ 0 / 0
                r.degold = 0;
                r.wingold = 0;
            }
            else if (r.result == "0")
            {
                // 一般未結算淨輸贏都是給0，因為計算要先算輸，之後結算在沖銷回來
                r.wingold = 0;
            }
            r.Pre_gold = r.gold;
            r.Pre_degold = r.degold;
            r.Pre_wingold = r.wingold;

            var record = await _CRDBService.GetCRRecords(r.id, r.adddate) ?? new List<Wager_Data>();

            // if (record.Any(x => new { x.billNo, x.createtime, x.billstatus }.Equals(new { r.billNo, createtime = DateTimeOffset.FromUnixTimeMilliseconds(r.createtime).ToOffset(TimeSpan.FromHours(8)).DateTime, r.billstatus })))
            if ((r.result == "0" && record.Any()) || record.Any(x => x.id == r.id && x.adddate == r.adddate && x.result == r.result && x.resultdate == r.resultdate))
            {
                // 收到未結算單若已經有任何歷史注單則判斷為重複單
                // 如果相同pkey資料就設定狀態為F，並return
                r.result = "F";
                return;
            }
            if (record.Any())
            {
                // 取最後一筆結算單來訂正沖銷(update)
                var lastpp = record.OrderByDescending(x => x.resultdate).First();

                r.gold = r.gold - lastpp.Pre_gold;
                r.degold = r.degold - lastpp.Pre_degold;
                r.wingold = r.wingold - lastpp.Pre_wingold;
            }
            await _CRDBService.DeleteCRRunningRecord(conn, tran, r);
        }

        /// <summary>
        /// 補單
        /// 手動補單依照req給定SearchMode
        /// 自動補單預設SearchMode為BetTime(下注時間)
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> Repair(DateTime startTime, DateTime endTime, SearchMode searchMode = SearchMode.BetTime)
        {
            //var w1data = await _CRDBService.GetCRRecordsByPartition(startTime, endTime);
            List<Wager_Data> gameProviderBetRecords = new List<Wager_Data>();
            var resPK = gameProviderBetRecords.Select(x => new { x.id, x.cashoutid, x.adddate }).ToHashSet();
            // 當前頁碼
            var pageIndex = 1;
            // 取得總頁數
            int? pageCount = null;
            //// 每頁取得筆數
            //var pageLimit = 50;
            // 取得注單delay時間(毫秒)
            int callAPIdelayMS = 1000;


            //每頁50筆
            var recordRequest = new ALLWagerRequest
            {
                dateStart = startTime.AddHours(-12),
                dateEnd = endTime.AddHours(-12),
                settle = 1,
                langx = "en-us",
                page = pageIndex
            };

            do
            {
                // 設定頁碼
                recordRequest.page = pageIndex;

                var betRecord = await _CRApiService.ALLWagerAsync(recordRequest, 2);
                if (betRecord.wager_data != null && betRecord.wager_data.Count > 0)
                {
                    foreach (var item in betRecord.wager_data)
                    {
                        if (!item.resultdate.HasValue)
                        {
                            item.resultdate = item.adddate;
                        }
                        item.adddate = item.adddate.AddHours(12);
                        item.resultdate = item.resultdate.Value.AddHours(12);

                        if (resPK.Add(new { item.id, item.cashoutid, item.adddate }))
                        {
                            gameProviderBetRecords.Add(item);
                        }
                    }
                }


                pageCount = betRecord.wager_totalpage;

                pageIndex++;
                await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
            } while (pageCount > pageIndex);


            //同時段條件讀取未結算單
            recordRequest.settle = 0;
            pageIndex = 1;

            do
            {
                // 設定頁碼
                recordRequest.page = pageIndex;
                var betRecord = await _CRApiService.ALLWagerAsync(recordRequest, 2);
                if (betRecord.wager_data != null && betRecord.wager_data.Count > 0)
                {
                    foreach (var item in betRecord.wager_data)
                    {
                        if (!item.resultdate.HasValue)
                        {
                            item.resultdate = item.adddate;
                        }
                        item.adddate = item.adddate.AddHours(12);
                        item.resultdate = item.resultdate.Value.AddHours(12);

                        if (resPK.Add(new { item.id, item.cashoutid, item.adddate }))
                        {
                            gameProviderBetRecords.Add(item);
                        }
                    }
                }

                pageCount = betRecord.wager_totalpage;

                pageIndex++;
                await Task.Delay(callAPIdelayMS); // (查詢注單限制，如果有在設定時間)
            } while (pageCount > pageIndex);



            int effRecord = 0;
            if (gameProviderBetRecords.Any() == false) return 0;

            if (gameProviderBetRecords.Any() == true)
            {
                // 排除重複注單
                var postBetRecords = gameProviderBetRecords.DistinctBy(record => new { record.id, record.cashoutid, record.result, record.adddate }).ToList();


                // 使用 LINQ 提取不重複的日期
                var uniqueDates = postBetRecords?.Select(x => Convert.ToDateTime(x.adddate).Date).Distinct().ToList();
                // 這裡讀取資料排除範圍重複單   之後再補 先用內層判斷重複單
                //var w1data = await _CRDBService.GetCRRecordsByPartition(startTime, endTime);

                if (postBetRecords.Any() == true)
                {
                    effRecord = await PostCRRecord(postBetRecords);
                }
            }

            return effRecord;
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

            // TODO: RecordSummary需要移除DateTime startTime, DateTime endTime，Schedule 跟著把DateTime startTime, DateTime endTime 邏輯處理刪除
            // 取得匯總需要的起始和結束時間
            //DateTime? startTime = null;
            //DateTime? endTime = null;

            // 將老虎機、魚機記錄好的 reporttime > playtime 取出
            var redisKey = $"{RedisCacheKeys.CRBetSummaryTime}:{reportDatetime.ToString("yyyy-MM-dd HH:mm")}";

            //redisKey 空值的話會變 
            var timeStringList = await _commonService._cacheDataService.ListGetAsync<string>(redisKey);

            // 使用 LINQ 提取不重複的日期
            var uniqueDates = timeStringList?.Select(date => Convert.ToDateTime(date).Date).Distinct().ToList();
            List<(int count, decimal netWin, decimal bet, decimal betValidBet, decimal jackpot, string userid, int game_type, DateTime partitionTime)> summaryRecords = new List<(int, decimal, decimal, decimal, decimal, string, int, DateTime)>();

            if (uniqueDates != null && uniqueDates.Any())
            {
                foreach (DateTime item in uniqueDates)
                {
                    var _summaryRecords = await _CRDBService.SummaryGameRecord(reportDatetime, item, item.AddDays(1).AddMilliseconds(-1));
                    summaryRecords.AddRange(_summaryRecords);
                }
            }
            else
            {
                summaryRecords.AddRange(await _CRDBService.SummaryGameRecord(reportDatetime, reportDatetime.AddDays(-2), reportDatetime.AddHours(1).AddMilliseconds(-1)));

                //_logger.LogError($"CRBetSummaryTime Error, reportDatetime={reportDatetime.ToString("yyyy-MM-dd HH:mm")}");
            }
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => ConvertGamePlatformUserToClubInfo(x.userid)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();

            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.betValidBet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.CR);
                summaryData.Game_type = 0;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => (x.netWin + x.betValidBet));
                summaryData.Netwin = summaryRecord.Sum(x => x.netWin);
                summaryRecordList.Add(summaryData);

                foreach (var item in summaryRecord)
                {
                    var mapping = new t_summary_bet_record_mapping()
                    {
                        summary_id = summaryData.id,
                        report_time = reportDatetime,
                        partition_time = item.partitionTime
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

        #endregion GameRecordService

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
            return PlatformType.Sport;
        }

        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Task HealthCheck(Platform platform)
        {
            return _CRApiService.healthcheckAsync();
        }

        /// <summary>
        /// 取得未結算
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> betRecords = await _CRDBService.GetCRRunningRecord(RecordReq);
            // 統一輸出格式為 RespRecordLevel2_Electronic
            res.Data = betRecords.OrderByDescending(e => e.adddate).Select(obj => new RespRecordLevel2_Sport_Unsettle
            {
                RecordId = obj.id,
                BetTime = obj.adddate,
                GameId = obj.gtype,
                BetAmount = obj.gold,
                NetWin = obj.wingold - obj.degold,
                SettlementTime = (obj.result == "0" ? null : obj.resultdate),
                LeagueName = obj.league,
                HomeTeamName = obj.tname_home,
                AwayTeamName = obj.tname_away,
                BetTeam = obj.order,
                Odds = obj.ioratio ?? 0,
                OddsType = obj.odds,
                Club_id = obj.Club_id,
                Franchiser_id = obj.Franchiser_id,
            }).Cast<dynamic>().ToList();
            return res;
        }

        /// <summary>
        /// 遊戲商注單明細表 GamePlatformUser 轉換 Club Info 屬性規則
        /// 使用情境：後彙總排程從遊戲明細查詢使用者遊戲帳號 轉換 為H1的Club_Id 提供 wallet 查詢使用到
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        private string ConvertGamePlatformUserToClubInfo(string propertyValue)
        {
            string result = "";
            //依照環境變數調整Prefix
            int prefixLength = Config.OneWalletAPI.Prefix_Key.Length;
            result = propertyValue.Substring(prefixLength);
            return result.ToUpper();
        }

        /// <summary>
        /// H1 Club Info轉換GamePlatformUser 屬性規則
        /// 使用情境：第二層明細查詢從H1傳來的Club_Id組合wallet 和 遊戲明細資料表的遊戲帳號會使用到
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        private string ConvertClubInfoToGamePlatformUser(string propertyValue)
        {
            string result = "";
            //依照環境變數調整Prefix
            result = Config.OneWalletAPI.Prefix_Key + propertyValue;
            return result.ToUpper();
        }
    }
}