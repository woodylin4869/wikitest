using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Request;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using WS168setup = H1_ThirdPartyWalletAPI.Model.Game.WS168.WS168;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.WS168
{
    public interface IWS168InterfaceService : IGameInterfaceService
    {
        Task<ResCodeBase> PostWS168Record(List<SearchingOrdersStatusResponse.Datum> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }
    public class WS168_RecordService : IWS168InterfaceService
    {
        private readonly ILogger<WS168_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IDBService _dbService;
        private readonly ISummaryDBService _summaryDbService;
        private readonly IWS168DBService _ws168DBService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public WS168_RecordService(ILogger<WS168_RecordService> logger,
            ICommonService commonService,
            IGameApiService gameApiService,
            IDBService dbService,
            ISummaryDBService summaryDbService,
            IWS168DBService ws168DBService,
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameApiService;
            _dbService = dbService;
            _summaryDbService = summaryDbService;
            _gameReportDBService = gameReportDBService;
            _ws168DBService = ws168DBService;
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
                var responseData = await _gameApiService._Ws168API.QueryPlayerBalanceAsync(new QueryPlayerBalanceRequest
                {
                    account = platform_user.game_user_id,
                });

                if (responseData.code != "OK")
                {
                    throw new Exception(responseData.code);
                }
                Balance.Amount = responseData.balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("WS168餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.WS168);
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
                var req = new PlayerLogoutRequest()
                {
                    account = platform_user.game_user_id
                };

                var response = await _gameApiService._Ws168API.PlayerLogoutAsync(req);

                if (response.code != "OK") throw new(response.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ws168登出會員失敗 Msg: {Message}", ex.Message);
            }
            return true;
        }
        public async Task<bool> KickAllUser(Platform platform)
        {
            try
            {
                var req = new PlayerLogoutRequest();

                var response = await _gameApiService._Ws168API.PlayerLogoutAsync(req);

                if (response.code != "OK") throw new(response.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ws168全站踢線失敗 Msg: {Message}", ex.Message);
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
            if (!WS168setup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new CreatePlayerRequest()
                {
                    account = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    password = "!8@8%8",
                    name = userData.Club_Ename
                };


                var response = await _gameApiService._Ws168API.CreatePlayerAsync(req);
                if (response.code == "OK")
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.account;
                    gameUser.game_platform = Platform.WS168.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ws168建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "WS168 " + ex.Message.ToString());
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

                var responseData = await _gameApiService._Ws168API.DepositAsync(new DepositRequest
                {
                    account = platform_user.game_user_id,
                    merchant_order_num = RecordData.id.ToString(),
                    amount = RecordData.amount,
                });

                if (responseData.code != "OK")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("WS168 Deposit: {Message}", responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WS168 TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WS168 Deposit: {Message}", ex.Message);
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
                var responseData = await _gameApiService._Ws168API.WithdrawAsync(new WithdrawRequest
                {
                    account = platform_user.game_user_id,
                    merchant_order_num = RecordData.id.ToString(),
                    amount = -1 * (RecordData.amount),
                });

                if (responseData.code != "OK")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("WS168 Withdraw : {ex}", responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WS168 TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("WS168 Withdraw : {ex}", ex.Message);
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
            WS168setup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            PlayerLoginRequest UrlRequest = new PlayerLoginRequest
            {
                login = platformUser.game_user_id,
                password = "!8@8%8",
                lang = lang ?? WS168setup.lang["en-US"]
            };

            //if (request.GameConfig.ContainsKey("lobbyURL"))
            //{
            //    UrlRequest.returnurl = request.GameConfig["lobbyURL"];
            //}

            try
            {
                var token_res = await _gameApiService._Ws168API.PlayerLoginAsync(UrlRequest);
                if (token_res.code != "OK")
                {
                    throw new Exception(token_res.message);
                }
                return token_res.game_link.ToString();
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "WS168: " + ex.Message.ToString());
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

            var WS168Reuslt = await _gameApiService._Ws168API.SearchingOrdersStatusAsync(new SearchingOrdersStatusRequest
            {
                merchant_order_num = transfer_record.id.ToString()
            });
            if (WS168Reuslt.code == "OK")
            {
                if (transfer_record.target == nameof(Platform.WS168))//轉入WS168直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.WS168))
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
                if (transfer_record.target == nameof(Platform.WS168))//轉入WS168直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.WS168))
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
            var batRecords = new List<SearchingOrdersStatusResponse.Datum>();  // 修改类型为 List<BetRecord>
            var res = new GetBetRecord();
            var createtimePair = await _summaryDbService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _ws168DBService.GetRecordsBytime(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                results = results.OrderByDescending(e => e.bet_at).ToList();
                batRecords.AddRange(results);  // 直接添加 BetRecord 列表
            }
            if (!batRecords.Any())
            {
                var results = await _ws168DBService.GetWS168RecordsBySummary(RecordReq);
                results = results.OrderByDescending(e => e.bet_at).ToList();
                batRecords.AddRange(results);  // 直接添加 BetRecord 列表
            }
            IEnumerable<dynamic> ws168_results = batRecords.OrderByDescending(e => e.settled_at);

            res.Data = ws168_results.ToList();
            return res;
        }
        /// <summary>
        /// 取得遊戲住單明細-轉跳
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 取得自訂第三層
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RCGRowData> GameRowData(GetRowDataReq RecordDetailReq)
        {
            RCGRowData rCGRowData = new RCGRowData();

            var ws168_results = new List<SearchingOrdersStatusResponse.Datum>();
            var createtimePair = await _summaryDbService.GetPartitionTime(RecordDetailReq.summary_id, RecordDetailReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _ws168DBService.GetWS168RecordsV2(RecordDetailReq.record_id, createTime);
                results = results.OrderByDescending(e => e.bet_at).ToList();
                ws168_results.AddRange(results);  // 直接添加 BetRecord 列表
            }
            if (!ws168_results.Any())
            {
                var results = await _ws168DBService.GetWS168Records(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
                results = results.OrderByDescending(e => e.bet_at).ToList();
                ws168_results.AddRange(results);  // 直接添加 BetRecord 列表
            }
            if (!ws168_results.Any())
            {
                throw new Exception("no data");
            }
            List<object> res = new List<object>();
            var ws168data = ws168_results.OrderByDescending(x => x.settled_at).First();

            res.Add(ws168data);

            rCGRowData.dataList = res;
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
                RepairCount += await RepairWS168(startTime, endTime);
                startTime = endTime;
                await Task.Delay(11000);
            }
            RepairCount += await RepairWS168(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime.AddSeconds(-1));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._Ws168API.QueryPlayerBalanceAsync(new QueryPlayerBalanceRequest
            {
                account = "HealthCheck",
            });
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<ResCodeBase> PostWS168Record(List<SearchingOrdersStatusResponse.Datum> recordData)
        {

            ResCodeBase res = new ResCodeBase();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var ordLogs = (await _ws168DBService.GetRecordsByBetTime(recordData.Min(l => l.bet_at), recordData.Max(l => l.bet_at)))
            .Select(l => new { l.slug, l.bet_at })
            .ToHashSet();

            IEnumerable<IGrouping<string, SearchingOrdersStatusResponse.Datum>> linqRes = recordData.GroupBy(x => x.account);

            foreach (IGrouping<string, SearchingOrdersStatusResponse.Datum> group in linqRes)
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
                            ;
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                            //已結算注單
                            List<SearchingOrdersStatusResponse.Datum> betDetailData = new List<SearchingOrdersStatusResponse.Datum>();
                            //位結算
                            List<SearchingOrdersStatusResponse.Datum> betDetailDataRunning = new List<SearchingOrdersStatusResponse.Datum>();

                            foreach (SearchingOrdersStatusResponse.Datum item in group)
                            {
                                if (!ordLogs.Add(new { item.slug, item.bet_at }))
                                    continue;

                                item.report_time = reportTime;

                                item.bet_at = item.bet_at.ToLocalTime();
                                item.settled_at = item.settled_at?.ToLocalTime();


                                item.partition_time = item.bet_at;
                                await Calculate(conn, tran, item);
                                switch (item.status)
                                {
                                    case "beted":
                                        item.club_id = memberWalletData.Club_id;
                                        item.franchiser_id = memberWalletData.Franchiser_id;
                                        betDetailDataRunning.Add(item);
                                        break;
                                    case "settled":
                                    case "cancel":
                                        betDetailData.Add(item);
                                        break;
                                }
                            }
                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = nameof(Platform.WS168)+$"{RedisCacheKeys.BetSummaryTime}:{item.Key}";
                                    await _commonService._cacheDataService.ListPushAsync(key, subItem);
                                }
                            }
                            if (betDetailData.Count > 0)
                            {
                                int PostRecordResult = await _ws168DBService.PostWS168Record(conn, tran, betDetailData);
                            }
                            if (betDetailDataRunning.Count > 0)
                            {
                                int PostRunningRecordResult = await _ws168DBService.Postws168RunningRecord(conn, tran, betDetailDataRunning);
                                await _ws168DBService.PostWS168Record(conn, tran, betDetailDataRunning);
                            }

                            //刪除已結算之未結算單
                            foreach (var settleRecord in betDetailData.Where(b => b.status == "settled" || b.status == "cancel"))
                                await _ws168DBService.Deletews168RunningRecord(conn, tran, settleRecord.slug, settleRecord.bet_at);

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

                            _logger.LogError("Run WS168 record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return res;
        }

        //public async Task<ResCodeBase> PostWS168Record_back(List<SearchingOrdersStatusResponse.Datum> recordData)
        //{

        //    ResCodeBase res = new ResCodeBase();
        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    IEnumerable<IGrouping<string, SearchingOrdersStatusResponse.Datum>> linqRes = recordData.GroupBy(x => x.account);

        //    foreach (IGrouping<string, SearchingOrdersStatusResponse.Datum> group in linqRes)
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
        //                    club_id = group.Key.Substring(3);
        //                    Wallet memberWalletData = await GetWalletCache(club_id);
        //                    if (memberWalletData == null || memberWalletData.Club_id == null)
        //                    {
        //                        throw new Exception("沒有會員id");
        //                    }

        //                    var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.WS168);
        //                    if (gameUser == null || gameUser.game_user_id != group.Key)
        //                    {
        //                        throw new Exception("No WS168 user");
        //                    }

        //                    //彙總注單
        //                    Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
        //                    //已結算注單
        //                    List<SearchingOrdersStatusResponse.Datum> betDetailData = new List<SearchingOrdersStatusResponse.Datum>();
        //                    //位結算
        //                    List<SearchingOrdersStatusResponse.Datum> betDetailDataRunning = new List<SearchingOrdersStatusResponse.Datum>();

        //                    foreach (SearchingOrdersStatusResponse.Datum item in group)
        //                    {
        //                        DateTime drawtime = DateTime.Now;

        //                        BetRecordSummary sumData = new BetRecordSummary();
        //                        sumData.Club_id = memberWalletData.Club_id;
        //                        sumData.Game_id = nameof(Platform.WS168);
        //                        sumData.Game_type = WS168setup.CodeToId[item.arena_no];
        //                        sumData.ReportDatetime = new DateTime(drawtime.Year, drawtime.Month, drawtime.Day, drawtime.Hour, (drawtime.Minute / 5) * 5, 0);

        //                        //先確認有沒有符合的匯總單
        //                        if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
        //                        {
        //                            sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
        //                            //合併處理
        //                            sumData = await Calculate(conn, tran, sumData, item);
        //                            summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
        //                        }
        //                        else
        //                        {
        //                            //用Club_id與ReportDatetime DB取得彙總注單
        //                            IEnumerable<dynamic> results = await _summaryDbService.GetRecordSummaryLock(conn, tran, sumData);
        //                            sw.Stop();
        //                            _logger.LogDebug("get summary record ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                            sw.Restart();
        //                            if (results.Count() == 0) //沒資料就建立新的
        //                            {
        //                                //建立新的Summary
        //                                sumData.Currency = memberWalletData.Currency;
        //                                sumData.Franchiser_id = memberWalletData.Franchiser_id;

        //                                //合併處理
        //                                sumData = await Calculate(conn, tran, sumData, item);
        //                            }
        //                            else //有資料就更新
        //                            {
        //                                sumData = results.SingleOrDefault();
        //                                //合併處理
        //                                sumData = await Calculate(conn, tran, sumData, item);
        //                            }
        //                            summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
        //                        }
        //                        item.summary_id = sumData.id;
        //                        switch (item.status)
        //                        {
        //                            case "beted":
        //                                item.club_id = memberWalletData.Club_id;
        //                                item.franchiser_id = memberWalletData.Franchiser_id;
        //                                betDetailDataRunning.Add(item);
        //                                break;
        //                            case "settled":
        //                            case "cancel":
        //                                betDetailData.Add(item);
        //                                break;
        //                        }
        //                    }
        //                    List<BetRecordSummary> summaryList = new();
        //                    foreach (var s in summaryData)
        //                    {
        //                        if (s.Value.RecordCount > 0)
        //                        {
        //                            summaryList.Add(s.Value);
        //                        }
        //                    }
        //                    int PostRecordSummaryReuslt = await _summaryDbService.PostRecordSummary(conn, tran, summaryList);
        //                    if (betDetailData.Count > 0)
        //                    {
        //                        int PostRecordResult = await _ws168DBService.PostWS168Record(conn, tran, betDetailData);
        //                    }
        //                    if (betDetailDataRunning.Count > 0)
        //                    {
        //                        int PostRunningRecordResult = await _ws168DBService.Postws168RunningRecord(conn, tran, betDetailDataRunning);
        //                        await _ws168DBService.PostWS168Record(conn, tran, betDetailDataRunning);
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

        //                    _logger.LogError("Run WS168 record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

        //                }
        //            }
        //            await conn.CloseAsync();
        //        }
        //    }
        //    sw.Stop();
        //    _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //    return res;
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
                _logger.LogDebug("Create WS168 game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計

                // 遊戲商(轉帳中心的欄位格式)
                var gameEmptyReport = new GameReport
                {
                    platform = nameof(Platform.WS168),
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
                _logger.LogDebug("Create WS168 game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _ws168DBService.SumWS168BetRecordByBetTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.WS168);
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

        /// <summary>
        /// 統計5分鐘
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="SummaryData"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, SearchingOrdersStatusResponse.Datum r)
        {
            r.pre_bet_amount = r.bet_amount;
            r.pre_net_income = r.net_income;
            r.pre_valid_amount = r.valid_amount;
            if (r.status == "beted")
            {
                r.valid_amount = r.bet_amount;
                r.pre_valid_amount = r.valid_amount;
            }

            var Records = await _ws168DBService.GetWS168RecordsV2(r.slug, r.bet_at);
            if(!Records.Any())
                Records = await _ws168DBService.GetWS168Records(r.slug, r.bet_at);

            if (Records.Any(oldr => new { oldr.slug, oldr.status, oldr.settled_at }.Equals(new { r.slug, r.status, oldr.settled_at })))
            {
                r.status = "fail";
            }

            if (Records.Any())
            {
                //只會有未結算單不會有改排單，所以只會有未結算單後要沖銷
                var lastRecord = Records.OrderByDescending(r => r.settled_at).First(); //僅需沖銷最後一筆即可

                r.bet_amount = (Convert.ToDecimal(r.bet_amount) - Convert.ToDecimal(lastRecord.pre_bet_amount)).ToString();
                r.net_income = (Convert.ToDecimal(r.net_income) - Convert.ToDecimal(lastRecord.pre_net_income)).ToString();
                r.valid_amount = (Convert.ToDecimal(r.valid_amount) - Convert.ToDecimal(lastRecord.pre_valid_amount)).ToString();
            }
        }

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairWS168(DateTime startTime, DateTime endTime)
        {
            var Page = 1;
            var req = new BetLogRequest
            {
                time_type = "bet_at",
                start_time = startTime.ToUniversalTime(),
                end_time = endTime.AddMilliseconds(-1).ToUniversalTime(),
                page = Page,
                page_size = 10000
            };

            var res = new List<SearchingOrdersStatusResponse.Datum>();
            while (true)
            {
                req.page = Page;
                var betLogs = await _gameApiService._Ws168API.BetLogAsync(req);

                if (betLogs.total_count == 0)
                {
                    break;
                }
                res.AddRange(betLogs.data);

                Page++;
                if (Page > betLogs.total_page)
                    break;
                //api建議11秒爬一次
                await Task.Delay(11000);
            }

            List<SearchingOrdersStatusResponse.Datum> repairList = new List<SearchingOrdersStatusResponse.Datum>();

            if (res.Count == 0)
            {
                return repairList.Count;
            }

            var w1CenterList = await _ws168DBService.GetWS168RecordsBytime(startTime, endTime);

            foreach (var item in res)
            {
                var hasData = w1CenterList.Where(x => x.slug == item.slug && x.status == item.status).Any();
                if (hasData == false)
                {
                    repairList.Add(item);
                }

            }
            if (repairList.Count != 0)
            {
                await PostWS168Record(repairList);
            }


            return repairList.Count;
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
            return PlatformType.Animal;
        }

        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> ws168_results = await _ws168DBService.Getws168RunningRecord(RecordReq);
            ws168_results = ws168_results.OrderByDescending(e => e.bet_at);
            res.Data = ws168_results.ToList();
            return res;

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
            var summaryRecords = await _ws168DBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => new { x.userid, x.Game_type });
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => ConvertGamePlatformUserToClubInfo(x.userid)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();


            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key.userid), out var userWallet)) continue;
                var gameType = WS168setup.CodeToId[summaryRecord.Key.Game_type];

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.WS168);
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
                        partition_time = item.bettime
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


                        await _summaryDbService.BatchInsertRecordSummaryAsync(conn, group.ToList());



                        sw.Stop();
                        _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", group.Count(), sw.ElapsedMilliseconds);
                    }

                    await _summaryDbService.BulkInsertSummaryBetRecordMapping(tran, summaryBetRecordMappings);
                    await tran.CommitAsync();
                    await conn.CloseAsync();
                }
            }
            return true;
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
    }
}
