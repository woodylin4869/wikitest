using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.DS.Response;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Npgsql;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;

namespace H1_ThirdPartyWalletAPI.Service.Game.DS
{
    public class DS_RecordService : IDsInterfaceService
    {
        private readonly ILogger<DSApiServiceBase> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameApiService _gameApiService;
        private readonly IDsDBService _dsDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;

        private  readonly string _prefixKey = Config.OneWalletAPI.Prefix_Key;

        public DS_RecordService(ILogger<DSApiServiceBase> logger,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IGameApiService gameApiService,
            IDsDBService dsDBService,
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameApiService;
            _dsDBService = dsDBService;
            _summaryDBService = summaryDBService;
            _gameReportDBService = gameReportDBService;
        }

        #region GameInterfaceService

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            CheckBalanceRequest req = new CheckBalanceRequest();
            req.Account = platform_user.game_user_id;
            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _gameApiService._DsAPI.checkBalance(req);
                Balance.Amount = Convert.ToDecimal(res.balance);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Ds餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.DS);
            return Balance;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var reqData = new LogoutRequest();
                reqData.account = platform_user.game_user_id;
                await _gameApiService._DsAPI.Logout(reqData);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出DS使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }

        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            try
            {
                var transfer_amount = RecordData.amount;
                var requestData = new TransferRequest();
                requestData.serial = RecordData.id;
                requestData.account = platform_user.game_user_id;
                requestData.amount = transfer_amount.ToString();
                requestData.oper_type = TransactionType.Deposit;
                var responseData = await _gameApiService._DsAPI.Transfer(requestData);
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("DS TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("DS TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInDsFail Msg: {Message}", ex.Message);
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
                var requestData = new TransferRequest();
                requestData.serial = RecordData.id;
                requestData.account = platform_user.game_user_id;
                requestData.amount = game_balance.ToString();
                requestData.oper_type = TransactionType.Withdraw;
                var responseData = await _gameApiService._DsAPI.Transfer(requestData);
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("DS TransferOut Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("DS TransferOut Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("DS TransferOut fail ex : {ex}", ex.Message);
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
            }
            return RecordData.status;
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
            switch (Config.OneWalletAPI.Prefix_Key.ToLower().Trim())
            {
                case "local":
                case "dev":
                case "uat":
                case "prd":
                    result = propertyValue;
                    break;

                default:
                    throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);
            }

            return result;
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
            switch (Config.OneWalletAPI.Prefix_Key.ToLower().Trim())
            {
                case "local":
                case "dev":
                case "uat":
                case "prd":
                    result = propertyValue;
                    break;

                default:
                    throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);
            }

            return result;
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.DS.DS.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            ForwardGame res = new ForwardGame();
            //Step 1 Create Member
            CreateMemberRequest requestData = new CreateMemberRequest();
            requestData.Account = userData.Club_id;
            try
            {
                var result = await _gameApiService._DsAPI.CreateMember(requestData);
                var gameUser = new GamePlatformUser();
                gameUser.club_id = userData.Club_id;
                gameUser.game_user_id = requestData.Account;
                gameUser.game_platform = request.Platform;
                return gameUser;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.CreateDsUserFail, MessageCode.Message[(int)ResponseCode.CreateDsUserFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 3 Get Game URL
            var UrlRequest = new LoginGameRequest();
            UrlRequest.Account = platformUser.game_user_id;
            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.Backurl = request.GameConfig["lobbyURL"];
            }
            if (request.GameConfig.ContainsKey("gameCode"))
            {
                UrlRequest.Game_id = request.GameConfig["gameCode"];
            }
            if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && Model.Game.DS.DS.lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.Lang = Model.Game.DS.DS.lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.Lang = Model.Game.DS.DS.lang["en-US"];
            }
            try
            {
                var token_res = await _gameApiService._DsAPI.LoginGame(UrlRequest);
                return token_res.url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            Model.Game.DS.Request.VerifyRequest DsreqData = new Model.Game.DS.Request.VerifyRequest();
            DsreqData.Serial = transfer_record.id;
            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(transfer_record.Club_id, Platform.DS);
            DsreqData.Account = gameUser.game_user_id;
            var DsReuslt = await _gameApiService._DsAPI.verify(DsreqData);
            if (DsReuslt.result.code == (int)Model.Game.DS.error_code.succeeded)
            {
                if (transfer_record.target == nameof(Platform.DS))//轉入DS直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.DS))
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
            else if (DsReuslt.result.code == (int)Model.Game.DS.error_code.information_not_found)
            {
                if (transfer_record.target == nameof(Platform.DS))//轉入DS直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.DS))
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
        /// 第二層明細
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var batRecords = new List<dynamic>();
            GetBetRecord res = new();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _dsDBService.GetDsRecordByReportTimeV2(createTime, RecordReq.ReportTime, RecordReq.ClubId);
                foreach (var result in results)
                {
                    batRecords.Add(result);
                }
            }

            if (batRecords.Count == 0)
            {
                batRecords.AddRange(await _dsDBService.GetDsRecordBySummary(RecordReq));
            }

            res.Data = batRecords.OrderByDescending(e => e.finish_at).ToList();
            return res;
        }
        #region backup
        //{

        //    //GetBetRecord res = new GetBetRecord();
        //    //IEnumerable<dynamic> ds_results = await _dsDBService.GetDsRecordBySummary(RecordReq);
        //    //ds_results = ds_results.OrderByDescending(e => e.finish_at);
        //    //res.Data = ds_results.ToList();
        //    //return res;

        //    var cacheSeconds = 300;
        //    var key = $"{RedisCacheKeys.DsGetBetRecords}:{RecordReq.summary_id}:{RecordReq.Platform}:{RecordReq.ReportTime.ToString("yyyy-MM-dd HH:mm:ss")}";

        //    GetBetRecord res = new GetBetRecord();

        //    var data = await _commonService._cacheDataService.GetOrSetValueAsync(key, async () =>
        //    {
        //        var ds_results = await _dsDBService.GetDsRecordBySummary(RecordReq);

        //        // 第二層明細舊表
        //        if (ds_results != null && ds_results.Any())
        //        {
        //            ds_results = ds_results.OrderByDescending(e => e.finish_at);
        //        }
        //        else
        //        {
        //            // 舊表沒有資料的話可能是在新表
        //            var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
        //            if (summary != null)
        //            {
        //                var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
        //                summary.Club_id = ConvertClubInfoToGamePlatformUser(summary.Club_id);
        //                if (createtimePair.Length == 0)//舊資料沒有分區對應，用ReportTime查詢
        //                {
        //                    ds_results = await _dsDBService.GetDsRecordByReportTime(summary);
        //                }
        //                else
        //                {
        //                    foreach (var createTime in createtimePair)
        //                    {
        //                        var results = await _dsDBService.GetDsRecordByReportTime( RecordReq.ReportTime, summary.Club_id);
        //                        results = results.OrderByDescending(e => e.partition_time).ToList();
        //                        foreach (var result in results)
        //                        {
        //                            ds_results = ds_results.Append(result);
        //                        }
        //                    }
        //                    if (ds_results != null && ds_results.Any())
        //                    {
        //                        ds_results = ds_results.OrderByDescending(e => e.finish_at);
        //                    }
        //                }
        //            }
        //        }

        //        return ds_results ?? new List<GetDSRecordsBySummaryReponse>();
        //    }, cacheSeconds);

        //    res.Data = data?.Select(x => (dynamic)x).ToList();
        //    return res;
        //}
        #endregion backup

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            //var record = await _dsDBService.GetDsRecord(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
                // TODO：先註解不用，確認不用拿掉
                #region 先註解不用，確認不用拿掉

                // 舊表沒有資料的話可能是在新表
                //var cacheSeconds = 300;
                //var key = $"{RedisCacheKeys.DsGameDetailURL}:{RecordDetailReq.summary_id}:{RecordDetailReq.ReportTime.ToString("yyyy-MM-dd HH:mm:ss")}";
                //// 第一層明細
                //var summary = await _commonService._cacheDataService.GetOrSetValueAsync(key, async () => await _summaryDBService.GetRecordSummaryById(new GetBetRecordReq()
                //{
                //    summary_id = RecordDetailReq.summary_id.ToString(),
                //    ReportTime = RecordDetailReq.ReportTime
                //}), cacheSeconds);

                // if (summary == null) return string.Empty;

                #endregion 先註解不用，確認不用拿掉
                var record = await _dsDBService.GetDsRecordByReportTime(RecordDetailReq.ReportTime, RecordDetailReq.record_id);

            if (record.Count() == 0)
            {
                record = await _dsDBService.GetDsRecordByReportTimeOld(RecordDetailReq.ReportTime, RecordDetailReq.record_id);
            }

            Model.Game.DS.Request.GetBetDetailPageRequest reqDataDS = new Model.Game.DS.Request.GetBetDetailPageRequest();
            reqDataDS.game_id = record.SingleOrDefault().game_id;
            reqDataDS.game_serial = record.SingleOrDefault().game_serial;
            if (Model.Game.DS.DS.lang.ContainsKey(RecordDetailReq.lang))
            {
                reqDataDS.lang = Model.Game.DS.DS.lang[RecordDetailReq.lang];
            }
            else
            {
                reqDataDS.lang = Model.Game.DS.DS.lang["en-US"];
            }

            GetBetDetailPageResponse resDataDS = await _gameApiService._DsAPI.GetBetDetailPage(reqDataDS);
            //res.Data = JsonConvert.SerializeObject(resDataDS);
            return resDataDS.url;
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 10)
            {
                endTime = startTime.AddMinutes(10);
                _logger.LogDebug("Repair Ds record start Time : {startTime} end Time : {endTime}", startTime, endTime);
                RepairCount += await RepairDs(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            _logger.LogDebug("Repair Ds record start Time : {startTime} end Time : {endTime}", startTime, RepairReq.EndTime);
            RepairCount += await RepairDs(startTime, RepairReq.EndTime);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
        /// <summary>
        /// 五分鐘匯總
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="reportDatetime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            // TODO: RecordSummary需要移除DateTime startTime, DateTime endTime，Schedule 跟著把DateTime startTime, DateTime endTime 邏輯處理刪除
            // 取得匯總需要的起始和結束時間
            (DateTime StartTime, DateTime EndTime) = await GetRecordSummaryDateTime(reportDatetime);
            startTime = StartTime;
            endTime = EndTime;
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _dsDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => ConvertGamePlatformUserToClubInfo(x.userid)).Distinct().ToList();
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
                summaryData.Turnover = Math.Abs(summaryRecord.Sum(x => x.bet_valid));
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.DS);
                summaryData.Bet_amount = Math.Abs(summaryRecord.Sum(x => x.bet));
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.win) - summaryRecord.Sum(x => x.bet) - summaryRecord.Sum(x => x.fee_amount);
                summaryData.updatedatetime = DateTime.Now;
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
            foreach (IEnumerable<BetRecordSummary> group in Chucklist)
            {
                await using NpgsqlConnection conn = new(Config.OneWalletAPI.DBConnection.BetLog.Master);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await conn.OpenAsync();
                await using (var tran = await conn.BeginTransactionAsync())
                {
                    await _summaryDBService.BatchInsertRecordSummaryAsync(conn, group.ToList());
                    await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMappings);
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
            return _gameApiService._DsAPI.GetGameInfoStateList();
        }

        #endregion GameInterfaceService

        #region GameRecordService
        
        /// <summary>
        /// 新增明細帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task PostDsRecordDetail(List<DSBetRecord> recordData)
        {
            var recordDataAgent = recordData.Where(l => l.agent == Config.CompanyToken.DS_AGENT);
            if (recordDataAgent.Count() < 1)
                return;

            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                var Chucklist = recordData.Chunk(20000);
                foreach (IEnumerable<DSBetRecord> group in Chucklist)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var betDetailData = new List<DSBetRecord>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                        // 紀錄 reportTime 跟 partition column finish_at 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();

                        foreach (DSBetRecord r in group)//loop club id bet detail
                        {
                            r.report_time = reportTime;
                            betDetailData.Add(r);

                            // 紀錄 reportTime 跟 partition column finish_at 的關聯
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }

                            dic[summaryTime].Add(r.finish_at.AddHours(8).ToString("yyyy-MM-dd HH:mm"));
                        }

                        try
                        {
                            int PostRecordResult = await _dsDBService.PostDsRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();

                            // 記錄到 Redis reportTime 跟 playTime 的關聯
                            foreach (var item in dic)
                            {
                                var key = $"{RedisCacheKeys.DsBetSummaryTime}:{item.Key}";
                                await _commonService._cacheDataService.SortedSetAddAsync(key,
                                    item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                            }
                        }
                        catch (ExceptionMessage ex)
                        {
                            await tran.RollbackAsync();
                            throw new ExceptionMessage(ex.MsgId, ex.Message);
                        }
                        finally
                        {
                            dic.Clear();
                        }

                        sw.Stop();
                        _logger.LogDebug("DsRecordSchedule 寫入{count}筆資料時間 : {time} MS", betDetailData.Count, sw.ElapsedMilliseconds);
                    }
                }
                await conn.CloseAsync();
            }
        }

        /// <summary>
        /// 新增 5 分鐘匯總帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        private async Task PostDsRecord_backup(List<DSBetRecord> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                //DS過濾單一Agent下的會員注單
                var recordDataAgent = recordData.Where(x => x.agent == Config.CompanyToken.DS_AGENT);
                if (recordDataAgent.Count() < 1)
                    return;
                await conn.OpenAsync();
                var linqRes = recordDataAgent.GroupBy(x => x.member);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            Wallet memberWalletData = await GetWalletCache(group.Key);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(group.Key, Platform.DS);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No ds user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData =
                                new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<DSBetRecord> betDetailData = new List<DSBetRecord>();

                            foreach (DSBetRecord r in group) //loop club id bet detail
                            {
                                // DS注單是UTC Time
                                r.bet_at = r.bet_at.ToLocalTime();
                                r.finish_at = r.finish_at.ToLocalTime();

                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.DS);
                                DateTime tempDateTime = r.finish_at;
                                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                sumData.ReportDatetime = tempDateTime;
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    //if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    //{
                                    //    DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                    //        DateTime.Now.Day, 12, 00, 0);
                                    //    if (sumData.ReportDatetime < ReportDateTime)
                                    //    {
                                    //        sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                    //        _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                    //    }
                                    //}
                                    //else
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.id);
                                        }
                                    }
                                }

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString()];
                                    //合併處理
                                    sumData = Calculate(sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results =
                                        await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    if (results.Count() == 0) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                    }

                                    summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
                                }

                                // r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }

                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }

                            int PostRecordResult = await _dsDBService.PostDsRecord(conn, tran, betDetailData);
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            _logger.LogDebug("insert DS record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (DSBetRecord r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.id, r.finish_at.ToString());
                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run ds record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }
                    }
                }

                await conn.CloseAsync();
            }
        }

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
                _logger.LogDebug("Create DS game provider report time {datetime}", reportTime);
                // 每小時投注匯總
                var result = await _gameApiService._DsAPI.GetAgentSummaryBetRecords(
                    new GetAgentSummaryBetRecordsRequest()
                    {
                        finish_time = new GetAgentSummaryBetRecordsRequest.FinishTime
                        {
                            start_time = reportTime,
                            end_time = reportTime.AddHours(1).AddSeconds(-1)
                        }
                    });
                // 只要當前的代理帳號
                // 匯總帳寫回 DB (雖然 API 回傳 List，但每次只查詢一小時，理論上只會有一筆資料)
                var target = result
                    .Rows
                    .SingleOrDefault(x => x.agent == Config.CompanyToken.DS_AGENT);

                // 沒有資料寫入空的匯總帳就結束排程
                if (target == null)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.DS),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = 0,
                        total_win = 0,
                        total_netwin = 0,
                        total_count = 0
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);

                    // 轉帳中心(轉帳中心的欄位格式)
                    var w1CenterEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.DS),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.GameBetRecord,
                        total_bet = 0,
                        total_win = 0,
                        total_netwin = 0,
                        total_count = 0
                    };

                    await _gameReportDBService.DeleteGameReport(w1CenterEmptyReport);
                    await _gameReportDBService.PostGameReport(w1CenterEmptyReport);

                    return;
                }

                // 遊戲商的每小時匯總報表(遊戲商的欄位格式)
                var data = new t_ds_game_report
                {
                    create_datetime = Convert.ToDateTime(reportTime),
                    agent = target.agent,
                    bet_count = target.bet_count,
                    bet_amount = target.bet_amount,
                    payout_amount = target.payout_amount,
                    valid_amount = target.valid_amount,
                    fee_amount = target.fee_amount,
                    jp_amount = target.jp_amount,
                };

                await _dsDBService.DeleteDsReport(data);
                await _dsDBService.PostDsReport(data);

                // 遊戲商的每小時匯總報表(轉帳中心的欄位格式)
                var dsSummaryReport = new GameReport
                {
                    platform = nameof(Platform.DS),
                    report_datetime = Convert.ToDateTime(reportTime),
                    report_type = (int)GameReport.e_report_type.FinancalReport,
                    total_bet = data.bet_amount,
                    total_win = data.payout_amount,
                    total_netwin = data.payout_amount - data.bet_amount - data.fee_amount,
                    total_count = long.Parse(data.bet_count)
                };
                await _gameReportDBService.DeleteGameReport(dsSummaryReport);
                await _gameReportDBService.PostGameReport(dsSummaryReport);
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
                _logger.LogDebug("Create DS game W1 report time {datetime}", reportTime);
                IEnumerable<dynamic> dailyReport = await _dsDBService.SumDsBetRecordHourly(reportTime);
                var HourlylyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.DS);
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

        private BetRecordSummary Calculate(BetRecordSummary SummaryData, DSBetRecord r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Math.Abs(r.bet_amount);
            SummaryData.Turnover += Math.Abs(r.valid_amount);
            SummaryData.Netwin += r.payout_amount - r.bet_amount - r.fee_amount;
            SummaryData.Win += r.payout_amount;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
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
        /// DS 帳務比對
        /// 1. 比對轉帳中心與遊戲商的匯總帳是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairDs(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的歷史下注紀錄
            var dsHistoryList = new List<DSBetRecord>();
            var isEnable = true;
            var pageIndex = 0;

            while (isEnable)
            {
                var betRecord = await _gameApiService._DsAPI.GetBetRecord(new GetBetRecordRequest()
                {
                    finish_time = new FinishTime
                    {
                        start_time = startTime,
                        end_time = endTime.AddSeconds(-1)
                    },
                    index = pageIndex,
                    limit = 5000
                });

                dsHistoryList.AddRange(betRecord.rows);

                if (dsHistoryList.Count >= Convert.ToInt32(betRecord.total))
                {
                    isEnable = false;
                }
                else
                {
                    pageIndex++;
                    await Task.Delay(5000); // 查詢注單限制 十秒內只允許五次查詢
                }
            }

            // 轉帳中心的歷史下注紀錄
            // 舊表資料
            var w1CenterList = await _dsDBService.SumDsBetRecord(startTime, endTime) ?? new List<t_ds_bet_record>();
            var convert = w1CenterList
                .Select(x => new DSBetRecord
                {
                    id = x.id,
                    agent = x.agent,
                    bet_amount = x.bet_amount,
                    payout_amount = x.payout_amount,
                    fee_amount = x.fee_amount,
                    status = x.status
                }).ToList();
            // 新表資料
            var w1CenterListV2 = await _dsDBService.SumDsBetRecordV2(startTime, endTime) ?? new List<t_ds_bet_record>();
            var convertV2 = w1CenterListV2
                .Select(x => new DSBetRecord
                {
                    id = x.id,
                    agent = x.agent,
                    bet_amount = x.bet_amount,
                    payout_amount = x.payout_amount,
                    fee_amount = x.fee_amount,
                    status = x.status
                }).ToList();

            // 合併資料
            convert.AddRange(convertV2);

            // 比對歷史下注紀錄
            var diffList = dsHistoryList.Except(convert, new DSBetRecordComparer()).ToList();

            // 注單明細補單、更新 5 分鐘匯總帳
            await PostDsRecordDetail(diffList);
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
            var redisKey = $"{RedisCacheKeys.DsBetSummaryTime}:{reportTime.ToString("yyyy-MM-dd HH:mm")}";

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

            return (startTime.Value, endTime.Value);
        }
    }

    /// <summary>
    /// DS 注單差異比對
    /// </summary>
    public class DSBetRecordComparer : IEqualityComparer<DSBetRecord>
    {
        public bool Equals(DSBetRecord x, DSBetRecord y)
        {
            //確認兩個物件的資料是否相同
            if (Object.ReferenceEquals(x, y)) return true;

            //確認兩個物件是否有任何資料為空值
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //這邊就依照個人需求比對各個屬性的值
            return x.id == y.id
                   && x.agent == y.agent
                   && x.bet_amount == y.bet_amount
                   && x.payout_amount == y.payout_amount
                   && x.fee_amount == y.fee_amount
                   && x.status == y.status;
        }

        public int GetHashCode(DSBetRecord e)
        {
            //確認物件是否為空值
            if (Object.ReferenceEquals(e, null)) return 0;

            //取得 id 欄位的HashCode
            int id = e.id == null ? 0 : e.id.GetHashCode();

            //取得 agent 欄位的HashCode
            int agent = e.agent == null ? 0 : e.agent.GetHashCode();

            //取得 bet_amount 欄位的HashCode
            int bet_amount = e.bet_amount == null ? 0 : e.bet_amount.GetHashCode();

            //計算HashCode，因為是XOR所以要全部都是1才會回傳1，否則都會回傳0
            return id ^ agent ^ bet_amount;
        }
    }
}