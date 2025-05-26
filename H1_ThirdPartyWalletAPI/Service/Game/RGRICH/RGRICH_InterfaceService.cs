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
using RGRICHsetup = ThirdPartyWallet.Share.Model.Game.RGRICH.RGRICH;
using H1_ThirdPartyWalletAPI.Model.Config;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Request;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Response;
using Status = ThirdPartyWallet.Share.Model.Game.RGRICH.Enum.Status;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.RGRICH.Enum;
using H1_ThirdPartyWalletAPI.Worker.Game.RGRICH;

namespace H1_ThirdPartyWalletAPI.Service.Game.RGRICH
{
    public interface IRGRICHInterfaceService : IGameInterfaceService
    {
        Task<int> PostRGRICHRecord(List<BetRecordResponse> recordData);

        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);

        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class RGRICH_InterfaceService : IRGRICHInterfaceService
    {
        private readonly ILogger<RGRICH_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;

        private readonly IRGRICHDBService _RGRICHDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IRGRICHApiService _RGRICHApiService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;

        public RGRICH_InterfaceService(ILogger<RGRICH_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IRGRICHDBService RGRICHDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IRGRICHApiService RGRICHApiService,
            ISystemParameterDbService systemParameterDbService)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _RGRICHDBService = RGRICHDBService;
            _RGRICHApiService = RGRICHApiService;
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
                var responseData = await _RGRICHApiService.BalanceAsync(new BalanceRequest
                {
                    UserName = platform_user.game_user_id,
                });

                if (responseData.Success != true)
                {
                    throw new Exception(responseData.Message);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.Data.Money), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("RGRICH餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.RGRICH);
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
                var responseData = await _RGRICHApiService.KickUserAsync(new KickUserRequest
                {
                    UserName = platform_user.game_user_id,
                    Min = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出RGRICH使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
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
            if (!RGRICHsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreateUserRequest()
                {
                    UserName = ConvertClubInfoToGamePlatformUser(userData.Club_id),
                    RealName = ConvertClubInfoToGamePlatformUser(userData.Club_id)
                };

                var response = await _RGRICHApiService.CreateUserAsync(req);
                if (response.Success == true)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.UserName;
                    gameUser.game_platform = Platform.RGRICH.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RGRICH建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "RGRICH " + ex.Message.ToString());
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
                var responseData = await _RGRICHApiService.RechargeAsync(new RechargeRequest
                {
                    UserName = platform_user.game_user_id,
                    FlowNumber = RecordData.id.ToString(),
                    Money = RecordData.amount,
                });

                if (responseData.Success != true)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("RGRICH Deposit: {Message}", responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RGRICH TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RGRICH Deposit: {Message}", ex.Message);
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
                var responseData = await _RGRICHApiService.WithdrawAsync(new WithdrawRequest
                {
                    UserName = platform_user.game_user_id,
                    FlowNumber = RecordData.id.ToString(),
                    Money = RecordData.amount,
                });

                if (responseData.Success != true)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("RGRICH Withdraw : {ex}", responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RGRICH TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RGRICH Withdraw : {ex}", ex.Message);
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
            RGRICHsetup.Lang.TryGetValue(request.GameConfig["lang"], out var lang);

            // 沒帶值預設給英文
            lang = lang ?? RGRICHsetup.Lang["en-US"];

            // 沒帶遊戲代碼拋錯
            request.GameConfig.TryGetValue("gameCode", out var gameCode);
            if (string.IsNullOrEmpty(gameCode))
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "RGRICH gameCode not allow empty");
            }

            GameUrlRequest UrlRequest = new GameUrlRequest
            {
                UserName = platformUser.game_user_id,
                PlatId = 2,
                GameCode = gameCode,
            };

            try
            {
                var res = await _RGRICHApiService.GameUrlAsync(UrlRequest, lang);
                if (res.Success != true)
                {
                    throw new Exception(res.Message);
                }
                return res.Data.Url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "RGRICH: " + ex.Message.ToString());
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

            var Reuslt = await _RGRICHApiService.RechargeOrWithdrawRecordAsync(new RechargeOrWithdrawRecordRequest
            {
                FlowNumber = transfer_record.id.ToString()
            });
            if (Reuslt.Success == true)
            {
                if (transfer_record.target == nameof(Platform.RGRICH))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.RGRICH))
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
                if (transfer_record.target == nameof(Platform.RGRICH))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.RGRICH))
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
            var betRecords = new List<BetRecordResponse>();

            GetBetRecord res = new();
            //var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);

            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _RGRICHDBService.GetRGRICHRecordsBytime(createTime, RecordReq.ReportTime, ConvertClubInfoToGamePlatformUser(RecordReq.ClubId));
                results = results.OrderByDescending(e => e.Bet_time).ToList();
                foreach (var result in results)
                {
                    betRecords.Add(result);
                }
            }

            // 統一輸出格式為 RespRecordLevel2_Electronic
            res.Data = betRecords.OrderByDescending(e => e.Bet_time).Select(obj => new RespRecordLevel2_Electronic
            {
                RecordId = obj.Bet_no,
                BetTime = obj.Bet_time,
                GameType = obj.Game_type,
                GameId = obj.Game_code,
                BetAmount = obj.Bet_real,
                NetWin = obj.Payoff,
                Jackpot = obj.Jackpot,
                BetStatus = obj.Status.ToString(),
                SettleTime = obj.Payout_time,
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
            RGRICHsetup.Lang.TryGetValue(RecordDetailReq.lang, out var lang);

            // 沒帶值預設給英文
            lang = lang ?? RGRICHsetup.Lang["en-US"];

            var data = await _RGRICHDBService.GetRGRICHRecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            var Settled = data?.Where(x => x.Status == Status.Sellte).First();
            if (Settled == null)
            {
                throw new Exception("未結算無法轉跳廠商URL");
            }

            BetDetailUrlRequest request = new BetDetailUrlRequest()
            {
                UserName = Settled.Username,
                BetNo = RecordDetailReq.record_id
            };
            var resp = await _RGRICHApiService.BetDetailUrlAsync(request, lang);

            return resp?.Data?.Url ?? "";
        }

        /// <summary>
        /// 補單-會總
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            #region 檢查拉單進度(補單不可先於拉單，否則會有重複單)

            var recordScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(RGRICHRecordSchedule.SYSTEM_PARAMETERS_KEY)).value);
            if (RepairReq.StartTime > recordScheduleTime || RepairReq.EndTime > recordScheduleTime)
                throw new ExceptionMessage(ResponseCode.Fail, "RGRICH補單不可先於拉單!");

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
            var ReportScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(RGRICHReportSchedule.SYSTEM_PARAMETERS_KEY)).value);

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
        public async Task<int> PostRGRICHRecord(List<BetRecordResponse> recordData)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, BetRecordResponse>> linqRes = recordData.GroupBy(x => x.Username);

            var postResult = 0;
            foreach (IGrouping<string, BetRecordResponse> group in linqRes)
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

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.RGRICH);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No RGRICH user");
                            }
                            // 紀錄 reportTime 跟 Bet_time(下注時間) 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            List<BetRecordResponse> betDetailData = new List<BetRecordResponse>();

                            List<BetRecordResponse> betDetailDataRunning = new List<BetRecordResponse>();
                            foreach (BetRecordResponse item in group)
                            {
                                item.Report_time = reportTime;
                                // 把Partition col(bet_time) binding 到 col Partition_time
                                item.Bet_time = item.Bet_time;

                                await Calculate(conn, tran, item);

                                switch (item.Status)
                                {
                                    case Status.UnSellte:
                                        item.Club_id = memberWalletData.Club_id;
                                        item.Franchiser_id = memberWalletData.Franchiser_id;
                                        betDetailDataRunning.Add(item);
                                        break;

                                    case Status.Duplicate:
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

                                dic[summaryTime].Add(item.Bet_time.ToString("yyyy-MM-dd HH:mm"));
                            }

                            if (betDetailDataRunning.Count > 0)
                            {
                                await _RGRICHDBService.PostRGRICHRunningRecord(conn, tran, betDetailDataRunning);
                                postResult += await _RGRICHDBService.PostRGRICHRecord(conn, tran, betDetailDataRunning);
                            }

                            if (betDetailData.Count > 0)
                            {
                                postResult += await _RGRICHDBService.PostRGRICHRecord(conn, tran, betDetailData);
                            }
                            await tran.CommitAsync();

                            // 記錄到 Redis reportTime 跟 Bet_time(下注時間) 的關聯
                            foreach (var item in dic)
                            {
                                var key = $"{RedisCacheKeys.RGRICHBetSummaryTime}:{item.Key}";
                                await _commonService._cacheDataService.SortedSetAddAsync(key,
                                    item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
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

                            _logger.LogError("Run RGRICH record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
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
                _logger.LogDebug("Create RGRICH game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                // 每日統計
                var resp = await _RGRICHApiService.ReportHourAsync(new ReportHourRequest()
                {
                    Hour = reportTime
                });
                var gameEmptyReport = new GameReport();
                if (resp.Data.Bet_total == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.RGRICH);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;
                }
                else
                {
                    gameEmptyReport.platform = nameof(Platform.RGRICH);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = (decimal)resp.Data.Bet_total;
                    gameEmptyReport.total_win = (decimal)resp.Data.Payoff + resp.Data.Bet_real;
                    gameEmptyReport.total_netwin = (decimal)resp.Data.Payoff;
                    gameEmptyReport.total_count = resp.Data.Bet_count;
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
                _logger.LogDebug("Create RGRICH game W1 report time {datetime}", reportTime);
                var (TotalCount, TotalBetValid, TotalNetWin) = await _RGRICHDBService.SumRGRICHBetRecordByPartitionTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.RGRICH);
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
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordResponse r)
        {
            if (r.Status == Status.UnSellte)
            {
                r.Bet_real = r.Bet_total;
                // 一般未結算淨輸贏都是給0，因為計算要先算輸，之後結算在沖銷回來
                r.Payoff -= r.Bet_total;
            }
            r.Pre_Bet_total = r.Bet_total;
            r.Pre_Bet_real = r.Bet_real;
            r.Pre_Payoff = r.Payoff;

            var record = await _RGRICHDBService.GetRGRICHRecords(r.Bet_no, r.Bet_time) ?? new List<BetRecordResponse>();

            // if (record.Any(x => new { x.billNo, x.createtime, x.billstatus }.Equals(new { r.billNo, createtime = DateTimeOffset.FromUnixTimeMilliseconds(r.createtime).ToOffset(TimeSpan.FromHours(8)).DateTime, r.billstatus })))
            if (record.Any(x => x.Bet_no == r.Bet_no && x.Bet_time == r.Bet_time && x.Status == r.Status))
            {
                // 如果相同pkey資料就設定狀態為Duplicate，並return
                r.Status = Status.Duplicate;
                return;
            }
            if (record.Any())
            {
                // 取最後一筆結算單來訂正沖銷(update)
                var lastpp = record.OrderByDescending(x => x.Payout_time).First();

                r.Bet_total = r.Bet_total - lastpp.Pre_Bet_total;
                r.Bet_real = r.Bet_real - lastpp.Pre_Bet_real;
                r.Payoff = r.Payoff - lastpp.Pre_Payoff;
            }
            await _RGRICHDBService.DeleteRGRICHRunningRecord(conn, tran, r);
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
            var w1data = await _RGRICHDBService.GetRGRICHRecordsByPartition(startTime, endTime);

            var existPK = w1data.Select(x => new { x.Bet_no, x.Status, x.Updated_at }).ToHashSet();

            var adds = new List<BetRecordResponse>();
            // 每日統計
            BetRecordRequest req = new BetRecordRequest()
            {
                SearchMode = searchMode,
                StartTime = startTime,
                EndTime = endTime,
                Page = 0,
                PerPage = 5000
            };

            var page = 0;
            while (true)
            {
                req.Page = page;
                var betLogs = await _RGRICHApiService.BetRecordAsync(req);

                if (betLogs.Data.Any() == false)
                {
                    break;
                }
                foreach (var item in betLogs.Data)
                {
                    // 如果不存在於DB資料
                    if (existPK.Add(new { item.Bet_no, item.Status, item.Updated_at }))
                    {
                        adds.Add(item);
                    }
                }
                page++;

                #region 遊戲商如果無注單Data會給空陣列[]，所以可以先註解以下break語法

                //if (page > betLogs.Meta.Total / 1000)
                //    break;

                #endregion 遊戲商如果無注單Data會給空陣列[]，所以可以先註解以下break語法

                await Task.Delay(1000);
            }

            if (adds.Any() == false) return 0;

            var effRecord = await PostRGRICHRecord(adds);

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
            var summaryRecords = await _RGRICHDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.RGRICH);
                summaryData.Game_type = 3;
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
            return PlatformType.Electronic;
        }

        /// <summary>
        /// 健康度
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Task HealthCheck(Platform platform)
        {
            return _RGRICHApiService.HealthCheckAsync(new());
        }

        /// <summary>
        /// 取得位結算
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> results = await _RGRICHDBService.GetRGRICHRunningRecord(RecordReq);
            results = results.OrderByDescending(e => e.createtime);
            res.Data = results.ToList();
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
            return result;
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
            return result;
        }
    }
}