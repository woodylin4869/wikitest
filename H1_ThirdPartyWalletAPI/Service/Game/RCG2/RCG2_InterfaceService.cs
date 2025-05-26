using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.DB.RCG2.DBResponse;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RCG2.Response;
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
using Platform = H1_ThirdPartyWalletAPI.Code.Platform;
using RCG2setup = H1_ThirdPartyWalletAPI.Model.Game.RCG2.RCG2;

namespace H1_ThirdPartyWalletAPI.Service.Game.RCG2
{
    public interface IRCG2InterfaceService : IGameInterfaceService
    {
        Task<BaseResponse<GetBetRecordListResponse>> CallRCG2Record(GetBetRecordListRequest request);
        Task<int> PostRcg2RecordDetail(List<RCG2BetRecord> recordData, string systemCode, string webId);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    }
    public class RCG2_InterfaceService : IRCG2InterfaceService
    {
        private readonly ILogger<RCG2_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IRcg2DBService _rcg2DBService;
        private readonly IGameReportDBService _gameReportDBService;

        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        private const int _limit = 2000;
        private readonly string _prefixKey;

        public RCG2_InterfaceService(ILogger<RCG2_InterfaceService> logger,
            ICommonService commonService,
            IWebHostEnvironment env,
            IGameApiService gameaApiService,
            ISummaryDBService summaryDBService,
            IDBService dbService,
            IRcg2DBService rcg2DBService,
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _env = env;
            _gameApiService = gameaApiService;
            _dbService = dbService;
            _summaryDBService = summaryDBService;
            _gameReportDBService = gameReportDBService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
            _rcg2DBService = rcg2DBService;
        }
        #region GameInterfaceService

        /// <summary>
        /// 建立會員
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!RCG2setup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new RCG_CreateOrSetUser()
                {
                    memberAccount = Config.OneWalletAPI.Prefix_Key + request.Club_id,
                    memberName = userData.Club_Ename,
                    stopBalance = -1,
                    betLimitGroup = request.GameConfig["betLimitGroup"],
                    openGameList = "ALL",
                    currency = RCG2setup.Currency[userData.Currency],
                    language = RCG2setup.lang.TryGetValue(request.GameConfig["lang"], out var lang) ? lang : RCG2setup.lang["en-US"],
                    h1SHIDString = request.GameConfig["unitHID"],
                };

                var response = await _gameApiService._RCG2API.CreateOrSetUser(req);
                if (response.msgId != 0) throw new Exception(response.message);

                var gameUser = new GamePlatformUser();
                gameUser.club_id = userData.Club_id;
                gameUser.game_user_id = req.memberAccount;
                gameUser.game_platform = Platform.RCG2.ToString();
                return gameUser;
            }
            catch (Exception ex)
            {
                _logger.LogError("RCG2建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "RCG2 " + ex.Message.ToString());
            }
        }

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
                var responseData = await _gameApiService._RCG2API.GetBalance(new RCG_GetBalance
                {
                    memberAccount = platform_user.game_user_id,
                });

                if (responseData.msgId != 0)
                {
                    throw new Exception(responseData.message);
                }
                Balance.Amount = responseData.data.balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("RCG2餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.RCG2);
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
                var req = new RCG_KickOut()
                {
                    memberAccount = platform_user.game_user_id
                };

                var response = await _gameApiService._RCG2API.KickOut(req);

                if (response.msgId != 0) throw new(response.message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RCG2登出會員失敗 Msg: {Message}", ex.Message);
            }
            return true;
        }
        public async Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
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

                var responseData = await _gameApiService._RCG2API.Deposit(new RCG_Deposit
                {
                    memberAccount = platform_user.game_user_id,
                    transactionId = RecordData.id.ToString(),
                    transctionAmount = RecordData.amount,
                });

                if (responseData.msgId != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("RCG2 Deposit: {Message}", responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RCG2 TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RCG2 Deposit: {Message}", ex.Message);
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
                var responseData = await _gameApiService._RCG2API.Withdraw(new RCG_Withdraw
                {
                    memberAccount = platform_user.game_user_id,
                    transactionId = RecordData.id.ToString(),
                    transctionAmount = RecordData.amount,
                });

                if (responseData.msgId != 0)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("RCG2 Withdraw : {ex}", responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RCG2 TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RCG2 Withdraw : {ex}", ex.Message);
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
            RCG2setup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            RCG_Login UrlRequest = new RCG_Login
            {
                memberAccount = platformUser.game_user_id,
                backUrl = request.GameConfig["lobbyURL"],
                lang = lang ?? RCG2setup.lang["en-US"],
                gameDeskID = request.GameConfig["deskid"]
            };

            //if (request.GameConfig.ContainsKey("lobbyURL"))
            //{
            //    UrlRequest.returnurl = request.GameConfig["lobbyURL"];
            //}

            try
            {
                var token_res = await _gameApiService._RCG2API.Login(UrlRequest);
                if (token_res.msgId != 0)
                {
                    throw new Exception(token_res.message);
                }
                return token_res.data.url.ToString();
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "RCG2: " + ex.Message.ToString());
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

            var Reuslt = await _gameApiService._RCG2API.GetTransactionLog(new RCG_GetTransactionLog
            {
                transactionId = transfer_record.id.ToString()
            });
            if (Reuslt.data.status == 200)
            {
                if (transfer_record.target == nameof(Platform.RCG2))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RCG2))
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
            else if (Reuslt.data.status == 402)
            {
                if (transfer_record.target == nameof(Platform.RCG2))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RCG2))
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
            GetBetRecord res = new();
            var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
            var reportDTPair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            var batRecords = new List<dynamic>();
            foreach (var reportDT in reportDTPair)
            {
                batRecords.AddRange(await _rcg2DBService.GetRcg2RecordsBySummary(reportDT, RecordReq.ReportTime, summary.Game_type, Config.OneWalletAPI.Prefix_Key + RecordReq.ClubId));
            }

            if (batRecords.Count() == 0)
            {
                foreach (var reportDT in reportDTPair)
                {
                    batRecords.AddRange(await _rcg2DBService.GetRcg2RecordsBySummary_Old(new()
                    {
                        summary_id = RecordReq.summary_id,
                        ReportTime = reportDT
                    }));
                }
            }
            batRecords = batRecords.OrderByDescending(e => e.reportDT).ToList();
            res.Data = batRecords.ToList();
            return res;
        }

        /// <summary>
        /// 取得遊戲注單明細-轉跳 含注區
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var requestData = new SingleRecordWithGameResultRequest()
            {
                RecordId = long.Parse(RecordDetailReq.record_id),
                ShowVideoRecord = false,
                Lang = RCG2setup.lang.TryGetValue(RecordDetailReq.lang, out var lang) ? lang : RCG2setup.lang["zh-CN"]
            };

            try
            {
                var responseData = await _gameApiService._RCG2API.SingleRecordWithGameResult(requestData);
                if (responseData.msgId != 0)
                {
                    _logger.LogInformation("RCG2 SingleRecordWithGameResultRequest 失敗 id:{id}, msgId: {msgId}, message: {message}", RecordDetailReq.record_id, responseData.msgId, responseData.message);
                    return (responseData.msgId).ToString() + ", " + responseData.message;
                }
                return responseData.data.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogInformation("RCG2 SingleRecordWithGameResultRequest 失敗 id:{id}, Ex: {Message}", RecordDetailReq.record_id, ex.Message.ToString());
                return ex.Message.ToString();
            }
        }

        /// <summary>
        /// 呼叫 RCG 拉單
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BaseResponse<GetBetRecordListResponse>> CallRCG2Record(GetBetRecordListRequest request)
        {
            return await _gameApiService._RCG2API.GetBetRecordList(request); ;
        }

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns></returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            // 起訖時間完全依照輸入秒數至3位 也不用特別調整 因reportDT有到毫秒3位
            DateTime StartTime = new(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, RepairReq.StartTime.Second, RepairReq.StartTime.Millisecond);
            DateTime EndTime = new(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, RepairReq.EndTime.Second, RepairReq.StartTime.Millisecond);

            var postResult = 0;

            var response = new GetBetRecordListByDateRangeRequest()
            {
                startDate = StartTime,
                endDate = EndTime,
                pageIndex = 1,
                pageSize = 1000
            };


            List<RCG2BetRecord> betRecords = new List<RCG2BetRecord>();
            var data = await _gameApiService._RCG2API.GetBetRecordListByDateRange(response);

            if (data.data.total != 0)
            {
                betRecords.AddRange(data.data.dataList);
                for (int i = 2; i < (double)data.data.total / 1000; i++)
                {
                    response.pageIndex = i;
                    data = await _gameApiService._RCG2API.GetBetRecordListByDateRange(response);
                    betRecords.AddRange(data.data.dataList);
                }

                int postRcg2Result = await PostRcg2RecordDetail(betRecords, data.data.systemCode, data.data.webId);
                postResult += postRcg2Result;
                #region 重產匯總帳
                await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
                #endregion 重產匯總帳   
            }

            return $"Game: RCG2 新增資料筆數: {postResult}";

            #region 拉尾號
            // 拉尾號
            // 注單往前找5天，避免漏單(時間由外面傳入5天x24=-120小時)
            // todo: 要是120小時前都沒單就...QQ
            //int offset = -120;
            //var dbBetList = await _rcg2DBService.GetRcg2RealIDByReportDT(StartTime.AddHours(offset), EndTime);

            //var firstOne = dbBetList
            //                .Where(x => x.reportDT <= StartTime) // reportDT < StartTime 或 reportDT <= StartTime (至少null可從這筆等號開始)
            //                .OrderBy(x => x.real_id)
            //                .FirstOrDefault();

            //if (firstOne == null)
            //{
            //    string errormsg = $"RCG2 Cannot find the previous data, it may be over {offset} hours.";
            //    _logger.LogWarning(errormsg);
            //    return errormsg;
            //}

            //var first = firstOne.real_id;

            //while (true)
            //{
            //    // 請求 RCG 拉單
            //    var RecordResponse = await _gameApiService._RCG2API.GetBetRecordList(new()
            //    {
            //        maxId = first,
            //        rows = _limit
            //    });

            //    var betRecords = RecordResponse.data.dataList
            //                                      .OrderBy(x => x.id)
            //                                      .ToList();
            //    if (betRecords.Count == 0) break;
            //    if (betRecords.All(x => x.reportDT > EndTime)) break;

            //    var list = betRecords
            //                         .Where(x => x.reportDT >= StartTime && x.reportDT <= EndTime)
            //                         .ToList();

            //    if (list.Any())
            //    {
            //        int postRcg2Result = await PostRcg2Record(RecordResponse.data.dataList, RecordResponse.data.systemCode, RecordResponse.data.webId);
            //        postResult += postRcg2Result;
            //    }

            //    first = betRecords.Max(x => x.id);
            //}

            #endregion 拉尾號
        }

        public async Task<ResCodeBase> SetLimit(SetLimitReq request, GamePlatformUser gameUser, Wallet memberWalletData)
        {

            var Limitdata = System.Text.Json.JsonSerializer.Serialize(request.bet_setting);
            var setLimit = System.Text.Json.JsonSerializer.Deserialize<SetLimit>(Limitdata);

            var ForwardGameReq = new ForwardGameReq()
            {
                Club_id = request.Club_id,
                Platform = request.Platform,
                GameConfig = new Dictionary<string, string>
                {
                    {"betLimitGroup",setLimit.betLimitGroup},
                    {"unitHID",setLimit.unitHID},
                    {"lang","en-US"},
                }
            };
            var res = new ResCodeBase();
            var Rcg2res = await CreateGameUser(ForwardGameReq, memberWalletData);
            if (gameUser == null)
            {
                await _commonService._gamePlatformUserService.PostGamePlatformUserAsync(Rcg2res);
            }

            res.code = (int)ResponseCode.Success;
            res.Message = MessageCode.Message[(int)ResponseCode.Success];
            return res;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._RCG2API.HelloWorld();
        }

        #endregion

        #region GameRecordService

        public async Task<int> PostRcg2RecordDetail(List<RCG2BetRecord> betInfos, string systemCode, string webId)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!betInfos.Any()) return 0;

            betInfos = betInfos.Where(x => x.memberAccount.ToLower().StartsWith(_prefixKey.ToLower())).ToList();

            // 取得現有已存在注單編號
            var existsPK = (await _rcg2DBService.GetRcg2RealIDByReportDT(betInfos.Min(b => b.reportDT), betInfos.Max(b => b.reportDT)))
                .Select(b => new { id = b.real_id })
                .ToHashSet();

            var postResult = 0;
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                var linqRes = betInfos.GroupBy(x => x.memberAccount);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            //var clubId = group.Key[_prefixKey.Length..];
                            //Wallet memberWalletData = await GetWalletCache(clubId);
                            //if (memberWalletData == null || memberWalletData.Club_id == null)
                            //{
                            //    throw new Exception("沒有會員id");
                            //}

                            // 紀錄 reportTime 跟 reportDT(結算時間) 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                            // 注單明細
                            List<RCG2BetRecord> betDetailData = new List<RCG2BetRecord>();
                            // t_summary_bet_record_mapping
                            var summaryBetRecordMappings = new HashSet<t_summary_bet_record_mapping>();
                            foreach (RCG2BetRecord item in group)
                            {
                                // 排除重複注單
                                if (!existsPK.Add(new { item.id })) continue;

                                item.systemCode = systemCode;
                                item.webId = webId;

                                item.Create_time = dt;
                                item.Report_time = reportTime;
                                item.Partition_time = item.reportDT;
                                await Calculate(conn, tran, item);

                                betDetailData.Add(item);
                                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }
                                dic[summaryTime].Add(item.reportDT.ToString("yyyy-MM-dd HH:mm"));
                            }
                            _logger.LogDebug("insert RCG2 record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);


                            if (betDetailData.Count > 0)
                            {
                                postResult += await _rcg2DBService.PostRcg2Record(conn, tran, betDetailData);
                            }
                            await tran.CommitAsync();

                            // 記錄到 Redis reportTime 跟 reportDT(結算時間) 的關聯
                            foreach (var item in dic)
                            {
                                var key = $"RCG2{RedisCacheKeys.BetSummaryTime}:{item.Key}";
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

                            _logger.LogError("Run RCG2 record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                        }
                    }
                }

                await conn.CloseAsync();
            }

            return postResult;
        }


        /// <summary>
        /// 小時帳 遊戲廠商
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            // nothing
            await Task.Delay(1);
        }

        /// <summary>
        /// 小時帳 w1
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
                _logger.LogDebug("Create RCG2 game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _rcg2DBService.SumRCG2BetRecordByReportDT(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.RCG2);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin + totalBetValid;
                reportData.total_netwin = totalWin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }


        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, RCG2BetRecord r)
        {
            // 真人注單有改牌邏輯，需先儲存原始資料
            r.real_id = r.id;
            r.pre_bet = r.bet;
            r.pre_available = r.available;
            r.pre_winlose = r.winLose;
            r.pre_status = r.status;
            // rootRecordId 有值則有改單
            if (r.rootRecordId != -1)
            {
                // RCG情況特殊 當下改單是刪除當下的單並產生新單 若遇改單時 則注單編號還是採用 改單前的最初注單編號
                r.id = r.rootRecordId;
            }
            switch (r.status)
            {
                case (int)RCG2setup.BetStatusEnum.Cancel:
                case (int)RCG2setup.BetStatusEnum.Change:
                    // 優先嘗試從主要來源獲取舊紀錄
                    var oldRecords = await _rcg2DBService.GetRcg2PreRecordById(tran, r.id, r.reportDT)
                 ?? await _rcg2DBService.GetRcg2PreRecordById_old(tran, r.id, r.reportDT)
                 ?? new List<RCG2PreRecordDBResponse>();

                    if (oldRecords.Any(oldr => new { oldr.id, oldr.originRecordId, oldr.rootRecordId }.Equals(new { r.id, r.originRecordId, r.rootRecordId })))
                    {
                        return;
                    }
                    if (oldRecords.Any())
                    {
                        // max(real_id)
                        var lastRecord = oldRecords.OrderByDescending(r => r.real_id).First(); //僅需沖銷最後一筆即可
                        r.bet = r.bet - lastRecord.pre_bet;
                        r.available = r.available - lastRecord.pre_available;
                        r.winLose = r.winLose - lastRecord.pre_winlose;
                    }
                    break;
            }
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
            return PlatformType.Live;
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
            var sw2 = System.Diagnostics.Stopwatch.StartNew();


            // TODO: RecordSummary需要移除DateTime startTime, DateTime endTime，Schedule 跟著把DateTime startTime, DateTime endTime 邏輯處理刪除
            // 取得匯總需要的起始和結束時間
            (DateTime StartTime, DateTime EndTime) = await GetRecordSummaryDateTime(reportDatetime);
            startTime = StartTime;
            endTime = EndTime;

            var summaryRecords = await _rcg2DBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.RCG2);
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
            sw2.Stop();
            _logger.LogInformation("RCG2 summary record 寫入完成時間 {time}, 五分鐘匯總帳時間: {reporttime}, 開始時間: {starttime} 結束時間: {endtime}",
                 sw2.ElapsedMilliseconds,
                 reportDatetime,
                 startTime.ToString("yyyy-MM-dd HH:mm"),
                 endTime.ToString("yyyy-MM-dd HH:mm"));

            return true;
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
            var redisKey = $"RCG2{RedisCacheKeys.BetSummaryTime}:{reportTime.ToString("yyyy-MM-dd HH:mm")}";

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

        /// <summary>
        /// 使用情境：後彙總排程從遊戲明細查詢使用者遊戲帳號 轉換 為H1的Club_Id 提供 wallet 查詢使用到
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private string ConvertGamePlatformUserToClubInfo(string propertyValue)
        {
            return propertyValue.Substring(3).ToUpper();
        }

    }
}
