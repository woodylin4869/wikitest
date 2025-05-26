using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.IDN;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ThirdPartyWallet.Share.Model.Game.Common.Response;
using ThirdPartyWallet.Share.Model.Game.IDN;
using ThirdPartyWallet.Share.Model.Game.IDN.Request;
using ThirdPartyWallet.Share.Model.Game.IDN.Response;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static ThirdPartyWallet.Share.Model.Game.IDN.IDN;
using IDNsetup = ThirdPartyWallet.Share.Model.Game.IDN.IDN;

namespace H1_ThirdPartyWalletAPI.Service.Game.IDN
{
    public interface IIDNInterfaceService : IGameInterfaceService
    {
        Task<int> PostIDNRecord(List<Bet_History> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
        Task refreashToken([CallerMemberName] string callerName = "", bool setToken = false);
        Task DelToken();
        Task<ResponseBase<bethistoryResponse>> Getbethistory(bethistoryRequest source);
    }

    public class IDN_InterfaceService : IIDNInterfaceService
    {
        private readonly ILogger<IDN_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IDBService _dbService;
        private readonly IGameApiService _gameApiService;
        private readonly IIDNDBService _IDNDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private readonly IIDNApiService _IDNApiService;
        private readonly IOptions<IDNConfig> _options;
        const int _cacheSeconds = 3600000;
        const int _cacheFranchiserUser = 1800;
        public IDN_InterfaceService(ILogger<IDN_InterfaceService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            IDBService dbService,
            IIDNDBService IDNDBService,
            ISummaryDBService summaryDBService,
            IGameReportDBService gameReportDBService,
            IIDNApiService IDNApiService,
            IOptions<IDNConfig> options)
        {
            _logger = logger;
            _commonService = commonService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _dbService = dbService;
            _IDNDBService = IDNDBService;
            _IDNApiService = IDNApiService;
            _options = options;

        }

        public async Task<ResponseBase<bethistoryResponse>> Getbethistory(bethistoryRequest source)
        {
            return await _IDNApiService.bethistoryAsync(source);
        }

        public async Task SetToken(string IDN_access_token)
        {
            if (!string.IsNullOrEmpty(IDN_access_token))
            {
                _IDNApiService.SetAuthToken(IDN_access_token);
            }
        }

        public async Task refreashToken([CallerMemberName] string callerName = "", bool setToken = false)
        {
            string IDN_access_token = "";
            int refreashcacheSeconds = _cacheSeconds;
            if (Config.OneWalletAPI.Prefix_Key == "dev")
            {
                refreashcacheSeconds = _cacheSeconds + 18000;
            }
            if (setToken)
            {
                IDN_access_token = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.LoginToken}:{Platform.IDN}",
                async () =>
                {
                    try
                    {
                        AuthRequest authRequest = new AuthRequest();
                        authRequest.scope = "";
                        AuthResponse authResponse = await _IDNApiService.AuthAsync(authRequest);
                        return authResponse.access_token;
                    }
                    catch
                    {
                        return null;
                    }
                },
                refreashcacheSeconds);
            }
            else
            {
                IDN_access_token = await _commonService._cacheDataService.StringGetAsync<string>($"{RedisCacheKeys.LoginToken}:{Platform.IDN}");
            }

            if (!string.IsNullOrEmpty(IDN_access_token))
            {
                _IDNApiService.SetAuthToken(IDN_access_token);
            }
            else
            {
                _logger.LogError("IDN refreashToken Error:{CallerMemberName}", callerName);
            }
        }

        public async Task GetToken()
        {
            string IDN_access_token = await _commonService._cacheDataService.StringGetAsync<string>($"{RedisCacheKeys.LoginToken}:{Platform.IDN}");

            if (!string.IsNullOrEmpty(IDN_access_token))
            {
                _IDNApiService.SetAuthToken(IDN_access_token);
            }
        }

        public async Task DelToken()
        {
            await _commonService._cacheDataService.KeyDelete($"{RedisCacheKeys.LoginToken}:{Platform.IDN}");
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
                await refreashToken();
                var responseData = await _IDNApiService.BalanceAsync(new BalanceRequest
                {
                    UserName = platform_user.game_user_id,
                });
                if (!responseData.success)
                {
                    throw new Exception(responseData.Message);
                }
                Balance.Amount = decimal.Round(Convert.ToDecimal(responseData.data.total_wallet), 2, MidpointRounding.ToZero);
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("IDN餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.IDN);
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
                await refreashToken();
                var responseData = await _IDNApiService.CalibrateAsync(new CalibrateRequest()
                {
                    UserName = platform_user.game_user_id
                });
            }
            catch (Exception ex)
            {
                _logger.LogInformation("KickUser 踢出IDN使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
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
            if (!IDNsetup.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //創建帳號
            try
            {
                var req = new RegistrationRequest()
                {
                    username = Config.OneWalletAPI.Prefix_Key + userData.Club_id
                };
                req.username = req.username.ToLower();
                await refreashToken();
                var responseData = await _IDNApiService.RegistrationAsync(req);

                if (responseData.success || responseData.response_code == 422 && responseData.Message.Contains("Username already registered"))
                {
                    var gameUser = new GamePlatformUser();
                    gameUser.club_id = userData.Club_id;
                    gameUser.game_user_id = req.username;
                    gameUser.game_platform = Platform.IDN.ToString();
                    return gameUser;
                }
                else
                {
                    throw new Exception(responseData.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("IDN建立會員失敗 Msg: {Message}", ex.Message);
                throw new ExceptionMessage(ResponseCode.Fail, "IDN " + ex.Message.ToString());
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
                await refreashToken();
                var responseData = await _IDNApiService.DepositAsync(platform_user.game_user_id, new DepositRequest
                {
                    order_id = RecordData.id.ToString(),
                    amount = RecordData.amount
                });

                if (!responseData.success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("IDN Deposit: {Message}", responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("IDN TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("IDN Deposit: {Message}", ex.Message);
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
                await refreashToken();
                var responseData = await _IDNApiService.WithdrawAsync(platform_user.game_user_id, new WithdrawRequest
                {
                    order_id = RecordData.id.ToString(),
                    amount = RecordData.amount
                });

                //取款失敗且response_code == 422 回應額度不足錯誤 執行餘額校正回主錢包動作
                if (!responseData.success && responseData.response_code == 422)
                {
                    await _IDNApiService.CalibrateAsync(new CalibrateRequest
                    {
                        UserName = platform_user.game_user_id
                    });

                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("IDN Withdraw : {ex}", responseData.Message);
                }
                else if (!responseData.success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("IDN Withdraw : {ex}", responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("IDN TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("IDN Withdraw : {ex}", ex.Message);
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
            string gameCode = "";
            LaunchRequest UrlRequest = new LaunchRequest
            {
                create = "1"
            };

            if (request.GameConfig.ContainsKey("lang") && IDNsetup.Lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.lang = IDNsetup.Lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.lang = IDNsetup.Lang["en-US"];
            }

            if (request.GameConfig.ContainsKey("gameCode"))
            {
                gameCode = request.GameConfig["gameCode"];
            }


            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.lobbyUrl = request.GameConfig["lobbyURL"];
            }

            try
            {
                await refreashToken();
                var responseData = await _IDNApiService.LaunchAsync(platformUser.game_user_id, gameCode, UrlRequest);
                if (!responseData.success)
                {
                    throw new Exception(responseData.Message);
                }
                return responseData.data.launch_url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + "IDN: " + ex.Message.ToString());
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


            int page = 1;
            int paginate = 5000;
            DateTime now = transfer_record.create_datetime.AddHours(-1);
            DateTime nowToMinutes = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);


            string fromDateTime = nowToMinutes.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            string toDateTime = nowToMinutes.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);


            bool apiSuccess = false;
            bool hasNextPage = true;
            bool isSuccess = false;

            await refreashToken();
            //存款交易檢查
            if (transfer_record.source == nameof(Platform.W1) && transfer_record.target == nameof(Platform.IDN))
            {
                while (hasNextPage)
                {

                    var Reuslt = await _IDNApiService.CheckDepositListAsync(page, new CheckDepositListRequest
                    {
                        from = fromDateTime,
                        to = toDateTime,
                        paginate = paginate
                    });

                    //失敗則結束迴圈
                    if (!Reuslt.success)
                    {
                        hasNextPage = false;
                        break;
                    }
                    else
                    {
                        apiSuccess = true;
                    }

                    //找到交易ID則表示成功
                    if (Reuslt.data.deposits.data.Count > 0)
                    {
                        foreach (var item in Reuslt.data.deposits.data)
                        {
                            if (item.order_id == transfer_record.id.ToString())
                            {
                                isSuccess = true;
                                break;
                            }
                        }
                    }

                    //下一頁
                    if (page < Reuslt.data.deposits.last_page)
                    {
                        page++;
                    }
                    else
                    {
                        hasNextPage = false;
                        break;
                    }
                }
            }
            else if (transfer_record.source == nameof(Platform.IDN) && transfer_record.target == nameof(Platform.W1))
            {
                while (hasNextPage)
                {
                    var Reuslt = await _IDNApiService.CheckWithdrawListAsync(page, new CheckWithdrawListRequest
                    {
                        from = fromDateTime,
                        to = toDateTime,
                        paginate = paginate
                    });

                    //失敗則結束迴圈
                    if (!Reuslt.success)
                    {
                        hasNextPage = false;
                        break;
                    }
                    else
                    {
                        apiSuccess = true;
                    }

                    //找到交易ID則表示成功
                    if (Reuslt.data.withdraws.data.Count > 0)
                    {
                        foreach (var item in Reuslt.data.withdraws.data)
                        {
                            if (item.order_id == transfer_record.id.ToString())
                            {
                                isSuccess = true;
                                break;
                            }
                        }
                    }

                    //下一頁
                    if (page < Reuslt.data.withdraws.last_page)
                    {
                        page++;
                    }
                    else
                    {
                        hasNextPage = false;
                        break;
                    }
                }
            }

            if (apiSuccess && isSuccess)
            {
                if (transfer_record.target == nameof(Platform.IDN))//轉入直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.IDN))
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
            else if (apiSuccess)
            {
                if (transfer_record.target == nameof(Platform.IDN))//轉入直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.IDN))
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
            var batRecords = new List<dynamic>();
            GetBetRecord res = new();
            string UserGameID = (_options.Value.IDN_whitelabel_code + Config.OneWalletAPI.Prefix_Key + RecordReq.ClubId).ToLower();

            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            var Getsummary = await _summaryDBService.GetRecordSummaryById(new GetBetRecordReq
            {
                summary_id = RecordReq.summary_id,
                ReportTime = RecordReq.ReportTime
            });

            foreach (var createTime in createtimePair)
            {
                var results = await _IDNDBService.GetIDNRecordsBytime(createTime, RecordReq.ReportTime, UserGameID, Getsummary.Game_type);
                results = results.OrderByDescending(e => e.date).ToList();
                foreach (var result in results)
                {
                    batRecords.Add(result);

                }
            }

            // 統一輸出格式為 RespRecordLevel2_Electronic
            res.Data = batRecords.OrderByDescending(e => e.date).Select(obj => new RespRecordLevel2_Electronic
            {
                RecordId = obj.bet_id,
                BetTime = obj.date,
                GameType = obj.groupgametype.ToString(),
                GameId = obj.game_id,
                BetAmount = obj.bet,
                NetWin = obj.win,
                Jackpot = 0,
                BetStatus = obj.bet_type.ToString(),
                SettleTime = obj.date,
            }).Cast<dynamic>().ToList();
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
            return "";
            //var data = await _IDNDBService.GetIDNRecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            //var Settled = data.Where(x => x.billstatus == "Settled").First();
            //if (Settled == null)
            //{
            //    throw new Exception("未結算無法轉跳廠商URL");
            //}

            //GamedetailRequest request = new GamedetailRequest()
            //{
            //    username = Settled.username,
            //    gamecode = Settled.gamecode,
            //};
            //var url = await _IDNApiService.GamedetailAsync(request);

            //return url.data;
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
                RepairCount += await Repair(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            RepairCount += await Repair(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0));


            DateTime IDNLocalHour = DateTime.Now.AddHours(-3);
            if (startTime < new DateTime(IDNLocalHour.Year, IDNLocalHour.Month, IDNLocalHour.Day, 0, 0, 0))
            {
                await SummaryGameProviderReport(new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0), new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 0, 0));
            }


            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }
        #endregion

        #region GameRecordService

        public async Task<int> PostIDNRecord(List<Bet_History> recordData)
        {
            if (recordData is null)
                throw new ArgumentNullException(nameof(recordData));

            if (!recordData.Any())
                return 0;

            List<string> ExceptionGameID = new List<string>();

            foreach (var bet in recordData)
            {
                if (LiveGameMap.CodeToId.TryGetValue(bet.game_id, out var code))
                {
                    bet.groupgametype = code;
                }
                else if (!ExceptionGameID.Contains(bet.game_id))
                {
                    ExceptionGameID.Add(bet.game_id);
                }
            }
            int Error_groupgametype_count = recordData.RemoveAll(x => x.groupgametype == 0);
            if (ExceptionGameID.Count > 0)
            {
                _logger.LogError("IDN groupgametype Count:{Error_groupgametype_count} Error: {ExceptionGameID}", Error_groupgametype_count, string.Join(",", ExceptionGameID));
            }

            var postResult = 0;
            await using NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master);
            await conn.OpenAsync();

            try
            {
                foreach (IEnumerable<Bet_History> group in recordData.Chunk(20000))
                {
                    await using var tran = await conn.BeginTransactionAsync();
                    try
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var betDetailData = new List<Bet_History>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                        // 紀錄 reportTime 跟 playTime 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();

                        foreach (var item in group)//loop club id bet detail
                        {
                            item.game_id = item.game_id.ToLower();
                            item.Report_time = reportTime;
                            //+7時區 > +8
                            item.date = item.date.AddHours(1);

                            betDetailData.Add(item);

                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }

                            dic[summaryTime].Add(item.date.ToString("yyyy-MM-dd HH:mm"));
                        }

                        if (betDetailData.Count > 0)
                        {
                            postResult += await _IDNDBService.PostIDNRecord(conn, tran, betDetailData);
                        }

                        await tran.CommitAsync();

                        // 記錄到 Redis reportTime 跟 Bet_time(下注時間) 的關聯
                        foreach (var item in dic)
                        {
                            var key = $"{RedisCacheKeys.IDNBetSummaryTime}:{item.Key}";
                            await _commonService._cacheDataService.SortedSetAddAsync(key,
                                item.Value.ToDictionary(s => s, s => (double)DateTime.Parse(s).Ticks));
                        }

                        dic.Clear();

                        sw.Stop();
                        _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync();
                        _logger.LogError(ex, "Error processing IDN record batch: {Message}", ex.Message);
                        throw; // Re-throw to handle at outer level
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in PostIDNRecord: {Message}", ex.Message);
                throw;
            }
            finally
            {
                await conn.CloseAsync();
            }

            return postResult;
        }


        /// <summary>
        /// 統計遊戲商
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public async Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime)
        {
            //校正成+7時區
            startDateTime = startDateTime.AddHours(-1);
            endDateTime = endDateTime.AddHours(-1);

            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                DateTime reportDate = reportTime;
                dailyreportRequest source = new dailyreportRequest();
                source.date = reportDate.ToString("yyyy-MM-dd");

                ResponseBase<dailyreportResponse> responseData = await _IDNApiService.dailyreportAsync(source);
                List<Daily_Report> daily_report = new List<Daily_Report>();
                if (responseData.success)
                {
                    if (responseData.data != null && responseData.data.daily_report.Count > 0)
                    {
                        daily_report = responseData.data.daily_report;
                    }
                }
                else
                {
                    _logger.LogDebug("IDNBaseRespones Error : {Error}", responseData.Message);
                }

                if (daily_report.Count == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.IDN),
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

                    foreach (var item in daily_report)
                    {
                        Sum_total_bet += item.turn_over;
                        Sum_total_win += item.player_wl + item.turn_over;
                        Sum_total_netwin += item.player_wl;

                    }

                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.IDN),
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
                if (reportTime > endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }

                if (reportTime.Hour == DateTime.Now.Hour)
                {
                    break;
                }

                _logger.LogDebug("Create IDN game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalnetwin) = await _IDNDBService.SumIDNBetRecordByPartitionTime(reportTime, reportTime.AddHours(1));

                GameReport reportData = new();
                reportData.platform = nameof(Platform.IDN);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalnetwin + totalBetValid;
                reportData.total_netwin = totalnetwin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);
            }
        }


        /// <summary>
        /// 補單
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> Repair(DateTime startTime, DateTime endTime)
        {
            var w1data = await _IDNDBService.GetIDNRecordsByPartition(startTime, endTime);

            var resPK = w1data.Select(x => new { x.id, x.bet_type }).ToHashSet();

            //從IDN取得的原始資料
            List<Bet_History> gameProviderBetRecords = new List<Bet_History>();
            //要寫入的資料
            List<Bet_History> postBetRecords = new List<Bet_History>();

            var recordRequest = new bethistoryRequest
            {
                date = startTime.AddHours(-1).ToString("yyyy-MM-dd"),
                from = startTime.AddHours(-1).ToString("HH:mm:ss"),
                to = endTime.AddHours(-1).AddSeconds(-1).ToString("HH:mm:ss")
            };

            await refreashToken();
            var betRecord = await _IDNApiService.bethistoryAsync(recordRequest);
            // 有錯誤就拋
            if (string.IsNullOrEmpty(betRecord.Message) == false && betRecord.success == false && betRecord.response_code != 404)
            {
                throw new Exception(betRecord.Message);
            }
            else if (betRecord.data is not null && betRecord.data.bet_history.Count > 0)
            {

                foreach (var item in betRecord.data.bet_history)
                {
                    if (resPK.Add(new { item.id, item.bet_type }))
                    {
                        gameProviderBetRecords.Add(item);
                    }
                }
            }


            if (gameProviderBetRecords.Any() == true)
            {
                // 排除重複注單
                postBetRecords = gameProviderBetRecords.DistinctBy(record => new { record.id, record.bet_type, record.date }).ToList();

            }

            if (postBetRecords.Count == 0)
            {
                return 0;
            }

            return await PostIDNRecord(postBetRecords);
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
            var summaryRecords = await _IDNDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
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
                summaryData.Game_id = nameof(Platform.IDN);
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
            return true;
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
        /// 健康度
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public Task HealthCheck(Platform platform)
        {
            GetToken().GetAwaiter().GetResult();
            return _IDNApiService.HealthCheckAsync();
        }
        /// <summary>
        /// 取得未結算
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            //IEnumerable<dynamic> bti_results = await _IDNDBService.GetIDNRunningRecord(RecordReq);
            //bti_results = bti_results.OrderByDescending(e => e.createtime);
            //res.Data = bti_results.ToList();
            return res;
        }

        /// <summary>
        /// 取得遊戲住單明細-RCG格式
        /// </summary>
        /// <param name="RecordDetailReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<RCGRowData> GameRowData(GetRowDataReq RecordDetailReq)
        {
            RCGRowData rCGRowData = new RCGRowData();

            List<Bet_History> idnRecord = await _IDNDBService.GetIDNRecords(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            if (idnRecord == null || idnRecord.Count == 0)
            {
                throw new Exception("no data");
            }
            Bet_History record = idnRecord.FirstOrDefault();

            IDN_Mapping.OpenListModelBase modelBase = new IDN_Mapping.OpenListModelBase();


            await refreashToken();
            var Reuslt = await _IDNApiService.GetGameResultAsync(new GetGameResultRequest
            {
                gameId = record.game_id,
                matchId = record.match_id,
                date = record.date.AddHours(-1).ToString("yyMMdd")
            });
            if (!string.IsNullOrEmpty(Reuslt.Message))
            {
                throw new Exception(Reuslt.Message);
            }

            rCGRowData.GameId = record.game_id;
            rCGRowData.betAmount = record.bet;
            rCGRowData.netWin = record.win;
            rCGRowData.BetResult = ConvertBetDetailsToString(record.raw_data);

            modelBase.ServerId = "";
            modelBase.NoRun = Reuslt.roundId.ToString();
            modelBase.NoActive = Reuslt.periode?.ToString() ?? "";
            //modelBase.ServerId = Reuslt.gameSet;
            modelBase.DateTime = record.date;
            List<object> MappingResult = IDN_Mapping.RCG_MappingFunc(record.game_id, Reuslt, modelBase);
            rCGRowData.dataList = MappingResult;

            return rCGRowData;
        }

        public static List<string> ConvertBetDetailsToString(string jsonData)
        {
            // 去除外層的引號，將其轉換為合法的 JSON 字串
            string fixedJsonData = JsonConvert.DeserializeObject<string>(jsonData);

            // 解析修正過的 JSON 資料
            var transaction = ExtractBetValues(fixedJsonData);
            return transaction;
        }

        public static List<string> ExtractBetValues(string jsonData)
        {
            // 嘗試解析 JSON 成 JObject
            var parsedJson = JObject.Parse(jsonData);

            // 檢查是否有 "details" 字段 (第一種情況)
            if (parsedJson["details"] != null)
            {
                // 反序列化成 BetTransaction 類型並提取 details 中的 button 值
                var betTransaction = JsonConvert.DeserializeObject<BetTransaction>(jsonData);
                return betTransaction.Details.Select(d => d.Button).ToList();
            }
            // 檢查是否有 "bet" 字段 (第二種情況)
            else if (parsedJson["bet"] != null)
            {
                // 反序列化成 SimpleBet 類型並提取 bet 值
                var simpleBet = JsonConvert.DeserializeObject<SimpleBet>(jsonData);
                return new List<string> { simpleBet.Bet };
            }

            // 如果都不是，返回空的列表
            return new List<string>();
        }

        public class BetDetail
        {
            [JsonProperty("button")]
            public string Button { get; set; }

            [JsonProperty("value")]
            public decimal Value { get; set; }

            [JsonProperty("group")]
            public string Group { get; set; }

            [JsonProperty("transId")]
            public long TransId { get; set; }
        }

        // 第二種情況的 SimpleBet 類別
        public class SimpleBet
        {
            [JsonProperty("bet")]
            public string Bet { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("prize")]
            public decimal Prize { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("total")]
            public decimal Total { get; set; }
        }

        public class BetTransaction
        {
            [JsonProperty("details")]
            public List<BetDetail> Details { get; set; }
        }


        /// <summary>
        /// 使用情境：後彙總排程從遊戲明細查詢使用者遊戲帳號 轉換 為H1的Club_Id 提供 wallet 查詢使用到
        /// </summary>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private string ConvertGamePlatformUserToClubInfo(string propertyValue)
        {
            return propertyValue.Replace(_options.Value.IDN_whitelabel_code, "").Substring(3).ToUpper();
        }
    }
}
