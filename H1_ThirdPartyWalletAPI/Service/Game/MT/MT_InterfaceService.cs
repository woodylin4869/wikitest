using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using MTsetup = H1_ThirdPartyWalletAPI.Model.Game.MT.MT;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Request;
using H1_ThirdPartyWalletAPI.Model.Game.MT.Response;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using ThirdPartyWallet.Share.Model.Game.PS.Response;

namespace H1_ThirdPartyWalletAPI.Service.Game.MT
{

    public interface IMTInterfaceService : IGameInterfaceService
    {
        Task<ResCodeBase> PostMTRecord(List<queryMerchantGameRecord2Response.Translist> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
    }
    public class MT_InterfaceService : IMTInterfaceService
    {
        private readonly ILogger<MT_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IMTDBService _mtDBService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;

        public MT_InterfaceService(ILogger<MT_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IMTDBService mtDBService,
            ISummaryDBService summaryDBService, 
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _mtDBService = mtDBService;
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
                var responseData = await _gameApiService._MTAPI.getPlayerBalanceAsync(new getPlayerBalanceRequest
                {
                    playerName = platform_user.game_user_id,
                });

                if (responseData.resultCode != "1")
                {
                    throw new Exception(MTsetup.ErrorCode[responseData.resultCode]);
                }
                Balance.Amount = decimal.Round(responseData.coinBalance, 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("MT餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.MT);
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
                var responseData = await _gameApiService._MTAPI.logOutGameAsync(new logOutGameRequest()
                {
                    playerName = platform_user.game_user_id,
                });

                if (responseData.resultCode != "1")
                {
                    throw new Exception(MTsetup.ErrorCode[responseData.resultCode]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出MT使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
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
            if (!MTsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new PlayerCreateRequest()
                {
                    playerName = Config.OneWalletAPI.Prefix_Key + userData.Club_id,
                    pwd = "!8@8%8",
                };


                var response = await _gameApiService._MTAPI.playerCreateAsync(req);
                if (response.resultCode == "1" || response.resultCode == "5")
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.playerName;
                    gameUser.game_platform = Platform.MT.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(MTsetup.ErrorCode[response.resultCode]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("MT建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "MT " + ex.Message.ToString());
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
                var responseData = await _gameApiService._MTAPI.deposit2Async(new deposit2Request
                {
                    playerName = platform_user.game_user_id,
                    extTransId = RecordData.id.ToString().Replace("-", ""),
                    coins = RecordData.amount.ToString("#0.0000"),
                });

                if (responseData.resultCode != "1")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("MT Deposit: {Message}", MTsetup.ErrorCode[responseData.resultCode]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MT TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MT Deposit: {Message}", ex.Message);
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
                var responseData = await _gameApiService._MTAPI.withdraw2Async(new withdraw2Request
                {
                    playerName = platform_user.game_user_id,
                    extTransId = RecordData.id.ToString().Replace("-", ""),
                    coins = RecordData.amount.ToString("#0.0000"),
                });

                if (responseData.resultCode != "1")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("MT Withdraw : {ex}", MTsetup.ErrorCode[responseData.resultCode]);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MT TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("MT Withdraw : {ex}", ex.Message);
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
            MTsetup.lang.TryGetValue(request.GameConfig["lang"], out var lang);

            PlayerPlatformUrlRequest UrlRequest = new PlayerPlatformUrlRequest
            {
                playerName = platformUser.game_user_id,
                pwd = "!8@8%8"
            };
            PlayerPlatformUrlrawData rawData = new PlayerPlatformUrlrawData()
            {
                gameHall = "2", //棋牌
                gameCode = request.GameConfig["gameCode"],
                lang = lang ?? MTsetup.lang["en-US"],
            };
            //if (request.GameConfig.ContainsKey("lobbyURL"))
            //{
            //    UrlRequest.returnurl = request.GameConfig["lobbyURL"];
            //}

            try
            {
                var token_res = await _gameApiService._MTAPI.playerPlatformUrlAsync(UrlRequest, rawData);
                if (token_res.resultCode != "1")
                {
                    throw new Exception(MTsetup.ErrorCode[token_res.resultCode]);
                }
                var url = token_res.url.ToString();
                //return Helper.ReplaceBetween(url, "://", "/", "mtl.furonghua.mobi");
                return url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "MT: " + ex.Message.ToString());
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

            var Reuslt = await _gameApiService._MTAPI.queryTransbyIdAsync(new QueryTransbyIdRequest
            {
                playerName = Config.OneWalletAPI.Prefix_Key + transfer_record.Club_id,
                extTransId = transfer_record.id.ToString().Replace("-", "")
            });
            if (Reuslt.resultCode == "1" && Reuslt.status == "1")
            {
                if (transfer_record.target == nameof(Platform.MT))//轉入MT直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.MT))
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
                if (transfer_record.target == nameof(Platform.MT))//轉入MT直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.MT))
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
            var mt_results = new List<queryMerchantGameRecord2Response.Translist>();
            GetBetRecord res = new GetBetRecord();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            foreach (var createTime in createtimePair)
            {
                var results = await _mtDBService.GetMTRecordsV2BySummary(RecordReq);
                results = results.OrderByDescending(e => e.partition_time).ToList();
                mt_results.AddRange(results);  // 直接添加 BetRecord 列表
            }
            if(!mt_results.Any())
                mt_results = await _mtDBService.GetMTRecordsBySummary(RecordReq);

            res.Data = mt_results.OrderByDescending(e => e.gameDate).Select(obj => (dynamic)obj).ToList();

            return res;
        }
        /// <summary>
        /// 取得遊戲住單明細-轉跳
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            MTsetup.lang.TryGetValue(RecordDetailReq.lang, out var lang);

            playCheckUrlrawData data = new playCheckUrlrawData()
            {
                rowID = RecordDetailReq.record_id,
                lang = lang ?? MTsetup.lang["en-US"],
            };
            var url = await _gameApiService._MTAPI.playCheckUrlAsync(data);

            return url.url;
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
                RepairCount += await RepairMT(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await RepairMT(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day).AddDays(1));
            await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day), new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day).AddDays(1));
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._MTAPI.getPlayerBalanceAsync(new getPlayerBalanceRequest
            {
                playerName = "HealthCheck"
            });
        }
        #endregion
        #region GameRecordService
        /// <summary>
        /// 住單寫入W1
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<ResCodeBase> PostMTRecord(List<queryMerchantGameRecord2Response.Translist> recordData)
        {
            ResCodeBase res = new ResCodeBase();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, queryMerchantGameRecord2Response.Translist>> linqRes = recordData.GroupBy(x => x.playerName);

            foreach (IGrouping<string, queryMerchantGameRecord2Response.Translist> group in linqRes)
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
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            DateTime drawtime = DateTime.Now;
                            DateTime report_time = new DateTime(drawtime.Year, drawtime.Month, drawtime.Day, drawtime.Hour, (drawtime.Minute / 5) * 5, 0);

                            //已結算注單
                            List<queryMerchantGameRecord2Response.Translist> betDetailData = new List<queryMerchantGameRecord2Response.Translist>();
                            foreach (queryMerchantGameRecord2Response.Translist item in group)
                            {
                                item.report_time = report_time;
                                item.partition_time = item.gameDate;

                                betDetailData.Add(item);

                                var summaryTime = report_time.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }
                                dic[summaryTime].Add(item.partition_time.ToString("yyyy-MM-dd HH:mm"));
                            }

                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = nameof(Platform.MT)+ $"{RedisCacheKeys.BetSummaryTime}:{item.Key}";
                                    await _commonService._cacheDataService.ListPushAsync(key, subItem);
                                }
                            }
                            if (betDetailData.Count > 0)
                            {
                                int PostRecordResult = await _mtDBService.PostMTRecord(conn, tran, betDetailData);
                            }
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

                            _logger.LogError("Run MT record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return res;
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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create MT game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));
                // 每日統計
                var req = await _gameApiService._MTAPI.queryMerchantGameDataAsync(new queryMerchantGameDatarawData()
                {
                    startDate = reportTime.ToString("yyyy-MM-dd"),
                    endDate = reportTime.ToString("yyyy-MM-dd"),
                    currency = MTsetup.Currency["THB"]
                });
                var gameEmptyReport = new GameReport();
                if (req.transList.Count == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.MT);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {

                    gameEmptyReport.platform = nameof(Platform.MT);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = req.transList[0].betAmount;
                    gameEmptyReport.total_win = req.transList[0].winAmount;
                    gameEmptyReport.total_netwin = req.transList[0].winAmount - req.transList[0].betAmount;
                    gameEmptyReport.total_count = req.transList[0].numberOfGames;
                }
                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
                startDateTime = startDateTime.AddDays(1);

                await Task.Delay(3000);
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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create MT game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _mtDBService.SumMTBetRecordByBetTime(reportTime, reportTime.AddDays(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.MT);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin;
                reportData.total_netwin = totalWin - totalBetValid;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddDays(1);
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
        //private async Task<BetRecordSummary> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, queryMerchantGameRecord2Response.Translist r)
        //{

        //    SummaryData.RecordCount++;
        //    SummaryData.Bet_amount += Convert.ToDecimal(r.betAmount);
        //    SummaryData.Turnover += Convert.ToDecimal(r.commissionable);
        //    SummaryData.Netwin += Convert.ToDecimal(r.income);
        //    SummaryData.Win += Convert.ToDecimal(r.winAmount) == 0 ? 0 : Convert.ToDecimal(r.winAmount);
        //    SummaryData.updatedatetime = DateTime.Now;

        //    SummaryData.JackpotWin = 0;
        //    return SummaryData;
        //}

        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairMT(DateTime startTime, DateTime endTime)
        {
            var req = new QueryMerchantGameRecord2rawData
            {
                recordID = 0,
                gameType = "2",
                startTime = startTime.ToString("yyyyMMddHHmmss"),
                endTime = endTime.ToString("yyyyMMddHHmmss"),
                currency = MTsetup.Currency["THB"]
            };


            var res = await _gameApiService._MTAPI.queryMerchantGameRecord2Async(req);


            List<queryMerchantGameRecord2Response.Translist> repairList = new List<queryMerchantGameRecord2Response.Translist>();

            if (res.transList.Count == 0)
            {
                return repairList.Count;
            }

            var w1CenterList = await _mtDBService.GetMTRecordsBytime(startTime, endTime.AddMinutes(1));

            foreach (var item in res.transList)
            {
                var hasData = w1CenterList.Where(x => x.rowID == item.rowID).Any();
                if (hasData == false)
                {
                    repairList.Add(item);
                }

            }
            if (repairList.Count != 0)
            {
                await PostMTRecord(repairList);
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
            return PlatformType.Chess;
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
            var summaryRecords = await _mtDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid.Substring(3)).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();


            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key.Substring(3), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.turnover);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.MT);
                summaryData.Game_type = summaryRecord.Sum(x => x.game_type);
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.netwin);
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
    }
}
