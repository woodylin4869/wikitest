using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.XG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using H1_ThirdPartyWalletAPI.Model.Game.WS168.Request;

namespace H1_ThirdPartyWalletAPI.Service.Game.XG
{
    public interface IXGInterfaceService : IGameInterfaceService
    {
        Task<int> PostXGRecord(List<GetBetRecordByTimeResponse.Result> recordData);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    }

    public class XG_RecordService : IXGInterfaceService
    {
        private readonly ILogger<XG_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameApiService _gameApiService;
        private readonly IXgDBService _xgDBService;
        private readonly IDBService _dbService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ICacheDataService _cacheService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        const int pageLimit = 100;
        const int getDelayMS = 200;
        private readonly string _prefixKey;
        private readonly string _xgAgentId;

        public XG_RecordService(ILogger<XG_RecordService> logger,
            ICommonService commonService,
            IGameApiService gameaApiService,
            ISummaryDBService summaryDbService,
            IXgDBService xgDBService,
            IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameaApiService;
            _prefixKey = Config.OneWalletAPI.Prefix_Key;
            _xgDBService = xgDBService;
            _dbService = commonService._serviceDB;
            _summaryDBService = summaryDbService;
            _cacheService = commonService._cacheDataService;
            _xgAgentId = Config.CompanyToken.XG_AgentID;
            _gameReportDBService = gameReportDBService;
        }

        #region GameInterfaceService

        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platformUser)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                var requestData = new AccountRequest();
                requestData.AgentId = _xgAgentId;
                requestData.Account = platformUser.game_user_id;
                var responseData = await _gameApiService._XgAPI.Account(requestData);

                if (responseData.ErrorCode != 0)
                {
                    Balance.Amount = 0;
                    Balance.code = (int)ResponseCode.Fail;
                    Balance.Message = responseData.ErrorCode.ToString() + responseData.Message;
                }
                Balance.Amount = responseData.Data.Balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("XG餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.XG);
            return Balance;
        }

        public async Task<string> Deposit(GamePlatformUser platformUser, Wallet walletData, WalletTransferRecord RecordData)
        {
            if (!Model.Game.XG.XG.Currency.ContainsKey(walletData.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            var transfer_amount = RecordData.amount;
            try
            {
                var requestData = new TransferRequest();
                requestData.AgentId = _xgAgentId;
                requestData.TransferType = (int)TransferTypeEnum.Deposit;
                requestData.Account = platformUser.game_user_id;
                // 轉入點數(無條件捨去到小數點第二位)
                requestData.Amount = Math.Round(transfer_amount, 2);
                // 自定義單號，限英數字，長度 4 ~ 40 字
                requestData.TransactionId = RecordData.id.ToString().Replace("-", "");
                var responseData = await _gameApiService._XgAPI.Transfer(requestData);
                if (responseData.ErrorCode == 0 && responseData.Data.Status == (int)TransferStatusEnum.Success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (
                    (responseData.ErrorCode == 0 && responseData.Data.Status == (int)TransferStatusEnum.Fail) ||
                    (responseData.ErrorCode == 15) // 15  參數是必須的 或 數據格式錯誤 或 參數驗證錯誤
                    )
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("XG Deposit Fail: {id}, {code}, {message}", RecordData.id.ToString(), responseData.ErrorCode, responseData.Message);
                }
                else // 收到未知錯誤要改pending
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    _logger.LogError("XG Deposit WTF Fail: {id}, {code}, {message}", RecordData.id.ToString(), responseData.ErrorCode, responseData.Message);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("XG Deposit TaskCanceledException: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("XG Deposit ExceptionMessage: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("XG Deposit Exception: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            return RecordData.status;
        }

        public async Task<string> Withdraw(GamePlatformUser platformUser, Wallet walletData, WalletTransferRecord RecordData)
        {
            if (!Model.Game.XG.XG.Currency.ContainsKey(walletData.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            var transfer_amount = RecordData.amount;
            Platform platform = (Platform)Enum.Parse(typeof(Platform), RecordData.type, true);
            try
            {
                var requestData = new TransferRequest();
                requestData.AgentId = _xgAgentId;
                requestData.TransferType = (int)TransferTypeEnum.Withdraw;
                requestData.Account = platformUser.game_user_id;
                // 轉入點數(無條件捨去到小數點第二位)
                requestData.Amount = Math.Round(transfer_amount, 2);
                // 自定義單號，限英數字，長度 4 ~ 40 字
                requestData.TransactionId = RecordData.id.ToString().Replace("-", "");
                var responseData = await _gameApiService._XgAPI.Transfer(requestData);
                if (responseData.ErrorCode == 0 && responseData.Data.Status == (int)TransferStatusEnum.Success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (
                    (responseData.ErrorCode == 0 && responseData.Data.Status == (int)TransferStatusEnum.Fail) ||
                    (responseData.ErrorCode == 15) // 15  參數是必須的 或 數據格式錯誤 或 參數驗證錯誤
                    )
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("XG Withdraw Fail: {id}, {code}, {message}", RecordData.id.ToString(), responseData.ErrorCode, responseData.Message);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    _logger.LogError("XG Withdraw WTF Fail: {id}, {code}, {message}", RecordData.id.ToString(), responseData.ErrorCode, responseData.Message);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("XG Withdraw TaskCanceledException: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("XG Withdraw ExceptionMessage: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("XG Withdraw Exception: {id}, {ex}", RecordData.id.ToString(), ex.Message);
            }
            return RecordData.status;
        }

        public async Task<bool> KickUser(Platform platform, GamePlatformUser platformUser)
        {
            try
            {
                var requestData = new KickMemberRequest();
                requestData.AgentId = _xgAgentId;
                requestData.Account = platformUser.game_user_id;
                var responseData = await _gameApiService._XgAPI.KickMember(requestData);
                if (responseData.ErrorCode != 0)
                {
                    _logger.LogInformation("XG踢線失敗 id:{account} Msg: {Message}", platformUser.game_user_id, (responseData.ErrorCode + responseData.Message));
                }
                // 廠商建議請求踢線之後 等個1秒再後續...因有快取還在線上的問題!?
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("XG踢線例外 id:{account} Msg: {Message}", platformUser.game_user_id, ex.Message);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.XG.XG.Currency.ContainsKey(userData.Currency))
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);

            var gameUser = new GamePlatformUser();
            var requestData = new CreateMemberRequest();
            requestData.AgentId = _xgAgentId;
            requestData.Account = _prefixKey + userData.Club_id;
            requestData.Currency = Model.Game.XG.XG.Currency[userData.Currency];
            try
            {
                // RegUserInfoResponse
                // todo: 共用return方法 
                var responseData = await _gameApiService._XgAPI.CreateMember(requestData);

                // todo: 回應錯誤代碼字串列舉
                if (responseData.ErrorCode == 0)
                {
                    // 成功
                }
                else if (responseData.ErrorCode == 8) // 8	帳號已存在	404
                {
                    // "Message": "xxx 会员帐号重复"
                    gameUser.game_user_id = requestData.Account;
                }
                else
                {
                    throw new Exception(responseData.Message);
                }
                gameUser.game_user_id = requestData.Account;
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
            try
            {
                // Step 1: 設定會員限注
                // 不應沒帶限注就開不了遊戲
                if (request.GameConfig.ContainsKey("betLimitGroup") && request.GameConfig["betLimitGroup"] != null && request.GameConfig["betLimitGroup"] != "")
                {
                    // 限注的設定 每次產遊戲連結時會去set limit (由請求端決定 在w1 login時 betLimitGroup 不帶  就不去多叫廠商set limit ...可能之後在優化)
                    var requestSetData = new SetTemplateRequest();
                    requestSetData.AgentId = _xgAgentId;
                    requestSetData.Account = platformUser.game_user_id;
                    requestSetData.Template = request.GameConfig["betLimitGroup"];
                    var responseSetData = await _gameApiService._XgAPI.SetTemplate(requestSetData);
                    if (responseSetData.ErrorCode != 0)
                    {
                        throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + responseSetData.ErrorCode + responseSetData.Message);
                    }
                }

                // Step 2: 取遊戲連結 
                // XG 沒提供 leave_url
                string gameUrl = string.Empty;
                var requestData = new LoginRequest();
                requestData.AgentId = _xgAgentId;
                requestData.Account = platformUser.game_user_id;

                // 遊戲類別，直接入桌時使用 (現況 w1 直接轉大廳 此參數可有可無)
                if (request.GameConfig.ContainsKey("GameType") && request.GameConfig["GameType"] != null && request.GameConfig["GameType"] != "")
                {
                    requestData.GameType = Int32.Parse(request.GameConfig["GameType"]);
                }

                // 遊戲局桌檯Id，直接入桌時使用 (現況 w1 直接轉大廳 此參數可有可無)
                if (request.GameConfig.ContainsKey("TableId") && request.GameConfig["TableId"] != null && request.GameConfig["TableId"] != "")
                {
                    requestData.TableId = request.GameConfig["TableId"];
                }

                // 進遊戲語系
                if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && Model.Game.XG.XG.GameLang.ContainsKey(request.GameConfig["lang"]))
                {
                    requestData.Lang = Model.Game.XG.XG.GameLang[request.GameConfig["lang"]];
                }
                else
                {
                    requestData.Lang = Model.Game.XG.XG.GameLang["en-US"];
                }

                var responseData = await _gameApiService._XgAPI.Login(requestData);
                if (responseData.ErrorCode == 0)
                {
                    gameUrl = responseData.Data.LoginUrl;
                }
                else
                {
                    throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + responseData.ErrorCode + responseData.Message);
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
            var requestData = new CheckTransferRequest();
            requestData.AgentId = _xgAgentId;
            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(transfer_record.Club_id, Platform.XG);
            requestData.Account = gameUser.game_user_id;
            // 當時自定義單號，限英數字，長度 4 ~ 40 字
            requestData.TransactionId = transfer_record.id.ToString().Replace("-", "");
            var responseData = await _gameApiService._XgAPI.CheckTransfer(requestData);
            // 交易狀態，1 = 成功，2 = 失敗，9 = 處理中
            if (responseData.ErrorCode == 0 && responseData.Data.Status == (int)TransferStatusEnum.Success)
            {
                if (transfer_record.target == nameof(Platform.XG))//轉入XG直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.XG))
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
                (responseData.ErrorCode == 0 && responseData.Data.Status == (int)TransferStatusEnum.Fail) ||
                (responseData.ErrorCode == 4) // 4 未找到該資料 404
                )
            {
                if (transfer_record.target == nameof(Platform.XG))//轉入XG直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.XG))
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
            var requestData = new GetGameDetailUrlRequest();
            requestData.AgentId = _xgAgentId;
            requestData.WagersId = RecordDetailReq.record_id;
            if (Model.Game.XG.XG.ApiLang.ContainsKey(RecordDetailReq.lang))
            {
                requestData.ApiLang = Model.Game.XG.XG.ApiLang[RecordDetailReq.lang];
            }
            else
            {
                requestData.ApiLang = Model.Game.XG.XG.ApiLang["en-US"];
            }
            try
            {
                var responseData = await _gameApiService._XgAPI.GetGameDetailUrl(requestData);
                if (responseData.ErrorCode != 0)
                {
                    _logger.LogInformation("XG GameDetailURL 失敗 WagersId:{WagersId} Msg: {Message}", requestData.WagersId, (responseData.ErrorCode + responseData.Message));
                }
                return responseData.Data;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("XG GameDetailURL 例外 WagersId:{WagersId} Msg: {Message}", requestData.WagersId, ex.Message);
                return String.Empty;
            }
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
                RepairCount = await RepairXG(startTime, endTime);
                TotalRepairCount += RepairCount;
                startTime = endTime;
                _logger.LogDebug("Repair XG record loop {startTime} ~ {endTime}, count: {RepairCount}", startTime, endTime, RepairCount);
            }
            TotalRepairCount += await RepairXG(startTime, RepairReq.EndTime);
            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, TotalRepairCount);
            _logger.LogDebug("Repair XG record start Time : {startTime} end Time : {endTime}, {returnString}", startTime, RepairReq.EndTime, returnString);
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
        private async Task<int> RepairXG(DateTime startTime, DateTime endTime)
        {
            // 頁數1
            var Page = 1;
            var req = new GetBetRecordByTimeRequest
            {
                // 請求 XG 起始時間 <= 回傳資料時間 <= 結束時間
                // XG 請求時間以及回傳時間的欄位 皆是(UTC-4) 最後改在請求參數時先轉換
                // XG 有限制 請求的起訖時間要小於1小時
                AgentId = _xgAgentId,
                StartTime = DateTime.Parse(startTime.ToString("yyyy-MM-ddTHH:mm:ss")).AddHours(-12),
                EndTime = DateTime.Parse(endTime.ToString("yyyy-MM-ddTHH:mm:ss")).AddHours(-12).AddSeconds(-1),
                Page = Page,
                PageLimit = 10000
            };

            // 共用注單model
            GetBetRecordByTimeResponse.DataInfo res = new GetBetRecordByTimeResponse.DataInfo()
            {
                Result = new List<GetBetRecordByTimeResponse.Result>()
            };

            // 分頁拉取注單
            while (true)
            {
                req.Page = Page;
                var betData = await _gameApiService._XgAPI.GetBetRecordByTime(req);

                if (betData.Data.Pagination.TotalNumber == 0)
                {
                    break;
                }
                res.Result.AddRange(betData.Data.Result);

                Page++;
                if (Page > betData.Data.Pagination.TotalPages)
                    break;

                //api建議 ? 秒爬一次
                await Task.Delay(1000);
            }

            // 轉帳中心的歷史下注紀錄
            var w1CenterList = new List<GetBetRecordByTimeResponse.Result>();
            foreach (var group in res.Result.GroupBy(r => r.WagersTime.Ticks - (r.WagersTime.Ticks % TimeSpan.FromHours(3).Ticks)))
            {
                var minWagersTime = group.Min(r => r.WagersTime).AddHours(12);
                var maxWagersTime = group.Max(r => r.WagersTime).AddHours(12).AddSeconds(1);
                w1CenterList.AddRange(await _xgDBService.GetXgRecordsBytime(minWagersTime, maxWagersTime));
            }

            var w1RecordPK = w1CenterList.Select(r => new { r.WagersId, r.WagersTime, r.PayoffTime }).ToHashSet();

            var repairList = res.Result.Where(item => !w1RecordPK.Contains(new { item.WagersId, WagersTime = item.WagersTime.AddHours(12), PayoffTime = item.PayoffTime.AddHours(12) })).ToList();

            if (repairList.Any())
                return await PostXGRecord(repairList);

            return 0;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._XgAPI.Health(new()
            {
                AgentId = _xgAgentId
            });
        }
        #endregion

        #region GameRecordService

        public async Task<int> PostXGRecord(List<GetBetRecordByTimeResponse.Result> recordData)
        {
            var postRsult = 0;
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                var linqRes = recordData
                    .Where(x => x.Account.StartsWith(_prefixKey))
                    .GroupBy(x => x.Account);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            // 移除環境別前贅字
                            var club_id = group.Key[_prefixKey.Length..];
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.XG);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No XG user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData =
                                new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<GetBetRecordByTimeResponse.Result> betDetailData = new List<GetBetRecordByTimeResponse.Result>();

                            foreach (GetBetRecordByTimeResponse.Result r in group) //loop club id bet detail
                            {
                                r.WagersTime = r.WagersTime.AddHours(12);
                                r.SettlementTime = r.SettlementTime.AddHours(12);
                                r.PayoffTime = r.PayoffTime.AddHours(12);
                                // 若當下取到Transactions為null時就塞空字串
                                r.Transactions = (r.Transactions != null ? r.Transactions : "");

                                //真人注單有改牌邏輯，需先儲存原始資料
                                r.pre_BetAmount = r.BetAmount;
                                r.pre_validBetAmount = r.validBetAmount;
                                r.pre_PayoffAmount = r.PayoffAmount;
                                r.pre_Status = r.Status;

                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.XG);
                                sumData.Game_type = r.GameType;
                                DateTime tempDateTime = DateTime.Now;
                              
                                sumData.ReportDatetime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day, tempDateTime.Hour, (tempDateTime.Minute / 5) * 5, 0);


                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                    //合併處理
                                    sumData = await Calculate(tran, sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    if (results.Count() == 0) //沒資料就建立新的
                                    {
                                        //建立新的Summary
                                        sumData.Currency = memberWalletData.Currency;
                                        sumData.Franchiser_id = memberWalletData.Franchiser_id;

                                        //合併處理
                                        sumData = await Calculate(tran, sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = await Calculate(tran, sumData, r);
                                    }
                                    summaryData.Add(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString(), sumData);
                                }
                                r.summary_id = sumData.id;
                                betDetailData.Add(r);
                            }

                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                summaryList.Add(s.Value);
                            }

                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            postRsult += await _xgDBService.PostXgRecord(conn, tran, betDetailData);
                            _logger.LogDebug("insert XG record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (GetBetRecordByTimeResponse.Result r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.WagersId, r.PayoffTime);

                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run XG record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }
                    }
                }

                await conn.CloseAsync();
            }

            return postRsult;
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
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);

                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }

                _logger.LogDebug("Create XG game provider report time {datetime}", reportTime);

                // 每小時投注匯總
                var requestData = new GetApiReportUrlRequest()
                {
                    // 請求 XG 起始時間 <= 回傳資料時間 <= 結束時間
                    // XG 請求時間以及回傳時間的欄位 皆是(UTC-4) 最後改在請求參數時先轉換
                    // XG 有限制 請求的起訖時間要小於1小時
                    AgentId = _xgAgentId,
                    StartTime = DateTime.Parse(reportTime.ToString("yyyy-MM-ddTHH:mm:ss")).AddHours(-12),
                    EndTime = DateTime.Parse(reportTime.ToString("yyyy-MM-ddTHH:mm:ss")).AddHours(1).AddHours(-12).AddSeconds(-1)
                };

                var report = new GetApiReportUrlResponse.DataInfo();
                foreach(var currency in Model.Game.XG.XG.Currency.Values)
                {
                    requestData.Currency = currency;
                    //取得這小時
                    GetApiReportUrlResponse responseData = await _gameApiService._XgAPI.GetApiReportUrl(requestData);
                    report.WagersCount += responseData.Data.WagersCount;
                    report.ActualBettingAmount += responseData.Data.ActualBettingAmount;
                    report.TotalPayoff += responseData.Data.TotalPayoff;
                }

                if (report.WagersCount == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.XG),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = 0,
                        total_win = 0,
                        total_netwin = 0,
                        total_count = 0
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddHours(1);
                }
                else
                {
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.XG),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = report.ActualBettingAmount,
                        total_win = report.ActualBettingAmount + report.TotalPayoff,
                        total_netwin = report.TotalPayoff,
                        total_count = report.WagersCount
                    };

                    await _gameReportDBService.DeleteGameReport(gameEmptyReport);
                    await _gameReportDBService.PostGameReport(gameEmptyReport);
                    startDateTime = startDateTime.AddHours(1);
                }
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
                _logger.LogDebug("Create XG game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _xgDBService.SumXgBetRecordByBetTime(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.XG);
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

        #endregion

        private async Task<BetRecordSummary> Calculate(IDbTransaction tran, BetRecordSummary SummaryData, GetBetRecordByTimeResponse.Result r)
        {
            switch (r.Status)
            {
                case (int)BetStatusEnum.Cancel:
                case (int)BetStatusEnum.Change:
                    var oldRecords = await _xgDBService.GetXgRecords(tran, r.WagersId, r.WagersTime);
                    oldRecords ??= new();
                    if (oldRecords.Any(oldr => new { oldr.WagersId, oldr.PayoffAmount, oldr.Status, oldr.PayoffTime }.Equals(new { r.WagersId, r.PayoffAmount, r.Status, updatedAt = DateTime.Parse(r.PayoffTime.ToString("yyyy-MM-ddTHH:mm:ss")).AddHours(-12) })))
                    {
                        return SummaryData;
                    }

                    if (oldRecords.Any())
                    {
                        var lastRecord = oldRecords.OrderByDescending(r => r.PayoffTime).First(); //僅需沖銷最後一筆即可
                        r.BetAmount = r.BetAmount - lastRecord.pre_BetAmount;
                        r.validBetAmount = r.validBetAmount - lastRecord.pre_validBetAmount;
                        r.PayoffAmount = r.PayoffAmount - lastRecord.pre_PayoffAmount;
                    }
                    break;
            }
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.BetAmount;
            SummaryData.Turnover += r.validBetAmount;
            SummaryData.Netwin += r.PayoffAmount;
            SummaryData.Win += r.PayoffAmount + r.BetAmount;
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

        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            IEnumerable<dynamic> xg_results = await _xgDBService.GetXgRecordsBySummary(RecordReq);
            xg_results = xg_results.OrderByDescending(e => e.PayoffTime);
            res.Data = xg_results.ToList();
            return res;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Live;
        }
    }
}
