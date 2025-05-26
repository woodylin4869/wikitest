using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Request;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Request;
using H1_ThirdPartyWalletAPI.Model.Game.FC.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using KickOutRequest = H1_ThirdPartyWalletAPI.Model.Game.FC.Request.KickOutRequest;
using static Google.Rpc.Context.AttributeContext.Types;
using System.Security.Principal;

namespace H1_ThirdPartyWalletAPI.Service.Game.FC
{
    public interface IFCInterfaceService : IGameInterfaceService
    {
        Task<int> PostFcRecord(List<Record> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class FC_RecordService : IFCInterfaceService
    {
        private readonly ILogger<FC_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IDBService _dbService;
        private readonly IFCDBService _fcDBService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IFCApiService _apiService;
        private readonly IGameReportDBService _gameReportDBService;

        private readonly string _prefixKey;

        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public FC_RecordService(ILogger<FC_RecordService> logger,
            ICommonService commonService,
            IWebHostEnvironment env,
            IGameApiService gameaApiService,
            IDBService dbService,
            IFCDBService fcDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _apiService = gameaApiService._FcAPI;
            _dbService = dbService;
            _fcDBService = fcDBService;
            _summaryDBService = summaryDBService;
            _cacheService = commonService._cacheDataService;
            _gameReportDBService = gameReportDBService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
        }
        #region GameInterfaceService
        /// <summary>
        /// 縮短GUID長度
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string CutGuidTo30Characters(Guid guid)
        {
            return guid.ToString("N")[2..];
        }
        /// <summary>
        /// 判斷會員的環境
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private bool Whereprefixkey(string account)
        {
            return account.StartsWith(_prefixKey.Substring(0, 1));
        }
        public async Task<int> PostFcRecord(List<Record> recordData)
        {
            recordData = recordData.Where(l => Whereprefixkey(l.account)).ToList();

            var betdata = recordData.OrderBy(o => o.bdate).ToList();

            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                int retcount = 0;
                var Chucklist = betdata.Chunk(20000);
                foreach (var group in Chucklist)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var sw = System.Diagnostics.Stopwatch.StartNew();
                            var betDetailData = new List<Record>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            foreach (var r in group) //loop club id bet detail
                            {
                                r.report_time = reportTime;
                                //拉回的住單時間是 UTC-4 要轉換回UTC +8 +12小
                                r.bdate = r.bdate.AddHours(12);
                                r.partition_time = r.bdate;
                                betDetailData.Add(r);
                                // 紀錄 reportTime 跟 partition column bdate 的關聯
                                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }
                                dic[summaryTime].Add(r.bdate.ToString("yyyy-MM-dd HH:mm"));
                            }
                            int PostRecordResult = await _fcDBService.PostfcRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();
                            // 記錄到 Redis reportTime 跟 playTime 的關聯
                            foreach (var item in dic)
                            {
                                var key = $"{RedisCacheKeys.FcBetSummaryTime}:{item.Key}";
                                await _commonService._cacheDataService.SortedSetAddAsync(key,
                                    item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                            }
                            dic.Clear();
                            sw.Stop();
                            retcount += PostRecordResult;
                            _logger.LogDebug("FcRecordSchedule 寫入{count}筆資料時間 : {time} MS", betDetailData.Count,
                                sw.ElapsedMilliseconds);
                        }
                        catch (Exception ex)
                        {
                            foreach (var r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.recordID, r.bdate);
                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run fc record  exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                 ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }
                    }
                }
                return retcount;
            }
        }
        /// <summary>
        /// FC後匯總
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="reportDatetime"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _fcDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);
            // 取得遊戲內帳號轉為為Club_id集合
            var userSummaries = summaryRecords.GroupBy(s => s.userid);
            var userlist = userSummaries.Select(x => x.Key[_prefixKey.Length..]).Distinct().ToList();
            // 批次處理，每次1000筆
            var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
            {
                return (await _commonService._serviceDB.GetWallet(betch));
            }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);

            var summaryRecordList = new List<(BetRecordSummary summay, HashSet<t_summary_bet_record_mapping> mappings)>();
            foreach (var summaryRecord in userSummaries)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key[_prefixKey.Length..], out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = Math.Abs(summaryRecord.Sum(x => x.bet));
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.FC);
                summaryData.Game_type = 3;
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = Math.Abs(summaryRecord.Sum(x => x.bet));
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.netwin);
                summaryData.updatedatetime = DateTime.Now;

                var mapping = new HashSet<t_summary_bet_record_mapping>();
                foreach (var tickDateTime in summaryRecord.Select(s => s.bettime))
                {
                    mapping.Add(new()
                    {
                        summary_id = summaryData.id,
                        report_time = summaryData.ReportDatetime.Value,
                        partition_time = tickDateTime
                    });
                }
                summaryRecordList.Add((summaryData, mapping));
            }

            var Chucklist = summaryRecordList.Chunk(10000);
            foreach (var group in Chucklist)
            {
                await using NpgsqlConnection conn = new(Config.OneWalletAPI.DBConnection.BetLog.Master);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await conn.OpenAsync();
                await using (var tran = await conn.BeginTransactionAsync())
                {
                    await _summaryDBService.BatchInsertRecordSummaryAsync(conn, group.Select(c => c.summay).ToList());
                    await _summaryDBService.BulkInsertSummaryBetRecordMapping(tran, group.SelectMany(c => c.mappings));
                    await tran.CommitAsync();
                }
                await conn.CloseAsync();
                sw.Stop();
                _logger.LogDebug("寫入{count}筆資料時間 : {time} MS", group.Count(), sw.ElapsedMilliseconds);
            }
            return true;
        }
        /// <summary>
        /// 廠商小時帳
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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
                _logger.LogDebug("Create FC game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計
                GetCurrencyReportRequest req = new GetCurrencyReportRequest()
                {
                    Date = startDateTime.ToString("yyyy-MM-dd")
                };

                //取得這小時
                GetCurrencyReportResponse FCCenterList = await _apiService.GetCurrencyReport(req);
                if (FCCenterList.Round == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.FC),
                        report_datetime = DateTime.Parse(reportTime.ToString("yyyy-MM-dd")),
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = 0,
                        total_win = 0,
                        total_netwin = 0,
                        total_count = 0
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddDays(1);
                }
                else
                {
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.FC),
                        report_datetime = DateTime.Parse(reportTime.ToString("yyyy-MM-dd")),
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = FCCenterList.Bet,
                        total_win = FCCenterList.Win,
                        total_netwin = FCCenterList.Winlose,
                        total_count = FCCenterList.Round,
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddDays(1);
                }
                await Task.Delay(5000);
            }
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
                _logger.LogDebug("Create FC game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalNetWin) = await _fcDBService.SumfcBetRecordByBetTime(reportTime.AddHours(12), reportTime.AddHours(12).AddDays(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.FC);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalBetValid + totalNetWin;
                reportData.total_netwin = totalNetWin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            SearchMemberRequest req = new SearchMemberRequest();
            req.MemberAccount = platform_user.game_user_id;


            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _apiService.SearchMember(req);
                Balance.Amount = Convert.ToDecimal(res.Points);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("FC餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.FC);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var result = await _apiService.KickOut(new KickOutRequest()
                {
                    MemberAccount = platform_user.game_user_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("KickUser 踢出FC使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }
        public async Task<bool> KickAllUser(Platform platform)
        {
            try
            {
                var result = await _apiService.KickoutAll(new KickoutAllRequest()
                {

                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("KickAllUser fc user fail MSG : {Message}", ex.Message);
                throw new ExceptionMessage((int)ResponseCode.KickUserFail, MessageCode.Message[(int)ResponseCode.KickUserFail]);
            }
        }
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            #region STEP2: 發起存款

            var transfer_amount = RecordData.amount;
            var currency = walletData.Currency;

            //檢查幣別
            if (!Model.Game.FC.FC.Currency.ContainsKey(currency))
                throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

            try
            {
                var req = new SetPointsRequest
                {
                    MemberAccount = platform_user.game_user_id,
                    AllOut = 0,
                    Points = Math.Round(transfer_amount, 4),
                    TrsID = CutGuidTo30Characters(RecordData.id)
                };
                var responseData = await _apiService.SetPoints(req);
                if (responseData.Result != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage(responseData.Result, responseData.Result.ToString());
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);

            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FC TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("FC TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInFcFail Msg: {Message}", ex.Message);
            }

            #endregion

            return RecordData.status;
        }
        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var responseData = await _apiService.SetPoints(new SetPointsRequest
                {
                    MemberAccount = platform_user.game_user_id,
                    AllOut = 0,
                    Points = -Math.Round(transfer_amount, 4),
                    TrsID = CutGuidTo30Characters(RecordData.id)
                });

                if (responseData.Result != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage(responseData.Result, responseData.Result.ToString());
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FC TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("FC TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInFcFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.FC.FC.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            string Account = Config.OneWalletAPI.Prefix_Key + userData.Club_id;

            var req = new CreateMemberRequest()
            {
                MemberAccount = Account
            };
            //創建帳號
            try
            {
                var response = await _apiService.CreateMember(req);

                if (response.Result == (int)ErrorCodeEnum.Success || response.Result == (int)ErrorCodeEnum.Account_Already_Exists)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = Account;
                    gameUser.game_platform = request.Platform;
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.Result.ToString());
                }

            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message.ToString());
            }


        }
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);
            if(summary == null || summary.Game_type != 3)
            {
                return new GetBetRecord();
            }

            var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();

            var fc_results = new List<dynamic>();

            foreach (var partition in partitions)
            {
                fc_results.AddRange(await _fcDBService.GetFcBetRecords(summary, partition, partition.AddDays(1)));
            }

            var res = new GetBetRecord
            {
                Data = fc_results.OrderByDescending(e => e.bdate).Select(x => x).ToList()
            };

            return res;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 4 Get Game URL
            var requestData = new LoginRequest()
            {
                MemberAccount = platformUser.game_user_id,
                JackpotStatus = false,
                LoginGameHall = false
            };

            if (request.GameConfig.ContainsKey("gameCode"))
            {
                int tempGameID = 0;
                int.TryParse(request.GameConfig["gameCode"], out tempGameID);
                requestData.GameID = tempGameID;
            }

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                requestData.HomeUrl = request.GameConfig["lobbyURL"];
            }

            if (request.GameConfig.ContainsKey("lang") && Model.Game.FC.FC.lang.ContainsKey(request.GameConfig["lang"]))
            {
                requestData.LanguageID = Model.Game.FC.FC.lang[request.GameConfig["lang"]];
            }
            else
            {
                requestData.LanguageID = Model.Game.FC.FC.lang["en-US"];
            }

            try
            {
                var response = await _apiService.Login(requestData);
                if (response.Result != (int)ErrorCodeEnum.Success)
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail]);
                }
                return response.Url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var FCReuslt = await _apiService.GetSingleBill(new GetSingleBillRequest
            {
                TrsID = CutGuidTo30Characters(transfer_record.id)
            });
            if (FCReuslt.status == 1)
            {
                if (transfer_record.target == nameof(Platform.FC))//轉入FC直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.FC))
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
            else if (FCReuslt.status == 0 || FCReuslt.Result != (int)ErrorCodeEnum.Success)
            {
                if (transfer_record.target == nameof(Platform.FC))//轉入FC直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.FC))
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

        public async Task<string> GameDetailURL(GetBetDetailReq request)
        {
            var fcRecord = await _fcDBService.GetFcRecordByReportTime(request, request.record_id);
            if (fcRecord == null)
            {
                return string.Empty;
            }

            var source = new GetPlayerReportRequest
            {
                RecordID = request.record_id,
                MemberAccount = fcRecord.account
            };

            if (Model.Game.FC.FC.lang.ContainsKey(request.lang))
            {
                source.LanguageID = Model.Game.FC.FC.lang[request.lang];
            }
            else
            {
                source.LanguageID = Model.Game.FC.FC.lang["en-US"];
            }

            var response = await _apiService.GetPlayerReport(source);
            if (response.Result != (int)ErrorCodeEnum.Success)
            {
                throw new Exception(response.Result.ToString());
            }

            return response.Url;
        }
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes >= 5)
            {
                endTime = startTime.AddMinutes(5);
                RepairCount += await RepairFc(startTime, endTime);
                startTime = endTime;

                await Task.Delay(3000);
            }
            RepairCount += await RepairFc(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            return _apiService.SearchMember(new()
            {
                MemberAccount = "HealthCheck"
            });
        }
        #endregion
        /// <summary>
        /// FC 帳務比對
        /// 1. 比對轉帳中心與遊戲商的下注明細是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairFc(DateTime startTime, DateTime endTime)
        {
            if (DateTime.Compare(startTime, endTime) == 0)
            {
                return 0;
            }

            List<Record> res = new List<Record>();
            if (DateTime.Now.Subtract(startTime).Hours >= 2)
            {
                var req = new GetHistoryRecordListRequest
                {
                    StartDate = startTime.AddHours(-12),
                    EndDate = endTime.AddHours(-12).AddSeconds(-1)
                };
                var betLogs = await _apiService.GetHistoryRecordList(req);

                if (betLogs.Result == (int)ErrorCodeEnum.Success && betLogs.Records.Count > 0)
                {
                    res.AddRange(betLogs.Records);
                }
            }
            else
            {
                var req = new GetRecordListRequest
                {
                    StartDate = startTime.AddHours(-12),
                    EndDate = endTime.AddHours(-12).AddSeconds(-1)
                };
                var betLogs = await _apiService.GetRecordList(req);

                if (betLogs.Result == (int)ErrorCodeEnum.Success && betLogs.Records.Count > 0)
                {
                    res.AddRange(betLogs.Records);
                }
            }
            int repairCount = 0;
            if (res.Count != 0)
            {
                repairCount = await PostFcRecord(res);
            }
            return repairCount;
        }
    }
}
