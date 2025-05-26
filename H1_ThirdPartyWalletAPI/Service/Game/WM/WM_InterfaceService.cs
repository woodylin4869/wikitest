using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WM.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static H1_ThirdPartyWalletAPI.Model.Game.WM.WM_Mapping;
using WMsetup = H1_ThirdPartyWalletAPI.Model.Game.WM.WM;

namespace H1_ThirdPartyWalletAPI.Service.Game.WM
{
    public interface IWMInterfaceService : IGameInterfaceService
    {
        Task<List<WMDataReportResponse.Result>> GetWMBetRecords(DateTime start, DateTime end, bool bySettleTime);
        Task<int> PostWMRecord(List<WMDataReportResponse.Result> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    }

    public class WM_RecordService : IWMInterfaceService
    {
        private readonly ILogger<WM_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IWMDBService _wMDBService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public WM_RecordService(ILogger<WM_RecordService> logger,
            ICommonService commonService,
            IWebHostEnvironment env,
            IGameApiService gameaApiService,
            IDBService dbService,
            ISummaryDBService summaryDBService,
            IWMDBService wMDBService,
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _wMDBService = wMDBService;
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
                var wallet = await GetWalletCache(platform_user.club_id);
                var responseData = await _gameApiService._WMAPI.GetBalanceAsync(new GetBalanceRequest()
                {
                    cmd = "GetBalance",
                    vendorId = WMVendorId(wallet),
                    signature = WMSignature(wallet),
                    user = platform_user.game_user_id,
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                });
                if (responseData.errorCode == 10501)
                {
                    var req = new MemberRegisterRequest()
                    {
                        cmd = "MemberRegister",
                        vendorId = WMVendorId(wallet),
                        signature = WMSignature(wallet),
                        user = Config.OneWalletAPI.Prefix_Key + wallet.Club_id,
                        password = "!8@8%8",
                        username = wallet.Club_Ename,
                        limitType = "124,125,9,126,127,129,149,131,584",
                        timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };
                    var response = await _gameApiService._WMAPI.MemberRegisterAsync(req);

                    if (response.errorCode == 0 || response.errorCode == 104)
                    {
                        responseData = await _gameApiService._WMAPI.GetBalanceAsync(new GetBalanceRequest()
                        {
                            cmd = "GetBalance",
                            vendorId = WMVendorId(wallet),
                            signature = WMSignature(wallet),
                            user = platform_user.game_user_id,
                            timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                        });
                    }
                    else
                    {
                        throw new Exception(response.errorMessage);
                    }
                }
                if (responseData.errorCode != 0)
                {
                    throw new Exception(responseData.errorMessage);
                }
                Balance.Amount = Convert.ToDecimal(responseData.result);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("WM餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.WM);
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
                var wallet = await GetWalletCache(platform_user.club_id);
                var responseData = await _gameApiService._WMAPI.LogoutGameAsync(new LogoutGameRequest()
                {
                    cmd = "LogoutGame",
                    vendorId = WMVendorId(wallet),
                    signature = WMSignature(wallet),
                    user = platform_user.game_user_id,
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                });

                if (responseData.errorCode != 0)
                {
                    throw new Exception(responseData.errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出WM使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
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
            if (!WMsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new MemberRegisterRequest()
                {
                    cmd = "MemberRegister",
                    vendorId = WMVendorId(userData),
                    signature = WMSignature(userData),
                    user = Config.OneWalletAPI.Prefix_Key + request.Club_id,
                    password = "!8@8%8",
                    username = userData.Club_Ename,
                    limitType = "124,125,9,126,127,129,149,131,584",
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                };
                var response = await _gameApiService._WMAPI.MemberRegisterAsync(req);
                if (response.errorCode == 0 || response.errorCode == 104)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.user;
                    gameUser.game_platform = Platform.WM.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("WM建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "WM " + ex.Message.ToString());
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
            if (!WMsetup.Currency.ContainsKey(walletData.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            try
            {
                var responseData = await _gameApiService._WMAPI.ChangeBalanceAsync(new ChangeBalanceRequest
                {
                    cmd = "ChangeBalance",
                    vendorId = WMVendorId(walletData),
                    signature = WMSignature(walletData),
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    order = RecordData.id.ToString().Replace("-", ""),
                    user = platform_user.game_user_id,
                    money = RecordData.amount,
                });

                if (responseData.errorCode == 10501)
                {
                    var req = new MemberRegisterRequest()
                    {
                        cmd = "MemberRegister",
                        vendorId = WMVendorId(walletData),
                        signature = WMSignature(walletData),
                        user = Config.OneWalletAPI.Prefix_Key + walletData.Club_id,
                        password = "!8@8%8",
                        username = walletData.Club_Ename,
                        limitType = "124,125,9,126,127,129,149,131,584",
                        timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };
                    var response = await _gameApiService._WMAPI.MemberRegisterAsync(req);

                    if (response.errorCode != 0)
                    {
                        RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                        throw new Exception(response.errorMessage);
                    }

                    responseData = await _gameApiService._WMAPI.ChangeBalanceAsync(new ChangeBalanceRequest
                    {
                        cmd = "ChangeBalance",
                        vendorId = WMVendorId(walletData),
                        signature = WMSignature(walletData),
                        timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        order = RecordData.id.ToString().Replace("-", ""),
                        user = platform_user.game_user_id,
                        money = RecordData.amount,
                    });

                }

                if (responseData.errorCode != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("WM Deposit: {Message}", responseData.errorMessage);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WM TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WM Deposit: {Message}", ex.Message);
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
            if (!WMsetup.Currency.ContainsKey(walletData.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            try
            {
                var responseData = await _gameApiService._WMAPI.ChangeBalanceAsync(new ChangeBalanceRequest
                {
                    cmd = "ChangeBalance",
                    vendorId = WMVendorId(walletData),
                    signature = WMSignature(walletData),
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    order = RecordData.id.ToString().Replace("-", ""),
                    user = platform_user.game_user_id,
                    money = -1 * (RecordData.amount),
                });

                if (responseData.errorCode != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("WM Withdraw : {ex}", responseData.errorMessage);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WM TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WM Withdraw : {ex}", ex.Message);
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
            WMsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            EditLimitRequest WNEditLimit = new EditLimitRequest
            {
                cmd = "EditLimit",
                vendorId = WMVendorId(userData),
                signature = WMSignature(userData),
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                user = platformUser.game_user_id,
                limitType = request.GameConfig["betLimitGroup"],
            };
            var responseData = await _gameApiService._WMAPI.EditLimitAsync(WNEditLimit);
            if (responseData.errorCode == 10501)
            {
                var req = new MemberRegisterRequest()
                {
                    cmd = "MemberRegister",
                    vendorId = WMVendorId(userData),
                    signature = WMSignature(userData),
                    user = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    password = "!8@8%8",
                    username = userData.Club_Ename,
                    limitType = "124,125,9,126,127,129,149,131,584",
                    timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                };
                var response = await _gameApiService._WMAPI.MemberRegisterAsync(req);
                if (response.errorCode == 0)
                {
                    await _gameApiService._WMAPI.EditLimitAsync(WNEditLimit);
                }
                else
                    throw new Exception(response.errorMessage);
            }

            //Step 3 Get Game URL
            SigninGameRequest UrlRequest = new SigninGameRequest();
            UrlRequest.cmd = "SigninGame";
            UrlRequest.user = platformUser.game_user_id;
            UrlRequest.vendorId = WMVendorId(userData);
            UrlRequest.signature = WMSignature(userData);
            UrlRequest.timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            UrlRequest.password = "!8@8%8";
            UrlRequest.lang = lang;

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.returnurl = request.GameConfig["lobbyURL"];
            }

            try
            {
                var token_res = await _gameApiService._WMAPI.SigninGameAsync(UrlRequest);
                if (responseData.errorCode == 10501)
                {
                    await Task.Delay(1000);
                    token_res = await _gameApiService._WMAPI.SigninGameAsync(UrlRequest);
                }
                return token_res.result.ToString();
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
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

            var wallet = await GetWalletCache(transfer_record.Club_id);
            var WMReuslt = await _gameApiService._WMAPI.GetMemberTradeReportAsync(new GetMemberTradeReportRequest
            {
                cmd = "GetMemberTradeReport",
                vendorId = WMVendorId(wallet),
                signature = WMSignature(wallet),
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                order = transfer_record.id.ToString().Replace("-", ""),
                user = Config.OneWalletAPI.Prefix_Key + transfer_record.Club_id,
            });
            if (WMReuslt.errorCode == 0)
            {
                if (transfer_record.target == nameof(Platform.WM))//轉入WM直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.WM))
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
                if (transfer_record.target == nameof(Platform.WM))//轉入WM直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.WM))
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
            List<dynamic> wm_results = new List<dynamic>();
            GetBetRecord res = new GetBetRecord();

            var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
            var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();

            foreach (var partition in partitions)
            {
                wm_results.AddRange(await _wMDBService.GetRecordByReportTime(summary, partition, partition.AddDays(1).AddMilliseconds(-1), Config.OneWalletAPI.Prefix_Key+ summary.Club_id));
            }
            wm_results.AddRange(await _wMDBService.GetWMRecordsBySummary(RecordReq));


            res.Data = wm_results.OrderByDescending(e => e.settime).ToList();
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
        /// 取得遊戲住單明細-RCG格式
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RCGRowData> GameRowData(GetRowDataReq RecordDetailReq)
        {
            RCGRowData rCGRowData = new RCGRowData();
            var partitions = await _summaryDBService.GetPartitionTime(RecordDetailReq.summary_id, RecordDetailReq.ReportTime) ?? Array.Empty<DateTime>();
            List<WMDataReportResponse.Result> wmRecord = new List<WMDataReportResponse.Result>();
            foreach (var betTime in partitions)
            {
                wmRecord.AddRange(await _wMDBService.GetWMRecordsV2(RecordDetailReq.record_id, betTime));
            }
            if (!wmRecord.Any())
                wmRecord = await _wMDBService.GetWMRecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);

            if (wmRecord == null || wmRecord.Count == 0)
            {
                throw new Exception("no data");
            }
            WMDataReportResponse.Result record = wmRecord.FirstOrDefault();

            OpenListModelBase modelBase = new OpenListModelBase();
            modelBase.NoRun = record.Event;
            modelBase.NoActive = record.eventChild;
            modelBase.ServerId = record.betId;
            modelBase.DateTime = record.betTime;

            List<object> MappingResult = RCG_MappingFunc(record.gid, record.gameResult, modelBase);

            rCGRowData.dataList = MappingResult;
            return rCGRowData;
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
                RepairCount += await RepairWM(startTime, endTime, RepairReq.SearchType == 2);
                startTime = endTime;
            }
            RepairCount += await RepairWM(startTime, RepairReq.EndTime, RepairReq.SearchType == 2);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
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

            EditLimitRequest WNEditLimit = new EditLimitRequest
            {
                cmd = "EditLimit",
                vendorId = WM_RecordService.WMVendorId(memberWalletData),
                signature = WM_RecordService.WMSignature(memberWalletData),
                timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                user = gameUser.game_user_id,
                limitType = request.bet_setting.ToString(),
            };
            var res=new ResCodeBase();
            var wmreq = await _gameApiService._WMAPI.EditLimitAsync(WNEditLimit);
            if (wmreq.errorCode != 0)
            {
                res.code = wmreq.errorCode;
                res.Message = wmreq.errorMessage;
                return res;
            }
            res.code = (int)ResponseCode.Success;
            res.Message = MessageCode.Message[(int)ResponseCode.Success];
            return res;
        }
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Live;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._WMAPI.HelloAsync(new()
            {
                cmd = "Hello",
                vendorId = WMVendorId("THB"),
                signature = WMSignature("THB"),
            });
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostWMRecord(List<WMDataReportResponse.Result> recordData)
        {
            var postresult = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var linqRes = recordData
                .Where(x => x.user.StartsWith(Config.OneWalletAPI.Prefix_Key))
                .GroupBy(x => x.user);
            foreach (var group in linqRes)
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
                            club_id = group.Key[Config.OneWalletAPI.Prefix_Key.Length..];
                            Wallet memberWalletData = await GetWalletCache(club_id);

                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                            //已結算注單
                            List<WMDataReportResponse.Result> betDetailData = new List<WMDataReportResponse.Result>();
                            foreach (WMDataReportResponse.Result item in group)
                            {
                                item.pre_bet = item.bet;
                                item.pre_validbet = item.validbet;
                                item.pre_winLoss = item.winLoss;
                                item.report_time = reportTime;
                                item.partition_time = item.betTime;

                                await Calculate(tran, item);
                                  
                                if (item.reset != "D")
                                {
                                    betDetailData.Add(item);
                                }
                                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }
                                dic[summaryTime].Add(item.betTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            }

                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = nameof(Platform.WM) +$"{RedisCacheKeys.BetSummaryTime}:{item.Key}";
                                    await _commonService._cacheDataService.ListPushAsync(key, subItem);
                                }
                            }
                            if (betDetailData.Count > 0)
                            {
                                postresult += await _wMDBService.PostWMRecord(conn, tran, betDetailData);
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

                            _logger.LogError("Run WM record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return postresult;
        }

        //public async Task<int> PostWMRecordback(List<WMDataReportResponse.Result> recordData)
        //{
        //    var postresult = 0;
        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    var linqRes = recordData
        //        .Where(x => x.user.StartsWith(Config.OneWalletAPI.Prefix_Key))
        //        .GroupBy(x => x.user);
        //    foreach (var group in linqRes)
        //    {
        //        using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
        //        {
        //            await conn.OpenAsync();
        //            using (var tran = conn.BeginTransaction())
        //            {
        //                try
        //                {
        //                    sw.Stop();
        //                    _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                    sw.Restart();
        //                    string club_id;
        //                    club_id = group.Key[Config.OneWalletAPI.Prefix_Key.Length..];
        //                    Wallet memberWalletData = await GetWalletCache(club_id);
        //                    if (memberWalletData == null || memberWalletData.Club_id == null)
        //                    {
        //                        throw new Exception("沒有會員id");
        //                    }

        //                    var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.WM);
        //                    if (gameUser == null || gameUser.game_user_id != group.Key)
        //                    {
        //                        throw new Exception("No WM user");
        //                    }

        //                    //彙總注單
        //                    Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
        //                    //已結算注單
        //                    List<WMDataReportResponse.Result> betDetailData = new List<WMDataReportResponse.Result>();
        //                    foreach (WMDataReportResponse.Result item in group)
        //                    {

        //                        DateTime drawtime = DateTime.Now;

        //                        BetRecordSummary sumData = new BetRecordSummary();
        //                        sumData.Club_id = memberWalletData.Club_id;
        //                        sumData.Game_id = nameof(Platform.WM);
        //                        sumData.Game_type = item.gid;
        //                        sumData.ReportDatetime = new DateTime(drawtime.Year, drawtime.Month, drawtime.Day, drawtime.Hour, (drawtime.Minute / 5) * 5, 0);

        //                        //先確認有沒有符合的匯總單
        //                        if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
        //                        {
        //                            sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
        //                            //合併處理
        //                            sumData = await Calculate(tran, sumData, item);
        //                            summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
        //                        }
        //                        else
        //                        {
        //                            //用Club_id與ReportDatetime DB取得彙總注單
        //                            IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
        //                            sw.Stop();
        //                            _logger.LogDebug("get summary record ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                            sw.Restart();
        //                            if (!results.Any()) //沒資料就建立新的
        //                            {
        //                                //建立新的Summary
        //                                sumData.Currency = memberWalletData.Currency;
        //                                sumData.Franchiser_id = memberWalletData.Franchiser_id;

        //                                //合併處理
        //                                sumData = await Calculate(tran, sumData, item);
        //                            }
        //                            else //有資料就更新
        //                            {
        //                                sumData = results.SingleOrDefault();
        //                                //合併處理
        //                                sumData = await Calculate(tran, sumData, item);
        //                            }
        //                            summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
        //                        }
        //                        item.summary_id = sumData.id;
        //                        if (item.reset != "D")
        //                        {
        //                            betDetailData.Add(item);
        //                        }
        //                    }
        //                    List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
        //                    foreach (var s in summaryData)
        //                    {
        //                        if (s.Value.RecordCount > 0)
        //                        {
        //                            summaryList.Add(s.Value);
        //                        }
        //                    }
        //                    int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
        //                    if (betDetailData.Count > 0)
        //                    {
        //                        postresult += await _wMDBService.PostWMRecord(conn, tran, betDetailData);
        //                    }
        //                    tran.Commit();
        //                    sw.Stop();
        //                    _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                    sw.Restart();
        //                }
        //                catch (Exception ex)
        //                {
        //                    tran.Rollback();
        //                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

        //                    _logger.LogError("Run WM record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

        //                }
        //            }
        //            await conn.CloseAsync();
        //        }
        //    }
        //    sw.Stop();
        //    _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //    return postresult;
        //}

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
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create WM game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計

                // 遊戲商(轉帳中心的欄位格式)
                var gameEmptyReport = new GameReport
                {
                    platform = nameof(Platform.WM),
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

                await Task.Delay(65000);
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
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create ＷＭ game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _wMDBService.SumWMBetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.WM);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalBetValid + totalWin;
                reportData.total_netwin = totalWin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }

        /// <summary>
        /// 拉單
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<WMDataReportResponse.Result>> GetWMBetRecords(DateTime start, DateTime end, bool bySettleTime)
        {
            var result = new List<WMDataReportResponse.Result>();

            var req = new GetDateTimeReportRequest
            {
                cmd = "GetDateTimeReport",
                //vendorId = Config.CompanyToken.WM_vendorId,
                //signature = Config.CompanyToken.WM_signature,
                startTime = start.ToString("yyyy-MM-dd HH:mm:ss"),
                endTime = end.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                //timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                timetype = bySettleTime ? 1 : 0,
                datatype = 2,
                syslang = 1
            };

            foreach (var wmCurrency in WMsetup.Currency.Values)
            {
                req.vendorId = WMVendorId(wmCurrency);
                req.signature = WMSignature(wmCurrency);

                var response = await _gameApiService._WMAPI.GetDateTimeReportAsync(req);

                if (response.result == null || !response.result.Any()) continue;

                result.AddRange(response.result);
            }

            return result;
        }

        /// <summary>
        /// 統計5分鐘
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="SummaryData"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task Calculate(IDbTransaction tran, WMDataReportResponse.Result r)
        {
            #region back
            //r.pre_bet = r.bet;
            //r.pre_validbet = r.validbet;
            //r.pre_winLoss = r.winLoss;


            //var oldRecords = await _wMDBService.GetWMRecords(tran, r.betId, r.betTime);
            //oldRecords ??= new();
            //if (oldRecords.Any(oldr => new { oldr.betId, oldr.settime, oldr.betTime }.Equals(new { r.betId, r.settime, r.betTime })))
            //{
            //    r.reset = "D";
            //    return SummaryData;
            //}

            //if (oldRecords.Any())
            //{
            //    var lastRecord = oldRecords.OrderByDescending(r => r.settime).First(); //僅需沖銷最後一筆即可
            //    r.bet = r.bet - lastRecord.pre_bet;
            //    r.validbet = r.validbet - lastRecord.pre_validbet;
            //    r.winLoss = r.winLoss - lastRecord.pre_winLoss;
            //}

            //SummaryData.RecordCount++;
            //SummaryData.Bet_amount += r.bet;
            //SummaryData.Turnover += r.validbet;
            //SummaryData.Netwin += r.winLoss;
            //SummaryData.Win += r.winLoss + r.bet;
            //SummaryData.updatedatetime = DateTime.Now;

            //SummaryData.JackpotWin = 0;
            //return SummaryData;
            #endregion back

            var oldRecords = await _wMDBService.GetWMRecordsV2(tran, r.betId, r.betTime);
            if(!oldRecords.Any())
                oldRecords = await _wMDBService.GetWMRecords(tran, r.betId, r.betTime);
            
            if (oldRecords.Any(oldr => new { oldr.betId, oldr.settime, oldr.betTime }.Equals(new { r.betId, r.settime, r.betTime })))
            {
                r.reset = "D";
            }
            if (oldRecords.Any())
            {
                var lastRecord = oldRecords.OrderByDescending(r => r.settime).First(); //僅需沖銷最後一筆即可
                r.bet = r.bet - lastRecord.pre_bet;
                r.validbet = r.validbet - lastRecord.pre_validbet;
                r.winLoss = r.winLoss - lastRecord.pre_winLoss;
            }
        }

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="bySettleTime"></param>
        /// <returns></returns>
        private async Task<int> RepairWM(DateTime startTime, DateTime endTime, bool bySettleTime)
        {
            var betRecords = await GetWMBetRecords(startTime, endTime, bySettleTime);
            if (!betRecords.Any())
                return 0;

            var w1CenterList = new List<WMDataReportResponse.Result>();
            foreach (var group in betRecords.GroupBy(r => r.betTime.Ticks - (r.betTime.Ticks % TimeSpan.FromHours(3).Ticks)))
            {
                w1CenterList.AddRange(await _wMDBService.GetWMRecordsBytime(group.Min(r => r.betTime), group.Max(r => r.betTime).AddSeconds(1)));
            }

            var w1RecordPK = w1CenterList.Select(r => new { r.betId, r.betTime, r.settime }).ToHashSet();

            var repairList = betRecords.Where(item => !w1RecordPK.Contains(new { item.betId, item.betTime, item.settime })).ToList();

            if (repairList.Any())
                return await PostWMRecord(repairList);

            return 0;
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
            var summaryRecords = await _wMDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => new { x.userid, x.game_type });
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => ConvertGamePlatformUserToClubInfo(x.userid)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();


            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key.userid), out var userWallet)) continue;
                var gameType = summaryRecord.Key.game_type;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.WM);
                summaryData.Game_type = gameType;
                summaryData.JackpotWin = 0;
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.netwin) + summaryRecord.Sum(x => x.bet);
                summaryData.Netwin = summaryRecord.Sum(x => x.netwin);
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
        /// <summary>
        /// 使用情境：後彙總排程從遊戲明細查詢使用者遊戲帳號 轉換 為H1的Club_Id 提供 wallet 查詢使用到
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private string ConvertGamePlatformUserToClubInfo(string propertyValue)
        {
            return propertyValue[Config.OneWalletAPI.Prefix_Key.Length..].ToUpper();
        }
        private async Task<string> GetWalletCurrencyCache(string Club_id)
        {
            var wallet = await GetWalletCache(Club_id);
            return wallet.Currency;
        }

        public static string WMVendorId(Wallet wallet)
        {
            if (!WMsetup.Currency.ContainsKey(wallet.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            var wmCurrency = WMsetup.Currency[wallet.Currency];
            return WMVendorId(wmCurrency);
        }

        private static string WMVendorId(string wmCurrency) => wmCurrency switch
        {
            "USD" => Config.CompanyToken.WM_USD_vendorId, //美元代理
            "THB" => Config.CompanyToken.WM_THB_vendorId, //泰銖代理
            _ => throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency])
        };

        public static string WMSignature(Wallet wallet)
        {
            if (!WMsetup.Currency.ContainsKey(wallet.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            var wmCurrency = WMsetup.Currency[wallet.Currency];
            return WMSignature(wmCurrency);
        }

        private static string WMSignature(string wmCurrency) => wmCurrency switch
        {
            "USD" => Config.CompanyToken.WM_USD_signature, //美元代理
            "THB" => Config.CompanyToken.WM_THB_signature, //泰銖代理
            _ => throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency])
        };
    }
}
