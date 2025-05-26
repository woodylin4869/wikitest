using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.JOKER.Request;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Request;
using H1_ThirdPartyWalletAPI.Model.Game.KS.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game.JDB.API.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;

namespace H1_ThirdPartyWalletAPI.Service.Game.KS
{
    public interface IKSInterfaceService : IGameInterfaceService
    {
        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
        Task PostKSRecord(List<Record> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

    }
    public class KS_RecordService : IKSInterfaceService
    {
        private readonly ILogger<KS_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IDBService _dbService;
        private readonly IKSDBService _ksdbService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IKSApiService _apiService;
        private readonly IGameReportDBService _gameReportDBService;

        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public KS_RecordService(ILogger<KS_RecordService> logger,
            ICommonService commonService,
            ISummaryDBService summaryDBService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IKSDBService ksdbService,
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _apiService = gameaApiService._KSAPI;
            _dbService = dbService;
            _ksdbService = ksdbService;
            _summaryDBService = summaryDBService;
            _cacheService = commonService._cacheDataService;
            _gameReportDBService = gameReportDBService;
        }
        #region GameInterfaceService

        private string GetKSTransferID(Guid guid)
        {
            return guid.ToString("N");
        }

        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            var batRecords = new List<dynamic>();
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            var ks_results = await _ksdbService.GetKSRunningRecord(RecordReq);


            ks_results.Where(r => (r.rewardat.HasValue) && (r.rewardat.Value == new DateTime(1900, 01, 01, 00, 00, 00, 000)))
           .ForEach(r =>
           {
               r.rewardat = null;
           });
            ks_results.Where(r => r.type == "Combo")
           .ForEach(r =>
           {
               r.cateid = "Combo";
           });
            ks_results.Where(r => r.type == "Smart")
      .ForEach(r =>
      {
          r.cateid = "Smart";
      });
            if (ks_results.Count() > 0)
            {
                batRecords.AddRange(ks_results);
                batRecords = batRecords.OrderByDescending(e => e.createat).ToList();
            }


            res.Data = batRecords.ToList();
            return res;
        }

        public async Task PostKSRecord(List<Record> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();


                //      var oldLogs = (await _dbService.GetKSRecordsBytime(recordData.Min(l => l.CreateAt), recordData.Max(l => l.CreateAt)))
                //.Select(l => new { l.OrderID, l.CreateAt, l.Status }).ToHashSet();

                //去除同批內重複注單，並以CreateAt較大的為主
                recordData = recordData.OrderByDescending(r => r.CreateAt)
                                       .DistinctBy(r => new { r.OrderID, r.CreateAt, r.Status })
                                       .Reverse()
                                       .ToList();
                var linqRes = recordData.GroupBy(x => x.UserName);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            if (Config.OneWalletAPI.Prefix_Key != "PRD")
                            {
                                if (!group.Key.Contains(Config.OneWalletAPI.Prefix_Key))
                                {
                                    continue;
                                }
                            }

                            string club_id;
                            club_id = group.Key.Substring(3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.KS);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No KS user");
                            }

                            //彙總帳Mapping表
                            var summaryBetRecordMapping = new HashSet<t_summary_bet_record_mapping>();

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new();

                            //已結算注單
                            List<Record> betDetailData = new();

                            //未結算注單
                            List<Record> betDetailDataRunning = new();
                            foreach (Record r in group) //loop club id bet detail
                            {
                                ////跳過重複注單
                                //if (oldLogs.Contains(new { r.OrderID, r.CreateAt, r.Status }))
                                //    continue;

                                DateTime drawtime = DateTime.Now;
                                int Game_type = 0;

                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.KS);
                                sumData.Game_type = Game_type;

                                sumData.ReportDatetime = new DateTime(drawtime.Year, drawtime.Month, drawtime.Day, drawtime.Hour, (drawtime.Minute / 5) * 5, 0);

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                    //合併處理
                                    sumData = await Calculate(conn, tran, sumData, r);
                                    if (sumData.RecordCount > 0)
                                    {
                                        summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                                    }
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results =
                                        await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    results = results.Where(x => x.Game_type == sumData.Game_type).ToList();
                                    if (!results.Any()) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = await Calculate(conn, tran, sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = await Calculate(conn, tran, sumData, r);
                                    }
                                    if (sumData.RecordCount > 0)
                                    {
                                        summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
                                    }
                                }

                                switch (r.Status)
                                {
                                    case "None":
                                        betDetailDataRunning.Add(r);
                                        break;
                                    case "Win":
                                    case "Lose":
                                    case "Cancel":
                                    case "Revoke":
                                        betDetailData.Add(r);
                                        break;
                                }

                                r.club_id = memberWalletData.Club_id;
                                r.franchiser_id = memberWalletData.Franchiser_id;
                                r.summary_id = sumData.id;

                                var mapping = new t_summary_bet_record_mapping()
                                {
                                    summary_id = sumData.id,
                                    report_time = sumData.ReportDatetime.Value,
                                    partition_time = r.CreateAt.Date
                                };
                                summaryBetRecordMapping.Add(mapping);
                            }

                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }


                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);

                            //寫入匯總帳對應
                            await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMapping);


                            //寫入未結算單
                            if (betDetailDataRunning.Count > 0)
                            {
                                int PostRunningRecordResult = await _ksdbService.PostKSRunningRecord(conn, tran, betDetailDataRunning);
                                await _ksdbService.PostKSRecord(conn, tran, betDetailDataRunning);
                            }

                            //寫入明細帳
                            if (betDetailData.Count > 0)
                            {
                                int PostRecordResult = await _ksdbService.PostKSRecord(conn, tran, betDetailData);
                            }

                            _logger.LogDebug("insert KS record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (Record r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.OrderID, r.CreateAt);

                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run KS record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }

                await conn.CloseAsync();
            }
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
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create KS game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計
                SiteReportRequest req = new SiteReportRequest()
                {
                    StartAt = reportTime,
                    EndAt = reportTime,
                    //Date = startDateTime.ToString("yyyy-MM-dd")
                };
                SiteReportResponse KSCenterList = new SiteReportResponse();
                KSCenterList.list = new List<SiteReportList>();
                //取得這小時
                KSBaseRespones<SiteReportResponse> kSBaseRespones = await _apiService.SiteReport(req);
                if (kSBaseRespones.success == 1)
                {
                    if (kSBaseRespones.info != null)
                    {
                        KSCenterList.list = kSBaseRespones.info.list;
                    }
                }
                else
                {
                    _logger.LogDebug("kSBaseRespones Error : {Error}", kSBaseRespones.Error);
                }

                if (KSCenterList.list.Count == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.KS),
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

                    decimal Sum_total_bet = 0;
                    decimal Sum_total_win = 0;
                    decimal Sum_total_netwin = 0;
                    int Sum_total_count = 0;

                    foreach (SiteReportList item in KSCenterList.list)
                    {
                        Sum_total_bet += item.BetMoney;
                        Sum_total_win += item.Reward;
                        Sum_total_netwin += item.Money;
                        Sum_total_count += item.OrderCount;
                    }

                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.KS),
                        report_datetime = DateTime.Parse(reportTime.ToString("yyyy-MM-dd")),
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = Sum_total_bet,
                        total_win = Sum_total_win,
                        total_netwin = Sum_total_netwin,
                        total_count = Sum_total_count,
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
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create KS game W1 report time {datetime}", reportTime);
                //var (totalCount, totalBetValid, totalNetWin) = await _commonService._serviceDB.SumKSBetRecordByBetTime(reportTime, reportTime.AddDays(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.KS);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                //reportData.total_bet = totalBetValid;
                //reportData.total_win = totalBetValid + totalNetWin;
                //reportData.total_netwin = totalNetWin;
                //reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }


        /// <summary>
        /// 取得會員餘額
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="platform_user"></param>
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            UserBalanceRequest req = new UserBalanceRequest();
            req.UserName = platform_user.game_user_id;


            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _apiService.UserBalance(req);
                if (res.success != (int)ErrorCodeEnum.success)
                {
                    throw new ExceptionMessage(res.success, res.Error.ToString());
                }

                Balance.Amount = Convert.ToDecimal(res.info.Money);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("KS餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.KS);
            return Balance;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                var result = await _apiService.UserLogout(new UserLogoutRequest()
                {
                    UserName = platform_user.game_user_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("KickUser 踢出KS使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            try
            {
                var result = await _apiService.UserLogout(new UserLogoutRequest()
                {
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("KickAllUser ks user fail MSG : {Message}", ex.Message);
                throw new ExceptionMessage((int)ResponseCode.KickUserFail, MessageCode.Message[(int)ResponseCode.KickUserFail]);
            }
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            #region STEP2: 發起存款

            var transfer_amount = RecordData.amount;
            var currency = walletData.Currency;

            //檢查幣別
            if (!Model.Game.KS.KS.Currency.ContainsKey(currency))
                throw new ExceptionMessage(ResponseCode.UnavailableCurrency);

            //檢查最大轉帳額度
            if (currency == "THB" && transfer_amount > 5000000)
            {
                string KSErrorMsg = "KS Transfer Fail :transfer_amount more than 5000000";
                _logger.LogError("FundTransferInKSFail Msg: {Message}", KSErrorMsg);
                throw new ExceptionMessage(ResponseCode.Fail, KSErrorMsg);
            }

            try
            {
                var req = new UserTransferRequest
                {
                    UserName = platform_user.game_user_id,
                    Type = "IN",
                    Money = Math.Round(transfer_amount, 2),
                    ID = GetKSTransferID(RecordData.id),
                    Currency = Model.Game.KS.KS.Currency[currency]
                };
                var responseData = await _apiService.UserTransfer(req);
                if (responseData.success != (int)ErrorCodeEnum.success)
                {
                    throw new ExceptionMessage(responseData.success, responseData.Error.ToString());
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);

            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("KS TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("KS TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInKSFail Msg: {Message}", ex.Message);
            }

            #endregion

            return RecordData.status;
        }

        public async Task<string> Withdraw(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            var currency = walletData.Currency;
            try
            {
                //檢查最大轉帳額度
                if (currency == "THB" && transfer_amount > 5000000)
                {
                    string KSErrorMsg = "KS Transfer Fail :transfer_amount more than 5000000";
                    _logger.LogError("FundTransferInKSFail Msg: {Message}", KSErrorMsg);
                    throw new ExceptionMessage(ResponseCode.Fail, KSErrorMsg);
                }


                var req = new UserTransferRequest
                {
                    UserName = platform_user.game_user_id,
                    Type = "OUT",
                    Money = Math.Round(transfer_amount, 2),
                    ID = GetKSTransferID(RecordData.id),
                    Currency = Model.Game.KS.KS.Currency[walletData.Currency]
                };
                var responseData = await _apiService.UserTransfer(req);
                if (responseData.success != (int)ErrorCodeEnum.success)
                {
                    throw new ExceptionMessage(responseData.success, responseData.Error.ToString());
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("KS TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("KS TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInKSFail Msg: {Message}", ex.Message);
            }
            return RecordData.status;
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.KS.KS.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            string Account = Config.OneWalletAPI.Prefix_Key + request.Club_id;

            var req = new UserRegisterRequest()
            {
                UserName = Account
            };
            //創建帳號
            try
            {
                var response = await _apiService.UserRegister(req);

                if (response.success == (int)ErrorCodeEnum.success || response.Error == ErrorCodeEnum.EXISTSUSER.ToString())
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = Account;
                    gameUser.game_platform = request.Platform;
                    return gameUser;
                }
                else
                {
                    throw new Exception(response.Error.ToString());
                }

            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message.ToString());
            }
        }


        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new();
            var bettimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            var batRecords = new List<dynamic>();
            foreach (var betTime in bettimePair)
            {
                var records = await _ksdbService.GetKSRecordsBySummary(new()
                {
                    summary_id = RecordReq.summary_id,
                    ReportTime = betTime
                });

                records.Where(r => (r.rewardat.HasValue) && (r.rewardat.Value == new DateTime(1900, 01, 01, 00, 00, 00, 000)))
             .ForEach(r =>
             {
                 r.rewardat = null;
             });
                records.Where(r => r.type == "Combo")
            .ForEach(r =>
            {
                r.cateid = "Combo";
            });
                records.Where(r => r.type == "Smart")
          .ForEach(r =>
          {
              r.cateid = "Smart";
          });

                batRecords.AddRange(records);
            }

            batRecords = batRecords.OrderByDescending(e => e.createat).ToList();
            res.Data = batRecords.ToList();
            return res;
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 4 Get Game URL
            var requestData = new UserLoginRequest()
            {
                UserName = platformUser.game_user_id,
            };

            if (request.GameConfig.ContainsKey("gameCode"))
            {
                int tempGameID = 0;
                int.TryParse(request.GameConfig["gameCode"], out tempGameID);
                requestData.CateID = tempGameID;
            }

            //if (request.GameConfig.ContainsKey("lobbyURL"))
            //{
            //    requestData.HomeUrl = request.GameConfig["lobbyURL"];
            //}

            if (request.GameConfig.ContainsKey("lang") && Model.Game.KS.KS.lang.ContainsKey(request.GameConfig["lang"]))
            {
                _apiService.SetContentLanguage(Model.Game.KS.KS.lang[request.GameConfig["lang"]]);
            }
            else
            {
                _apiService.SetContentLanguage(Model.Game.KS.KS.lang["en-US"]);
            }

            try
            {
                var response = await _apiService.UserLogin(requestData);
                if (response.success != (int)ErrorCodeEnum.success)
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail]);
                }
                return response.info.Url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            var KSReuslt = await _apiService.UserTransferInfo(new UserTransferInfoRequest
            {
                ID = GetKSTransferID(transfer_record.id)
            });


            if (KSReuslt.success == 1 && KSReuslt.info.Status == "Finish")
            {
                if (transfer_record.target == nameof(Platform.KS))//轉入KS直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.KS))
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
            else if ((KSReuslt.success == 0 && KSReuslt.Error == ErrorCodeEnum.NOORDER.ToString()) || (KSReuslt.success == 1 && KSReuslt.info.Status == "Faild"))
            {
                if (transfer_record.target == nameof(Platform.KS))//轉入KS直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.KS))
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
            // 廠商目前沒有
            throw new NotImplementedException();
        }

        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            int SearchType = (RepairReq.SearchType == 1 ? 1 : 2);
            var RepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 30)
            {
                endTime = startTime.AddMinutes(30);
                RepairCount += await RepairKS(startTime, endTime, SearchType);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await RepairKS(startTime, RepairReq.EndTime, SearchType);
            await Task.Delay(1000);
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day).AddDays(1));
            await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day).AddDays(1));
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

            UserGroupRequest KSEditLimit = new UserGroupRequest
            {
                UserName = gameUser.game_user_id,
                GroupID = request.bet_setting.ToString(),
            };
            var res = new ResCodeBase();
            var KSreq = await _apiService.UserGroup(KSEditLimit);
            if (KSreq.success != (int)Model.Game.KS.Enum.ErrorCodeEnum.success)
            {
                res.code = (int)ResponseCode.SetLimitFail;
                res.Message = KSreq.msg;
                return res;
            }
            res.code = (int)ResponseCode.Success;
            res.Message = MessageCode.Message[(int)ResponseCode.Success];
            return res;
        }
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.ESport;
        }

        public Task HealthCheck(Platform platform)
        {
            return _apiService.UserBalance(new()
            {
                UserName = "HealthCheck"
            });
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
        /// KS 帳務比對
        /// 1. 比對轉帳中心與遊戲商的下注明細是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="SearchType"></param>
        /// <returns></returns>
        private async Task<int> RepairKS(DateTime startTime, DateTime endTime, int SearchType)
        {
            List<Record> repairList = new List<Record>();
            if (DateTime.Compare(startTime, endTime) == 0)
            {
                return 0;
            }
            var Page = 1;
            List<Record> res = new List<Record>();

            string LogGetType = SearchType == 1 ? "CreateAt" : "RewardAt";
            var req = new LogGetRequest
            {
                OrderType = "All",
                Type = LogGetType,
                PageIndex = Page,
                PageSize = 1000,
                StartAt = startTime,
                EndAt = endTime.AddSeconds(-1)
            };

            while (true)
            {
                req.PageIndex = Page;
                var betLogs = await _apiService.LogGet(req);
                if (betLogs.success == (int)ErrorCodeEnum.success && betLogs.info.list.Count > 0)
                {
                    res.AddRange(betLogs.info.list);

                    if (Page * req.PageSize >= betLogs.info.RecordCount)
                    {
                        break;
                    }
                    Page++;
                    //api建議4秒爬一次
                    await Task.Delay(4000);
                }
                else
                {
                    break;
                }

            }


            res = res.ToList();
            var w1CenterList = await _ksdbService.GetKSRecordsBytime(startTime, endTime);

            foreach (var item in res)
            {
                var hasData = w1CenterList.Where(x => x.OrderID == item.OrderID && x.Status == item.Status && x.UpdateAt == item.UpdateAt).Any();
                if (hasData == false)
                {
                    repairList.Add(item);
                }

            }
            if (repairList.Count != 0)
            {
                await PostKSRecord(repairList);
            }


            return repairList.Count;
        }

        private async Task<BetRecordSummary> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, Record r)
        {
            if (r.Type == "Combo")
            {
                r.CateID = "0";
                if (r.Details.Count() > 0)
                {
                    r.League = "Parlay (" + r.Details.Count() + ")";
                }
            }

            //未結算單認列輸
            if (r.Status == "None")
            {
                r.BetMoney = r.BetAmount;
                r.Money = -Convert.ToDecimal(r.BetAmount);
            }

            //紀錄修改前額度
            r.Pre_BetAmount = r.BetAmount;
            r.Pre_BetMoney = r.BetMoney;
            r.Pre_Money = r.Money;


            var oldRecords = await _ksdbService.GetKSRecords(conn, tran, r.OrderID, r.CreateAt);
            oldRecords ??= new();
            if (oldRecords.Any(oldr => new { oldr.OrderID, oldr.Status, oldr.RewardAt }.Equals(new { r.OrderID, r.Status, oldr.RewardAt })))
            {
                r.Status = "Fail";
                return SummaryData;
            }

            if (oldRecords.Any())
            {
                //只會有未結算單不會有改排單，所以只會有未結算單後要沖銷
                Record lastRecord = oldRecords.OrderByDescending(r => r.UpdateAt).First();
                r.BetAmount -= lastRecord.Pre_BetAmount;
                r.BetMoney -= lastRecord.Pre_BetMoney;
                r.Money -= lastRecord.Pre_Money;
            }
            switch (r.Status)
            {
                case "None":
                    SummaryData.RecordCount++;
                    SummaryData.Bet_amount += Convert.ToDecimal(r.BetAmount);
                    SummaryData.Turnover += Convert.ToDecimal(r.BetMoney);
                    SummaryData.Netwin += Convert.ToDecimal(r.Money);
                    SummaryData.Win += 0;
                    break;
                case "Win":
                case "Lose":
                case "Cancel":
                case "Revoke":
                    SummaryData.RecordCount++;
                    SummaryData.Bet_amount += Convert.ToDecimal(r.BetAmount);
                    SummaryData.Turnover += Convert.ToDecimal(r.BetMoney);
                    SummaryData.Netwin += Convert.ToDecimal(r.Money);
                    SummaryData.Win += Convert.ToDecimal(r.Money) + Convert.ToDecimal(r.BetAmount);
                    break;
            }
            SummaryData.JackpotWin = 0;
            SummaryData.updatedatetime = DateTime.Now;
            await _ksdbService.DeleteKSRunningRecord(conn, tran, r);

            return SummaryData;
        }


    }
}
