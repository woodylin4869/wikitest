using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Worker;
using H1_ThirdPartyWalletAPI.Worker.Game.RSG;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using H1_ThirdPartyWalletAPI.Extensions;
using System.Threading;
using System.Transactions;
using System.Data;
using Microsoft.IdentityModel.Tokens;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IRsgH1InterfaceService : IGameInterfaceService
    {
        Task<ResCodeBase> PostRsgRecord(List<SessionDetail> rsgBetRecord);

        Task<int> PostRsgRecordDetail(List<GameDetail> rsgBetRecord);

        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);

        Task<int> PostRsgFishRecord(GetGameMinReportResponse.DataInfo rsgBetRecords, string webid);

    }

    public class RSG_H1RecordService : IRsgH1InterfaceService
    {
        private readonly ILogger<RSG_H1RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameApiService _gameApiService;
        private readonly ISystemParameterDbService _systemParameterDbService;
        private readonly IRsgDBService _rsgDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;
        private const int pageLimit = 20000;

        private readonly IMemoryCache _memoryCache;
        private const int memory_cache_min = 30; //分鐘
        private const string memory_cache_key = "RSG_System_Web_Code";
        private readonly static SemaphoreSlim postRsgRecordLock = new(5, 5);

        public RSG_H1RecordService(ILogger<RSG_H1RecordService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            ISummaryDBService summaryDBService,
            IMemoryCache memoryCache,
            ISystemParameterDbService systemParameterDbService,
            IRsgDBService rsgDBService,
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameaApiService;
            _summaryDBService = summaryDBService;
            _memoryCache = memoryCache;
            _systemParameterDbService = systemParameterDbService;
            _rsgDBService = rsgDBService;
            _gameReportDBService = gameReportDBService;
        }

        #region GameInterfaceService

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                RcgToken rsgData = await _commonService._serviceDB.GetRsgToken(platform_user.club_id);
                var walletCache = await GetWalletCache(platform_user.club_id);
                var responseData = await _gameApiService._RsgAPI.GetBalanceAsync(new GetBalanceRequest()
                {
                    SystemCode = rsgData.system_code,
                    WebId = rsgData.web_id,
                    UserId = platform_user.game_user_id,
                    Currency = Model.Game.RSG.RSG.Currency[walletCache.Currency]
                });

                if (responseData.ErrorCode != (int)ErrorCodeEnum.OK)
                {
                    throw new Exception(responseData.ErrorMessage);
                }
                Balance.Amount = responseData.Data.CurrentPlayerBalance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Rsg餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.RSG);
            return Balance;
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            var currency = walletData.Currency;
            try
            {
                var rsgData = await _commonService._serviceDB.GetRsgToken(platform_user.club_id);

                var responseData = await _gameApiService._RsgAPI.DepositAsync(new DepositRequest
                {
                    SystemCode = rsgData.system_code,
                    WebId = rsgData.web_id,
                    UserId = platform_user.game_user_id,
                    TransactionID = RecordData.id.ToString().Replace("-", "").Substring(0, 20),
                    Currency = Model.Game.RSG.RSG.Currency[currency],
                    Balance = Math.Round(transfer_amount, 2)
                });

                if (responseData.ErrorCode != (int)ErrorCodeEnum.OK)
                {
                    throw new ExceptionMessage(responseData.ErrorCode, responseData.ErrorMessage);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RSG TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("RSG TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInRsgFail Msg: {Message}", ex.Message);
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
                var rsgData = await _commonService._serviceDB.GetRsgToken(platform_user.club_id);
                var responseData = await _gameApiService._RsgAPI.WithdrawAsync(new WithdrawRequest()
                {
                    SystemCode = Config.CompanyToken.RSG_SystemCode,
                    WebId = rsgData.web_id,
                    UserId = platform_user.game_user_id,
                    TransactionID = RecordData.id.ToString().Replace("-", "").Substring(0, 20),
                    Currency = Model.Game.RSG.RSG.Currency[currency],
                    Balance = Math.Round(game_balance, 2)
                });

                if (responseData.ErrorCode != (int)ErrorCodeEnum.OK)
                {
                    throw new ExceptionMessage(responseData.ErrorCode, responseData.ErrorMessage);
                }

                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RSG TransferOut Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("RSG TransferOut Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RSG TransferOut Fail ex : {ex}", ex.Message);
            }
            return RecordData.status;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var result = await _commonService._serviceDB.GetRsgToken(platform_user.club_id);
                if (result == null)
                {
                    throw new Exception("沒有使用者RSG token");
                }
                var responseData = await _gameApiService._RsgAPI.KickoutAsync(new KickoutRequest()
                {
                    KickType = 4,
                    SystemCode = result.system_code,
                    WebId = result.web_id,
                    UserId = platform_user.game_user_id,
                    GameId = 0,
                });
                if (responseData.ErrorCode != (int)ErrorCodeEnum.OK)
                {
                    _logger.LogInformation("踢出RSG使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, responseData.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("KickRsgUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            var kickRsgRequest = new KickoutRequest();
            kickRsgRequest.KickType = 1; // kick all system
            kickRsgRequest.SystemCode = Config.CompanyToken.RSG_SystemCode;
            kickRsgRequest.GameId = 0;
            kickRsgRequest.WebId = "";
            kickRsgRequest.UserId = "";
            await _gameApiService._RsgAPI.KickoutAsync(kickRsgRequest);
            return true;
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.RSG.RSG.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            string webid = request.GameConfig["webid"];
            //Step 1 Create Member
            CreatePlayerRequest requestData = new CreatePlayerRequest();
            requestData.SystemCode = Config.CompanyToken.RSG_SystemCode;
            requestData.WebId = webid;
            requestData.UserId = userData.Club_id;
            requestData.Currency = Model.Game.RSG.RSG.Currency[userData.Currency];
            try
            {
                var result = await _gameApiService._RsgAPI.CreatePlayerAsync(requestData);
                if (result.ErrorCode != (int)ErrorCodeEnum.OK && result.ErrorCode != (int)ErrorCodeEnum.ThePlayerIsCurrencyAlreadyExists)
                {
                    throw new Exception(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.CreateRsgUserFail, MessageCode.Message[(int)ResponseCode.CreateRsgUserFail] + "|" + ex.Message.ToString());
            }

            //Step 2 add web id
            var results = await _commonService._serviceDB.GetRsgToken(userData.Club_id);
            if (results == null)
            {
                RsgToken rcgdata = new RsgToken();
                rcgdata.club_id = userData.Club_id;
                rcgdata.system_code = Config.CompanyToken.RSG_SystemCode;
                rcgdata.web_id = webid;
                if (await _commonService._serviceDB.PostRsgToken(rcgdata) != 1)
                {
                    throw new ExceptionMessage((int)ResponseCode.CreateRcgUserTokenFail, MessageCode.Message[(int)ResponseCode.CreateRcgUserTokenFail]);
                }
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = requestData.UserId;
            gameUser.game_platform = request.Platform;
            gameUser.agent_id = webid;
            return gameUser;
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            string webid = request.GameConfig["webid"];
            //Step 4 Get Game URL
            GetURLTokenRequest UrlRequest = new GetURLTokenRequest();
            UrlRequest.SystemCode = Config.CompanyToken.RSG_SystemCode;
            UrlRequest.WebId = webid;
            UrlRequest.UserId = platformUser.game_user_id;
            UrlRequest.UserName = userData.Club_Ename;
            UrlRequest.Currency = Model.Game.RSG.RSG.Currency[userData.Currency];

            if (!request.GameConfig.ContainsKey("gameCode"))
            {
                throw new Exception("game code not found");
            }

            UrlRequest.GameId = Convert.ToInt32(request.GameConfig["gameCode"]);

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.ExitAction = request.GameConfig["lobbyURL"];
            }
            if (request.GameConfig.ContainsKey("lang") && Model.Game.RSG.RSG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.Language = Model.Game.RSG.RSG.lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.Language = Model.Game.RSG.RSG.lang["en-US"];
            }

            try
            {
                var token_res = await _gameApiService._RsgAPI.GetURLTokenAsync(UrlRequest);
                return token_res.Data.URL;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            GetTransactionResultRequest RsgReqData = new GetTransactionResultRequest();
            RsgReqData.SystemCode = Config.CompanyToken.RSG_SystemCode;
            RsgReqData.TransactionID = transfer_record.id.ToString().Replace("-", "").Substring(0, 20);
            var RsgReuslt = await _gameApiService._RsgAPI.GetTransactionResultAsync(RsgReqData);
            if (RsgReuslt.ErrorCode == (int)ErrorCodeEnum.OK)
            {
                if (transfer_record.target == nameof(Platform.RSG))//轉入RSG直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.RSG))
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
            else if (RsgReuslt.ErrorCode == (int)ErrorCodeEnum.TransactionIsNotFound)
            {
                if (transfer_record.target == nameof(Platform.RSG))//轉入RSG直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.RSG))
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

        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var cacheSeconds = 300;
            var key = $"{RedisCacheKeys.RsgGetBetRecords}:{RecordReq.summary_id}:{RecordReq.Platform}:{RecordReq.ReportTime.ToString("yyyy-MM-dd HH:mm:ss")}";

            GetBetRecord res = new GetBetRecord();

            var data = await _commonService._cacheDataService.GetOrSetValueAsync(key, async () =>
            {
                var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
                if (summary != null)
                {
                    var rsg_results = await _rsgDBService.GetRsgRecordByReportTime(summary);
                    rsg_results = rsg_results.OrderByDescending(e => e.playtime).ThenByDescending(x => x.sequennumber);
                    return rsg_results.Select(x => new RSGBetRecord()
                    {
                        sequenNumber = x.sequennumber,
                        playtime = x.playtime,
                        betamt = x.betamt,
                        winamt = x.winamt,
                        jackpotwin = x.jackpotwin,
                        GameId = summary.Game_type.ToString()
                    }).ToList();
                }

                return new List<RSGBetRecord>();
            }, cacheSeconds);

            res.Data = data?.Select(x => (dynamic)x).ToList();
            return res;
        }

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            // 語系
            var language = Model.Game.RSG.RSG.lang.ContainsKey(RecordDetailReq.lang)
                ? Model.Game.RSG.RSG.lang[RecordDetailReq.lang]
                : Model.Game.RSG.RSG.lang["en-US"];

            var GetBetRecordReq = new GetBetRecordReq
            {
                summary_id = RecordDetailReq.summary_id.ToString(),
                ReportTime = RecordDetailReq.ReportTime
            };

            var cacheSeconds = 300;
            var key = $"{RedisCacheKeys.RsgGameDetailURL}:{RecordDetailReq.summary_id}:{RecordDetailReq.ReportTime.ToString("yyyy-MM-dd HH:mm:ss")}";

            // 第一層明細
            var summary = await _commonService._cacheDataService.GetOrSetValueAsync(key, async () => await _summaryDBService.GetRecordSummaryById(new GetBetRecordReq()
            {
                summary_id = RecordDetailReq.summary_id.ToString(),
                ReportTime = RecordDetailReq.ReportTime
            }), cacheSeconds);

            if (summary == null) return string.Empty;

            var rsgToken = await _commonService._serviceDB.GetRsgToken(summary.Club_id);
            if (rsgToken == null ||
                string.IsNullOrEmpty(rsgToken?.system_code) ||
                string.IsNullOrEmpty(rsgToken?.web_id))
                return string.Empty;

            if (summary.Game_type / 1000 == 3)
            {
                var RsgresponseData = await _gameApiService._RsgAPI.GetGameMinDetailURLTokenAsync(new GetGameMinDetailURLTokenRequest()
                {
                    SystemCode = rsgToken?.system_code,
                    WebId = rsgToken?.web_id,
                    UserId = summary.Club_id,
                    Currency = Model.Game.RSG.RSG.Currency[summary.Currency],
                    GameType = 2,
                    GameId = summary.Game_type,
                    Time = RecordDetailReq.ReportTime.ToString("yyyy-MM-dd HH:mm"),
                    Language = language
                });

                return RsgresponseData.Data.URL;
            }
            else
            {
                long.TryParse(RecordDetailReq.record_id, out var seq);

                var RsgresponseData = await _gameApiService._RsgAPI.GetSlotGameRecordURLTokenAsync(new GetSlotGameRecordURLTokenRequest()
                {
                    SystemCode = rsgToken?.system_code,
                    WebId = rsgToken?.web_id,
                    UserId = summary.Club_id,
                    Currency = Model.Game.RSG.RSG.Currency[summary.Currency],
                    GameId = summary.Game_type,
                    SequenNumber = seq,
                    Language = language
                });

                return RsgresponseData.Data.URL;
            }
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            #region 檢查拉單進度(RSG補單不可先於拉單，否則會有重複單)

            var slotRecordTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(RsgSlotRecordSchedule.PARAMETER_KEY)).value);
            var fishRecordTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(RsgFishRecordSchedule.PARAMETER_KEY)).value);
            var minRecordTime = slotRecordTime <= fishRecordTime ? slotRecordTime : fishRecordTime;

            if (RepairReq.StartTime > minRecordTime || RepairReq.EndTime > minRecordTime)
                throw new ExceptionMessage(ResponseCode.Fail, "RSG補單不可先於拉單!");

            #endregion 檢查拉單進度(RSG補單不可先於拉單，否則會有重複單)

            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddSeconds(-RepairReq.EndTime.Second).AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;

            // 檢查補帳
            RepairCount += await RepairRsg(startTime, endTime);
            RepairCount += await RepairRsgFishGame(startTime, endTime);

            // 重新統計彙總帳
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            return string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
        }

        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _rsgDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid).Distinct().ToList();
            // 批次處理，每次1000筆
            var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
            {
                return (await _commonService._serviceDB.GetWallet(betch));
            }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);

            var summaryRecordList = new List<BetRecordSummary>();
            foreach (var summaryRecord in summaryRecords)
            {
                if (!userWalletList.TryGetValue(summaryRecord.userid, out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.bet;
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.count;
                summaryData.Game_id = nameof(Platform.RSG);
                summaryData.Game_type = summaryRecord.gameid;
                summaryData.JackpotWin = summaryRecord.jackpot;
                summaryData.Bet_amount = summaryRecord.bet;
                summaryData.Win = summaryRecord.win;
                summaryData.Netwin = summaryRecord.win - summaryRecord.bet;
                summaryRecordList.Add(summaryData);
            }

            var Chucklist = summaryRecordList.Chunk(10000);
            foreach (IEnumerable<BetRecordSummary> group in Chucklist)
            {
                await using NpgsqlConnection conn = new(Config.OneWalletAPI.DBConnection.BetLog.Master);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await conn.OpenAsync();
                await using (var tran = await conn.BeginTransactionAsync())
                {
                    await _summaryDBService.BatchInsertRecordSummaryAsync(conn, group.ToList());
                    await tran.CommitAsync();
                }
                await conn.CloseAsync();
                sw.Stop();
                _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", group.Count(), sw.ElapsedMilliseconds);
            }
            return true;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._RsgAPI.HealthCheck();
        }

        #endregion GameInterfaceService

        #region GameRecordService

        public async Task<ResCodeBase> PostRsgRecord(List<SessionDetail> rsgBetRecord)
        {
            ResCodeBase res = new ResCodeBase();
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                IEnumerable<IGrouping<string, SessionDetail>> linqRes = rsgBetRecord.GroupBy(x => x.userid);
                foreach (IGrouping<string, SessionDetail> group in linqRes)
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
                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.RSG);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No rsg user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<SessionDetail> betDetailData = new List<SessionDetail>();
                            foreach (SessionDetail r in group)//loop club id bet detail
                            {
                                if (Config.OneWalletAPI.Prefix_Key == "dev")
                                {
                                    if (r.webid != "RoyalDev")
                                    {
                                        continue;
                                    }
                                }
                                else if (Config.OneWalletAPI.Prefix_Key == "uat")
                                {
                                    if (r.webid != "RoyalUat")
                                    {
                                        continue;
                                    }
                                }

                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.RSG);
                                sumData.Game_type = r.gameid; //RSG遊戲id
                                DateTime tempDateTime = r.playendtime;
                                sumData.ReportDatetime = tempDateTime;
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    //if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    //{
                                    //    DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                    //    if (sumData.ReportDatetime < ReportDateTime)
                                    //    {
                                    //        sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                    //        _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                    //    }
                                    //}
                                    //else
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 0, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                }

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime?.ToString("yyyy-MM-dd HH:mm:ss.ffffff")))
                                {
                                    var sumDataNew = new BetRecordSummary()
                                    {
                                        RecordCount = sumData.RecordCount,
                                        Club_id = sumData.Club_id,
                                        Currency = memberWalletData.Currency,
                                        Franchiser_id = memberWalletData.Franchiser_id,
                                        Game_id = sumData.Game_id,
                                        Game_type = sumData.Game_type,
                                        Bet_type = sumData.Bet_type,
                                        Bet_amount = sumData.Bet_amount,
                                        Turnover = sumData.Turnover,
                                        Win = sumData.Win,
                                        Netwin = sumData.Netwin,
                                        ReportDatetime = sumData.ReportDatetime,
                                        updatedatetime = sumData.updatedatetime
                                    };
                                    //合併處理
                                    sumDataNew = Calculate(sumDataNew, r);
                                    sumDataNew.ReportDatetime = DateTime.Now;
                                    summaryData.Add(sumDataNew.ReportDatetime?.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), sumDataNew);
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
                                        summaryData.Add(sumData.ReportDatetime?.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), sumData);
                                    }
                                    else //有資料就更換key
                                    {
                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                        //更新報表索引到現在時間
                                        sumData.ReportDatetime = DateTime.Now;
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;
                                        summaryData.Add(sumData.ReportDatetime?.ToString("yyyy-MM-dd HH:mm:ss.ffffff"), sumData);
                                    }
                                }
                                r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }
                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                if (s.Value.RecordCount > 0)
                                {
                                    summaryList.Add(s.Value);
                                }
                            }
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            int PostRecordResult = await _commonService._serviceDB.PostH1RsgRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run rsg record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine}", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
                            await tran.RollbackAsync();
                        }
                    }
                }
                await conn.CloseAsync();
            }

            return res;
        }

        public async Task<int> PostRsgRecordDetail(List<GameDetail> rsgBetRecord)
        {

            if (rsgBetRecord is null)
                throw new ArgumentNullException(nameof(rsgBetRecord));

            if (!rsgBetRecord.Any())
                return 0;

            var postResult = 0;
            await using NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
            await conn.OpenAsync();
            foreach (IEnumerable<GameDetail> group in rsgBetRecord.Chunk(20000))
            {
                await using var tran = await conn.BeginTransactionAsync();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var betDetailData = new List<GameDetail>();
                var dt = DateTime.Now;
                var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                // 紀錄 reportTime 跟 playTime 的關聯
                var dic = new Dictionary<string, HashSet<string>>();

                foreach (var r in group)//loop club id bet detail
                {
                    r.report_time = reportTime;
                    //todo 彩金要測試
                    if (r.subgametype == 3) //彩金
                    {
                        r.jackpotwin = r.winamt;
                        r.winamt = 0;
                    }
                    betDetailData.Add(r);

                    // 紀錄 reportTime 跟 playTime 的關聯
                    var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                    if (!dic.ContainsKey(summaryTime))
                    {
                        dic.Add(summaryTime, new HashSet<string>());
                    }

                    dic[summaryTime].Add(r.playtime.ToString("yyyy-MM-dd HH:mm"));
                }

                await postRsgRecordLock.WaitAsync();
                try
                {
                    postResult += await _rsgDBService.PostRsgRecord(conn, tran, betDetailData);
                }
                finally
                {
                    postRsgRecordLock.Release();
                }
                await tran.CommitAsync();

                // 記錄到 Redis reportTime 跟 playTime 的關聯
                foreach (var item in dic)
                {
                    var key = $"{RedisCacheKeys.RsgBetSummaryTime}:{item.Key}";
                    await _commonService._cacheDataService.SortedSetAddAsync(key,
                        item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                }

                dic.Clear();

                sw.Stop();
                _logger.LogDebug("RsgSlotRecordSchedule 寫入{count}筆資料時間 : {time} MS", postResult, sw.ElapsedMilliseconds);
            }
            await conn.CloseAsync();
            return postResult;
        }

        public async Task<int> PostRsgFishRecord(GetGameMinReportResponse.DataInfo rsgBetRecords, string webid)
        {
            if (rsgBetRecords is null)
                throw new ArgumentNullException(nameof(rsgBetRecords));

            if (rsgBetRecords.GameReport is null)
                throw new ArgumentNullException(nameof(rsgBetRecords.GameReport));

            if (!rsgBetRecords.GameReport.Any())
                return 0;

            // 紀錄 reportTime 跟 playTime 的關聯
            var dic = new Dictionary<string, HashSet<string>>();
            var postResult = 0;
            await using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                await using (var tran = await conn.BeginTransactionAsync())
                {
                    var betDetailData = new List<GameDetail>();
                    foreach (MinGameReport r in rsgBetRecords.GameReport)//loop club id bet detail
                    {
                        var gamrecord = RsgFishGamePaser(r);
                        betDetailData.Add(gamrecord);

                        // 紀錄 reportTime 跟 playTime 的關聯
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                        var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                        if (!dic.ContainsKey(summaryTime))
                        {
                            dic.Add(summaryTime, new HashSet<string>());
                        }

                        dic[summaryTime].Add(gamrecord.playtime.ToString("yyyy-MM-dd HH:mm"));
                    }

                    await postRsgRecordLock.WaitAsync();
                    try
                    {
                        postResult += await _rsgDBService.PostRsgRecord(conn, tran, betDetailData);
                    }
                    finally
                    {
                        postRsgRecordLock.Release();
                    }
                    await tran.CommitAsync();
                }
                await conn.CloseAsync();
            }

            // 記錄到 Redis reportTime 跟 playTime 的關聯
            foreach (var item in dic)
            {
                var key = $"{RedisCacheKeys.RsgBetSummaryTime}:{item.Key}";
                await _commonService._cacheDataService.SortedSetAddAsync(key,
                    item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
            }

            return postResult;
        }

        private BetRecordSummary Calculate(BetRecordSummary SummaryData, SessionDetail r)
        {
            SummaryData.RecordCount = r.recordcount;
            SummaryData.Bet_amount += r.betsum;
            SummaryData.Turnover += r.betsum;
            SummaryData.Netwin += (r.netwinsum - r.jackpotwinsum);
            SummaryData.Win += (r.netwinsum + r.betsum - r.jackpotwinsum);
            SummaryData.JackpotWin = SummaryData.JackpotWin.GetValueOrDefault() + r.jackpotwinsum;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }

        private BetRecordSummary CalculateDetail(BetRecordSummary SummaryData, GameDetail r)
        {
            //0 一般(Spin)
            //1 免費
            //2 比倍
            //3 Jackpot
            //4 重轉
            //5 選擇
            //6 連鎖
            //7 消除
            //99 特色
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.betamt;
            SummaryData.Turnover += r.betamt;
            decimal jackpotwin = 0;
            if (r.subgametype == 3)
            {
                jackpotwin = r.winamt;
            }
            SummaryData.Netwin += (r.winamt - r.betamt - jackpotwin);
            SummaryData.Win += r.winamt - jackpotwin;
            SummaryData.JackpotWin = SummaryData.JackpotWin.GetValueOrDefault() + jackpotwin;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }

        private BetRecordSummary CalculateFish(BetRecordSummary SummaryData, MinGameReport r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.BetSum;
            SummaryData.Turnover += r.BetSum;
            decimal jackpotwin = r.JackpotWinSum;
            SummaryData.Netwin += (r.NetWinSum - jackpotwin);
            SummaryData.Win += r.WinSum - jackpotwin;
            SummaryData.JackpotWin = SummaryData.JackpotWin.GetValueOrDefault() + jackpotwin;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }

        private GameDetail RsgFishGamePaser(MinGameReport gameDetailFish)
        {
            var returnData = new GameDetail();
            returnData.betamt = gameDetailFish.BetSum;
            returnData.userid = gameDetailFish.UserId;
            //todo 彩金需要測試
            returnData.winamt = gameDetailFish.WinSum;
            returnData.playtime = gameDetailFish.TimeMinute;
            returnData.sequennumber = gameDetailFish.SequenNumber;
            returnData.currency = gameDetailFish.Currency;
            returnData.gameid = gameDetailFish.GameId;
            returnData.webid = gameDetailFish.WebId;
            var dt = DateTime.Now;
            returnData.report_time = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
            returnData.jackpotcontribution = gameDetailFish.JackpotContributionSum;
            returnData.jackpotwin = gameDetailFish.JackpotWinSum;
            return returnData;
        }

        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            // 匯總帳單位5分鐘
            var start = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, (startDateTime.Minute / 5) * 5, 0);
            var end = endDateTime.Minute % 5 == 0
                ? endDateTime
                : new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, (endDateTime.Minute / 5) * 5, 0).AddMinutes(5);

            while (start <= end)
            {
                var reportList = new List<MinGameReport>();

                // 取得 SystemCode、WebId
                IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetRSgSystemWebCode();

                foreach (var code in systemWebCode)
                {
                    // 1: 老虎機, 2: 魚機
                    for (var i = 1; i <= 2; i++)
                    {
                        var result = await _gameApiService._RsgAPI.GetGameMinReportAsync(new GetGameMinReportRequest()
                        {
                            SystemCode = code.system_code,
                            WebId = code.web_id,
                            GameType = i,
                            TimeStart = start.ToString("yyyy-MM-dd HH:mm"),
                            TimeEnd = start.AddMinutes(5).AddSeconds(-1).ToString("yyyy-MM-dd HH:mm")
                        });

                        if (result.Data == null) continue;

                        if (result.Data.GameReport.Count != 0)
                        {
                            reportList.AddRange(result.Data.GameReport);
                        }
                    }
                }

                var slotCount = reportList.Where(x => x.GameId / 1000 != 3).Sum(x => x.RecordCount);
                var fishCount = reportList.Count(x => x.GameId / 1000 == 3);

                // 遊戲商(轉帳中心的欄位格式)
                var rsgSummaryReport = new GameReport
                {
                    platform = nameof(Platform.RSG),
                    report_datetime = start,
                    report_type = (int)GameReport.e_report_type.FinancalReport,
                    total_bet = reportList.Sum(x => x.BetSum),
                    total_win = reportList.Sum(x => x.WinSum),
                    total_netwin = reportList.Sum(x => x.NetWinSum),
                    total_count = slotCount + fishCount,
                };

                await _gameReportDBService.DeleteGameReport(rsgSummaryReport);
                await _gameReportDBService.PostGameReport(rsgSummaryReport);
                start = start.AddMinutes(5);

                _logger.LogDebug("Create RSG game provider report time {datetime}", start);
                await Task.Delay(3000);
            }
        }

        public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
        {
            // 匯總帳單位5分鐘
            var start = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, (startDateTime.Minute / 5) * 5, 0);
            var end = endDateTime.Minute % 5 == 0
                ? endDateTime
                : new DateTime(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, (endDateTime.Minute / 5) * 5, 0).AddMinutes(5);

            while (start <= end)
            {
                IEnumerable<dynamic> dailyReport = await _rsgDBService.SumRsgBetRecordMinutely(start);
                var HourlylyReportData = dailyReport.SingleOrDefault();

                dynamic total_jackpot = HourlylyReportData.total_jackpot == null ? 0 : HourlylyReportData.total_jackpot;

                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.RSG);
                reportData.report_datetime = start;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = HourlylyReportData.total_bet == null ? 0 : Math.Abs(HourlylyReportData.total_bet);
                reportData.total_win = HourlylyReportData.total_win == null ? 0 : HourlylyReportData.total_win;
                reportData.total_netwin = (reportData.total_win - reportData.total_bet) + total_jackpot;
                reportData.total_count = HourlylyReportData.total_cont == null ? 0 : Convert.ToInt64(HourlylyReportData.total_cont);

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                start = start.AddMinutes(5);

                _logger.LogDebug("Create RSG game W1 report time {datetime}", start);
                await Task.Delay(3000);
            }
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

        /// <summary>
        /// RSG 帳務比對
        /// 1. 比對轉帳中心與遊戲商的匯總帳是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairRsg(DateTime startTime, DateTime endTime)
        {
            var count = 0;

            // 準備 RSG 注單資料
            IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetRSgSystemWebCode();

            foreach (var agent in systemWebCode)
            {
                var requestTime = startTime;
                while (requestTime <= endTime)
                {
                    // 遊戲商的歷史下注紀錄
                    var rsgHistoryList = new List<GameDetail>();
                    var getRrcordList = new List<Task<GetPagedGameDetailResponse>>();

                    // 先取得第一頁
                    var responseData = await _gameApiService._RsgAPI.GetPagedGameDetailAsync(new GetPagedGameDetailRequest()
                    {
                        SystemCode = Config.CompanyToken.RSG_SystemCode,
                        WebId = agent.web_id,
                        GameType = 1,
                        TimeStart = requestTime.ToString("yyyy-MM-dd HH:mm"),
                        TimeEnd = requestTime.ToString("yyyy-MM-dd HH:mm"),
                        Page = 1,
                        Rows = pageLimit
                    });

                    // 請求失敗
                    if (responseData.ErrorCode != (int)ErrorCodeEnum.OK)
                    {
                        throw new Exception(responseData.ErrorMessage);
                    }

                    // 計算總比數
                    var pageCount = responseData.Data.PageCount;
                    //var pageCount = (responseData.Data.DataCount / pageLimit) + 1; //原方法整除後會有多出空白頁

                    // 第一頁的資料存進 List
                    rsgHistoryList.AddRange(responseData.Data.GameDetail);

                    // 跑完第二頁到最後
                    for (var i = 2; i <= pageCount; i++)
                    {
                        getRrcordList.Add(_gameApiService._RsgAPI.GetPagedGameDetailAsync(new GetPagedGameDetailRequest()
                        {
                            SystemCode = Config.CompanyToken.RSG_SystemCode,
                            WebId = agent.web_id,
                            GameType = 1,
                            TimeStart = requestTime.ToString("yyyy-MM-dd HH:mm"),
                            TimeEnd = requestTime.ToString("yyyy-MM-dd HH:mm"),
                            Page = i,
                            Rows = pageLimit
                        }));
                    }

                    // 接回第二頁到最後的注單資料
                    var allRecord = await Task.WhenAll(getRrcordList);

                    foreach (var item in allRecord)
                    {
                        if (item.ErrorCode != (int)ErrorCodeEnum.OK)
                        {
                            throw new Exception(responseData.ErrorMessage);
                        }

                        rsgHistoryList.AddRange(item.Data.GameDetail);
                    }

                    if (rsgHistoryList.Any())
                        count += await PostRsgRecordDetail(rsgHistoryList);

                    requestTime = requestTime.AddMinutes(1);
                }
            }

            return count;
        }

        private async Task<int> RepairRsgFishGame(DateTime startTime, DateTime endTime)
        {
            var count = 0;

            // 準備 RSG 注單資料
            IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetRSgSystemWebCode();

            foreach (var agent in systemWebCode)
            {
                var requestTime = startTime;
                while (requestTime <= endTime)
                {
                    // 遊戲商的歷史下注紀錄
                    var rsgHistoryList = new List<GameDetail>();

                    var responseData = await _gameApiService._RsgAPI.GetGameMinReportAsync(new GetGameMinReportRequest()
                    {
                        SystemCode = Config.CompanyToken.RSG_SystemCode,
                        WebId = agent.web_id,
                        GameType = 2, //只拉捕魚機
                        TimeStart = requestTime.ToString("yyyy-MM-dd HH:mm"),
                        TimeEnd = requestTime.ToString("yyyy-MM-dd HH:mm")
                    });

                    // 請求失敗
                    if (responseData.ErrorCode != (int)ErrorCodeEnum.OK)
                    {
                        throw new Exception(responseData.ErrorMessage);
                    }

                    foreach (MinGameReport r in responseData.Data.GameReport)//loop club id bet detail
                    {
                        rsgHistoryList.Add(RsgFishGamePaser(r));
                    }

                    if (rsgHistoryList.Any())
                        count += await PostRsgRecordDetail(rsgHistoryList);

                    requestTime = requestTime.AddMinutes(1);
                }
            }

            return count;
        }

        private async Task<IEnumerable<GetPlayerGameHistoryResponse.GameDetail>> GetRsgPlayerGameHistory(SessionDetail sessionDetail)
        {
            var H1GetPlayerGameHistoryRequest = new GetPlayerGameHistoryRequest();
            H1GetPlayerGameHistoryRequest.SystemCode = Config.CompanyToken.RSG_SystemCode;
            H1GetPlayerGameHistoryRequest.WebId = sessionDetail.webid;
            H1GetPlayerGameHistoryRequest.UserId = sessionDetail.userid;
            H1GetPlayerGameHistoryRequest.SessionId = sessionDetail.sessionid;

            var betRecords = new List<GetPlayerGameHistoryResponse.GameDetail>();

            H1GetPlayerGameHistoryRequest.Rows = 2000;
            H1GetPlayerGameHistoryRequest.Page = 1;
            var isEnable = true;
            while (isEnable)
            {
                var gameHistory = await _gameApiService._RsgAPI.H1GetPlayerGameHistory(H1GetPlayerGameHistoryRequest);
                return gameHistory.Data.GameDetail;
                if (gameHistory.Data.GameDetail.Count < H1GetPlayerGameHistoryRequest.Rows)
                {
                    isEnable = false;
                }
                else
                {
                    H1GetPlayerGameHistoryRequest.Page++;
                    await Task.Delay(100);
                }
                betRecords.AddRange(gameHistory.Data.GameDetail);
            }
            return betRecords;
        }

        private async Task<IEnumerable<GetPlayerFishGameMinuteSummaryResponse.GameDetail>> GetRsgPlayerFishGameHistory(SessionDetail sessionDetail)
        {
            var H1GetPlayerFishGameMinuteSummaryRequest = new GetPlayerFishGameMinuteSummaryRequest();
            H1GetPlayerFishGameMinuteSummaryRequest.SystemCode = Config.CompanyToken.RSG_SystemCode;
            H1GetPlayerFishGameMinuteSummaryRequest.WebId = sessionDetail.webid;
            H1GetPlayerFishGameMinuteSummaryRequest.UserId = sessionDetail.userid;
            H1GetPlayerFishGameMinuteSummaryRequest.SessionId = sessionDetail.sessionid;
            H1GetPlayerFishGameMinuteSummaryRequest.Rows = 2000;
            H1GetPlayerFishGameMinuteSummaryRequest.Page = 1;
            var gameHistory = await _gameApiService._RsgAPI.H1GetPlayerFishGameMinuteSummary(H1GetPlayerFishGameMinuteSummaryRequest);
            return gameHistory.Data.GameDetail;
        }

        private IEnumerable<IEnumerable<T>> ChunkBy<T>(IEnumerable<T> source, int chunkSize)
        {
            return source.Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / chunkSize).Select(x => x.Select(v => v.Value));
        }
    }
}