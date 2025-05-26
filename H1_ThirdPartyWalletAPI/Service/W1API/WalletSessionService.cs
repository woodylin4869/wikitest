using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model.H1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Config;
using Npgsql;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace H1_ThirdPartyWalletAPI.Service.W1API
{
    public interface IWalletSessionService
    {
        Task WithdrawSession(long taskId, WalletSessionV2 walletsession);
        Task<WalletSessionV2> RefundSession(long taskId, WalletSessionV2 walletsession);
        Task<WalletSessionV2> GetWalletSessionById(Guid session_id);
        Task<WalletSessionV2> GetWalletSessionByIdAndStartTime(Guid session_id, DateTime startTime);
        Task<WalletSessionV2> GetWalletSessionByClub(string club_id);        
        Task<GetClubSessionV2Res> GetWalletSession(GetWalletSessionV2Req Req);
        Task<bool> MoveRefundedWalletSessionToHistory();
        Task<bool> SettleWalletSesssion(WalletSessionV2 walletsession);
        Task<bool> GetFreshWalletSession();
        void Enqueue(List<BetRecordSession> records);
        Task<bool> H1HealthCheck();
        Task<bool> CheckBetRecordSession(BetRecordSession.Recordstatus recordstatus);
    }
    public class WalletSessionService : IWalletSessionService
    {
        private readonly ILogger<WalletSessionService> _logger;
        private readonly ICommonService _commonService;
        private readonly ITransferService _transferService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ITransferWalletService _transferWalletService;
        private int wallet_session_cacheSeconds = 10;
        private int api_wallet_session_cacheSeconds = 30;
        private static DateTime lastUpdateTime = DateTime.Now;
        private readonly ConcurrentQueue<BetRecordSession> _SessionRecordQueue;

        public WalletSessionService(ILogger<WalletSessionService> logger
            , ICommonService commonService
            , ITransferService transferService
            , IGameApiService gameApiService
            , IGameInterfaceService gameInterfaceService
            , ITransferWalletService transferWalletService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _transferService = transferService;
            _gameApiService = gameApiService;
            _SessionRecordQueue = new ConcurrentQueue<BetRecordSession>();
            _gameInterfaceService = gameInterfaceService;
            _transferWalletService = transferWalletService;
        }


        /// <summary>
        /// 確認session狀態為可提款後，對所有遊戲館作提款
        /// </summary>
        public async Task WithdrawSession(long taskId, WalletSessionV2 walletsession)
        {
            using var withdrawSessionLoggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "WithdrawSessionExecId", Guid.NewGuid().ToString() },
                { "SessionId", walletsession.session_id },
                { "SessionClubId", walletsession.club_id },
            });

            try
            {
                var expiry = TimeSpan.FromSeconds(60);
                var wait = TimeSpan.FromMilliseconds(10);
                var retry = TimeSpan.FromMilliseconds(10);

                var key = $"{LockRedisCacheKeys.W1WalletLock}:{walletsession.club_id}";

                await _commonService._cacheDataService.LockAsyncRegular(key, async () =>
                {
                    var session =
                        await _commonService._serviceDB.GetWalletSessionV2ByIdFromMaster(walletsession.session_id)
                        ?? throw new Exception("Session not found"); //找不到Session

                    if (session.status != WalletSessionV2.SessionStatus.WITHDRAW) //非提款狀態
                        throw new Exception("Session status not withdraw");

                    if (new DateTime(session.update_time.Ticks / TimeSpan.TicksPerMillisecond)
                        != new DateTime(walletsession.update_time.Ticks / TimeSpan.TicksPerMillisecond)) //版本與資料庫不同
                        throw new Exception($"Session version error db:{session.update_time:O} redis:{walletsession.update_time:O}");

                    await WithdrawSessionCore(taskId, session);

                }, expiry, wait, retry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} {status}", nameof(WithdrawSession), LogLevel.Error);
            }
        }

        /// <summary>
        /// 接受Seesion提款後對所有遊戲館作提款
        /// </summary>
        private async Task WithdrawSessionCore(long taskId, WalletSessionV2 walletsession)
        {
            var platform_user = await _commonService._gamePlatformUserService.GetGamePlatformUserAsync(walletsession.club_id);
            var wallet = await _transferWalletService.GetWalletCache(walletsession.club_id);
            if (platform_user == null)
            {
                throw new Exception("No User Data");
            }
            List<string> openGame = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));

            var KicktaskList = new List<Task<bool>>();
            foreach (string r in openGame)
            {
                //只踢最後進入的遊戲館
                if (r == wallet.last_platform)
                {
                    var gameUser = platform_user.FirstOrDefault(x => x.game_platform == r);
                    if (gameUser != null)
                    {
                        KicktaskList.Add(_transferService.KickUser(gameUser, r));
                    }
                }
            }
            var KickResult = await Task.WhenAll(KicktaskList);
            var CashoutTaskList = new List<Task<string>>();

            var egameDepositRecord = await _transferService.GetElectronicDepositRecordCache(wallet.Club_id);
            foreach (string r in openGame)
            {
                Platform source = (Platform)Enum.Parse(typeof(Platform), r, true);
                var gameUser = platform_user.FirstOrDefault(x => x.game_platform == r);
                if (gameUser != null)
                {
                    var platformtype = _gameInterfaceService.GetPlatformType(source);
                    if ((int)platformtype != (int)PlatformType.Electronic //非純電子廠商皆轉出
                        || !egameDepositRecord.Any()                      //無電子轉入紀錄則全部轉出
                        || egameDepositRecord.Any(rec => rec.type == r))  //電子廠商僅轉出最後轉入的3間
                    {
                        CashoutTaskList.Add(_transferService.WithDrawGameWalletV2(gameUser, source, Platform.W1));
                    }
                }
            }
            var CashoutResult = await Task.WhenAll(CashoutTaskList);

            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var transferResult = await _transferService.TransferMemberWallet(conn, tran, Guid.NewGuid(), walletsession.club_id, Platform.W1, Platform.H1, 0, true);

                        if (transferResult.code == (int)ResponseCode.InsufficientBalance)
                        {
                            walletsession.end_balance = 0;
                        }
                        else if (transferResult.code != (int)ResponseCode.Success)
                        {
                            throw new Exception(transferResult.Message);
                        }
                        else
                        {
                            walletsession.end_balance = transferResult.Data.amount;
                        }

                        var SessionData = await _commonService._serviceDB.GetWalletSessionV2Lock(conn, tran, walletsession.session_id, WalletSessionV2.SessionStatus.WITHDRAW);
                        if (SessionData == null)
                        {
                            throw new Exception("Session not found");
                        }
                        walletsession.status = WalletSessionV2.SessionStatus.REFUND;
                        walletsession.update_time = DateTime.Now;
                        walletsession.end_time = DateTime.Now;
                        if (!await _commonService._serviceDB.PutWalletSessionV2(conn, tran, walletsession))
                        {
                            throw new Exception("Session update fail");
                        }
                        await tran.CommitAsync();
                        _logger.LogDebug("Session update to REFUND finish");
                        //更新Redis user session 狀態
                        await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{walletsession.club_id}", walletsession, wallet_session_cacheSeconds);
                        //更新Redis session id 狀態
                        await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{walletsession.session_id}", walletsession, wallet_session_cacheSeconds);
                        //新增Session到待退款Redis list
                        await _commonService._cacheDataService.ListPushAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.refund_list}", walletsession);
                    }
                    catch (Exception ex)
                    {
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError("Task id : {tid} WithdrawSession Id : {id} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", taskId, walletsession.session_id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                        await tran.RollbackAsync();
                    }
                }
                await conn.CloseAsync();
            }
        }
        /// <summary>
        /// 遊戲館提款完成後，退款回平台方
        /// </summary>
        public async Task<WalletSessionV2> RefundSession(long taskId, WalletSessionV2 walletsession)
        {
            using var refundSessionLoggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "RefundSessionExecId", Guid.NewGuid().ToString() },
                    { "SessionId", walletsession.session_id },
                    { "SessionClubId", walletsession.club_id },
                });

            try
            {
                GetTransactionSummaryReq getTransactionSummaryReq = new GetTransactionSummaryReq();
                getTransactionSummaryReq.Club_id = walletsession.club_id;
                getTransactionSummaryReq.StartTime = walletsession.start_time;
                getTransactionSummaryReq.EndTime = walletsession.end_time;

                var WalletTrasferDate = await _commonService._serviceDB.GetSessionTransferRecord(getTransactionSummaryReq);
                var transferlist = WalletTrasferDate.ToList();
                var totalin = transferlist.Where(x => x.source == nameof(Platform.W1)).ToList().Sum(x => x.amount);
                var totalout = transferlist.Where(x => x.target == nameof(Platform.W1)).ToList().Sum(x => x.amount);
                walletsession.status = WalletSessionV2.SessionStatus.UNSETTLE;
                walletsession.update_time = DateTime.Now;
                walletsession.total_in = totalin;
                walletsession.total_out = totalout;
                walletsession.amount_change = walletsession.end_balance - walletsession.start_balance;
                if (walletsession.push_times < 120) //最高retry時間10mins
                    walletsession.push_times++;
                RefundAmountReq req = new RefundAmountReq();
                req.Session_id = walletsession.session_id;
                req.Amount = walletsession.end_balance;
                //調整不送遊戲交易紀錄
                //req.GameTransferData = transferlist;
                req.GameTransferData = new List<WalletTransferRecord>();
                req.SessionData = walletsession;
                var res = await _gameApiService._h1API.Refund(req);
                if (res.code != (int)ResponseCode.Success)
                {
                    walletsession.status = WalletSessionV2.SessionStatus.REFUND;
                    await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{walletsession.session_id}", walletsession, wallet_session_cacheSeconds);
                }
                else
                {
                    walletsession.push_times = 0;
                    using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                    {
                        await conn.OpenAsync();
                        using (var tran = conn.BeginTransaction())
                        {
                            try
                            {
                                var SessionData = await _commonService._serviceDB.GetWalletSessionV2Lock(conn, tran, walletsession.session_id, WalletSessionV2.SessionStatus.REFUND);
                                if (SessionData == null)
                                {
                                    throw new Exception("Session not found");
                                }
                                if (!await _commonService._serviceDB.PutWalletSessionV2(conn, tran, walletsession))
                                {
                                    throw new Exception("Session update fail");
                                }
                                await tran.CommitAsync();
                                await _commonService._cacheDataService.KeyDelete($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{walletsession.club_id}");
                                await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{walletsession.session_id}", walletsession, wallet_session_cacheSeconds);
                            }
                            catch (Exception ex)
                            {
                                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                                _logger.LogError("Task id : {tid} RefundSession Id : {id} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", taskId, walletsession.session_id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                                await tran.RollbackAsync();
                            }
                            await conn.CloseAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} {status}", nameof(RefundSession), LogLevel.Error);
            }
            return walletsession;
        }
        /// <summary>
        /// GUID查詢Session
        /// </summary>
        public async Task<WalletSessionV2> GetWalletSessionById(Guid session_id)
        {
            var session = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{session_id}",
            async () =>
            {
                var data = await _commonService._serviceDB.GetWalletSessionV2ById(session_id);
                if (data == null)
                {
                    data = await _commonService._serviceDB.GetalletSessionV2HistoryById(session_id);
                }
                return data;
            },
            wallet_session_cacheSeconds);
            return session;
        }
        /// <summary>
        /// GUID查詢Session，歷史紀錄就在加入Partition Key(startTime)查詢
        /// </summary>
        public async Task<WalletSessionV2> GetWalletSessionByIdAndStartTime(Guid session_id,DateTime startTime)
        {
            var session = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{session_id}",
                async () =>
                {
                    var data = await _commonService._serviceDB.GetWalletSessionV2ById(session_id);
                    if (data == null)
                    {
                        data = await _commonService._serviceDB.GetWalletSessionV2HistoryByIdAndStartTime(session_id, startTime);
                    }
                    return data;
                },
                wallet_session_cacheSeconds);
            return session;
        }
        /// <summary>
        /// Club_id查詢Session
        /// </summary>
        public async Task<WalletSessionV2> GetWalletSessionByClub(string club_id)
        {
            var session = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{club_id}",
            async () =>
            {
                List<short> status = new List<short>{
                              (short)WalletSessionV2.SessionStatus.DEPOSIT
                            , (short)WalletSessionV2.SessionStatus.WITHDRAW
                            , (short)WalletSessionV2.SessionStatus.REFUND
                         };
                var walletSessionV2 = await _commonService._serviceDB.GetWalletSessionV2(status, club_id);
                return walletSessionV2.SingleOrDefault();
            },
            wallet_session_cacheSeconds);
            return session;
        }
        /// <summary>
        /// Franchiser_id查詢Session
        /// </summary>
        public async Task<GetClubSessionV2Res> GetWalletSession(GetWalletSessionV2Req Req)
        {
            GetClubSessionV2Res res = new GetClubSessionV2Res();
            string Franchiser = Req.Franchiser ?? "ALL";
            var api_response = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.franchiser}/{Franchiser}",
            async () =>
            {
                var sessionList = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.all_session}/H1",
                async () =>
                {
                    List<short> status = new List<short>{
                            (short)WalletSessionV2.SessionStatus.DEPOSIT,
                            (short)WalletSessionV2.SessionStatus.WITHDRAW,
                            (short)WalletSessionV2.SessionStatus.REFUND,
                         };
                    var result = await _commonService._serviceDB.GetWalletSessionStatus(status);
                    return result;
                },
                wallet_session_cacheSeconds);

                if (Req.Franchiser != null)
                {
                    res.Data = sessionList.Where(x => x.franchiser_id == Req.Franchiser).Select(x =>
                    {
                        return new WalletSessionClub((short)x.Status, x.club_id);
                    }).ToList();
                }
                else
                {
                    res.Data = sessionList.Select(x =>
                    {
                        return new WalletSessionClub((short)x.Status, x.club_id);
                    }).ToList();
                }
                return res;
            },
            api_wallet_session_cacheSeconds);
            return api_response;
        }
        /// <summary>
        /// 移動已經退款的Session到歷史區
        /// </summary>
        public async Task<bool> MoveRefundedWalletSessionToHistory()
        {
            await _commonService._serviceDB.MoveWalletSessionToHistory(new List<short>
            {
                (short)WalletSessionV2.SessionStatus.UNSETTLE,
                (short)WalletSessionV2.SessionStatus.SETTLE
            }, 20000);

            return true;
        }
        /// <summary>
        /// 對歷史區已經退款但是帳務未結算的Session作帳務結算
        /// </summary>
        public async Task<bool> SettleWalletSesssion(WalletSessionV2 walletsession)
        {
            //1. 取得所有結算注單
            var session_record = await _commonService._serviceDB.GetRecordSessionBySessionId(walletsession.session_id);
            var session_record_list = session_record.ToList();
            decimal netwin = 0;
            decimal betAmount = 0;
            decimal PreBetAmount = 0;
            if (session_record_list.Any())
            {
                netwin = session_record_list.Sum(x => x.Netwin);
                betAmount = session_record_list.Sum(x => x.Bet_amount);
            }
            //2. 取得所有投注注單
            var bet_session_record = await _commonService._serviceDB.GetRecordSessionByBetSessionId(walletsession.session_id);
            var bet_session_record_list = bet_session_record.ToList();
            if (bet_session_record_list.Any())
            {
                PreBetAmount = bet_session_record_list.Sum(x => x.Bet_amount);
            }
            //3. Sesssion 輸贏 = 所有結算單Win - 所有投注單Bet
            var seesion_netwin = netwin + betAmount - PreBetAmount;
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        bool updateFlag = false;
                        var SessionData = await _commonService._serviceDB.GetalletSessionV2HistoryLock(conn, tran, walletsession.session_id, WalletSessionV2.SessionStatus.UNSETTLE);
                        if (SessionData == null)
                        {
                            throw new Exception("Session not found");
                        }
                        if (SessionData.netwin != seesion_netwin)
                        {
                            SessionData.update_time = DateTime.Now;
                            SessionData.netwin = seesion_netwin;
                            updateFlag = true;
                        }
                        if (SessionData.amount_change == SessionData.netwin)
                        {
                            SessionData.status = WalletSessionV2.SessionStatus.SETTLE;
                            SessionData.update_time = DateTime.Now;
                            updateFlag = true;
                        }
                        if (updateFlag)
                        {
                            if (!await _commonService._serviceDB.PutWalletSessionV2History(conn, tran, SessionData))
                            {
                                throw new Exception("Session update fail");
                            }
                        }
                        await tran.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError("SettleSession Id : {id} exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", walletsession.session_id, ex.GetType().FullName, ex.Message, errorFile, errorLine);
                        await tran.RollbackAsync();
                        return false;
                    }
                    await conn.CloseAsync();
                }
            }

            return true;
        }
        /// <summary>
        /// 對Session結束後的遊戲紀錄作歸帳(歸到下一個Session)
        /// </summary>
        public async Task<bool> CheckBetRecordSession(BetRecordSession.Recordstatus recordstatus)
        {
            //1. 找到結算時間沒在Session狀態內的遊戲彙總記錄
            var session_record = await _commonService._serviceDB.GetRecordSessionByStatus(recordstatus);
            //2. 處理所有未入帳遊戲彙總記錄
            foreach (BetRecordSession betRecordSession in session_record)
            {
                List<short> status = new List<short>{
                              (short)WalletSessionV2.SessionStatus.UNSETTLE
                            };
                var seachDateTime = betRecordSession.EndDatetime ?? DateTime.Now.AddDays(-3);
                //3. 查詢結算時間之後的第一筆Session
                var UnSettleSession = await _commonService._serviceDB.GetalletSessionV2History(status, betRecordSession.Club_id, seachDateTime);
                //4. 更新RecordSession
                if (UnSettleSession.Count() >= 1)
                {
                    var WalletSession = UnSettleSession.OrderBy(x => x.start_time).FirstOrDefault();
                    betRecordSession.Session_id = WalletSession.session_id;
                    betRecordSession.status = BetRecordSession.Recordstatus.InSession;
                    await _commonService._serviceDB.PutRecordSession(betRecordSession);
                }
            }
            return true;
        }
        /// <summary>
        /// 找到有更新的Session, 並推送彙總帳到平台方
        /// </summary>
        public async Task<bool> GetFreshWalletSession()
        {
            var session_record = await _commonService._serviceDB.GetWalletSessionV2byUpdateTime(lastUpdateTime, DateTime.Now);
            if (session_record.Any())
            {
                foreach (WalletSessionV2 sessionV2 in session_record)
                {
                    var SessionRecord = await _commonService._serviceDB.GetRecordSessionByBetSessionId(sessionV2.session_id);
                    if (session_record.Any())
                    {
                        var settleBetRecordReq = new SettleBetRecordReq();
                        settleBetRecordReq.Session_id = sessionV2.session_id;
                        settleBetRecordReq.BetRecordData = SessionRecord.ToList();
                        var res = await _gameApiService._h1API.Settle(settleBetRecordReq);
                    }
                }
                lastUpdateTime = session_record.Max(x => x.update_time);
            }
            return true;
        }

        /// <summary>
        /// 將要更新的Session Record 加到queue
        /// </summary>
        public void Enqueue(List<BetRecordSession> records)
        {
            foreach (var Session_record in records)
            {
                _SessionRecordQueue.Enqueue(Session_record);
            }
        }
        /// <summary>
        /// 對W1H退款API健康檢查
        /// </summary>
        public async Task<bool> H1HealthCheck()
        {
            await _gameApiService._h1API.HealthCheck();
            return true;
        }

    }
}
