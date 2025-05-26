using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.GameAPI.Service.Game.WE;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.WE;
using ThirdPartyWallet.Share.Model.Game.WE.Request;
using ThirdPartyWallet.Share.Model.Game.WE.Response;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using WEsetup = ThirdPartyWallet.Share.Model.Game.WE.WE;

namespace H1_ThirdPartyWalletAPI.Service.Game.WE
{
    public interface IWEInterfaceService : IGameInterfaceService
    {
        Task<int> PostWERecord(List<BetRecordResponse.Datum> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class WE_InterfaceService : IWEInterfaceService
    {
        private readonly ILogger<WE_InterfaceService> _logger;
        private readonly IOptions<WEConfig> _options;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IWEDBService _WEDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IWEApiService _WEApiService;
        private readonly IGameTypeMappingDBService _gametypemappingbdservice;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        public WE_InterfaceService(ILogger<WE_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IOptions<WEConfig> options,
            IWEDBService WEDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IWEApiService WEApiService,
            IGameTypeMappingDBService gametypemappingbdservice)
        {
            _logger = logger;
            _options = options;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _WEDBService = WEDBService;
            _WEApiService = WEApiService;
            _gametypemappingbdservice = gametypemappingbdservice;
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
            //特殊需求WE  取餘額先除100
            try
            {
                var responseData = await _WEApiService.BalanceAsync(new BalanceRequest
                {
                    playerID = platform_user.game_user_id,
                });

                if (responseData.code != 0)
                {
                    throw new Exception(WEsetup.ErrorCode[responseData.code]);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.balance) / 100, 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("WE餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.WE);
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
                var responseData = await _WEApiService.LogoutAsync(new LogoutRequest
                {
                    playerID = platform_user.game_user_id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出WE使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
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
            if (!WEsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreateUserRequest()
                {
                    nickname = userData.Club_Ename,
                    playerID = Config.OneWalletAPI.Prefix_Key + userData.Club_id
                };


                var response = await _WEApiService.CreateUserAsync(req);
                if (response.code == 0 || response.code == 12005)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.playerID;
                    gameUser.game_platform = Platform.WE.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(WEsetup.ErrorCode[response.code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("WE建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "WE " + ex.Message.ToString());
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
                //特殊需求WE 入款要*100
                var responseData = await _WEApiService.DepositAsync(new DepositRequest
                {
                    playerID = platform_user.game_user_id,
                    uid = RecordData.id.ToString(),
                    amount = Math.Truncate(RecordData.amount * 100),
                });

                if (responseData.code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("WE Deposit: {Message}", responseData.code);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WE TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WE Deposit: {Message}", ex.Message);
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
                //因取餘額邊先除100 所以這邊要*100取得餘額
                var responseData = await _WEApiService.WithdrawAsync(new WithdrawRequest
                {
                    playerID = platform_user.game_user_id,
                    uid = RecordData.id.ToString(),
                    amount = Math.Truncate(RecordData.amount * 100),
                });

                if (responseData.code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("WE Withdraw : {ex}", WEsetup.ErrorCode[responseData.code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WE TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WE Withdraw : {ex}", ex.Message);
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
            WEsetup.Lang.TryGetValue(request.GameConfig["lang"], out var lang);
            LoginRequest UrlRequest = new LoginRequest
            {
                operatorID = _options.Value.WE_operatorrID,
                playerID = platformUser.game_user_id,
                tableID= request.GameConfig["gameid"],
                redirectUrl = request.GameConfig["lobbyURL"],
                uiMode = request.GameConfig["device"] == "DESKTOP" ? "desktop" : "mobile",
                lang = lang,
                category = "Live",
                clientIP= request.GameConfig["ip"]
            };
            try
            {
                var res = await _WEApiService.LoginAsync(UrlRequest);

                if (res.code != 0)
                {
                    throw new Exception(WEsetup.ErrorCode[res.code]);
                }


                return res.url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "WE: " + ex.Message.ToString());
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

            var Reuslt = await _WEApiService.TransferAsync(new TransferRequest
            {
                uid = transfer_record.id.ToString(),
            });
            if (Reuslt.dataCount > 0)
            {
                if (transfer_record.target == nameof(Platform.WE))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.WE))
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
                if (transfer_record.target == nameof(Platform.WE))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.WE))
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
            var Getsummary = await _summaryDBService.GetRecordSummaryById(new GetBetRecordReq
            {
                summary_id = RecordReq.summary_id,
                ReportTime = RecordReq.ReportTime
            });
            foreach (var createTime in createtimePair)
            {
                var results = await _WEDBService.GetWERecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId, Getsummary.Game_type);
                results = results.OrderByDescending(e => e.betDateTime).ToList();
                foreach (var result in results)
                {
                    batRecords.Add(result);

                }
            }

            // 統一輸出格式為 RespRecordLevel2_Electronic
            res.Data = batRecords.OrderByDescending(e => e.betDateTime).Select(obj => new RespRecordLevel2_Electronic
            {
                RecordId = obj.betID,
                BetTime = obj.betDateTime,
                GameType = obj.category,
                GameId = obj.tableID,
                BetAmount = obj.betAmount,
                NetWin = obj.winlossAmount,
                Jackpot = 0,
                BetStatus = obj.betStatus.ToString(),
                SettleTime = obj.settlementTime,
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
            WEsetup.Lang.TryGetValue(RecordDetailReq.lang, out var lang);
            var data = await _WEDBService.GetWERecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            var Settled = data.Where(x => x.betStatus == "complete").First();
            if (Settled == null)
            {
                throw new Exception("未結算無法轉跳廠商URL");
            }

            BetDetailUrlRequest request = new BetDetailUrlRequest()
            {
                betID = Settled.betID,
            };
            var res = await _WEApiService.BetDetailUrlAsync(request);
            res.url = res.url + $"&lang={lang}";
            return res.url;
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
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0));
            await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0));
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
        public async Task<int> PostWERecord(List<BetRecordResponse.Datum> recordData)
        {

            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, BetRecordResponse.Datum>> linqRes = recordData.GroupBy(x => x.playerID);

            var redisKey = $"{RedisCacheKeys.WEGetGameTypeMapping}";
            var GetGametype = await _commonService._cacheDataService.ListGetAsync<t_gametype_mapping>(redisKey);
            var GameTypeMapping = new List<t_gametype_mapping>();
            if (GetGametype.Count != 0)
            {
                GameTypeMapping = GetGametype.ToList();
            }
            else
            {
                GameTypeMapping = await _gametypemappingbdservice.GetGameTypeMapping(Platform.WE);

                var key = $"{RedisCacheKeys.WEGetGameTypeMapping}";
                foreach (var item in GameTypeMapping)
                {
                    await _commonService._cacheDataService.ListPushAsync(key, item);
                }
            }


            var postResult = 0;
            foreach (IGrouping<string, BetRecordResponse.Datum> group in linqRes)
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

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.WE);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No WE user");
                            }
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            List<BetRecordResponse.Datum> betDetailData = new List<BetRecordResponse.Datum>();

                            List<BetRecordResponse.Datum> betDetailDataRunning = new List<BetRecordResponse.Datum>();

                            foreach (BetRecordResponse.Datum item in group)
                            {

                                var GroupGameType = GameTypeMapping.Where(x => x.gametype == item.gameType);
                                if (!GroupGameType.Any())
                                    continue;

                                item.report_time = reportTime;
                                item.GroupGameType = GroupGameType.FirstOrDefault().groupgametype;
                                item.Groupgametype_id = GroupGameType.FirstOrDefault().groupgametype_id;
                                await Calculate(conn, tran, item);


                                switch (item.betStatus)
                                {
                                    case "F":
                                        //重複單不做處理
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

                                dic[summaryTime].Add(DateTimeOffset.FromUnixTimeSeconds(item.settlementTime).ToOffset(TimeSpan.FromHours(8)).DateTime.ToString("yyyy-MM-dd HH:mm"));
                            }


                            if (betDetailDataRunning.Count > 0)
                            {
                                await _WEDBService.PostWERunningRecord(conn, tran, betDetailDataRunning);
                                postResult += await _WEDBService.PostWERecord(conn, tran, betDetailDataRunning);
                            }

                            if (betDetailData.Count > 0)
                            {
                                postResult += await _WEDBService.PostWERecord(conn, tran, betDetailData);
                            }
                            tran.Commit();

                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = $"{RedisCacheKeys.WEBetSummaryTime}:{item.Key}";
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

                            _logger.LogError("Run WE record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day,
                    startDateTime.Hour, 0, 0);
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }

                _logger.LogDebug("Create WE game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _WEApiService.ReportHourAsync(new ReportHourRequest()
                {
                    startTime = (long)(startDateTime - unixEpoch).TotalSeconds,
                    endTime = (long)(startDateTime.AddHours(1).AddSeconds(-1) - unixEpoch).TotalSeconds,
                });
                var gameEmptyReport = new GameReport();
                if (req.numberOfBet == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.WE);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {

                    gameEmptyReport.platform = nameof(Platform.WE);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = req.currencyBetAmount / 100;
                    gameEmptyReport.total_win = req.currencyWinAmount / 100 + req.currencyBetAmount / 100;
                    gameEmptyReport.total_netwin = req.currencyWinAmount / 100;
                    gameEmptyReport.total_count = req.numberOfBet;
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
                _logger.LogDebug("Create WE game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalnetwin) = await _WEDBService.SumWEBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.WE);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalnetwin + totalBetValid;
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
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordResponse.Datum r)
        {


            if (r.betStatus == "new")
            {
                r.betAmount = r.betAmount;
                r.winlossAmount = r.winlossAmount;
            }
            else
            {
                r.validBetAmount = r.validBetAmount / 100;
                r.betAmount = r.betAmount / 100;
                r.winlossAmount = r.winlossAmount / 100;
                r.Pre_Bet = r.betAmount;
                r.Pre_NetWin = r.winlossAmount;
                r.Pre_Win = r.winlossAmount + r.betAmount;
            }



            var Record = await _WEDBService.GetWERecords(r.betID, DateTimeOffset.FromUnixTimeSeconds(r.betDateTime).ToOffset(TimeSpan.FromHours(8)).DateTime);

            Record ??= new();
            if (Record.Any(x => new { x.betID, x.betDateTime, x.betStatus, x.resettleTime }.Equals(new { r.betID, betDateTime = DateTimeOffset.FromUnixTimeSeconds(r.betDateTime).ToOffset(TimeSpan.FromHours(8)).DateTime, r.betStatus, resettleTime = DateTimeOffset.FromUnixTimeSeconds(r.resettleTime).ToOffset(TimeSpan.FromHours(8)).DateTime })))
            {
                r.betStatus = "F";
                return;
            }
            if (Record.Any())
            {
                var lastpp = Record.OrderByDescending(x => x.resettleTime).First();

                r.betAmount = r.betAmount - lastpp.Pre_Bet;
                r.winlossAmount = r.winlossAmount - lastpp.Pre_NetWin;
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

            var w1data = await _WEDBService.GetWERecordsBycreatetime(startTime, endTime);

            var resPK = w1data.Select(x => new { x.betID, x.betStatus, x.resettleTime }).ToHashSet();


            var res = new List<BetRecordResponse.Datum>();
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            string[] betstatus = { "complete", "cancel" };
            // 每日統計
            BetRecordRequest req = new BetRecordRequest()
            {
                betstatus = "complete",
                startTime = (long)(startTime - unixEpoch).TotalSeconds,
                endTime = (long)(endTime - unixEpoch).TotalSeconds,
                offset = 0,
                limit = 500
            };
            foreach (var item in betstatus)
            {
                var Page = 1;
                req.betstatus = item;

                while (true)
                {

                    var betLogs = await _WEApiService.BetRecordAsync(req);
                    req.offset = (Page - 1) * req.limit;
                    if (betLogs.dataCount == 0)
                    {
                        break;
                    }
                    foreach (var itme in betLogs.data)
                    {
                       
                        if (resPK.Add(new { itme.betID, itme.betStatus, resettleTime = DateTimeOffset.FromUnixTimeSeconds(itme.resettleTime).ToOffset(TimeSpan.FromHours(8)).DateTime }))
                        {
                            res.Add(itme);
                        }
                    }

                    if (Page > betLogs.dataCount / 500)
                        break;

                    Page++;

                    await Task.Delay(1000);
                }
            }

            if (res.Count == 0)
            {
                return 0;
            }

            return await PostWERecord(res);
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
            var summaryRecords = await _WEDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => new { x.userid,x.game_type});
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid.Substring(3)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();

            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key.userid.Substring(3), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x=>x.turnover);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.WE);
                summaryData.Game_type = summaryRecord.Key.game_type;
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
            return _WEApiService.HealthCheckAsync(new());
        }
        public Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            throw new NotImplementedException();
        }
        ///// <summary>
        ///// 取得位結算
        ///// </summary>
        ///// <param name="RecordReq"></param>
        ///// <returns></returns>
        //public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        //{
        //    GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
        //    IEnumerable<dynamic> bti_results = await _WEDBService.GetWERunningRecord(RecordReq);
        //    bti_results = bti_results.OrderByDescending(e => e.createtime);
        //    res.Data = bti_results.ToList();
        //    return res;
        //}

        public async Task<List<object>> GetGameApiList(Platform platform)
        {
            var Gamelist = await _WEApiService.GameListAsync(new GameListRequest
            {
                gamecategory = "Live"
            });

            return Gamelist.Cast<object>().ToList();
        }

        /// <summary>
        /// 設定限紅
        /// </summary>
        /// <param name="request"></param>
        /// <param name="gameUser"></param>
        /// <param name="memberWalletData"></param>
        /// <returns></returns>
        public async Task<ResCodeBase> SetLimit(SetLimitReq request, GamePlatformUser gameUser, Wallet memberWalletData)
        {
            if (gameUser == null)
            {
                gameUser = await CreateGameUser(
                    new ForwardGameReq
                    {
                        Platform = request.Platform,
                        Club_id = memberWalletData.Club_id
                    }, memberWalletData);
                await _commonService._gamePlatformUserService.PostGamePlatformUserAsync(gameUser);
            }

            var SetbetlimitRequest = new SetbetlimitRequest()
            {
                playerID = gameUser.game_user_id,
                betlimit = request.bet_setting.ToString()
            };
            var res = new ResCodeBase();
            var data = await _WEApiService.SetBetLimitAsync(SetbetlimitRequest);

            if (!string.IsNullOrEmpty(data.error))
            {
                res.code = 1;
                res.Message = data.error;
                return res;
            }

            res.code = (int)ResponseCode.Success;
            res.Message = MessageCode.Message[(int)ResponseCode.Success];
            return res;
        }
    }

}
