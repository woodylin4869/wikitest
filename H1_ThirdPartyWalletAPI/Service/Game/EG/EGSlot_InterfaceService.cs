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
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.EGSlot;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Request;
using ThirdPartyWallet.Share.Model.Game.EGSlot.Response;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using EGSlotsetup = ThirdPartyWallet.Share.Model.Game.EGSlot.EGSlot;




namespace H1_ThirdPartyWalletAPI.Service.Game.EGSlot
{
    public interface IEGSlotInterfaceService : IGameInterfaceService
    {
        Task<int> PostEGSlotRecord(List<Datum> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }

    public class EGSlot_InterfaceService : IEGSlotInterfaceService
    {
        private readonly ILogger<EGSlot_InterfaceService> _logger;
        private readonly IOptions<EGSlotConfig> _options;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IEGSlotDBService _EGSlotDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IEGSlotApiService _EGSlotApiService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        private readonly TimeSpan BATCH_OFFSET = TimeSpan.FromHours(3);

        public EGSlot_InterfaceService(ILogger<EGSlot_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IEGSlotDBService EGSlotDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IOptions<EGSlotConfig> options,
            IEGSlotApiService EGSlotApiService)
        {
            _logger = logger;
            _options = options;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _EGSlotDBService = EGSlotDBService;
            _EGSlotApiService = EGSlotApiService;
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
                var responseData = await _EGSlotApiService.StatusAsync(new StatusRequest
                {
                    Username = platform_user.game_user_id,
                    AgentName = _options.Value.EGSlot_MerchantCode
                });

                if (responseData.ErrorCode != 0)
                {
                    throw new Exception(EGSlotsetup.ErrorCode[responseData.ErrorCode]);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.Balance), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("EGSlot餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.EGSLOT);
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
                var responseData = await _EGSlotApiService.LogoutAsync(new LogoutRequest
                {
                    Username = platform_user.game_user_id,
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出EGSlot使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }
        /// <summary>
        /// 踢線
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platform_user"></param>
        /// <returns></returns>
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
            if (!EGSlotsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new PlayersRequest()
                {
                    Username = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    Currency = userData.Currency,
                    AgentName = _options.Value.EGSlot_MerchantCode
                };


                var response = await _EGSlotApiService.PlayersAsync(req);

                if (response.ErrorCode == 0 || response.ErrorCode == 8)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.Username;
                    gameUser.game_platform = Platform.EGSLOT.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(EGSlotsetup.ErrorCode[response.ErrorCode]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("EGSlot建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "EGSlot " + ex.Message.ToString());
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
                var responseData = await _EGSlotApiService.TransferinAsync(new TransferinRequest
                {
                    Username = platform_user.game_user_id,
                    ReferenceCode = RecordData.id.ToString(),
                    Amount = RecordData.amount,
                    AgentName = _options.Value.EGSlot_MerchantCode,
                });

                if (responseData.ErrorCode != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("EGSlot Deposit: {Message}", EGSlotsetup.ErrorCode[responseData.ErrorCode]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("EGSlot TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("EGSlot Deposit: {Message}", ex.Message);
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
                var responseData = await _EGSlotApiService.TransferoutAsync(new TransferoutRequest
                {
                    Username = platform_user.game_user_id,
                    ReferenceCode = RecordData.id.ToString(),
                    Amount = (RecordData.amount * -1),
                    AgentName = _options.Value.EGSlot_MerchantCode
                });

                if (responseData.ErrorCode != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("EGSlot Withdraw : {ex}", EGSlotsetup.ErrorCode[responseData.ErrorCode]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("EGSlot TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("EGSlot Withdraw : {ex}", ex.Message);
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
            EGSlotsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            LoginRequest UrlRequest = new LoginRequest
            {
                Username = platformUser.game_user_id,
                //遊戲商名稱
                GameID = request.GameConfig["gameCode"],
                //沒帶值預設為英文
                Lang = lang ?? EGSlotsetup.lang["en-US"],
                //營運商固定
                AgentName = _options.Value.EGSlot_MerchantCode,
                HomeURL = request.GameConfig["lobbyURL"]
            };
            try
            {
                var res = await _EGSlotApiService.LoginAsync(UrlRequest);
                if (res.ErrorCode != 0)
                {
                    throw new Exception(EGSlotsetup.ErrorCode[res.ErrorCode]);
                }
                string GameUrl = res.URL;
                if (Config.OneWalletAPI.Prefix_Key == "prd")
                {
                    string newDomain = "https://game.eazygaming.net";

                    GameUrl = ThirdPartyWallet.GameAPI.Service.Game.EGSlot.Helper.ReplaceDomain(GameUrl, newDomain);
                }


                return GameUrl;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "EGSlot: " + ex.Message.ToString());
            }
        }
        /// <summary>
        /// 廠商統計
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
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create EGSlot game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _EGSlotApiService.GethourdataAsync(new GethourdataRequest()
                {

                    StartTime = (long)(startDateTime - unixEpoch).TotalMilliseconds,
                    EndTime = (long)(startDateTime.AddHours(1).AddMilliseconds(-1) - unixEpoch).TotalMilliseconds,

                });
                var gameEmptyReport = new GameReport();
                if (req.Data.Count == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.EGSLOT);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {

                    gameEmptyReport.platform = nameof(Platform.EGSLOT);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = req.Data.Select(x => decimal.Parse(x.TotalBet)).Sum();
                    gameEmptyReport.total_win = req.Data.Select(x => decimal.Parse(x.TotalWin)).Sum();
                    gameEmptyReport.total_netwin = gameEmptyReport.total_win - gameEmptyReport.total_bet;
                    gameEmptyReport.total_count = req.Data.Sum(x => x.MainTxCount);
                }


                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
                startDateTime = startDateTime.AddHours(1);

                await Task.Delay(3000);
            }
        }
        /// <summary>
        /// W1統計
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
                _logger.LogDebug("Create EGSlot game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin, totalnetwin) = await _EGSlotDBService.SumEGSlotBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.EGSLOT);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin;
                reportData.total_netwin = totalnetwin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }

        public Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            throw new NotImplementedException();
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);



            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var Reuslt = await _EGSlotApiService.TransferHistoryAsync(new TransferHistoryRequest
            {
                StartTime = (long)(transfer_record.create_datetime.AddMinutes(-5) - unixEpoch).TotalMilliseconds,
                EndTime = (long)(transfer_record.create_datetime.AddMinutes(30) - unixEpoch).TotalMilliseconds,
                Username = Config.OneWalletAPI.Prefix_Key + transfer_record.Club_id,
                ReferenceCode = transfer_record.id.ToString(),
                AgentName = _options.Value.EGSlot_MerchantCode
            });
            //Status 狀態 1: 處理中、2: 成功、3: 失敗
            if (Reuslt.Data.Length != 0 && Reuslt.Data[0].Status == 2 && Reuslt.ErrorCode == 0)
            {
                if (transfer_record.target == nameof(Platform.EGSLOT))//轉入PP直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.EGSLOT))
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
            else if (Reuslt.Data.Length == 0 || Reuslt.Data[0].Status == 3)
            {
                if (transfer_record.target == nameof(Platform.EGSLOT))//轉入PP直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.EGSLOT))
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
        /// 取得第二層明細
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var batRecords = new List<dynamic>();
            GetBetRecord res = new();
            //var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _EGSlotDBService.GetEGSlotRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                results = results.OrderByDescending(e => e.BetTime).ToList();
                foreach (var result in results)
                {
                    batRecords.Add(result);

                }
            }

            res.Data = batRecords.OrderByDescending(e => e.BetTime).Select(obj => new RespRecordLevel2_Electronic
            {
                RecordId = obj.MainTxID,
                BetTime = obj.BetTime,
                GameId = obj.GameID,
                BetAmount = obj.Bet,
                NetWin = obj.NetWin,
                Jackpot = 0,
                BetStatus = obj.Status.ToString(),
                SettleTime = obj.WinTime,
            }).Cast<dynamic>().ToList();
            return res;
        }
        /// <summary>
        /// 明細轉跳
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {

            EGSlotsetup.lang.TryGetValue(RecordDetailReq.lang, out var lang);
            var requstdata = new GetdetailurlRequest()
            {
                MainTxID = RecordDetailReq.record_id,
                Lang = lang
            };

            var data = await _EGSlotApiService.GetdetailurlAsync(requstdata);

            return data.url;
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
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
            await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> Repair(DateTime startTime, DateTime endTime)
        {

            var res = new List<Datum>();
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            // 每日統計
            var req = new TransactionRequest
            {
                Status = 1,
                Page = 1,
                PageSize = 2000,
                StartTime = (long)(startTime - unixEpoch).TotalMilliseconds,
                EndTime = (long)(endTime - unixEpoch).TotalMilliseconds,
                // StartTime = parameter.value
            };

            var Page = 1;

            while (true)
            {
                req.Page = Page;
                var betRecord = await _EGSlotApiService.TransactionAsync(req);

                // 有錯誤就拋
                if (string.IsNullOrEmpty(betRecord.Message) == false && betRecord.ErrorCode != 0)
                {
                    throw new Exception(betRecord.Message);
                }

                foreach (var itme in betRecord.Data)
                {
                    res.Add(itme);
                }

                if (!betRecord.Next)
                    break;

                Page++;

            }
            var postResult = 0;


            if (res.Count != 0)
            {
                foreach (var group in res.GroupBy(r => unixEpoch.AddMilliseconds(r.BetTime).DateTime.AddHours(8).Ticks / BATCH_OFFSET.Ticks * BATCH_OFFSET.Ticks))
                {
                    _logger.LogInformation("PostEGSLOTRecord Group Key:{key} Count:{count}", new DateTime(group.Key), group.Count());
                    try
                    {
                        postResult += await PostEGSlotRecord(group.ToList());
                    }
                    catch (Exception ex)
                    {
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError(ex, "Run EGSLOT record schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine} StartDate:{StartDate}", ex.GetType().FullName, ex.Message, errorFile, errorLine, new DateTime(group.Key));
                    }
                }
            }

            return postResult;



        }


        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostEGSlotRecord(List<Datum> recordData)
        {
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var oldLogs = (await _EGSlotDBService.GetEGSlotRecordsBycreatetime(recordData.Min(l => unixEpoch.AddMilliseconds(l.BetTime).DateTime.AddHours(8)), recordData.Max(l => (unixEpoch.AddMilliseconds(l.BetTime).DateTime.AddHours(8)))))
                .Select(l => new { l.MainTxID, l.Status }).ToHashSet();



            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, Datum>> linqRes = recordData.GroupBy(x => x.Username);

            var postResult = 0;
            foreach (IGrouping<string, Datum> group in linqRes)
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

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.EGSLOT);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No EGSLOT user");
                            }
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            List<Datum> betDetailData = new List<Datum>();

                            List<Datum> betDetailDataRunning = new List<Datum>();
                            foreach (Datum item in group)
                            {
                                if (!oldLogs.Add(new { item.MainTxID, item.Status }))
                                    continue;


                                item.report_time = reportTime;

                                await Calculate(conn, tran, item);


                                switch (item.Status)
                                {
                                    case 0:
                                        item.Club_id = memberWalletData.Club_id;
                                        item.Franchiser_id = memberWalletData.Franchiser_id;
                                        betDetailDataRunning.Add(item);
                                        break;
                                    case 2:
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

                                dic[summaryTime].Add(DateTimeOffset.FromUnixTimeMilliseconds(item.WinTime).ToOffset(TimeSpan.FromHours(8)).DateTime.ToString("yyyy-MM-dd HH:mm"));
                            }


                            if (betDetailDataRunning.Count > 0)
                            {
                                await _EGSlotDBService.PostEGSlotRunningRecord(conn, tran, betDetailDataRunning);
                                postResult += await _EGSlotDBService.PostEGSlotRecord(conn, tran, betDetailDataRunning);
                            }

                            if (betDetailData.Count > 0)
                            {
                                postResult += await _EGSlotDBService.PostEGSlotRecord(conn, tran, betDetailData);
                            }
                            tran.Commit();

                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = $"{RedisCacheKeys.EGSlotBetSummaryTime}:{item.Key}";
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

                            _logger.LogError("Run EGSlot record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

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
        /// 計算未結與結算
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, Datum r)
        {
            if (r.Status == 0)
            {
                r.Pre_Bet = r.Bet;
                r.Pre_Win = r.Win;
                r.Pre_NetWin = r.NetWin;
            }
            else
            {
                r.Pre_Bet = r.Bet;
                r.Pre_Win = r.Win;
                r.Pre_NetWin = r.NetWin;
            }



            var Record = await _EGSlotDBService.GetEGSlotRecords(r.MainTxID, DateTimeOffset.FromUnixTimeMilliseconds(r.BetTime).ToOffset(TimeSpan.FromHours(8)).DateTime);

            Record ??= new();
            if (Record.Any(x => new { x.MainTxID, x.BetTime, x.Status }.Equals(new { r.MainTxID, createtime = DateTimeOffset.FromUnixTimeMilliseconds(r.BetTime).ToOffset(TimeSpan.FromHours(8)).DateTime, r.Status })))
            {
                r.Status = 2;
                return;
            }
            if (Record.Any())
            {
                var lastpp = Record.OrderByDescending(x => x.WinTime).First();

                r.Bet = r.Bet - lastpp.Pre_Bet;
                r.Win = r.Win - lastpp.Pre_Win;
                r.NetWin = r.NetWin - lastpp.Pre_NetWin;
            }
            await _EGSlotDBService.DeleteEGSlotRunningRecord(conn, tran, r);
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
            var summaryRecords = await _EGSlotDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid.Substring(3)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();

            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key.Substring(3), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.EGSLOT);
                summaryData.Game_type = 3;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.win) - summaryRecord.Sum(x => x.bet);
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
        public Task HealthCheck(Platform platform)
        {
            return _EGSlotApiService.StatusAsync(new StatusRequest
            {
                Username = "HealthCheck",
                AgentName = _options.Value.EGSlot_MerchantCode
            });
        }
        #endregion
    }
}
