using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Exceptions;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Request;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IJdbInterfaceService : IGameInterfaceService
    {
        public Task<ResCodeBase> PostJdbRecordDetail(List<CommonBetRecord> jdbBetRecord);

        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);

        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    }

    public class JDB_Service : IJdbInterfaceService
    {
        private readonly ILogger<JDB_Service> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IWebHostEnvironment _env;
        private readonly IGameApiService _gameApiService;
        private readonly IJdbDBService _jdbDBService;
        private readonly IGameReportDBService _gameReportDBService;

        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;

        public JDB_Service(ILogger<JDB_Service> logger,
            ICommonService commonService,
            IWebHostEnvironment env,
            IGameApiService gameApiService,
            IJdbDBService jdbDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _env = env;
            _gameApiService = gameApiService;
            _summaryDBService = summaryDBService;
            _jdbDBService = jdbDBService;
            _gameReportDBService = gameReportDBService;
        }

        #region GameInterfaceService

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            QueryPlayerRequest req = new QueryPlayerRequest();
            req.Uid = platform_user.game_user_id;
            MemberBalance Balance = new MemberBalance();
            try
            {
                QueryPlayerResponse res = await _gameApiService._JdbAPI.Action15_QueryPlayer(req);

                Balance.Amount = res.Data.SingleOrDefault().Balance;
            }
            catch (JDBBadRequestException ex)
            {
                Balance.Amount = 0;
                Balance.code = int.Parse(ex.status);
                Balance.Message = ex.Message;
                _logger.LogError("Jdb餘額取得失敗 Code:{errorCode} Msg: {Message}", ex.status, ex.err_text);
            }
            catch (TaskCanceledException ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.TimeOut;
                Balance.Message = MessageCode.Message[(int)ResponseCode.TimeOut];
                _logger.LogError("Jdb餘額取得失敗 Msg: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Jdb餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.JDB);
            return Balance;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                KickOutRequest reqData = new KickOutRequest();
                reqData.Uid = platform_user.game_user_id;
                await _gameApiService._JdbAPI.Action17_KickOut(reqData);
            }
            catch (JDBBadRequestException ex)
            {
                _logger.LogInformation("踢出JDB使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.err_text);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            try
            {
                KickoutOfflineUsersRequest req = new KickoutOfflineUsersRequest();
                await _gameApiService._JdbAPI.Action58_KickOutOfflineUsers(req);
                return true;
            }
            catch (JDBBadRequestException ex)
            {
                _logger.LogError("kick all jdb user fail status : {status}  MSG : {Message}", ex.status, ex.err_text);
                throw new ExceptionMessage((int)ResponseCode.KickUserFail, MessageCode.Message[(int)ResponseCode.KickUserFail]);
            }
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            try
            {
                var transfer_amount = RecordData.amount;
                DepositOrWithdrawRequest requestData = new DepositOrWithdrawRequest();
                requestData.Uid = platform_user.game_user_id;
                requestData.SerialNo = RecordData.id.ToString("N");
                requestData.allCashOutFlag = "0"; //0: 不全部提领（默认值） 1: 全部提领（包含所有小数字金额
                requestData.amount = transfer_amount;
                await _gameApiService._JdbAPI.Action19_DepositOrWithdraw(requestData);
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferJdbTimeOut Msg: {Message}", ex.Message);
            }
            catch (JDBBadRequestException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("FundTransferInJdbFail Msg: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInJdbFail Msg: {Message}", ex.Message);
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
                DepositOrWithdrawRequest requestData = new DepositOrWithdrawRequest();
                requestData.Uid = platform_user.game_user_id;
                requestData.SerialNo = RecordData.id.ToString("N");
                requestData.allCashOutFlag = "0"; //0: 不全部提领（默认值） 1: 全部提领（包含所有小数字金额
                requestData.amount = -game_balance;
                await _gameApiService._JdbAPI.Action19_DepositOrWithdraw(requestData);
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (JDBBadRequestException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("FundTransferJdbFail Msg: {Message}", ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferJdbTimeOut Msg: {Message}", ex.Message);
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
                    result = "DEV" + propertyValue;
                    break;

                case "uat":
                    result = "UAT" + propertyValue;
                    break;

                case "prd":
                    result = propertyValue;
                    break;

                default:
                    throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);
            }

            return result;
        }

        /// <summary>
        /// W1 t_jdb_bet_record GamePlatformUser 轉換 Club Info 屬性規則
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
            switch (Config.OneWalletAPI.Prefix_Key.ToLower().Trim())
            {
                case "local":
                case "dev":
                    result = propertyValue.Substring(3);
                    break;

                case "uat":
                    result = propertyValue.Substring(prefixLength);
                    break;

                case "prd":
                    result = propertyValue;
                    break;

                default:
                    throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);
            }

            return result.ToUpper();
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            try
            {
                if (!Model.Game.JDB.JDB.Currency.ContainsKey(userData.Currency))
                {
                    throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
                }
                ForwardGame res = new ForwardGame();
                //Step 1 Create Member
                CreatePlayerRequest requestData = new CreatePlayerRequest();
                switch (Config.OneWalletAPI.Prefix_Key) //依照環境變數調整Prefix
                {
                    case "dev":
                        requestData.Name = "DEV" + userData.Club_Ename;
                        requestData.Uid = "DEV" + userData.Club_id;
                        break;

                    case "uat":
                        requestData.Name = "UAT" + userData.Club_Ename;
                        requestData.Uid = "UAT" + userData.Club_id;
                        break;

                    case "prd":
                        requestData.Name = userData.Club_Ename;
                        requestData.Uid = userData.Club_id;
                        break;

                    default:
                        throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);
                }
                try
                {
                    await _gameApiService._JdbAPI.Action12_CreatePlayer(requestData);
                }
                catch (JDBBadRequestException ex)
                {
                    if (ex.status != "7602")
                    {
                        throw new JDBBadRequestException(ex.status, ex.err_text);
                    }
                }
                var gameUser = new GamePlatformUser();
                gameUser.club_id = userData.Club_id;
                gameUser.game_user_id = requestData.Uid;
                gameUser.game_platform = request.Platform;
                return gameUser;
            }
            catch (JDBBadRequestException ex)
            {
                _logger.LogError("Forward JDB exception EX : {status}  MSG : {Message} ", ex.status, ex.err_text);
                throw new ExceptionMessage((int)ResponseCode.CreateJdbUserFail, MessageCode.Message[(int)ResponseCode.CreateJdbUserFail] + "|" + ex.err_text);
            }
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            try
            {
                //Step 3 Get Game URL
                GetTokenRequest TokenRequest = new GetTokenRequest();
                TokenRequest.Uid = platformUser.game_user_id;
                if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && Model.Game.JDB.JDB.lang.ContainsKey(request.GameConfig["lang"]))
                {
                    TokenRequest.Lang = Model.Game.JDB.JDB.lang[request.GameConfig["lang"]];
                }
                else
                {
                    TokenRequest.Lang = Model.Game.JDB.JDB.lang["en-US"];
                }
                if (request.GameConfig.ContainsKey("gType"))
                {
                    TokenRequest.gType = request.GameConfig["gType"];
                }
                if (request.GameConfig.ContainsKey("mType"))
                {
                    TokenRequest.mType = request.GameConfig["mType"];
                }
                if (request.GameConfig.ContainsKey("lobbyURL"))
                {
                    TokenRequest.lobbyURL = request.GameConfig["lobbyURL"];
                }
                TokenRequest.windowMode = "2"; //1.有大廳 2.無大廳

                GetTokenResponse token_res = await _gameApiService._JdbAPI.Action11_GetToken(TokenRequest);
                return token_res.Path;
            }
            catch (JDBBadRequestException ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Forward JDB exception EX : {status}  MSG : {Message} ", ex.status, ex.err_text);
                throw new ExceptionMessage((int)ResponseCode.CreateJdbUserFail, MessageCode.Message[(int)ResponseCode.CreateJdbUserFail] + "|" + ex.err_text);
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            GetCashTransferRecordRequest requestData = new GetCashTransferRecordRequest();
            requestData.serialNo = transfer_record.id.ToString("N");
            try
            {
                var JdbResult = await _gameApiService._JdbAPI.Action55_GetCashTransferRecord(requestData);
                if (transfer_record.target == nameof(Platform.JDB))//轉入JDB直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.JDB))
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
            catch (JDBBadRequestException ex)
            {
                if (ex.status == "9015") //data not found
                {
                    if (transfer_record.target == nameof(Platform.JDB))//轉入JDB直接改訂單狀態為失敗
                    {
                        CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                    else if (transfer_record.source == nameof(Platform.JDB))
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
                else
                {
                    throw new ExceptionMessage((int)ResponseCode.FundTransferJdbFail, MessageCode.Message[(int)ResponseCode.FundTransferJdbFail] + "|" + ex.err_text);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("FundTransferJdbTimeOut Msg: {Message}", ex.Message);
                transfer_record.status = nameof(WalletTransferRecord.TransferStatus.pending);
            }
            CheckTransferRecordResponse.TRecord = transfer_record;
            return CheckTransferRecordResponse;
        }

        /// <summary>
        /// 取得遊戲第二層注單明細
        /// </summary>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var cacheSeconds = 300;
            var key = $"{RedisCacheKeys.JdbGetBetRecords}:{RecordReq.summary_id}:{RecordReq.Platform}:{RecordReq.ReportTime.ToString("yyyy-MM-dd")}";
            GetBetRecord res = new GetBetRecord();

            var data = await _commonService._cacheDataService.GetOrSetValueAsync(key, async () =>
            {
                var jdb_results = new List<GetJdbRecordBySummaryResponse>();
                var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);

                if (summary != null)
                {
                    var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();

                    summary.Club_id = ConvertClubInfoToGamePlatformUser(summary.Club_id);

                    if (createtimePair.Length > 0)
                    {
                        var tasks = createtimePair.Select(createTime =>
                            _jdbDBService.GetJdbRecordByReportTime(createTime, createTime.AddDays(1), RecordReq.ReportTime, summary.Club_id)
                        ).ToList();

                        var results = await Task.WhenAll(tasks);
                        jdb_results.AddRange(results.SelectMany(r => r));

                        if (jdb_results.Any())
                        {
                            jdb_results = jdb_results.OrderByDescending(e => e.gamedate).ToList();
                        }
                    }
                }

                return jdb_results;
            }, cacheSeconds);

            res.Data = data?.Select(x => (dynamic)x).ToList();
            return res;
        }

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            GetGameResultRequest reqDataJDB = new GetGameResultRequest();

            var resJdbRecord = await _jdbDBService.GetJdbRecordByReportTime(RecordDetailReq.ReportTime, RecordDetailReq.record_id);

            if (resJdbRecord == null)
                return string.Empty;

            reqDataJDB.uid = resJdbRecord.playerid;
            reqDataJDB.gType = resJdbRecord.gtype;
            reqDataJDB.historyId = resJdbRecord.historyid;
            if (Model.Game.JDB.JDB.lang.ContainsKey(RecordDetailReq.lang))
            {
                reqDataJDB.lang = Model.Game.JDB.JDB.lang[RecordDetailReq.lang];
            }
            else
            {
                reqDataJDB.lang = Model.Game.JDB.JDB.lang["en-US"];
            }
            //reqData.trans_id = Convert.ToInt64(RecordDetailReq.record_id);
            GetGameResultResponse resDataJDB = await _gameApiService._JdbAPI.Action54_GetGameResult(reqDataJDB);
            return JsonConvert.SerializeObject(resDataJDB);
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            //JDB只接受到分鐘參數

            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddSeconds(-RepairReq.EndTime.Second).AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 5)
            {
                endTime = startTime.AddMinutes(5);
                _logger.LogDebug("Repair Jdb record start Time : {startTime} end Time : {endTime}", startTime, endTime);
                RepairCount += await RepairJdb(startTime, endTime);
                startTime = endTime;
                await Task.Delay(5000);
            }

            _logger.LogDebug("Repair Jdb record start Time : {startTime} end Time : {endTime}", startTime, RepairReq.EndTime);
            RepairCount += await RepairJdb(startTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);

            return returnString;
        }

        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            var swOuter = System.Diagnostics.Stopwatch.StartNew();

            // TODO: RecordSummary需要移除DateTime startTime, DateTime endTime，Schedule 跟著把DateTime startTime, DateTime endTime 邏輯處理刪除
            // 取得匯總需要的起始和結束時間
            (DateTime StartTime, DateTime EndTime) = await GetRecordSummaryDateTime(reportDatetime);
            startTime = StartTime;
            endTime = EndTime;

            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _jdbDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            // 取得遊戲內帳號轉為為Club_id集合
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
                summaryData.Turnover = Math.Abs(summaryRecord.Sum(x => x.bet));
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.JDB);
                summaryData.Game_type = 0;
                //summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = Math.Abs(summaryRecord.Sum(x => x.bet));
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.netwin);
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

            swOuter.Stop();
            _logger.LogInformation("jdb summary record 寫入完成時間 {time}, 五分鐘匯總帳時間: {reporttime}, 開始時間: {starttime} 結束時間: {endtime}",
                swOuter.ElapsedMilliseconds,
                reportDatetime.ToString("yyyy/mm/dd HH:mm:ss"),
                startTime.ToString("yyyy-MM-dd HH:mm"),
                endTime.ToString("yyyy-MM-dd HH:mm"));

            return true;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._JdbAPI.Action49_GetGameList(new());
        }

        #endregion GameInterfaceService

        #region GameRecordService

        public async Task<ResCodeBase> PostJdbRecordDetail(List<CommonBetRecord> jdbBetRecord)
        {
            ResCodeBase res = new ResCodeBase();
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                var Chucklist = jdbBetRecord.Chunk(20000);
                foreach (IEnumerable<CommonBetRecord> group in Chucklist)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var betDetailData = new List<CommonBetRecord>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                        // 紀錄 reportTime 跟 playTime 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();

                        foreach (CommonBetRecord r in group)//loop club id bet detail
                        {
                            r.report_time = reportTime;
                            betDetailData.Add(r);

                            // 紀錄 reportTime 跟 partition column lastmodifytime 的關聯
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }

                            dic[summaryTime].Add(r.lastModifyTime.ToString("yyyy-MM-dd HH:mm"));
                        }

                        try
                        {
                            int PostRecordResult = await _jdbDBService.PostJdbRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();

                            // 記錄到 Redis reportTime 跟 playTime 的關聯
                            foreach (var item in dic)
                            {
                                var key = $"{RedisCacheKeys.JdbBetSummaryTime}:{item.Key}";
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
                        _logger.LogDebug("JdbRecordSchedule 寫入{count}筆資料時間 : {time} MS", betDetailData.Count, sw.ElapsedMilliseconds);
                    }
                }
                await conn.CloseAsync();
            }

            return res;
        }

        public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
        {
            //校正成-4時區
            startDateTime = startDateTime.AddHours(-12);
            endDateTime = endDateTime.AddHours(-12);
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                DateTime reportDate = reportTime;
                IEnumerable<dynamic> dailyReport = await _jdbDBService.SumJdbBetReportDaily(reportDate);
                var DailyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.JDB);
                reportData.report_datetime = reportDate;
                reportData.report_type = (int)GameReport.e_report_type.FinancalReport;
                reportData.total_bet = DailyReportData.total_bet == null ? 0 : Math.Abs(DailyReportData.total_bet);
                reportData.total_win = DailyReportData.total_win == null ? 0 : DailyReportData.total_win;
                reportData.total_netwin = DailyReportData.total_netwin == null ? 0 : DailyReportData.total_netwin;
                reportData.total_count = DailyReportData.total_cont == null ? 0 : DailyReportData.total_cont;
                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);
                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }

        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            //校正成-4時區
            startDateTime = startDateTime.AddHours(-12);
            endDateTime = endDateTime.AddHours(-12);
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                DateTime reportDate = reportTime;
                IEnumerable<dynamic> dailyReport = await _jdbDBService.SumJdbBetRecordDaily(reportDate);
                var DailyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.JDB);
                reportData.report_datetime = reportDate;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = DailyReportData.total_bet == null ? 0 : Math.Abs(DailyReportData.total_bet);
                reportData.total_win = DailyReportData.total_win == null ? 0 : DailyReportData.total_win;
                reportData.total_netwin = DailyReportData.total_netwin == null ? 0 : DailyReportData.total_netwin;
                reportData.total_count = DailyReportData.total_cont == null ? 0 : DailyReportData.total_cont;
                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);
                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }

        #endregion GameRecordService

        private BetRecordSummary Calculate(BetRecordSummary SummaryData, CommonBetRecord r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Math.Abs(r.bet);
            SummaryData.Turnover += Math.Abs(r.bet);
            SummaryData.Netwin += r.total;
            SummaryData.Win += r.win;
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

        private async Task<int> RepairJdb(DateTime startTime, DateTime endTime)
        {
            // 根據時間判斷呼叫 API
            List<CommonBetRecord> data = await GetGameRecordsAsync(startTime, endTime);

            // 取得資料庫中的現有紀錄
            var jdbRecordV2 = await _jdbDBService.GetJdbRecordV2ByTime(startTime, endTime);

            // 直接轉成 HashSet 減少重複轉換
            var existsSeqNo = jdbRecordV2?.Select(r => r.historyid).ToHashSet() ?? new HashSet<string>();

            // 篩選出需要補的紀錄
            var repairRecords = data.Where(r => !existsSeqNo.Contains(r.historyId)).ToList();

            // 若有新的紀錄則進行資料補充
            if (repairRecords.Count > 0)
            {
                await PostJdbRecordDetail(repairRecords);
            }

            return repairRecords.Count;
        }

        // 抽取 API 呼叫邏輯，根據時間來選擇呼叫的 API
        private async Task<List<CommonBetRecord>> GetGameRecordsAsync(DateTime startTime, DateTime endTime)
        {
            if (DateTime.Now.Subtract(startTime).Hours >= 2)
            {
                var req = new GetGameHistoryRequest { Starttime = startTime, Endtime = endTime };
                var result = await _gameApiService._JdbAPI.Action64_NoClassification(req);
                return result.Data;
            }
            else
            {
                var req = new GetGameBetRecordRequest { Starttime = startTime, Endtime = endTime };
                var result = await _gameApiService._JdbAPI.Action29_GetGameBetRecord_NoClassification(req);
                return result.Data;
            }
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
            var redisKey = $"{RedisCacheKeys.JdbBetSummaryTime}:{reportTime.ToString("yyyy-MM-dd HH:mm")}";

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
}