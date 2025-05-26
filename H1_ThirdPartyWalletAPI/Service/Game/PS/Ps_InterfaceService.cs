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
using Pssetup = ThirdPartyWallet.Share.Model.Game.PS.Ps;
using H1_ThirdPartyWalletAPI.Model.Config;
using ThirdPartyWallet.Share.Model.Game.PS.Request;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using ThirdPartyWallet.Share.Model.Game.PS.Response;
using ThirdPartyWallet.GameAPI.Service.Game.PS;
using Microsoft.Extensions.Options;
using ThirdPartyWallet.Share.Model.Game.PS;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography.Xml;
using System.Web;
using static H1_ThirdPartyWalletAPI.Model.Game.TP.Response.BetLogResponse;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using Microsoft.Extensions.Hosting;
using YamlDotNet.Core.Tokens;
using System.Diagnostics.CodeAnalysis;


namespace H1_ThirdPartyWalletAPI.Service.Game.PS
{
    public interface IPsInterfaceService : IGameInterfaceService
    {
        Task<int> PostPsRecord(List<GetorderResponse.BetRecord> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class Ps_InterfaceService : IPsInterfaceService
    {
        private readonly ILogger<Ps_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IPSDBService _PsDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IOptions<PsConfig> _options;
        private readonly IPsApiService _PsApiService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        public Ps_InterfaceService(ILogger<Ps_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IPSDBService PsDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IOptions<PsConfig> options,
            IPsApiService PsApiService,
            ICacheDataService cacheService)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _PsDBService = PsDBService;
            _options = options;
            _PsApiService = PsApiService;
            _cacheService = cacheService;
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
                var responseData = await _PsApiService.GetBalanceAsync(new GetbalanceRequest
                {
                    host_id =  _options.Value.PS_hostid,
                    member_id = platform_user.game_user_id,
                    purpose = 0
                });

                if (responseData.status_code != 0)
                {
                    throw new Exception(Pssetup.ErrorCode[responseData.status_code]);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.balance/100), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Ps餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.PS);
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
                var responseData = await _PsApiService.KickUserAsync(new kickoutRequest
                {
                    host_id = _options.Value.PS_hostid,
                    member_id = platform_user.game_user_id
                });
                if (responseData.status_code == 4  )
                {
                    throw new Exception(Pssetup.ErrorCode[responseData.status_code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("PS踢線失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "Ps " + ex.Message.ToString());
            }
            return true;
        }
        /// <summary>
        /// 全站踢線
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public async Task<bool> KickAllUser(Platform platform)
        {
            try
            {
                var responseData = await _PsApiService.KickallAsync(new kickallRequest
                {
                    host_id = _options.Value.PS_hostid
                });
                if (responseData.status_code != 4)
                {
                    throw new Exception(Pssetup.ErrorCode[responseData.status_code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("PS踢線失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "Ps " + ex.Message.ToString());
            }
            return true;
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
            if (!Pssetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreateuserRequest()
                {
                    host_id = _options.Value.PS_hostid,
                    member_id = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    purpose =  0
                };


                var response = await _PsApiService.CreateplayerAsync(req);
                if (response.status_code == 0 || response.status_code == 12005)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.member_id;
                    gameUser.game_platform = Platform.PS.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(Pssetup.ErrorCode[response.status_code]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ps建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "Ps " + ex.Message.ToString());
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
                var responseData = await _PsApiService.MoneyinAsync(new DepositRequest
                {
                    host_id = _options.Value.PS_hostid,
                    member_id = platform_user.game_user_id,
                    txn_id = CutGuidTo20Characters(RecordData.id),
                    amount =Math.Round(RecordData.amount * 100,2)
                });

                if (responseData.status_code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("Ps Deposit: {Message}", Pssetup.ErrorCode[responseData.status_code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Ps TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Ps Deposit: {Message}", ex.Message);
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
                var responseData = await _PsApiService.MoneyoutAsync(new WithdrawRequest
                {
                    host_id = _options.Value.PS_hostid,
                    member_id = platform_user.game_user_id,
                    txn_id = CutGuidTo20Characters(RecordData.id),
                    amount = Math.Round(RecordData.amount*100,2)
                });

                if (responseData.status_code != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("Ps Withdraw : {ex}", Pssetup.ErrorCode[responseData.status_code]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Ps TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("Ps Withdraw : {ex}", ex.Message);
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
            Pssetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);
            var token = getaccesstoken(platformUser.club_id);
            lang ??= Pssetup.lang["en-US"];
            var hostid = _options.Value.PS_hostid;
            var URL = _options.Value.PS_URL;
            var game = request.GameConfig["gameCode"];
            var return_url = request.GameConfig["lobbyURL"];
            var return_target = "top";
            var tokenData = new LoginRequest 
            {
                Token = token,
                MemberId = platformUser.game_user_id 
            };
            var storeTokenTask = _cacheService.StringSetAsync($"{RedisCacheKeys.LoginToken}:{Platform.PS}:{token}", tokenData, (int)TimeSpan.FromMinutes(15).TotalSeconds);
            await storeTokenTask;
            return $"{URL}/launch/?host_id={hostid}&game_id={game}&lang={lang}&access_token={token}&return_url={return_url}&return_target={return_target}";
        }
        /// <summary>
        /// 確認交易紀錄
        /// </summary>
        /// <param name="transfer_record"></param>
        /// <returns></returns>
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var Reuslt = await _PsApiService.QueryorderAsync(new QueryorderRequest
            {
                host_id = _options.Value.PS_hostid,
                memver_id = transfer_record.Club_id,
                txn_id = CutGuidTo20Characters(transfer_record.id)
            });
            if (Reuslt.Count > 0)
            {
                
                if (transfer_record.target == nameof(Platform.PS))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.PS))
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
                if (transfer_record.target == nameof(Platform.PS))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.PS))
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
            var batRecords = new List<GetorderResponse.BetRecord>();  // 修改类型为 List<BetRecord>
            var res = new GetBetRecord();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _PsDBService.GetPsRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                results = results.OrderByDescending(e => e.s_tm).ToList();
                batRecords.AddRange(results);  // 直接添加 BetRecord 列表
            }
            res.Data = batRecords.OrderByDescending(e => e.s_tm).Select(obj => (dynamic)new RespRecordLevel2_Electronic
            {
                RecordId = obj.sn.ToString(),
                BetTime = obj.s_tm,
                GameType = obj.gt,
                GameId = obj.gid,
                BetAmount = obj.bet,
                NetWin = obj.win - obj.bet,
                Jackpot = obj.jp,
                BetStatus = "Sellte", // 注意：可能是 "Settle"
                SettleTime = obj.s_tm,
            }).ToList();
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
            string URL = _options.Value.PS_BACKURL;
            string sn = RecordDetailReq.record_id;
            string token = _options.Value.PS_token;
            string lang = Pssetup.lang["en-US"];
            return $"{URL}/Resource/game_history?token={token}&lang={lang}&sn={sn}";
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
            DateTime now = DateTime.Now.AddHours(-2);
            if (startTime<=now)
            {
                await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
                await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, 0, 0));
            }
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
        public async Task<int> PostPsRecord(List<GetorderResponse.BetRecord> recordData)
        {
            var betRecords = recordData.Where(x => x.member_id.ToLower().StartsWith(Config.OneWalletAPI.Prefix_Key.ToLower()))
                                                  .OrderBy(x => x.s_tm)
                                                  .ToList();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var postResult = 0; 
            foreach (var group in betRecords.Chunk(20000))
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        sw.Stop();
                        _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                        sw.Restart();
                        // 紀錄 reportTime 跟 playTime 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                        List<GetorderResponse.BetRecord> betDetailData = new List<GetorderResponse.BetRecord>();

                        foreach (GetorderResponse.BetRecord item in group)
                        {
                            item.report_time = reportTime;
                            item.partition_time = item.s_tm;
                            item.win = item.win / 100;
                            item.bet = item.bet / 100;
                            item.winamt = item.winamt / 100;
                            item.betamt = item.betamt / 100;
                            item.jp = item.jp / 100;

                            betDetailData.Add(item);

                            var utcCreatetime = DateTime.SpecifyKind(item.s_tm, DateTimeKind.Utc); // 指定 createtime 是 UTC
                            var createtimeOffset = new DateTimeOffset(utcCreatetime).ToOffset(TimeSpan.FromHours(8));
                            var formattedDateTime = createtimeOffset.DateTime.ToString("yyyy-MM-dd HH:mm");
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }
                            dic[summaryTime].Add(formattedDateTime);
                        }

                        if (betDetailData.Count > 0)
                        {
                            postResult += await _PsDBService.PostPsRecord(conn, tran, betDetailData);
                        }
                        tran.Commit();

                        foreach (var item in dic)
                        {
                            foreach (var subItem in item.Value)
                            {
                                var key = $"{RedisCacheKeys.PSBetSummaryTime}:{item.Key}";
                                await _commonService._cacheDataService.ListPushAsync(key, subItem);
                            }
                        }
                        sw.Stop();
                        _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                        sw.Restart();
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
                _logger.LogDebug("Create Ps game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _PsApiService.gamehistoryAsync(new GetorderRequest()
                {
                    host_id = _options.Value.PS_hostid,
                    start_dtm = startDateTime,
                    end_dtm = startDateTime.AddHours(1).AddMilliseconds(-1),
                    detail_type = 1
                });
                int count = 0;
                var gameEmptyReport = new GameReport();
                if (req.Count == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.PS);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {
                    foreach (var dateEntry in req)
                    {
                        foreach (var memberEntry in dateEntry.Value)
                        {
                            foreach (var record in memberEntry.Value)
                            {
                                gameEmptyReport.platform = nameof(Platform.PS);
                                gameEmptyReport.report_datetime = reportTime;
                                gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                                gameEmptyReport.total_bet += (decimal)record.bet / 100;
                                gameEmptyReport.total_win += (decimal)record.win / 100;
                                gameEmptyReport.total_netwin += ((decimal)record.win - (decimal)record.bet )/ 100;
                                gameEmptyReport.total_count++;
                            }
                        }
                    }
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
                _logger.LogDebug("Create Ps game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin, totalnetwin) = await _PsDBService.SumPsBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.PS);
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
        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> Repair(DateTime startTime, DateTime endTime)
        {
            string[] type_array = new string[] { "SLOT" };
            var w1data = await _PsDBService.GetPsRecordsBycreatetime(startTime, endTime);
            var resPK = w1data.Select(x => new { x.sn, x.s_tm }).ToHashSet();

            var res = new List<GetorderResponse.BetRecord>();
            DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            // 每日統計
            GetorderRequest req = new GetorderRequest()
            {
                host_id = _options.Value.PS_hostid,
                start_dtm = startTime,
                end_dtm = endTime,
                detail_type = 1
            };
            foreach (var game_type in type_array)
            {
                req.game_type = game_type;
                var betLogs = await _PsApiService.gamehistoryAsync(req);
                if (betLogs == null)
                {
                    break;
                }
                string member_id = "";
                foreach (var dateEntry in betLogs)
                {
                    foreach (var memberEntry in dateEntry.Value)
                    {
                        member_id = memberEntry.Key;

                        foreach (var record in memberEntry.Value)
                        {
                            if (resPK.Add(new { record.sn, record.s_tm }))
                            {
                                record.member_id = member_id;
                                res.Add(record);
                            }
                        }
                    }
                }
            }
            if (res.Count == 0)
            {
                return 0;
            }

            return await PostPsRecord(res);
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
            var summaryRecords = await _PsDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.PS);
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
                        partition_time = item.createtime
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
            healthcheckRequest req = new healthcheckRequest()
            {
               host_id = _options.Value.PS_hostid
            };
            return _PsApiService.healthcheckAsync(req);
        }
        /// <summary>
        /// 產生小於20碼整數GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private static string CutGuidTo20Characters(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();

            // 取前 8 个字节并转换为 long（64 位）
            long number = BitConverter.ToInt64(bytes, 0);

            // 确保数字不超过 20 位
            // 取模 10^20 以限制数字大小
            ulong maxValue = 10000000000000000000UL; // 10^20

            // 确保数字不超过 20 位
            ulong limitedNumber = (ulong)Math.Abs(number) % maxValue;

            return limitedNumber.ToString();
        }
        /// <summary>
        /// 產生access_token
        /// </summary>
        /// <param name="access"></param>
        /// <returns></returns>
        private static string getaccesstoken(string access)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                // 產生公鑰跟私鑰
                string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                string dataToEncrypt = $"{access}";

                // 用公鑰加密
                string encryptedData = EncryptData(dataToEncrypt, publicKey);
                return HttpUtility.UrlEncode(encryptedData);
            }
        }
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string EncryptData(string data, string publicKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] encryptedBytes;
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
                // 使用 RSA 加密
                encryptedBytes = rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA512);
            }
            // 加密後轉換成base64
            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
