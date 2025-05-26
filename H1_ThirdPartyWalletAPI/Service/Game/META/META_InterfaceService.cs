using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.META.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.META.Request;
using H1_ThirdPartyWalletAPI.Model.Game.META.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;

namespace H1_ThirdPartyWalletAPI.Service.Game.META
{
    public interface IMETAInterfaceService : IGameInterfaceService
    {
        Task PostMetaRecord(List<Record> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class META_RecordService : IMETAInterfaceService
    {
        private readonly ILogger<META_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IDBService _dbService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IMETAApiService _apiService;
        private readonly IMETADBService _metaDBService;
        private readonly IGameReportDBService _gameReportDBService;

        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public META_RecordService(ILogger<META_RecordService> logger,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IMETADBService metaDbService, 
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _apiService = gameaApiService._MetaApi;
            _dbService = dbService;
            _summaryDBService = summaryDBService;
            _cacheService = commonService._cacheDataService;
            _metaDBService = metaDbService;
            _gameReportDBService = gameReportDBService;
        }
        #region GameInterfaceService

        public async Task PostMetaRecord(List<Record> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                var linqRes = recordData.GroupBy(x => x.Account);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            if (Config.OneWalletAPI.Prefix_Key != "PRD")
                            {
                                if (!group.Key.Contains(Config.CompanyToken.META_PrefixCode + "-" + Config.OneWalletAPI.Prefix_Key))
                                {
                                    continue;
                                }
                            }

                            string club_id;
                            club_id = group.Key.Replace(Config.CompanyToken.META_PrefixCode + "-", "").Substring(3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.META);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No META user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData =
                                new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<Record> betDetailData = new List<Record>();

                            foreach (Record r in group) //loop club id bet detail
                            {
                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.META);
                                sumData.Game_type = 0;
                                DateTime tempDateTime = r.DateCreate;
                                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                sumData.ReportDatetime = tempDateTime;
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                            DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.Serial);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.Serial);
                                        }
                                    }
                                }

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_id.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_id.ToString()];
                                    //合併處理
                                    sumData = Calculate(sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString() + sumData.Game_id.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results =
                                        await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);

                                    if (!results.Any()) //沒資料就建立新的
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

                                    summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_id.ToString(), sumData);
                                }

                                r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }

                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }

                            int PostRecordResult = await _metaDBService.PostmetaRecord(conn, tran, betDetailData);
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            _logger.LogDebug("insert META record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (Record r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.Serial, r.DateCreate);

                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run META record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }

                await conn.CloseAsync();
            }
        }


        /// <summary>
        /// 廠商未提供小時帳
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
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
                _logger.LogDebug("Create META game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _metaDBService.SummetaBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.META);
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

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            CheckPointRequest req = new CheckPointRequest();
            req.Account = platform_user.game_user_id;
            req.Password = platform_user.game_user_id.Replace(Config.CompanyToken.META_PrefixCode + "-", "");
            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _apiService.CheckPoint(req);
                Balance.Amount = Convert.ToDecimal(res.rows.MemberPoint);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("META餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.META);
            return Balance;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var responseData = await _apiService.GameLogout(new GameLogoutRequest()
                {
                    Account = platform_user.game_user_id,
                    Password = platform_user.game_user_id.Replace(Config.CompanyToken.META_PrefixCode + "-", "")
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出META使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var responseData = await _apiService.TransPoint(new TransPointRequest
                {
                    Account = platform_user.game_user_id,
                    Password = platform_user.game_user_id.Replace(Config.CompanyToken.META_PrefixCode + "-", ""),
                    Points = Math.Round(transfer_amount, 4),
                    TradeOrder = RecordData.id.ToString()
                });

                if (!responseData.DecryptStatus)
                {
                    throw new ExceptionMessage(responseData.code, responseData.errMsg);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("META TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("META TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInMetaFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }

        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var responseData = await _apiService.TransPoint(new TransPointRequest
                {
                    Account = platform_user.game_user_id,
                    Password = platform_user.game_user_id.Replace(Config.CompanyToken.META_PrefixCode + "-", ""),
                    Points = -Math.Round(transfer_amount, 4),
                    TradeOrder = RecordData.id.ToString()
                });

                if (!responseData.DecryptStatus)
                {
                    throw new ExceptionMessage(responseData.code, responseData.errMsg);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("META TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("META TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInMetaFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.META.META.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            var req = new CreateMemberRequest()
            {
                Account = Config.OneWalletAPI.Prefix_Key + userData.Club_id
            };
            //創建帳號
            try
            {
                var response = await _apiService.CreateMember(req);

                if (response.DecryptStatus)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = response.rows.MemberAccount;
                    gameUser.game_platform = request.Platform;
                    return gameUser;
                }
                else if (response.code == (int)ErrorCodeEnum.Account_Already_Exists)
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = Config.CompanyToken.META_PrefixCode + "-" + req.Account;
                    gameUser.game_platform = request.Platform;
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.errMsg);
                }
                //{"totalRows":0,"rows":{"MemberAccount":"QdNPQ-dev1000999","Enable":true}}

            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message.ToString());
            }


        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 4 Get Game URL
            var requestData = new GameLoginRequest()
            {
                Account = platformUser.game_user_id,
                Password = platformUser.game_user_id.Replace(Config.CompanyToken.META_PrefixCode + "-", ""),
            };

            if (request.GameConfig.ContainsKey("gameCode"))
            {
                requestData.TableId = request.GameConfig["gameCode"];
            }

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                requestData.RedirectUrl = request.GameConfig["lobbyURL"];
            }


            if (request.GameConfig.ContainsKey("lang") && Model.Game.META.META.lang.ContainsKey(request.GameConfig["lang"]))
            {
                requestData.Lang = Model.Game.META.META.lang[request.GameConfig["lang"]];
            }
            else
            {
                requestData.Lang = Model.Game.META.META.lang["en-US"];
            }

            try
            {
                var response = await _apiService.GameLogin(requestData);
                if (!response.DecryptStatus)
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail]);
                }
                return response.rows[0].url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(transfer_record.Club_id, Platform.META);
            if (gameUser == null)
            {
                throw new Exception("No meta user");
            }

            var dt2 = new DateTimeOffset(transfer_record.create_datetime);
            long _UnixTime = dt2.ToUnixTimeSeconds();

            var requestData = new TransactionLogRequest();
            requestData.Account = gameUser.game_user_id;
            requestData.Date = _UnixTime;
            requestData.TradeOrder = transfer_record.id.ToString();
            requestData.Limit = 1;
            requestData.TranOrder = null;
            var _Reuslt = await _apiService.TransactionLog(requestData);
            if (_Reuslt.DecryptStatus)
            {
                if (transfer_record.target == nameof(Platform.META))//轉入META直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.META))
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    if (transfer_record.status != nameof(TransferStatus.init))
                    {
                        CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                    }
                }
                transfer_record.status = nameof(WalletTransferRecord.TransferStatus.success);
                transfer_record.success_datetime = DateTime.Now;
            }
            else
            {
                //if (_Reuslt.code == (int)ErrorCodeEnum.Point_Transfer_Failed)
                if (transfer_record.target == nameof(Platform.META))//轉入META直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.META))
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
            return null;
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            //DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;

            RepairCount += await RepairMeta(startTime, RepairReq.EndTime);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            //await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            throw new NotImplementedException();
        }
        #endregion


        private BetRecordSummary Calculate(BetRecordSummary SummaryData, Record r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Math.Abs(r.BetTotal);
            SummaryData.Turnover += Math.Abs(r.BetTotal);
            SummaryData.Netwin += r.Winnings;
            SummaryData.Win += r.BetTotal + r.Winnings;
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
        /// META 帳務比對 API追尾號拉帳，依照日期切分查詢區間，時間戳 (Unix timestamp)yyyy-mm-dd
        /// 1. 比對轉帳中心與遊戲商的下注明細是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairMeta(DateTime startTime, DateTime endTime)
        {
            startTime = startTime.Date;
            endTime = startTime.Date.AddDays(1).AddSeconds(-1);

            List<Record> metaBetRecord = new List<Record>();

            var isEnable = true;
            long? LastSerial = null;
            while (isEnable)
            {

                DateTime date = startTime;
                var dt2 = new DateTimeOffset(date);
                long _UnixTime = dt2.ToUnixTimeSeconds();
                BetOrderRecordResponse result = await _apiService.BetOrderRecord(new BetOrderRecordRequest()
                {
                    Date = _UnixTime,
                    LastSerial = LastSerial,
                    Limit = 100,
                });
                if (result.DecryptStatus)
                {
                    if (result.overRows == 0)
                    {
                        isEnable = false;
                    }

                    if (result.rows.Count > 0)
                    {
                        metaBetRecord.AddRange(result.rows);
                        long tempSerial = 0;
                        long.TryParse(result.rows.Max(x => x.Serial), out tempSerial);
                        if (tempSerial > 0) { LastSerial = tempSerial; }

                        await Task.Delay(100);
                    }
                }
                else
                {
                    isEnable = false;
                }
            }


            // 轉帳中心的歷史下注紀錄
            var w1CenterList = await _metaDBService.GetmetaRecordsBytime(startTime, endTime);

            var repairList = new List<Record>();
            foreach (var record in metaBetRecord)
            {
                var hasData = w1CenterList.Where(x => x.Serial == record.Serial).Any();
                if (hasData == false)
                {
                    repairList.Add(record);
                }
            }
            if (repairList.Count > 0)
            {
                await PostMetaRecord(repairList);
            }
            return repairList.Count;
        }


        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            IEnumerable<dynamic> meta_results = await _metaDBService.GetmetaRecordsBySummary(RecordReq);
            meta_results = meta_results.OrderByDescending(e => e.DateCreate);
            res.Data = meta_results.ToList();
            return res;
        }

    }
}
