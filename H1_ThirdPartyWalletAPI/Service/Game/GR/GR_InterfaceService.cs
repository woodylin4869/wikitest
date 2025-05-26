using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Request;
using H1_ThirdPartyWalletAPI.Model.Game.GR.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.Game;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Model.Game.DS.Response;
using H1_ThirdPartyWalletAPI.Model.DB.JDB.Response;
using static H1_ThirdPartyWalletAPI.Model.Game.GR.Response.CommBetDetailsResponse;
using H1_ThirdPartyWalletAPI.Model.Game.JDB.Response;
using static H1_ThirdPartyWalletAPI.Model.Game.OB.Response.ReportAgentResponse;
using ThirdPartyWallet.Share.Model.Game.PS.Request;

namespace H1_ThirdPartyWalletAPI.Service.Game.GR
{
    public interface IGRInterfaceService : IGameInterfaceService
    {
        Task<int> PostGRRecordDetail(List<CommBetDetailsResponse.CommBetDetails> recordData);

        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);

        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    }

    public class GR_RecordService : IGRInterfaceService
    {
        private readonly ILogger<GR_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly IDBService _dbService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IGrDBService _grDBService;
        private readonly IGameReportDBService _gameReportDBService;
        private const int _cacheSeconds = 600;
        private const int _cacheFranchiserUser = 1800;
        private const int pageLimit = 100;
        private const int getDelayMS = 200;
        private readonly string _prefixKey;
        private readonly string _grSiteCode;

        public GR_RecordService(ILogger<GR_RecordService> logger,
                                ICommonService commonService,
                                IGameApiService gameaApiService,
                                ISummaryDBService summaryDBService,
                                IGrDBService grDBService,
                                IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameaApiService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
            _grSiteCode = Config.CompanyToken.GR_Site_Code;
            _dbService = commonService._serviceDB;
            _summaryDBService = summaryDBService;
            _cacheService = commonService._cacheDataService;
            _grDBService = grDBService;
            _gameReportDBService = gameReportDBService;
        }

        #region GameInterfaceService

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platformUser)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                var requestData = new GetBalanceRequest();
                requestData.account = platformUser.game_user_id;
                var responseData = await _gameApiService._GrAPI.GetBalance(requestData);

                if (responseData.status != "Y")
                {
                    Balance.Amount = 0;
                    Balance.code = (int)ResponseCode.Fail;
                    Balance.Message = responseData.code + responseData.message;
                }
                Balance.Amount = responseData.data.balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("GR餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.GR);
            return Balance;
        }

        public async Task<string> Deposit(GamePlatformUser platformUser, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            try
            {
                var requestData = new CreditBalanceV3Request();
                requestData.account = platformUser.game_user_id;
                // 轉入點數(無條件捨去到小數點第二位)
                requestData.credit_amount = Math.Round(transfer_amount, 2);
                // 自定義單號, 長度不超過 50 個字
                requestData.order_id = RecordData.id.ToString();

                var responseData = await _gameApiService._GrAPI.CreditBalanceV3(requestData);
                if (responseData.status == "Y")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (responseData.status == "N" && (
                        responseData.code == "111090005" || // 使用者 Account 不存在
                        responseData.code == "112110001" || // 改變使用者餘額失敗,請檢查注單
                        responseData.code == "112110002" || //  輸入金額不能為負或零
                        responseData.code == "112260001" || //  自定義的 OrderID 長度不能超過 50
                        responseData.code == "112280001" || //  代理額度不足
                        responseData.code == "112280002"  //  代理目前被鎖定無法轉入轉出
                    ))
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("GR Deposit Fail: {id}, {code}, {message}", RecordData.id.ToString(), responseData.code, responseData.message);
                    //throw new ExceptionMessage(int.Parse(responseData.code), responseData.message);
                }
                else // 收到未知錯誤要改pending
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    _logger.LogError("GR Deposit Unknow Fail: {id}, {code}, {message}", RecordData.id.ToString(), responseData.code, responseData.message);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("GR Deposit TaskCanceledException: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("GR Deposit ExceptionMessage: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("GR Deposit Exception: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            return RecordData.status;
        }

        public async Task<string> Withdraw(GamePlatformUser platformUser, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            Platform platform = (Platform)Enum.Parse(typeof(Platform), RecordData.type, true);
            try
            {
                var requestData = new DebitBalanceV3Request();
                requestData.account = platformUser.game_user_id;
                // 轉入點數(無條件捨去到小數點第二位)
                requestData.debit_amount = Math.Round(transfer_amount, 2);
                // 自定義單號, 長度不超過 50 個字
                requestData.order_id = RecordData.id.ToString();

                var responseData = await _gameApiService._GrAPI.DebitBalanceV3(requestData);
                if (responseData.status == "Y")
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (responseData.status == "N" && (
                        responseData.code == "111090005" || // 使用者 Account 不存在
                        responseData.code == "112110001" || // 改變使用者餘額失敗,請檢查注單
                        responseData.code == "112110002" || //  輸入金額不能為負或零
                        responseData.code == "112240001" || //  使用者在遊戲中不能轉出金額錯誤
                        responseData.code == "112260001" || //  自定義的 OrderID 長度不能超過 50
                        responseData.code == "112280001" || //  代理額度不足
                        responseData.code == "112280002"  //  代理目前被鎖定無法轉入轉出
                    ))
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("GR Withdraw: {id}, {code}, {message}", RecordData.id.ToString(), responseData.code, responseData.message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    _logger.LogError("GR Withdraw: {id}, {code}, {message}", RecordData.id.ToString(), responseData.code, responseData.message);
                }
                return RecordData.status;
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("GR Withdraw TaskCanceledException: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("GR Withdraw ExceptionMessage: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("GR Withdraw Exception: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            return RecordData.status;
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
            int prefixLength = _prefixKey.Length;
            switch (Config.OneWalletAPI.Prefix_Key.ToLower().Trim())
            {
                case "local":
                case "dev":
                    result = propertyValue.Substring(prefixLength).Replace("@" + _grSiteCode, "");
                    break;

                case "uat":
                    result = propertyValue.Substring(prefixLength).Replace("@" + _grSiteCode, "");
                    break;

                case "prd":
                    result = propertyValue.Substring(prefixLength).Replace("@" + _grSiteCode, "");
                    break;

                default:
                    throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);
            }

            return result;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platformUser)
        {
            try
            {
                var requestData = new KickUserByAccountRequest();
                requestData.account = platformUser.game_user_id;
                var responseData = await _gameApiService._GrAPI.KickUserByAccount(requestData);
                if (responseData.status != "Y")
                {
                    _logger.LogInformation("踢出GR使用者失敗 id:{account} Msg: {Message}", platformUser.game_user_id, (responseData.code + responseData.message));
                }
                // 廠商建議請求踢線之後 等個1000毫秒=1秒再後續...因有快取還在線上的問題
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出GR使用者失敗 id:{account} Msg: {Message}", platformUser.game_user_id, ex.Message);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.GR.GR.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            var gameUser = new GamePlatformUser();
            var requestData = new RegUserInfoRequest();
            requestData.account = _prefixKey + userData.Club_id;
            requestData.display_name = _prefixKey + userData.Club_Ename;
            requestData.site_code = _grSiteCode;
            try
            {
                // RegUserInfoResponse
                // todo: 共用return方法
                var responseData = await _gameApiService._GrAPI.RegUserInfo(requestData);

                // todo: 回應錯誤代碼字串列舉
                if (responseData.status == "Y")
                {
                    // 成功
                }
                else if (responseData.status == "N" && responseData.code == "112100008") //(int)ErrorCodeEnum.Success)
                {
                    // "message": "此帳號已存在, 無法再次創建"
                    gameUser.game_user_id = requestData.account + "@" + _grSiteCode;
                }
                else
                {
                    throw new Exception(responseData.message);
                }
                gameUser.game_user_id = responseData.data.account;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.Fail, MessageCode.Message[(int)ResponseCode.Fail] + "|" + ex.Message.ToString());
            }
            gameUser.club_id = userData.Club_id;
            gameUser.game_platform = request.Platform;
            //gameUser.agent_id = requestData.WebId;
            return gameUser;
        }

        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 4 Get Game URL
            string gameUrl = string.Empty; //"{resposeUrl}/single/?sid={sid}&game_type={game_type}";
            var requestData = new GetSidByAccountRequest();
            requestData.account = platformUser.game_user_id;

            if (!request.GameConfig.ContainsKey("game_type"))
            {
                throw new Exception("game_type required");
            }

            try
            {
                // GetSidByAccountResponse
                var responseData = await _gameApiService._GrAPI.GetSidByAccount(requestData);
                if (responseData.status == "Y")
                {
                    // todo: 大廳沒做
                    gameUrl = responseData.data.game_url + "/single/?sid=" + responseData.data.sid + "&game_type=" + request.GameConfig["game_type"];
                    if (request.GameConfig.ContainsKey("lobbyURL") && request.GameConfig["lobbyURL"] != "")
                    {
                        gameUrl += "&leave_url=" + request.GameConfig["lobbyURL"];
                    }
                }
                else
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + responseData.code + responseData.message);
                }
                return gameUrl;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }

        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            var requestData = new CheckOrderExistV3Request();
            requestData.account = _prefixKey + transfer_record.Club_id + "@" + _grSiteCode;
            requestData.order_id = transfer_record.id.ToString();
            var responseData = await _gameApiService._GrAPI.CheckOrderExistV3(requestData);
            //responseData.data.order_state OrderStateEnum.Success
            //0.訂單不存在   -> 失敗
            //1.訂單處理中   -> 不理會 繼續保持 pending
            //2.訂單處理成功 -> 成功
            //3.訂單處理失敗 -> 失敗
            if (responseData.status == "Y" && responseData.data.order_state == 2)
            {
                if (transfer_record.target == nameof(Platform.GR)) //轉入GR直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.GR))
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
            else if (
                        (responseData.status == "Y" && responseData.data.order_state == 0) ||
                        (responseData.status == "Y" && responseData.data.order_state == 3) // || responseData.status == "N"
                    )
            {
                if (transfer_record.target == nameof(Platform.GR))//轉入GR直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.GR))
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

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            // 廠商目前沒有 實作完成 會再通知
            throw new NotImplementedException();
        }

        /// <summary>
        /// 補單 匯總
        /// 需要將輸入時間分段拉取(建議10分鐘1段)
        /// </summary>
        /// <param name="RepairReq"></param>
        /// <returns>要回傳新增注單筆數</returns>
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            var TotalRepairCount = 0;
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 10)
            {
                endTime = startTime.AddMinutes(10);
                RepairCount = await RepairGr(startTime, endTime);
                TotalRepairCount += RepairCount;
                startTime = endTime;
                _logger.LogDebug("Repair Gr record loop {startTime} ~ {endTime}, count: {RepairCount}", startTime, endTime, RepairCount);
            }
            RepairCount += await RepairGr(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            //await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, TotalRepairCount);
            _logger.LogDebug("Repair Gr record start Time : {startTime} end Time : {endTime}, {returnString}", startTime, RepairReq.EndTime, returnString);
            return returnString;
        }

        /// <summary>
        /// 補單
        /// 要確認起始與結束時間的格式與遊戲館拉取是否包含該段時間
        /// 重補注單需要取得W1明細先排除重複的再寫入
        /// 注單重拉之後要重新產出報表
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns>回傳新增注單筆數</returns>
        private async Task<int> RepairGr(DateTime startTime, DateTime endTime)
        {
            // 頁數1
            var Page = 1;
            var req = new CommBetDetailsRequest
            {
                // 請求 GR 起始時間 與 結束時間 是包含等於
                // 起始時間 <= 回傳資料時間 <= 結束時間
                start_time = startTime.AddHours(0).AddSeconds(1),
                end_time = endTime.AddHours(0).AddSeconds(0),
                page_index = Page,
                page_size = 10000
            };

            // 共用注單model
            CommBetDetailsResponse.DataInfo res = new CommBetDetailsResponse.DataInfo()
            {
                bet_details = new List<CommBetDetailsResponse.CommBetDetails>()
            };

            // 拉取 SLOT 注單
            while (true)
            {
                req.page_index = Page;
                var betData = await _gameApiService._GrAPI.GetSlotAllBetDetails(req);

                // 補單 廠商回失敗
                if (betData.status != "Y")
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameRecordFail, betData.code + "|" + betData.message);
                }

                if (betData.data.total_elements == 0)
                {
                    break;
                }
                res.bet_details.AddRange(betData.data.bet_details);

                Page++;
                if (Page > betData.data.total_pages)
                    break;

                //api建議 ? 秒爬一次
                await Task.Delay(1000);
            }

            // 拉取 FISH 注單
            // reset 頁數1
            Page = 1;
            while (true)
            {
                req.page_index = Page;
                var betData = await _gameApiService._GrAPI.GetFishAllBetDetails(req);

                // 補單 廠商回失敗
                if (betData.status != "Y")
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameRecordFail, betData.code + "|" + betData.message);
                }

                if (betData.data.total_elements == 0)
                {
                    break;
                }
                res.bet_details.AddRange(betData.data.bet_details);

                Page++;
                if (Page > betData.data.total_pages)
                    break;

                //api建議 ? 秒爬一次
                await Task.Delay(1000);
            }

            // 轉帳中心的歷史下注紀錄
            // 舊表資料
            var w1CenterList = await _grDBService.GetGrRecordsBytimeForRepair(startTime, endTime) ?? new List<CommBetDetails>();
            // 新表資料
            
            var repairList = new List<CommBetDetailsResponse.CommBetDetails>();
            foreach (var record in res.bet_details)
            {
                var hasData = w1CenterList.Where(x => x.sid == record.sid).Any();
                if (hasData == false)
                {
                    repairList.Add(record);
                }
            }
            var rescount=0;
            if (repairList.Count > 0)
            {
                rescount += await PostGRRecordDetail(repairList);
            }
            return rescount;
        }

        public async Task<bool> RecordSummary(Platform platform, DateTime reportDatetime, DateTime startTime, DateTime endTime)
        {
            var sw1 = System.Diagnostics.Stopwatch.StartNew();
            var summaryRecords = await _grDBService.SummaryGameRecord(reportDatetime, startTime, endTime);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            // 取得遊戲內帳號轉為為Club_id集合
            var userSummaries = summaryRecords.GroupBy(s => s.userid);
            var userlist = userSummaries.Select(x => ConvertGamePlatformUserToClubInfo(x.Key)).Distinct().ToList();
            // 批次處理，每次1000筆
            var userWalletList = (await Task.WhenAll(userlist.Chunk(1000).Select(async (betch) =>
            {
                return (await _commonService._serviceDB.GetWallet(betch));
            }))).SelectMany(x => x).ToDictionary(r => r.Club_id, r => r);

            var summaryRecordList = new List<(BetRecordSummary summay, HashSet<t_summary_bet_record_mapping> mappings)>();
            foreach (var summaryRecord in userSummaries)
            {
                if (!userWalletList.TryGetValue(ConvertGamePlatformUserToClubInfo(summaryRecord.Key), out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = Math.Abs(summaryRecord.Sum(x =>x.bet_valid));
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Game_type = 3;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x =>x.count);
                summaryData.Game_id = nameof(Platform.GR);
                summaryData.Bet_amount = Math.Abs(summaryRecord.Sum(x=>x.bet));
                summaryData.Win = summaryRecord.Sum(x=>x.win);
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

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Electronic;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._GrAPI.GetAgentGameList(new()
            {
                page_index = 1,
                page_size = 10,
            });
        }

        #endregion GameInterfaceService

        #region GameRecordService



        private bool wherePlatform(string propertyValue)
        {
            return propertyValue.StartsWith(_prefixKey.ToUpper());
        }
        /// <summary>
        /// 新增明細帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task<int> PostGRRecordDetail(List<CommBetDetailsResponse.CommBetDetails> recordData)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                recordData = recordData.Where(x => wherePlatform(x.account.ToUpper())).ToList();

                var Chucklist = recordData.Chunk(20000);
                var count = 0;
                foreach (IEnumerable<CommBetDetailsResponse.CommBetDetails> group in Chucklist)
                {
                    using (var tran = conn.BeginTransaction())
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        var betDetailData = new List<CommBetDetailsResponse.CommBetDetails>();
                        var dt = DateTime.Now;
                        var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);

                        // 紀錄 reportTime 跟 partition column create_time 的關聯
                        var dic = new Dictionary<string, HashSet<string>>();

                        foreach (CommBetDetailsResponse.CommBetDetails r in group)//loop club id bet detail
                        {
                            r.report_time = reportTime;
                            r.partition_time = r.create_time;
                            betDetailData.Add(r);

                            // 紀錄 reportTime 跟 partition column create_time 的關聯
                            var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                            if (!dic.ContainsKey(summaryTime))
                            {
                                dic.Add(summaryTime, new HashSet<string>());
                            }

                            dic[summaryTime].Add(r.create_time.ToString("yyyy-MM-dd HH:mm"));
                        }

                        try
                        {
                            count += await _grDBService.PostGrRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();

                            // 記錄到 Redis reportTime 跟 playTime 的關聯
                            foreach (var item in dic)
                            {
                                var key = $"{RedisCacheKeys.GrBetSummaryTime}:{item.Key}";
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
                    }
                }
                await conn.CloseAsync();
                return count;
            }
        }

        //private async Task PostGRRecord_backup(List<CommBetDetailsResponse.CommBetDetails> recordData)
        //{
        //    using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
        //    {
        //        await conn.OpenAsync();
        //        var linqRes = recordData.GroupBy(x => x.account);
        //        foreach (var group in linqRes)
        //        {
        //            using (var tran = await conn.BeginTransactionAsync())
        //            {
        //                try
        //                {
        //                    string club_id;
        //                    // 移除環境別前贅字 以及 廠商帳號後贅字
        //                    club_id = group.Key.Substring(3).Replace("@" + _grSiteCode, "");
        //                    Wallet memberWalletData = await GetWalletCache(club_id);
        //                    if (memberWalletData == null || memberWalletData.Club_id == null)
        //                    {
        //                        throw new Exception("沒有會員id");
        //                    }

        //                    var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.GR);
        //                    if (gameUser == null || gameUser.game_user_id != group.Key)
        //                    {
        //                        throw new Exception("No GR user");
        //                    }

        //                    //彙總注單
        //                    Dictionary<string, BetRecordSummary> summaryData =
        //                        new Dictionary<string, BetRecordSummary>();
        //                    //已結算注單
        //                    List<CommBetDetailsResponse.CommBetDetails> betDetailData = new List<CommBetDetailsResponse.CommBetDetails>();

        //                    foreach (CommBetDetailsResponse.CommBetDetails r in group) //loop club id bet detail
        //                    {
        //                        BetRecordSummary sumData = new BetRecordSummary();
        //                        sumData.Club_id = memberWalletData.Club_id;
        //                        sumData.Game_id = nameof(Platform.GR);
        //                        sumData.Game_type = 0;
        //                        DateTime tempDateTime = r.create_time;
        //                        tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
        //                        tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
        //                        tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
        //                        sumData.ReportDatetime = tempDateTime;
        //                        //確認是否已經超過搬帳時間 For H1 only
        //                        if (Config.OneWalletAPI.RCGMode == "H1")
        //                        {
        //                            if (DateTime.Now.Hour >= 12) //換日線
        //                            {
        //                                DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
        //                                    DateTime.Now.Day, 12, 00, 0);
        //                                if (sumData.ReportDatetime < ReportDateTime)
        //                                {
        //                                    sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
        //                                    _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.sid);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var lastday = DateTime.Now.AddDays(-1);
        //                                DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
        //                                if (sumData.ReportDatetime < ReportDateTime)
        //                                {
        //                                    sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
        //                                    _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.sid);
        //                                }
        //                            }
        //                        }

        //                        //先確認有沒有符合的匯總單
        //                        if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
        //                        {
        //                            sumData = summaryData[sumData.ReportDatetime.ToString()];
        //                            //合併處理
        //                            sumData = Calculate(sumData, r);
        //                            summaryData[sumData.ReportDatetime.ToString()] = sumData;
        //                        }
        //                        else
        //                        {
        //                            //用Club_id與ReportDatetime DB取得彙總注單
        //                            IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
        //                            if (results.Count() == 0) //沒資料就建立新的
        //                            {
        //                                //建立新的Summary
        //                                sumData.Currency = memberWalletData.Currency;
        //                                sumData.Franchiser_id = memberWalletData.Franchiser_id;

        //                                //合併處理
        //                                sumData = Calculate(sumData, r);
        //                            }
        //                            else //有資料就更新
        //                            {
        //                                sumData = results.SingleOrDefault();
        //                                //合併處理
        //                                sumData = Calculate(sumData, r);
        //                            }
        //                            summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
        //                        }
        //                        // r.summary_id = sumData.id;
        //                        betDetailData.Add(r);
        //                    }

        //                    List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
        //                    foreach (var s in summaryData)
        //                    {
        //                        summaryList.Add(s.Value);
        //                    }

        //                    int PostRecordResult = await _grDBService.PostGrRecord(conn, tran, betDetailData);
        //                    int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
        //                    _logger.LogDebug("insert GR record member: {group}, count: {count}", group.Key,
        //                        betDetailData.Count);
        //                    await tran.CommitAsync();
        //                }
        //                catch (Exception ex)
        //                {
        //                    foreach (CommBetDetailsResponse.CommBetDetails r in group) //loop club id bet detail
        //                    {
        //                        _logger.LogError("record id : {id}, time: {time}", r.sid, r.create_time);
        //                    }
        //                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
        //                    _logger.LogError("Run GR record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
        //                        group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
        //                    await tran.RollbackAsync();
        //                }
        //            }
        //        }

        //        await conn.CloseAsync();
        //    }
        //}

        /// <summary>
        /// 廠商未提供小時帳
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
                _logger.LogDebug("Create Ps game provider report time {datetime}", reportTime.ToString("yyyy-MM-dd"));

                DateTimeOffset unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                // 每日統計
                var req = await _gameApiService._GrAPI.GetReportList(new GetReportRequest()
                {
                    start_date = startDateTime.AddHours(0).ToString("yyyy-MM-dd"),
                    end_date = startDateTime.AddHours(0).ToString("yyyy-MM-dd"),
                    page_index = 1,
                    page_size = 1000
                });
                var gameEmptyReport = new GameReport();
                if (req.data.total_elements == 0)
                {
                    gameEmptyReport.platform = nameof(Platform.GR);
                    gameEmptyReport.report_datetime = reportTime;
                    gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                    gameEmptyReport.total_bet = 0;
                    gameEmptyReport.total_win = 0;
                    gameEmptyReport.total_netwin = 0;
                    gameEmptyReport.total_count = 0;

                }
                else
                {
                    foreach (var dateEntry in req.data.bet_report)
                    {
                        gameEmptyReport.platform = nameof(Platform.GR);
                        gameEmptyReport.report_datetime = reportTime;
                        gameEmptyReport.report_type = (int)GameReport.e_report_type.FinancalReport;
                        gameEmptyReport.total_bet += (decimal)dateEntry.total_bet;
                        gameEmptyReport.total_win += (decimal)dateEntry.total_win;
                        gameEmptyReport.total_netwin += ((decimal)dateEntry.total_win - (decimal)dateEntry.total_bet);
                        gameEmptyReport.total_count += dateEntry.total_bet_count;
                    }
                }

                await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                await _gameReportDBService.PostGameReport(gameEmptyReport);
                startDateTime = startDateTime.AddDays(1);

                await Task.Delay(3000);
            }
        }

        public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
        {
            while (true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, 0, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create GR game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _grDBService.SumGrBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.GR);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = totalBetValid;
                reportData.total_win = totalWin + totalBetValid;
                reportData.total_netwin = totalWin;
                reportData.total_count = totalCount;

                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);

                startDateTime = startDateTime.AddDays(1);
                await Task.Delay(3000);
            }
        }

        #endregion GameRecordService

        private BetRecordSummary Calculate(BetRecordSummary SummaryData, CommBetDetailsResponse.CommBetDetails r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += Math.Abs(r.bet);
            SummaryData.Turnover += Math.Abs(r.valid_bet);
            SummaryData.Netwin += r.profit;
            SummaryData.Win += r.win;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }

        /// <summary>
        /// 第二層明細
        /// </summary>
        /// <param name="RecordReq"></param>
        /// <returns></returns>
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {

            var summary = await _summaryDBService.GetRecordSummaryById(RecordReq);

            var partitions = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            GetBetRecord res = new GetBetRecord();
            if (summary.Game_type == 3) //Game_type = 3為電子注單，其餘為真人注單
            {
                List<dynamic> tp_results = new List<dynamic>();
                // 第二層明細舊表
                tp_results.AddRange(await _grDBService.GettGrRecordsBySummary(RecordReq));
                if (tp_results.Count == 0)
                {
                    foreach (var partition in partitions)
                    {
                        tp_results.AddRange(await _grDBService.GetGrRecordByReportTime(summary, partition, partition.AddDays(1).AddMilliseconds(-1),_prefixKey + summary.Club_id + "@" +_grSiteCode));
                    }
                }
                res.Data = tp_results.OrderByDescending(e => e.create_time).Select(x => x).ToList();
            }
            return res;
        }
    }
}