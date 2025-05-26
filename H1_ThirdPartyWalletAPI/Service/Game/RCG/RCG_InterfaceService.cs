using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using Microsoft.Extensions.Caching.Memory;
using RCGConfig = H1_ThirdPartyWalletAPI.Model.Game.RCG;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface IRcgInterfaceService : IGameInterfaceService
    {
        public Task PostRcgRecord(List<BetRecord> recordData, string systemCode, string webId);
        public Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
        //public Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
    }

    public class RCG_RecordService : IRcgInterfaceService
    {
        private readonly ILogger<RCG_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameApiService _gameApiService;
        private readonly IRcgDBService _rcgDBService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        private readonly IMemoryCache _memoryCache;
        private const string memory_cache_key = "RCG_System_Web_Code";
        private const int memory_cache_min = 15; //分鐘
        private const int _pageIndex = 1;   // 拉帳 預設起始頁
        private const int _pageSize = 1000; // 拉帳 預設筆數

        public RCG_RecordService(ILogger<RCG_RecordService> logger,
            ICommonService commonService,
            IGameApiService gameApiService,
            ISummaryDBService summaryDBService,
            IMemoryCache memoryCache,
            IRcgDBService rcgDbService, 
            IGameReportDBService gameReportDBService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _gameApiService = gameApiService;
            _summaryDBService = summaryDBService;
            _memoryCache = memoryCache;
            _rcgDBService = rcgDbService;
            _gameReportDBService = gameReportDBService;
        }
        #region GameInterfaceService
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            RCG_GetBalance req = new RCG_GetBalance();
            MemberBalance Balance = new MemberBalance();
            try
            {
                RcgToken rcgData = await _commonService._serviceDB.GetRcgToken(platform_user.club_id);
                if (rcgData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                req.memberAccount = platform_user.game_user_id;
                req.systemCode = rcgData.system_code;
                req.webId = rcgData.web_id;

                RCG_ResBase<RCG_GetBalance_Res> getRcgBalanceResult = await _gameApiService._RcgAPI.GetBalance(req);
                if (getRcgBalanceResult.msgId == (int)RCG.msgId.Success)
                {
                    Balance.Amount = getRcgBalanceResult.data.balance;
                }
                else
                {
                    Balance.Amount = 0;
                    Balance.code = getRcgBalanceResult.msgId;
                    Balance.Message = getRcgBalanceResult.message;
                    _logger.LogError("Rcg餘額取得失敗 Code:{errorCode} Msg: {Message}", getRcgBalanceResult.msgId, getRcgBalanceResult.message);
                }
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("RCG Credit exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }


            Balance.Wallet = nameof(Platform.RCG);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            RCG_ResBase<RCG_KickOut_Res> res = new RCG_ResBase<RCG_KickOut_Res>();
            try
            {
                var result = await _commonService._serviceDB.GetRcgToken(platform_user.club_id);
                if (result == null)
                {
                    throw new Exception("沒有使用者RCG token");
                }
                RCG_KickOut req = new RCG_KickOut();
                req.memberAccount = platform_user.club_id;
                req.webId = result.web_id;
                req.systemCode = result.system_code;
                res = await _gameApiService._RcgAPI.KickOut(req);

                if (res.msgId != (int)RCG.msgId.Success)
                {
                    _logger.LogInformation("踢出RCG使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, res.message);
                }
            }
            catch (Exception ex)
            {
                res.msgId = (int)RCG.msgId.Fail;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("KickRcgUser exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            return true;
        }

        public async Task<bool> KickAllUser(Platform platform)
        {
            var result = await _gameApiService._RcgAPI.KickOutByCompany();
            if (result.msgId != (int)RCG.msgId.Success)
            {
                throw new ExceptionMessage((int)ResponseCode.KickUserFail, MessageCode.Message[(int)ResponseCode.KickUserFail]);
            }
            return result.msgId == (int)RCG.msgId.Success;
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            try
            {
                var transfer_amount = RecordData.amount;
                var rcgData = await _commonService._serviceDB.GetRcgToken(platform_user.club_id);
                if (rcgData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                RCG_Deposit RCGTransferData = new RCG_Deposit();
                RCGTransferData.systemCode = rcgData.system_code;
                RCGTransferData.webId = rcgData.web_id;
                RCGTransferData.transactionId = RecordData.id.ToString();
                RCGTransferData.transctionAmount = transfer_amount;
                RCGTransferData.memberAccount = platform_user.game_user_id;
                RCG_ResBase<RCG_Deposit_Res> RCGTransferResult = await _gameApiService._RcgAPI.Deposit(RCGTransferData);
                if (RCGTransferResult.msgId == (int)RCG.msgId.Success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (RCGTransferResult.msgId == (int)RCG.msgId.TimeOut)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("FundTransferInRcgFail Msg: {Message}", RCGTransferResult.message);
                }
            }
            catch
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
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
                var rcgData = await _commonService._serviceDB.GetRcgToken(platform_user.club_id);
                if (rcgData == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                RCG_Withdraw RCGTransferData = new RCG_Withdraw();
                RCGTransferData.systemCode = rcgData.system_code;
                RCGTransferData.webId = rcgData.web_id;
                RCGTransferData.transactionId = RecordData.id.ToString();
                RCGTransferData.transctionAmount = game_balance;
                RCGTransferData.memberAccount = platform_user.game_user_id;
                RCG_ResBase<RCG_Withdraw_Res> RCGTransferResult = await _gameApiService._RcgAPI.Withdraw(RCGTransferData);
                if (RCGTransferResult.msgId == (int)RCG.msgId.Success)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else if (RCGTransferResult.msgId == (int)RCG.msgId.TimeOut)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                }
            }
            catch
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
            }
            return RecordData.status;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            ForwardGame res = new ForwardGame();
            //step 1 確認或建立遊戲商資料
            RCG_Login RCGloninReq = new RCG_Login();
            string systemcode = request.GameConfig["systemcode"];
            string webid = request.GameConfig["webid"];
            string lang = "en-US";
            if (request.GameConfig.ContainsKey("deskid"))
            {
                RCGloninReq.gameDeskID = request.GameConfig["deskid"];
            }
            if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && RCG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                lang = request.GameConfig["lang"];
            }
            if (!RCG.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                RCGloninReq.backUrl = request.GameConfig["lobbyURL"];
            }
            //H1特規自行Create User
            if (Config.OneWalletAPI.RCGMode != "H1")
            {
                RCG_CreateOrSetUser req = new RCG_CreateOrSetUser();

                req.memberAccount = userData.Club_id;
                req.memberName = userData.Club_Ename;
                req.webId = webid;
                req.systemCode = systemcode;
                req.currency = RCG.Currency[userData.Currency];
                req.language = RCG.lang[lang];
                if (request.GameConfig.ContainsKey("betLimitGroup"))
                {
                    req.betLimitGroup = request.GameConfig["betLimitGroup"];
                }
                if (request.GameConfig.ContainsKey("openGameList"))
                {
                    req.openGameList = request.GameConfig["openGameList"];
                }
                RCG_ResBase<RCG_CreateOrSetUser_Res> result = await _gameApiService._RcgAPI.CreateOrSetUser(req);
                if (result.msgId != (int)RCG.msgId.Success)
                {
                    throw new Exception("Rcg CreateOrSetUser fail");
                }
            }
            var results = await _commonService._serviceDB.GetRcgToken(userData.Club_id);

            if (results == null)
            {

                RcgToken rcgdata = new RcgToken();
                rcgdata.club_id = userData.Club_id;
                rcgdata.system_code = systemcode;
                rcgdata.web_id = webid;
                if (await _commonService._serviceDB.PostRcgToken(rcgdata) != 1)
                {
                    throw new ExceptionMessage((int)ResponseCode.CreateRcgUserTokenFail, MessageCode.Message[(int)ResponseCode.CreateRcgUserTokenFail]);
                }
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = userData.Club_id;
            gameUser.game_platform = request.Platform;
            gameUser.agent_id = webid;
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            RCG_Login RCGloninReq = new RCG_Login();
            string systemcode = request.GameConfig["systemcode"];
            string webid = request.GameConfig["webid"];
            string lang = "en-US";
            if (request.GameConfig.ContainsKey("deskid"))
            {
                RCGloninReq.gameDeskID = request.GameConfig["deskid"];
            }
            if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && RCG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                lang = RCG.lang[request.GameConfig["lang"]];
            }
            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                RCGloninReq.backUrl = request.GameConfig["lobbyURL"];
            }
            RCGloninReq.systemCode = systemcode;
            RCGloninReq.webId = webid;
            RCGloninReq.memberAccount = request.Club_id;
            RCGloninReq.lang = lang;
            RCG_ResBase<RCG_Login_Res> login_result = await _gameApiService._RcgAPI.Login(RCGloninReq);
            if (login_result.msgId != (int)RCG.msgId.Success)
            {
                var code = (int)ResponseCode.GetGameURLFail;
                var message = MessageCode.Message[(int)ResponseCode.GetGameURLFail] + " | " + login_result.message;
                throw new ExceptionMessage(code, message);
            }
            //更新RCG auth token
            if (await _commonService._serviceDB.PutRcgToken(platformUser.club_id, login_result.data.token) != 1)
            {
                throw new ExceptionMessage((int)ResponseCode.SaveRcgGameTokenFail, MessageCode.Message[(int)ResponseCode.SaveRcgGameTokenFail]);
            }
            return login_result.data.url;
        }
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            RCG_GetTransactionLog rCG_GetTransactionLog = new RCG_GetTransactionLog();
            var rcgData = await _commonService._serviceDB.GetRcgToken(transfer_record.Club_id);
            rCG_GetTransactionLog.systemCode = rcgData.system_code;
            rCG_GetTransactionLog.webId = rcgData.web_id;
            rCG_GetTransactionLog.transactionId = transfer_record.id.ToString();
            var rCG_TransactionLogRes = await _gameApiService._RcgAPI.GetTransactionLog(rCG_GetTransactionLog);
            if (rCG_TransactionLogRes.msgId == (int)RCG.msgId.Success && rCG_TransactionLogRes.data.status == 200)
            {
                if (transfer_record.target == nameof(Platform.RCG))//轉入RCG直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.RCG))
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
                if (transfer_record.target == nameof(Platform.RCG))//轉入RCGG直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RCG))
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
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            return res;
        }
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var rcgRunNo = await _rcgDBService.GetRcgRunNoById(long.Parse(RecordDetailReq.record_id), RecordDetailReq.ReportTime);
            if (rcgRunNo == null)
            {
                throw new Exception("no data");
            }

            var requestData = new RCG_GetOpenList();
            requestData.GameDeskID = rcgRunNo.desk;
            requestData.ActiveNo = rcgRunNo.activeNo;
            requestData.RunNo = rcgRunNo.runNo;
            requestData.Date = rcgRunNo.reportdt;

            try
            {
                var responseData = await _gameApiService._RcgAPI.GetOpenList(requestData);
                if (responseData.msgId != 0)
                {
                    _logger.LogInformation("RCG GameDetailURL 失敗 GameDeskID:{GameDeskID}, ActiveNo:{ActiveNo}, RunNo:{RunNo}, Date:{Date}, msgId: {msgId}, message: {message}",
                        requestData.GameDeskID, requestData.ActiveNo, requestData.RunNo, requestData.Date, responseData.msgId, responseData.message);
                    return (responseData.msgId).ToString() + ", " + responseData.message;
                }
                return responseData.data.dataList[0].url;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("RCG GameDetailURL 例外 GameDeskID:{GameDeskID}, ActiveNo:{ActiveNo}, RunNo:{RunNo}, Date:{Date} Msg: {Message}",
                    requestData.GameDeskID, requestData.ActiveNo, requestData.RunNo, requestData.Date, ex.Message);
                return ex.Message.ToString();
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
            DateTime startTime;
            DateTime endTime;
            string systemCode = string.Empty;
            string webId = string.Empty;

            // 取出各 web
            var systemWebCodeList = await _memoryCache.GetOrCreateAsync(memory_cache_key, async entry =>
            {
                IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetSystemWebCode();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(memory_cache_min));
                _memoryCache.Set(memory_cache_key, systemWebCode, cacheEntryOptions);
                return systemWebCode;
            });

            // loop 各 web 補帳
            foreach (var code in systemWebCodeList)
            {
                systemCode = code.system_code;
                webId = code.web_id;
                startTime = RepairReq.StartTime;
                endTime = RepairReq.EndTime;
                while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 10)
                {
                    endTime = startTime.AddMinutes(10);
                    RepairCount = await RepairRCG(systemCode, webId, startTime, endTime);
                    TotalRepairCount += RepairCount;
                    startTime = endTime;
                    _logger.LogDebug("Repair RCG record {systemCode}/{webId} loop {startTime} ~ {endTime}, count: {RepairCount}", systemCode, webId, startTime, endTime, RepairCount, RepairCount);
                }
                TotalRepairCount += await RepairRCG(systemCode, webId, startTime, RepairReq.EndTime);
            }

            await Task.Delay(1000);
            await SummaryW1Report(RepairReq.StartTime, RepairReq.EndTime);
            //await SummaryGameProviderReport(RepairReq.StartTime, RepairReq.EndTime);
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, TotalRepairCount);
            _logger.LogDebug("Repair RCG record start Time : {startTime} end Time : {endTime}, {returnString}", RepairReq.StartTime, RepairReq.EndTime, returnString);
            return returnString;
        }

        /// <summary>
        /// 補單
        /// 要確認起始與結束時間的格式與遊戲館拉取是否包含該段時間
        /// 重補注單需要取得W1明細先排除重複的再寫入
        /// 注單重拉之後要重新產出報表
        /// </summary>
        /// <param name="systemCode"></param>
        /// <param name="webId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns回傳新增注單筆數></returns>
        private async Task<int> RepairRCG(string systemCode, string webId, DateTime startTime, DateTime endTime)
        {
            RCG_H1GetBetRecordListByDateRange request = new RCG_H1GetBetRecordListByDateRange();
            // PostRcgRecord 注單 model
            RCG_GetBetRecordList_Res res = new RCG_GetBetRecordList_Res()
            {
                dataList = new List<BetRecord>()
            };

            int totalRepairCount = 0;
            int pageIndex = _pageIndex;
            request.systemCode = systemCode;
            request.webId = webId;
            request.startDate = startTime.AddHours(0).AddSeconds(0);
            request.endDate = endTime.AddHours(0).AddSeconds(0);
            request.pageSize = _pageSize; // 超過1000筆噴錯...

            // 分頁拉取注單
            while (true)
            {
                request.pageIndex = pageIndex;
                var betData = await _gameApiService._RcgAPI.H1GetBetRecordListByDateRange(request);

                // 確認下rcg回傳的總筆數
                if (betData.data.total == 0)
                {
                    break;
                }

                // 確認下rcg回傳的注單陣列
                if (betData.data.dataList.Count <= 0)
                {
                    break;
                }

                // 確認下取得筆數 是否在往下一個分頁呼叫
                if (res.dataList.Count >= betData.data.total)
                {
                    break;
                }

                res.dataList.AddRange(
                    betData.data.dataList.Select(
                        x => new BetRecord
                        {
                            memberAccount = x.memberAccount,
                            id = x.recordId,
                            gameId = x.gameId,
                            desk = x.serverId,
                            betArea = x.areaId,
                            bet = x.betPoint,
                            available = x.pointEffective,
                            winLose = x.winLosePoint,
                            waterRate = x.mbDiscountRate,
                            activeNo = x.noRun,
                            runNo = x.noActive,
                            balance = x.balance,
                            dateTime = x.betDT,
                            reportDT = x.reportDT,
                            ip = x.ip,
                            odds = x.odds,
                            originRecordId = x.originRecordId,
                            //transactions = x.transactions
                        }
                    ).ToArray()
                );

                pageIndex++;

                //api建議 ? 秒爬一次
                await Task.Delay(1000);
            }

            // 轉帳中心的歷史下注紀錄
            var w1CenterList = await _rcgDBService.GetRcgRecordByWebForRepair(systemCode, webId, startTime, endTime);

            // 比對不重複的注單
            var repairList = new List<BetRecord>();
            foreach (var record in res.dataList)
            {
                var hasData = w1CenterList.Where(x => x.id == record.id).Any();
                if (hasData == false)
                {
                    repairList.Add(record);
                }
            }
            if (repairList.Count > 0)
            {
                await PostRcgRecord(repairList, systemCode, webId);
                totalRepairCount += repairList.Count;
            }

            return totalRepairCount;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Live;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._RcgAPI.HelloWorld();
        }
        #endregion

        #region GameRecordService
        public async Task PostRcgRecord(List<BetRecord> recordData, string systemCode, string webId)
        {
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();
                var linqRes = recordData.GroupBy(x => x.memberAccount);
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            string club_id;
                            // 移除環境別前贅字
                            // H1 自己呼 RCG 建帳號...沒過環境別的前墜
                            // club_id = group.Key.Substring(3);
                            club_id = group.Key;
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.RCG);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No RCG user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData =
                                new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<BetRecord> betDetailData = new List<BetRecord>();

                            foreach (BetRecord r in group) //loop club id bet detail
                            {
                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.RCG);
                                // 匯總條件 使用遊戲桌 桌別需轉換 自行定義...!!
                                sumData.Game_type = RCGConfig.LiveGameMap.CodeToId[r.desk];
                                DateTime tempDateTime = DateTime.Now;
                                //DateTime tempDateTime = r.reportDT; // ??
                                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                sumData.ReportDatetime = tempDateTime;
                                r.systemCode = systemCode;
                                r.webId = webId;

                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString() + sumData.Game_type.ToString()];
                                    //合併處理
                                    sumData = await Calculate(sumData, r);
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
                                        sumData = await Calculate(sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = await Calculate(sumData, r);
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

                            int PostRecordResult = await _rcgDBService.PostRcgRecord(conn, tran, betDetailData);
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            _logger.LogDebug("insert RCG record member: {group}, count: {count}", group.Key,
                                betDetailData.Count);
                            await tran.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            foreach (BetRecord r in group) //loop club id bet detail
                            {
                                _logger.LogError("record id : {id}, time: {time}", r.id, r.reportDT);

                            }
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run RCG record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorLine}",
                                group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }

                await conn.CloseAsync();
            }
        }

        private async Task<BetRecordSummary> Calculate(BetRecordSummary SummaryData, BetRecord r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.bet;
            SummaryData.Turnover += r.available;
            SummaryData.Netwin += r.winLose + r.available;
            SummaryData.Win += r.winLose;
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
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
                _logger.LogDebug("Create RCG game W1 report time {datetime}", reportTime);
                var (totalCount, totalBetValid, totalWin) = await _rcgDBService.SumRcgBetRecordByReportdt(reportTime, endDateTime);

                GameReport reportData = new();
                reportData.platform = nameof(Platform.RCG);
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

        #endregion
    }

}
