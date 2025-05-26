using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Request;
using H1_ThirdPartyWalletAPI.Model.Game.AE.Response;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;
using H1_ThirdPartyWalletAPI.Utility;
using Microsoft.Extensions.Logging;
using Npgsql;
using ThirdPartyWallet.Share.Model.Game.PS.Request;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;

namespace H1_ThirdPartyWalletAPI.Service.Game.AE
{
    public class AE_RecordService : IAeInterfaceService
    {
        private readonly ILogger<AE_RecordService> _logger;
        private readonly IGameApiService _gameApiService;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IAeDBService _aeDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IBetLogsDbConnectionStringManager _betLogsDbConnectionStringManager;
        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;
        private readonly string _prefixKey;

        public AE_RecordService(ILogger<AE_RecordService> logger,
            IGameApiService gameaApiService,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IAeDBService aeDBService,
            IGameReportDBService gameReportDBService,
            IBetLogsDbConnectionStringManager betLogsDbConnectionStringManager)
        {
            _logger = logger;
            _gameApiService = gameaApiService;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _aeDBService = aeDBService;
            _gameReportDBService = gameReportDBService;
            _betLogsDbConnectionStringManager = betLogsDbConnectionStringManager;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
        }

        #region GameInterfaceService

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                var responseData = await _gameApiService._AeAPI.GetBalanceAsync(new GetBalanceRequest()
                {
                    site_id = Config.CompanyToken.AE_SiteId,
                    account_name = platform_user.game_user_id
                });

                if (responseData.Error_code != "OK")
                {
                    throw new Exception(responseData.Error_code);
                }
                Balance.Amount = responseData.balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Ae餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.AE);
            return Balance;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                //AE館踢線功能不穩定先不作
                //AE館踢線要檢查是否是最後進入遊戲館
                //if (platform_user.last_platform != nameof(Platform.AE))
                //{
                //    return "success";
                //}
                //if (platform_user.ae_id != null)
                //{
                //    try
                //    {
                //        var aeResponseData = await _gameaApiService._AeAPI.FreezePlayerAsync(new FreezePlayerRequest()
                //        {
                //            account_name = platform_user.ae_id,
                //            period = 30
                //        });

                //        if (aeResponseData.error_code != "OK")
                //        {
                //            throw new Exception(aeResponseData.error_code);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogInformation("踢出AE使用者失敗 id:{account} Msg: {Message}", platform_user.ae_id, ex.Message);
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出AE使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }

        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var responseData = await _gameApiService._AeAPI.DepositAsync(new Model.Game.AE.Request.DepositRequest
                {
                    site_id = Config.CompanyToken.AE_SiteId,
                    account_name = platform_user.game_user_id,
                    amount = transfer_amount,
                    tx_id = RecordData.id.ToString()
                });

                if (responseData.Error_code != "OK")
                {
                    throw new Exception(responseData.Error_code);
                }

                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("AE TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInAeFail Msg: {Message}", ex.Message);
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
                var responseData = await _gameApiService._AeAPI.WithdrawAsync(new Model.Game.AE.Request.WithdrawRequest()
                {
                    site_id = Config.CompanyToken.AE_SiteId,
                    account_name = platform_user.game_user_id,
                    amount = game_balance,
                    tx_id = RecordData.id.ToString()
                });

                if (responseData.Error_code != "OK")
                {
                    throw new Exception(responseData.Error_code);
                }

                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("AE TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("AE TransferOut Fail ex : {ex}", ex.Message);
            }
            return RecordData.status;
        }

        /// <summary>
        /// W1 t_ds_bet_record GamePlatformUser 轉換 Club Info 屬性規則
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

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.AE.AE.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }

            ForwardGame res = new ForwardGame();
            //Step 1 Create Member
            var requestData = new CreateAccountRequest();
            requestData.site_id = Config.CompanyToken.AE_SiteId;
            requestData.currency = Model.Game.AE.AE.Currency[userData.Currency];

            requestData.account_name = (Config.OneWalletAPI.Prefix_Key + userData.Club_id).ToLower();
            try
            {
                var result = await _gameApiService._AeAPI.CreateAccountAsync(requestData);
                if (result.Error_code != "OK" && result.Error_code != "PlayerAlreadyExists")
                {
                    throw new Exception(result.Error_code);
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.CreateAeUserFail, MessageCode.Message[(int)ResponseCode.CreateAeUserFail] + "|" + ex.Message.ToString());
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = requestData.account_name;
            gameUser.game_platform = request.Platform;
            return gameUser;
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 3 Get Game URL
            var UrlRequest = new GetLoginUrlRequest();
            UrlRequest.site_id = Config.CompanyToken.AE_SiteId;
            UrlRequest.account_name = platformUser.game_user_id;

            if (!request.GameConfig.ContainsKey("gameCode"))
            {
                throw new Exception("game code not found");
            }
            else
            {
                UrlRequest.game_id = Convert.ToInt32(request.GameConfig["gameCode"]);
            }

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.exit_url = request.GameConfig["lobbyURL"];
            }
            if (request.GameConfig.ContainsKey("lang") && Model.Game.AE.AE.lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.lang = Model.Game.AE.AE.lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.lang = Model.Game.AE.AE.lang["en-US"];
            }

            try
            {
                var token_res = await _gameApiService._AeAPI.GetLoginUrlAsync(UrlRequest);
                return token_res.game_url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            GetTransactionRequest AeReqData = new GetTransactionRequest();
            AeReqData.tx_id = transfer_record.id.ToString();
            var AeReuslt = await _gameApiService._AeAPI.GetTransactionInfo(AeReqData);
            if (AeReuslt.Error_code == "OK")
            {
                if (transfer_record.target == nameof(Platform.AE))//轉入AE直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.AE))
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
            else if (AeReuslt.Error_code == "TransactionNotFound")
            {
                if (transfer_record.target == nameof(Platform.AE))//轉入AE直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.AE))
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
            #region backup

            //GetBetRecord res = new GetBetRecord();
            //IEnumerable<dynamic> ae_results = await _aeDBService.GetAeRecordBySummary(RecordReq);
            //ae_results = ae_results.OrderByDescending(e => e.completed_at);
            //res.Data = ae_results.ToList();
            //return res;

            #endregion backup

            GetBetRecord res = new GetBetRecord();
            List<dynamic> results = new List<dynamic>();
            var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();


            results.AddRange(await _aeDBService.GetAeRecordBySummary(RecordReq));


            // 舊表沒有資料的話可能是在新表
            if (results.Count == 0)
            {
                foreach (var partition in partitions)
                {
                    results.AddRange(await _aeDBService.GetAeRecordByReportTime(Config.OneWalletAPI.Prefix_Key + RecordReq.ClubId, RecordReq.ReportTime, partition, partition.AddDays(1)));
                }
            }
            results = results.OrderByDescending(e => e.completed_at).ToList();

            res.Data = results;
            return res;
        }

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var aeRecord = await _aeDBService.GetAeRecord(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            if (aeRecord == null)
            {
                // 舊表沒有資料的話可能是在新表
                // TODO：先註解不用，確認不用拿掉

                #region 先註解不用，確認不用拿掉

                //var cacheSeconds = 300;
                //var key = $"{RedisCacheKeys.AeGameDetailURL}:{RecordDetailReq.summary_id}:{RecordDetailReq.ReportTime.ToString("yyyy-MM-dd HH:mm:ss")}";
                //// 第一層明細
                //var summary = await _commonService._cacheDataService.GetOrSetValueAsync(key, async () => await _summaryDBService.GetRecordSummaryById(new GetBetRecordReq()
                //{
                //    summary_id = RecordDetailReq.summary_id.ToString(),
                //    ReportTime = RecordDetailReq.ReportTime
                //}), cacheSeconds);

                //if (summary == null) return string.Empty;

                #endregion 先註解不用，確認不用拿掉

                aeRecord = await _aeDBService.GetAeRecordByReportTime(RecordDetailReq.ReportTime, Convert.ToInt64(RecordDetailReq.record_id));

                if (aeRecord == null)
                    return string.Empty;
            }

            var aeResponseData = await _gameApiService._AeAPI.GetGameHistoryUrl(new GetGameHistoryUrlRequest()
            {
                site_id = Config.CompanyToken.AE_SiteId,
                account_name = aeRecord.account_name,
                round_id = RecordDetailReq.record_id,
                lang = Model.Game.AE.AE.lang.ContainsKey(RecordDetailReq.lang) ? Model.Game.AE.AE.lang[RecordDetailReq.lang] : Model.Game.AE.AE.lang["en-US"]
            });

            if (aeResponseData != null && aeResponseData.Error_code != "OK")
            {
                throw new Exception("no data");
            }
            return aeResponseData.game_history_url;
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddSeconds(-RepairReq.EndTime.Second).AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 10)
            {
                endTime = startTime.AddMinutes(10);
                _logger.LogDebug("Repair AE record start Time : {startTime} end Time : {endTime}", startTime, endTime);
                RepairCount += await RepairAe(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            _logger.LogDebug("Repair AE record start Time : {startTime} end Time : {endTime}", startTime, RepairReq.EndTime);
            RepairCount += await RepairAe(startTime, RepairReq.EndTime);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            //await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            // TODO: RecordSummary需要移除DateTime startTime, DateTime endTime，Schedule 跟著把DateTime startTime, DateTime endTime 邏輯處理刪除
            // 取得匯總需要的起始和結束時間
            (DateTime StartTime, DateTime EndTime) = await GetRecordSummaryDateTime(reportDatetime);
            startTime = StartTime;
            endTime = EndTime;

            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _aeDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", Groupsummary.Count(), sw1.ElapsedMilliseconds);

            // 取得遊戲內帳號轉為為Club_id集合
            var userlist = Groupsummary.Select(x => ConvertGamePlatformUserToClubInfo(x.Key)).Distinct().ToList();
            // 批次處理，每次1000筆
            var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
            {
                return (await _commonService._serviceDB.GetWallet(betch));
            }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);

            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();
            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.AE);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.win) - summaryRecord.Sum(x => x.bet);
                summaryData.updatedatetime = DateTime.Now;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpotwin);
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

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._AeAPI.GetBalanceAsync(new GetBalanceRequest()
            {
                site_id = Config.CompanyToken.AE_SiteId,
                account_name = "HealthCheck"
            });
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        #endregion GameInterfaceService

        #region GameRecordService

        private bool Whereprefixkey(string account)
        {
            return account.StartsWith(_prefixKey.Substring(0, 1));
        }

        /// <summary>
        /// 新增 5 分鐘匯總帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task PostAeRecordDetail(List<BetHistory> recordData)
        {
            recordData = recordData.Where(l => Whereprefixkey(l.account_name)).ToList();
            var chucklist = recordData.Chunk(20000);
            foreach (IEnumerable<BetHistory> group in chucklist)
            {

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var betDetailData = new List<BetHistory>();
                var dt = DateTime.Now;
                var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                // 紀錄 reportTime 跟 completed_at 的關聯
                var dic = new Dictionary<string, HashSet<string>>();

                foreach (BetHistory r in group)//loop club id bet detail
                {   //判斷紅包免費遊戲
                    if (r.free) 
                    { 
                        r.bet_amt = "0"; 
                    }

                    r.report_time = reportTime;
                    betDetailData.Add(r);

                    // 紀錄 reportTime 跟 partition column completed_at 的關聯
                    var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                    if (!dic.ContainsKey(summaryTime))
                    {
                        dic.Add(summaryTime, new HashSet<string>());
                    }

                    dic[summaryTime].Add(r.completed_at.ToString("yyyy-MM-dd HH:mm"));
                }

                try
                {

                    if (betDetailData.Count > 0)
                    {
                        await using (var conn = new NpgsqlConnection(_betLogsDbConnectionStringManager.GetMasterConnectionString()))
                        {
                            await conn.OpenAsync();

                            foreach (var chunk in betDetailData.Chunk(20000))
                            {
                                await using var tran = await conn.BeginTransactionAsync();
                                await _aeDBService.PostAeRecord(conn, tran, chunk.ToList());
                                await tran.CommitAsync();
                            }

                        }
                    }
                    // 記錄到 Redis reportTime 跟 playTime 的關聯
                    foreach (var item in dic)
                    {
                        var key = $"{RedisCacheKeys.AeBetSummaryTime}:{item.Key}";
                        await _commonService._cacheDataService.SortedSetAddAsync(key,
                            item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                    }
                }
                catch (ExceptionMessage ex)
                {
                    throw new ExceptionMessage(ex.MsgId, ex.Message);
                }
                finally
                {
                    dic.Clear();
                }

                sw.Stop();
                _logger.LogDebug("AeRecordSchedule 寫入{count}筆資料時間 : {time} MS", betDetailData.Count, sw.ElapsedMilliseconds);

            }
        }

        public async Task PostAeRecord_backup(List<BetHistory> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();

                // 按照會員Id GroupBy
                var groupByList = recordData.GroupBy(x => x.account_name);

                foreach (var group in groupByList)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            var club_id = group.Key.Substring(3); // 去掉前墜的環境變數

                            // 檢查會員錢包
                            var memberWalletData = await GetWalletCache(club_id.ToUpper());
                            if (memberWalletData?.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            // 檢查會員
                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id.ToUpper(), Platform.AE);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No ae user");
                            }

                            //彙總注單 key: 時間(5分鐘為一個單位), value: 累加的匯總資料
                            var summaryDictionary = new Dictionary<string, BetRecordSummary>();

                            //已結算注單
                            var betDetailData = new List<BetHistory>();

                            foreach (var betHistory in group)
                            {
                                var betRecordSummary = new BetRecordSummary();
                                betRecordSummary.Club_id = memberWalletData.Club_id;
                                betRecordSummary.Game_id = nameof(Platform.AE);

                                var dt = Convert.ToDateTime(betHistory.completed_at);
                                //betRecordSummary.ReportDatetime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                                dt = dt.AddMinutes(-dt.Minute % 5);
                                dt = dt.AddSeconds(-dt.Second);
                                dt = dt.AddMilliseconds(-dt.Millisecond);
                                betRecordSummary.ReportDatetime = dt;

                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    //if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    //{
                                    //    DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                    //    if (betRecordSummary.ReportDatetime < ReportDateTime)
                                    //    {
                                    //        betRecordSummary.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                    //        _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", betHistory.round_id);
                                    //    }
                                    //}
                                    //else
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (betRecordSummary.ReportDatetime < ReportDateTime)
                                        {
                                            betRecordSummary.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", betHistory.round_id);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (betRecordSummary.ReportDatetime < ReportDateTime)
                                        {
                                            betRecordSummary.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", betHistory.round_id);
                                        }
                                    }
                                }

                                //先確認有沒有符合的匯總單
                                if (summaryDictionary.ContainsKey(betRecordSummary.ReportDatetime.ToString()))
                                {
                                    betRecordSummary = summaryDictionary[betRecordSummary.ReportDatetime.ToString()];
                                    //合併處理
                                    betRecordSummary = Calculate(betRecordSummary, betHistory);
                                    summaryDictionary[betRecordSummary.ReportDatetime.ToString()] = betRecordSummary;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    var results = await _summaryDBService.GetRecordSummaryLock(conn, tran, betRecordSummary);
                                    if (!results.Any()) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        betRecordSummary.Currency = memberWalletData.Currency;
                                        betRecordSummary.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        betRecordSummary = Calculate(betRecordSummary, betHistory);
                                    }
                                    else //有資料就更新
                                    {
                                        betRecordSummary = results.SingleOrDefault();
                                        //合併處理
                                        betRecordSummary = Calculate(betRecordSummary, betHistory);
                                    }

                                    summaryDictionary.Add(betRecordSummary.ReportDatetime.ToString(), betRecordSummary);
                                }

                                //betHistory.summary_id = betRecordSummary.id;
                                betDetailData.Add(betHistory);
                            }

                            var summaryList = summaryDictionary.Values.ToList();

                            await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            await _aeDBService.PostAeRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run ae record group: {key} exception EX : {ex}  MSG : {Message} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString());
                            await tran.RollbackAsync();
                        }
                    }
                }

                await conn.CloseAsync();
            }
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
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create Ps game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _gameApiService._AeAPI.GetReorts(new GetReportRequest()
                {
                    site_id = Config.CompanyToken.AE_SiteId,
                    from_time = startDateTime.ToString("yyyy-MM-dd'T'HH+08:00"),
                    to_time = startDateTime.AddHours(1).ToString("yyyy-MM-dd'T'HH+08:00")
                });
                int count = 0;
                var gameEmptyReport = new GameReport();
                if (req.winloss_summary.Count() == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.AE);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {
                    foreach (var dateEntry in req.winloss_summary)
                    {
                        gameEmptyReport.platform = nameof(Platform.AE);
                        gameEmptyReport.report_datetime = reportTime;
                        gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                        gameEmptyReport.total_bet += (decimal)dateEntry.bet_amt;
                        gameEmptyReport.total_win += (decimal)dateEntry.payout_amt;
                        gameEmptyReport.total_netwin += ((decimal)dateEntry.payout_amt - (decimal)dateEntry.bet_amt);
                        gameEmptyReport.total_count+= dateEntry.rounds;
                    }
                }

                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
                startDateTime = startDateTime.AddHours(1);

                await Task.Delay(3000);
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
                _logger.LogDebug("Create AE game W1 report time {datetime}", reportTime);
                IEnumerable<dynamic> dailyReport = await _aeDBService.SumAeBetRecordHourly(reportTime);
                var HourlylyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.AE);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = HourlylyReportData.total_bet == null ? 0 : Math.Abs(HourlylyReportData.total_bet);
                reportData.total_win = HourlylyReportData.total_win == null ? 0 : HourlylyReportData.total_win;
                reportData.total_netwin = reportData.total_win - reportData.total_bet;
                reportData.total_count = HourlylyReportData.total_cont == null ? 0 : HourlylyReportData.total_cont;
                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);
                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }

      
       

        #endregion GameRecordService

        private BetRecordSummary Calculate(BetRecordSummary summaryData, BetHistory gameDetail)
        {
            summaryData.RecordCount++;
            summaryData.Bet_amount += decimal.Parse(gameDetail.bet_amt);
            summaryData.Turnover += decimal.Parse(gameDetail.bet_amt);
            summaryData.Netwin += decimal.Parse(gameDetail.payout_amt) - decimal.Parse(gameDetail.bet_amt);
            summaryData.Win += decimal.Parse(gameDetail.payout_amt);
            summaryData.updatedatetime = DateTime.Now;
            return summaryData;
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
        /// AE 帳務比對
        /// 1. 比對轉帳中心與遊戲商的下注明細是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairAe(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的歷史下注紀錄，一次取得時間間隔要小於 15 分鐘
            //var aeHistoryList = new List<BetHistory>();
            var betRecord = await _gameApiService._AeAPI.GetBetHistories(new GetBetHistoriesRequest()
            {
                site_id = Config.CompanyToken.AE_SiteId,
                from_time = startTime,
                to_time = endTime
            });

            // 轉帳中心的歷史下注紀錄
            // 舊表資料
            var w1CenterList = await _aeDBService.SumAeBetRecord(startTime, endTime) ?? new List<t_ae_bet_record>();
            var convert = w1CenterList
                .Select(x => new BetHistory
                {
                    currency = x.currency,
                    account_name = x.account_name,
                    bet_amt = x.bet_amt,
                    payout_amt = x.payout_amt,
                    round_id = x.round_id
                }).ToList();

            var w1CenterListV2 = await _aeDBService.SumAeBetRecordV2(startTime, endTime) ?? new List<t_ae_bet_record>();
            var convertV2 = w1CenterListV2
                .Select(x => new BetHistory
                {
                    currency = x.currency,
                    account_name = x.account_name,
                    bet_amt = x.bet_amt,
                    payout_amt = x.payout_amt,
                    round_id = x.round_id
                }).ToList();

            convert.AddRange(convertV2);

            // 比對歷史下注紀錄
            var diffList = betRecord.bet_histories.Except(convert, new AEBetRecordComparer()).ToList();
            if (!diffList.Any())
            {
                return 0;
            }
            // 將匯總帳的 summaryId 寫回去
            foreach (var item in diffList)
            {
                // 設定預設值
                if (item.bet_amt == null)
                {
                    item.bet_amt = "0";
                }

                if (item.payout_amt == null)
                {
                    item.payout_amt = "0";
                }

                if (item.end_balance == null)
                {
                    item.end_balance = "0";
                }

                if (item.rebate_amt == null)
                {
                    item.rebate_amt = "0";
                }

                if (item.jp_pc_con_amt == null)
                {
                    item.jp_pc_con_amt = "0";
                }

                if (item.jp_jc_con_amt == null)
                {
                    item.jp_jc_con_amt = "0";
                }

                if (item.jp_pc_win_amt == null)
                {
                    item.jp_pc_win_amt = "0";
                }

                if (item.jp_jc_win_amt == null)
                {
                    item.jp_jc_win_amt = "0";
                }

                if (item.prize_amt == null)
                {
                    item.prize_amt = "0";
                }
            }
            // 注單明細補單、更新 5 分鐘匯總帳
            await PostAeRecordDetail(diffList);
            return diffList.Count();
        }

        /// <summary>
        /// 取得匯總需要的起始和結束時間
        /// </summary>
        /// <param name="reportTime">排程執行匯總時間</param>
        /// <returns>匯總需要的起始和結束時間</returns>
        private async Task<(DateTime StartTime, DateTime EndTime)> GetRecordSummaryDateTime(DateTime reportTime)
        {
            DateTime? startTime = null;
            DateTime? endTime = null;

            // 將老虎機、魚機記錄好的 reporttime > playtime 取出
            var redisKey = $"{RedisCacheKeys.AeBetSummaryTime}:{reportTime.ToString("yyyy-MM-dd HH:mm")}";

            var (timeStart, _) = await _commonService._cacheDataService.SortedSetPopMinAsync(redisKey);
            var (timeEnd, _) = await _commonService._cacheDataService.SortedSetPopMaxAsync(redisKey);

            if (timeStart != default && timeEnd != default)
            {
                // 找出最大最小值
                startTime = DateTime.Parse(timeStart).AddMinutes(-15);
                endTime = DateTime.Parse(timeEnd).AddMinutes(15);
            }

            // 預設值
            if (startTime == null || endTime == null)
            {
                startTime = reportTime.AddDays(-2);
                endTime = reportTime.AddHours(1);
            }

            // 檢查時間範圍不可超過 50 小時，超過就截斷
            var timeSpan = new TimeSpan(endTime.Value.Ticks - startTime.Value.Ticks);
            if (timeSpan.TotalHours > 240)
            {
                // 從最近的時間往前推 240 小時
                startTime = endTime.Value.AddHours(-240);
            }

            var result = await Task.FromResult((startTime.Value, endTime.Value));
            return result;
        }
    }

    /// <summary>
    /// AE 注單差異比對
    /// </summary>
    public class AEBetRecordComparer : IEqualityComparer<BetHistory>
    {
        public bool Equals(BetHistory x, BetHistory y)
        {
            //確認兩個物件的資料是否相同
            if (Object.ReferenceEquals(x, y)) return true;

            //確認兩個物件是否有任何資料為空值
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //這邊就依照個人需求比對各個屬性的值
            return x.currency == y.currency
                && x.account_name == y.account_name
                && decimal.Parse(x.bet_amt) == decimal.Parse(y.bet_amt)
                && decimal.Parse(x.payout_amt) == decimal.Parse(y.payout_amt)
                && x.round_id == y.round_id;
        }

        public int GetHashCode(BetHistory e)
        {
            //確認物件是否為空值
            if (Object.ReferenceEquals(e, null)) return 0;

            int parentBetId = e.account_name == null ? 0 : e.account_name.GetHashCode();
            int betId = e.round_id == null ? 0 : e.round_id.GetHashCode();

            //計算HashCode，因為是XOR所以要全部都是1才會回傳1，否則都會回傳0
            return parentBetId ^ betId;
        }
    }
}