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
using System.Data;
using Microsoft.AspNetCore.Hosting;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;

namespace H1_ThirdPartyWalletAPI.Service.Game
{
    public interface ISabaInterfaceService : IGameInterfaceService
    {
        public Task<ResCodeBase> PostSabaRecord(SABA_Game_Record recordData);
        public Task<ResCodeBase> PostGameReport(SABA_GetFinancialReportData sabaReportData);
        public Task<ResCodeBase> CreateGameReportFromBetRecord(DateTime reportDate);
        public Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
    }
    public class SABA_InterfaceService : ISabaInterfaceService
    {
        private readonly ILogger<SABA_InterfaceService> _logger;
        private readonly ICommonService _commonService;
        private readonly IWebHostEnvironment _env;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ISabaDbService _sabaDbService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _BalanceCacheSeconds = 10;
        const int _cacheFranchiserUser = 1800;
        const int _BetTypeCacheSeconds = 600;
        public SABA_InterfaceService(ILogger<SABA_InterfaceService> logger, 
            ICommonService commonService,
            IWebHostEnvironment env,
            IGameApiService gameaApiService,
            ISummaryDBService summaryDBService,
            ISabaDbService sabaDbService, 
            IGameReportDBService gameReportDBService
            )
        {
            _logger = logger;
            _commonService = commonService;
            _env = env;
            _gameReportDBService = gameReportDBService;
            _gameApiService = gameaApiService;
            _summaryDBService = summaryDBService;
            _sabaDbService = sabaDbService;
        }
        #region GameInterfaceService
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            MemberBalance Balance = new MemberBalance();
            SABA_CheckUserBalance getBalanceData = new SABA_CheckUserBalance();
            getBalanceData.vendor_member_ids = platform_user.game_user_id;
            SABA_CheckUserBalance_Res getBalanceResult = await _gameApiService._SabaAPI.CheckUserBalance(getBalanceData);
            if (getBalanceResult.error_code == (int)SABA_CheckUserBalance_Res.ErrorCode.Success)
            {
                Balance.Amount = getBalanceResult.Data[0].balance.GetValueOrDefault();
            }
            else
            {
                Balance.Amount = 0;
                Balance.code = getBalanceResult.error_code;
                Balance.Message = getBalanceResult.message;
                _logger.LogError("Saba餘額取得失敗 Code:{errorCode} Msg: {Message}", getBalanceResult.error_code, getBalanceResult.message);
            }
            Balance.Wallet = nameof(Platform.SABA);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            SABA_KickUser reqData = new SABA_KickUser();
            reqData.vendor_member_id = platform_user.game_user_id;
            SABA_KickUser_Res resData = await _gameApiService._SabaAPI.KickUser(reqData);
            if (resData.error_code != (int)SABA_KickUser_Res.ErrorCode.Success)
            {
                _logger.LogInformation("踢出SABA使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, resData.message);
            }
            return true;
        }
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            try
            {
                var transfer_amount = RecordData.amount;
                SABA_FundTransfer transferData = new SABA_FundTransfer();
                transferData.currency = SABA.Currency[walletData.Currency];
                transferData.amount = transfer_amount;
                transferData.direction = 1; //轉入遊戲
                transferData.vendor_member_id = platform_user.game_user_id;
                transferData.vendor_trans_id = RecordData.id.ToString();
                SABA_FundTransfer_Res transferResult = await _gameApiService._SabaAPI.FundTransfer(transferData);
                if (transferResult.error_code == (int)ResponseCode.TimeOut || transferResult.Data.status == (int)SABA_FundTransfer_Res.Status.Pending)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                }
                else if (transferResult.error_code == (int)ResponseCode.GameApiMaintain)
                {
                    PutApiHealthReq putApiHealthReq = new PutApiHealthReq();
                    putApiHealthReq.Platform = RecordData.type;
                    putApiHealthReq.Status = Status.MAINTAIN;
                    putApiHealthReq.Operator = RecordData.type;
                    await _commonService._apiHealthCheck.SetPlatformHealthInfo(putApiHealthReq);
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                }
                else if (transferResult.Data.status == (int)SABA_FundTransfer_Res.Status.OK)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                    _logger.LogError("FundTransferInSabaFail Msg: {Message}", transferResult.message);
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
                SABA_FundTransfer transferData = new SABA_FundTransfer();
                transferData.currency = SABA.Currency[currency];
                transferData.amount = game_balance;
                transferData.direction = 0; //轉出遊戲
                transferData.vendor_member_id = platform_user.game_user_id;
                transferData.vendor_trans_id = RecordData.id.ToString();
                SABA_FundTransfer_Res transferResult = await _gameApiService._SabaAPI.FundTransfer(transferData);
                if (transferResult.error_code == (int)ResponseCode.TimeOut || transferResult.Data.status == (int)SABA_FundTransfer_Res.Status.Pending)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                }
                else if (transferResult.error_code == (int)ResponseCode.GameApiMaintain)
                {
                    PutApiHealthReq putApiHealthReq = new PutApiHealthReq();
                    putApiHealthReq.Platform = platform.ToString();
                    putApiHealthReq.Status = Status.MAINTAIN;
                    putApiHealthReq.Operator = platform.ToString();
                    await _commonService._apiHealthCheck.SetPlatformHealthInfo(putApiHealthReq);
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                }
                else if (transferResult.Data.status == (int)SABA_FundTransfer_Res.Status.OK)
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                    walletData.Club_id = platform_user.club_id;
                    var platforminfo = await _commonService._apiHealthCheck.GetPlatformHealthInfo(platform);
                    if (platforminfo != null)
                    {
                        if (platforminfo.Status == Status.MAINTAIN && platforminfo.Operator == platform.ToString())
                        {
                            PutApiHealthReq putApiHealthReq = new PutApiHealthReq();
                            putApiHealthReq.Platform = platform.ToString();
                            putApiHealthReq.Status = Status.NORMAL;
                            putApiHealthReq.Operator = null;
                            await _commonService._apiHealthCheck.SetPlatformHealthInfo(putApiHealthReq);
                        }

                    }
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
            if (!SABA.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            ForwardGame res = new ForwardGame();
            //Step 1 Create Member

            SABA_CreateMember requestData = new SABA_CreateMember();
            requestData.currency = SABA.Currency[userData.Currency];
            if (request.GameConfig.ContainsKey("oddstype"))
            {
                requestData.oddstype = SABA.Odds_Type[request.GameConfig["oddstype"]].ToString();
            }
            else
            {
                requestData.oddstype = SABA.Odds_Type["Malay_Odds"].ToString();
            }
            switch (_env.EnvironmentName) //依照環境變數調整Prefix
            {
                case "Local":
                case "DEV":
                    requestData.username = "DEV" + userData.Club_Ename;
                    requestData.vendor_member_id = "DEV" + userData.Club_id;
                    break;
                case "UAT":
                    requestData.username = _env.EnvironmentName + userData.Club_Ename;
                    requestData.vendor_member_id = _env.EnvironmentName + userData.Club_id;
                    break;
                case "PRD":
                    requestData.username = userData.Club_Ename;
                    requestData.vendor_member_id = userData.Club_id;
                    break;
                default:
                    throw new ExceptionMessage((int)ResponseCode.UnknowEenvironment, MessageCode.Message[(int)ResponseCode.UnknowEenvironment]);

            }
            SABA_ResBase result = await _gameApiService._SabaAPI.CreateMember(requestData);
            _logger.LogDebug("Show result : {result}", result);
            if (result.error_code != (int)ResponseCode.Success)
            {
                res.code = (int)ResponseCode.CreateSabaUserFail;
                res.Message = MessageCode.Message[(int)ResponseCode.CreateSabaUserFail] + " | " + result;
                throw new ExceptionMessage(res.code, res.Message);
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = requestData.vendor_member_id;
            gameUser.game_platform = request.Platform;
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            ForwardGame res = new ForwardGame();         
            //Step 3 Get Game URL
            SABA_GetSabaUrl urlData = new SABA_GetSabaUrl();
            urlData.vendor_member_id = platformUser.game_user_id;
            if (request.GameConfig.ContainsKey("device"))
            {
                urlData.platform = SABA.Device[request.GameConfig["device"]];
            }
            else
            {
                urlData.platform = SABA.Device["DESKTOP"];
            }


            SABA_GetSabaUrl_Res urlResult = await _gameApiService._SabaAPI.GetSabaUrl(urlData);
            if (urlResult.error_code != (int)ResponseCode.Success)
            {
                res.code = (int)ResponseCode.GetGameURLFail;
                res.Message = MessageCode.Message[(int)ResponseCode.GetGameURLFail] + " | " + urlResult.message;
                throw new ExceptionMessage(res.code, res.Message);
            }
            else
            {
                if (request.GameConfig.ContainsKey("lang") && SABA.lang.ContainsKey(request.GameConfig["lang"]))
                {
                    res.Url = urlResult.Data + SABA.lang[request.GameConfig["lang"]];
                }
                else
                {
                    res.Url = urlResult.Data;
                }
                if (request.GameConfig.ContainsKey("lobbyURL") && request.GameConfig["lobbyURL"] != null)
                {
                    res.Url += "&homeUrl=" + request.GameConfig["lobbyURL"];
                }
                if (_env.EnvironmentName == "DEV" || _env.EnvironmentName == "Local")
                {
                    res.Url = res.Url.Replace("https", "http");
                }

            }
            return res.Url;
        }
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();

            SABA_CheckFundTransfer reqData = new SABA_CheckFundTransfer();
            reqData.vendor_trans_id = transfer_record.id.ToString();
            SABA_CheckFundTransfer_Res reuslt = await _gameApiService._SabaAPI.CheckFundTransfer(reqData);
            if (reuslt.error_code == (int)SABA_CheckFundTransfer_Res.ErrorCode.TidNotExist || reuslt.Data.status == (int)SABA_CheckFundTransfer_Res.Status.Failed)
            {
                if (transfer_record.target == nameof(Platform.SABA))//轉入SABA直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;
                }
                else if (transfer_record.source == nameof(Platform.SABA))
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
            else if (reuslt.Data.status == (int)SABA_CheckFundTransfer_Res.Status.OK)
            {
                if (transfer_record.target == nameof(Platform.SABA))//轉入SABA直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.SABA))
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
            CheckTransferRecordResponse.TRecord = transfer_record;
            return CheckTransferRecordResponse;
        }
        public async Task<dynamic> GetBetRecords(GetBetRecordReq RecordReq)
        {
            GetBetRecord res = new GetBetRecord();
            IEnumerable<dynamic> saba_results = await _sabaDbService.GetSabaRecordBySummary(RecordReq);
            await SabaRecordPaser(saba_results);
            saba_results = saba_results.OrderByDescending(e => e.transaction_time);
            res.Data = saba_results.ToList();
            return res;
        }
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            SABA_GetBetDetailByTransID reqData = new SABA_GetBetDetailByTransID();
            reqData.trans_id = Convert.ToInt64(RecordDetailReq.record_id);
            SABA_GetBetDetailByTransID_Res resData = await _gameApiService._SabaAPI.GetBetDetailByTransID(reqData);
            if (resData.error_code != (int)SABA_GetBetDetailByTransID_Res.ErrorCode.Success)
            {
                throw new Exception(resData.message);
            }
            return JsonConvert.SerializeObject(resData.Data);
        }
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            if (RepairReq.SearchType == 1)
            {
                DateTime tempStartTime = RepairReq.StartTime;
                DateTime tempEndTime = RepairReq.EndTime;
                TimeSpan df = tempEndTime.Subtract(tempStartTime);
                
                while (df.TotalSeconds > 3600 || tempEndTime.Hour != RepairReq.StartTime.Hour)
                {
                    RepairReq.EndTime = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, 59, 59);
                    _logger.LogDebug("Rub Repair Saba Record StartTime : {start} EndTime : {end}", RepairReq.StartTime, RepairReq.EndTime);
                    RepairCount += await RepairSaba(RepairReq);

                    RepairReq.StartTime = RepairReq.EndTime.AddSeconds(1);
                    df = tempEndTime.Subtract(RepairReq.StartTime);
                }
                if (df.TotalMinutes > 0)
                {
                    RepairReq.EndTime = tempEndTime.AddSeconds(-1);
                    _logger.LogDebug("Rub Repair Saba Record StartTime : {start} EndTime : {end}", RepairReq.StartTime, RepairReq.EndTime);
                    RepairCount += await RepairSaba(RepairReq);
                }
            }
            else
            {
                RepairCount += await RepairSaba(RepairReq);
            }
            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public async Task<ResCodeBase> SetLimit(SetLimitReq request, GamePlatformUser gameUser, Wallet memberWalletData)
        {
            if (gameUser == null)
            {
                throw new ExceptionMessage((int)ResponseCode.GetSabaUserFail, MessageCode.Message[(int)ResponseCode.GetSabaUserFail]);
            }
            //Set Member Limit
            SABA_SetMemberBetSetting requestData = new SABA_SetMemberBetSetting();
            requestData.vendor_member_id = gameUser.game_user_id;
            requestData.bet_setting = System.Text.Json.JsonSerializer.Serialize(request.bet_setting);
            List<SABA_BetSetting> betSettings = System.Text.Json.JsonSerializer.Deserialize<List<SABA_BetSetting>>(requestData.bet_setting);
            //if(betSettings.FirstOrDefault().sport_type.ToLower() == "all")
            SABA_BetSetting settingAll = betSettings.FirstOrDefault(x => x.sport_type.ToLower() == "all");
            if (settingAll != null)
            {
                betSettings.Clear();
                string[] sport_type = { "1", "2", "3", "5", "8", "10", "11", "43", "99", "99MP", "1MP" };
                foreach (var r in sport_type)
                {
                    SABA_BetSetting bet_setting_data = new SABA_BetSetting();
                    bet_setting_data.sport_type = r;
                    bet_setting_data.min_bet = settingAll.min_bet;
                    bet_setting_data.max_bet = settingAll.max_bet;
                    bet_setting_data.max_bet_per_match = settingAll.max_bet_per_match;
                    bet_setting_data.max_payout_per_match = settingAll.max_payout_per_match;
                    betSettings.Add(bet_setting_data);
                }
            }

            //非正式環境，SABA限制金額不得大於10
            if (Config.OneWalletAPI.Prefix_Key != "prd")
            {
                betSettings = betSettings.Select(betSetting =>
                {
                    betSetting.max_payout_per_match = Math.Min(10, betSetting.max_payout_per_match);
                    return betSetting;
                }).ToList();
            }
            ResCodeBase res = new ResCodeBase();
            requestData.bet_setting = System.Text.Json.JsonSerializer.Serialize(betSettings);

            SABA_ResBase result = await _gameApiService._SabaAPI.SetMemberBetSetting(requestData);
            _logger.LogDebug("Show result : {result}", result);
            if (result.error_code != (int)ResponseCode.Success)
            {
                res.code = result.error_code;
                res.Message = result.message;
                return res;
            }
            res.code = (int)ResponseCode.Success;
            res.Message = MessageCode.Message[(int)ResponseCode.Success];
            return res;
        }
        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Sport;
        }

        public Task HealthCheck(Platform platform)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region GameRecordService
        public async Task<ResCodeBase> PostSabaRecord(SABA_Game_Record recordData)
        {
            ResCodeBase res = new ResCodeBase();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, SABA_BetDetails>> linqRes = recordData.BetDetails.GroupBy(x => x.vendor_member_id);
            foreach (IGrouping<string, SABA_BetDetails> group in linqRes)
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
                            string club_id;
                            if (_env.EnvironmentName != "PRD")
                            {
                                club_id = group.Key.Substring(3);
                            }
                            else
                            {
                                club_id = group.Key;
                            }
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.SABA);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No saba user");
                            }
                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
                            //已結算注單
                            List<SABA_BetDetails> betDetailData = new List<SABA_BetDetails>();
                            //未結算注單
                            List<SABA_BetDetails> betDetailDataRunning = new List<SABA_BetDetails>();
                            foreach (SABA_BetDetails r in group)//loop club id bet detail
                            {
                                if (r.ticket_status == "running")
                                {
                                    IEnumerable<SABA_BetDetails> runningRecord = await _sabaDbService.GetSabaRunningRecord(conn, tran, r);
                                    if (runningRecord.Count() > 0) //取得重複Running注單
                                    {
                                        r.ticket_status = "duplicate";
                                    }
                                    r.club_id = memberWalletData.Club_id;
                                    r.franchiser_id = memberWalletData.Franchiser_id;
                                }
                                else if (r.ticket_status == "waiting")
                                {
                                    //waiting單不用管
                                }
                                else
                                {
                                    IEnumerable<SABA_BetDetails> runningRecord = await _sabaDbService.GetSabaRecord(r);
                                    if (runningRecord.Count() > 0) //取得重複Settle注單
                                    {
                                        r.ticket_status = "update";
                                    }
                                }
                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.SABA);
                                if (r.settlement_time != null)
                                {
                                    //GMT-4 調整成GMT+8
                                    r.transaction_time = Convert.ToDateTime(r.transaction_time.ToString()).AddHours(12);
                                    r.settlement_time = Convert.ToDateTime(r.settlement_time.ToString()).AddHours(12);
                                    DateTime tempDateTime = Convert.ToDateTime(r.settlement_time.ToString());
                                    //sumData.ReportDatetime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day, tempDateTime.Hour, (tempDateTime.Minute / 5) * 5, 0);
                                    tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                    tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                    tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                    sumData.ReportDatetime = tempDateTime;
                                }
                                else
                                {
                                    r.transaction_time = Convert.ToDateTime(r.transaction_time.ToString()).AddHours(12);
                                    DateTime tempDateTime = Convert.ToDateTime(r.transaction_time.ToString());
                                    //sumData.ReportDatetime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day, tempDateTime.Hour, (tempDateTime.Minute / 5) * 5, 0);
                                    tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                    tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                    tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                    sumData.ReportDatetime = tempDateTime;
                                }
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    //if (DateTime.Now.Hour == 11 && DateTime.Now.Minute >= 30)
                                    //{
                                    //    DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                    //    if (sumData.ReportDatetime < ReportDateTime)
                                    //    {
                                    //        sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                    //        _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.trans_id);
                                    //    }
                                    //}
                                    //else 
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.trans_id);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.trans_id);
                                        }
                                    }
                                }
                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString()];
                                    //合併處理
                                    sumData = await Calculate(conn, tran, sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString()] = sumData;
                                }
                                else
                                {
                                    //用Club_id與ReportDatetime DB取得彙總注單
                                    IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
                                    sw.Stop();
                                    _logger.LogDebug("get summary record ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
                                    sw.Restart();
                                    if (results.Count() == 0) //沒資料就建立新的
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
                                    summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
                                }
                                r.last_version_key = recordData.last_version_key;
                                r.summary_id = sumData.id;

                                switch (r.ticket_status)
                                {
                                    case "waiting":
                                    case "duplicate":
                                        //waiting單不存
                                        break;
                                    case "update":
                                        //已結算單要更新
                                        await _sabaDbService.PutSabaRecord(r);
                                        break;
                                    case "running":
                                        betDetailDataRunning.Add(r);
                                        break;
                                    case "void":
                                    case "refund":
                                    case "reject":
                                    case "draw":
                                    case "lose":
                                    case "won":
                                    case "half won":
                                    case "half lose":
                                        betDetailData.Add(r);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            List<BetRecordSummary> summaryList = new List<BetRecordSummary>();
                            foreach (var s in summaryData)
                            {
                                if(s.Value.RecordCount > 0)
                                {
                                    summaryList.Add(s.Value);
                                }                                
                            }
                            int PostRecordSummaryReuslt = await _summaryDBService.PostRecordSummary(conn, tran, summaryList);
                            if (betDetailData.Count > 0)
                            {
                                int PostRecordResult = await _sabaDbService.PostSabaRecord(conn, tran, betDetailData);
                            }
                            if (betDetailDataRunning.Count > 0)
                            {
                                int PostRunningRecordResult = await _sabaDbService.PostSabaRunningRecord(conn, tran, betDetailDataRunning);
                                await _sabaDbService.PostSabaRecord(conn, tran, betDetailDataRunning);
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
                            if (_env.EnvironmentName != "PRD" && (ex.Message == "沒有會員id" || ex.Message == "No saba user"))
                            {
                                _logger.LogDebug("Run saba record group: {key} exception EX : {ex}  MSG : {Message} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString());
                            }
                            else
                            {
                                _logger.LogError("Run saba record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
                            }                            
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return res;
        }
        public async Task<ResCodeBase> PostGameReport(SABA_GetFinancialReportData sabaReportData)
        {
            ResCodeBase res = new ResCodeBase();
            GameReport reportData = new GameReport();
            reportData.platform = nameof(Platform.SABA);
            reportData.report_datetime = DateTime.Parse(sabaReportData.FinancialDate);
            reportData.report_type = (int)GameReport.e_report_type.FinancalReport;
            reportData.total_bet = sabaReportData.TotalBetAmount;
            reportData.total_win = sabaReportData.TotalWinAmount;
            reportData.total_netwin = sabaReportData.TotalWinAmount - sabaReportData.TotalBetAmount;
            reportData.total_count = sabaReportData.TotalBetCount;
            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);
            return res;
        }
        public async Task<ResCodeBase> CreateGameReportFromBetRecord(DateTime reportDate)
        {
            ResCodeBase res = new ResCodeBase();
            IEnumerable<dynamic>sabaReportData = await _sabaDbService.SumSabaBetRecordDaily(reportDate, "H1royal");
            var DailyReportData = sabaReportData.SingleOrDefault();
            GameReport reportData = new GameReport();
            reportData.report_datetime = reportDate;
            reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
            reportData.total_bet = DailyReportData.total_bet == null ? 0 : DailyReportData.total_bet;
            reportData.total_count = DailyReportData.total_count == null ? 0 : DailyReportData.total_count;
            reportData.total_netwin = DailyReportData.total_netwin == null ? 0 : DailyReportData.total_netwin;
            reportData.platform = nameof(Platform.SABA);
            await _gameReportDBService.DeleteGameReport(reportData);
            await _gameReportDBService.PostGameReport(reportData);
            return res;
        }
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> saba_results = await _sabaDbService.GetSabaRunningRecord(RecordReq);
            await SabaRecordPaser(saba_results);
            saba_results = saba_results.OrderByDescending(e => e.transaction_time);
            res.Data = saba_results.ToList();
            return res;
        }
        #endregion
        private async Task<BetRecordSummary> Calculate(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, SABA_BetDetails r)
        {
            //處理聯賽主客對資訊串關顯示內容
            if (r.ParlayData != null)
            {
                r.leaguename_en = "Parlay (" + r.ParlayData.Count + ")";
                r.hometeamname_en = " - ";
                r.awayteamname_en = " - ";
                r.sport_type = 0;  
            }
            else
            {
                r.leaguename_en = (r.leaguename == null) ? null : r.leaguename.FirstOrDefault(x => x.lang == "en").name;
                r.hometeamname_en = (r.hometeamname == null) ? null : r.hometeamname.FirstOrDefault(x => x.lang == "en").name;
                r.awayteamname_en = (r.awayteamname == null) ? null : r.awayteamname.FirstOrDefault(x => x.lang == "en").name;
            }
            if (r.resettlementinfo != null)
            {
                return ReSettle(SummaryData, r);
            }
            else
            {
                return await Settle(conn, tran, SummaryData, r);
            }
        }
        private async Task<BetRecordSummary> Settle(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, SABA_BetDetails r)
        {
            r.stake = (r.stake == null) ? 0 : r.stake;
            r.winlost = (r.winlost == null) ? 0 : r.winlost;
            r.winlost_amount = (r.winlost_amount == null) ? 0 : r.winlost_amount;
            switch (r.ticket_status)
            {
                case "running":
                    SummaryData.RecordCount++;
                    SummaryData.Bet_amount += r.stake;
                    SummaryData.Turnover += 0;
                    SummaryData.Netwin += r.winlost_amount;
                    SummaryData.Win += 0;
                    if(r.winlost_datetime > DateTime.Now.AddDays(29))
                    {
                        r.winlost_datetime = DateTime.Now.Date.AddDays(1);
                    }
                    break;
                case "waiting":
                case "duplicate":
                    //waiting單暫不處理
                    break;
                case "update":
                    //更新單不入彙總帳
                    break;
                case "void":
                case "refund":
                case "reject":
                case "draw":
                    IEnumerable<SABA_BetDetails> Refoundresults = await _sabaDbService.GetSabaRunningRecord(conn, tran, r);
                    Refoundresults = Refoundresults.ToList();
                    if (Refoundresults.Count() != 1) //沒有未結算單
                    {
                        r.pre_winlost_amount = r.winlost_amount;
                        r.pre_stake = r.stake;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += r.stake;
                        SummaryData.Turnover += 0;
                        SummaryData.Netwin += r.winlost_amount;
                        SummaryData.Win += (r.winlost_amount + r.stake);
                    }
                    else
                    {
                        decimal? preWinLose = Refoundresults.SingleOrDefault().winlost_amount;
                        decimal? preBet = Refoundresults.SingleOrDefault().stake;
                        preWinLose = (preWinLose == null) ? 0 : preWinLose;
                        preBet = (preBet == null) ? 0 : preBet;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += (r.stake - preBet);
                        SummaryData.Turnover += 0;
                        SummaryData.Netwin += (r.winlost_amount - preWinLose);
                        SummaryData.Win += SummaryData.Netwin + SummaryData.Bet_amount;

                        //明細也要將未結算單預扣金額加回去
                        r.pre_winlost_amount = r.winlost_amount;
                        r.winlost_amount -= preWinLose;
                        r.pre_stake = r.stake;
                        r.stake -= preBet;
                        await _sabaDbService.DeleteSabaRunningRecord(conn, tran, r);
                    }
                    break;
                case "lose":
                case "won":
                case "half won":
                case "half lose":
                    IEnumerable<SABA_BetDetails> WinloseResults = await _sabaDbService.GetSabaRunningRecord(conn, tran, r);
                    if (WinloseResults.Count() != 1) //沒有未結算單
                    {
                        r.pre_winlost_amount = r.winlost_amount;
                        r.pre_stake = r.stake;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += r.stake;

                        if (r.odds < 0)
                        {
                            //馬來盤實投量要計算實際扣款額度
                            SummaryData.Turnover += Math.Abs((r.stake * r.odds).GetValueOrDefault());
                        }
                        else
                        {
                            SummaryData.Turnover += r.stake;
                        }

                        SummaryData.Netwin += r.winlost_amount;
                        SummaryData.Win += (r.winlost_amount + r.stake);
                    }
                    else
                    {
                        decimal preWinLose = WinloseResults.SingleOrDefault().winlost_amount.GetValueOrDefault();
                        decimal? preBet = WinloseResults.SingleOrDefault().stake;
                        preBet = (preBet == null) ? 0 : preBet;
                        SummaryData.RecordCount++;
                        SummaryData.Bet_amount += (r.stake - preBet);
                        //馬來盤實投量要計算實際扣款額度
                        SummaryData.Turnover += Math.Abs(preWinLose);
                        SummaryData.Netwin += (r.winlost_amount - preWinLose);
                        SummaryData.Win += SummaryData.Netwin + SummaryData.Bet_amount;
                        //明細也要將未結算單預扣金額加回去
                        r.pre_winlost_amount = r.winlost_amount;
                        r.winlost_amount -= preWinLose;
                        r.pre_stake = r.stake;
                        r.stake -= preBet;
                        await _sabaDbService.DeleteSabaRunningRecord(conn, tran, r);
                    }
                    break;
            }
            SummaryData.updatedatetime = DateTime.Now;
            return SummaryData;
        }
        private BetRecordSummary ReSettle(BetRecordSummary SummaryData, SABA_BetDetails r)
        {
            _logger.LogWarning("saba bet record resettle info :{info}", r);
            SummaryData.updatedatetime = DateTime.Now;
            foreach (SABA_ResettlementInfo rdata in r.resettlementinfo)
            {
                if (rdata.balancechange)
                {
                    r.winlost = (rdata.winlost == null) ? 0 : rdata.winlost;
                    r.winlost_amount = (r.winlost_amount == null) ? 0 : r.winlost_amount;
                    switch (r.ticket_status)
                    {
                        case "running":
                        case "waiting":
                        case "update":
                            break;
                        case "void":
                        case "refund":
                        case "reject":
                        case "draw":
                        case "lose":
                        case "won":
                        case "half won":
                        case "half lose":
                            decimal? adjustAmount = r.winlost_amount - r.winlost;
                            SummaryData.Netwin += adjustAmount;
                            SummaryData.Win += adjustAmount;
                            _logger.LogWarning("saba bet record summary adjust amount : {amount} user_id : {user_id}", adjustAmount, r.vendor_member_id);
                            break;
                    }
                    return SummaryData;
                }
            }
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
        private async Task<int> RepairSaba(RepairBetSummaryReq RepairReq)
        {
            ResCodeBase res = new ResCodeBase();
            SABA_GetBetDetailByTimeframe req = new SABA_GetBetDetailByTimeframe();
            req.start_date = RepairReq.StartTime.AddHours(-12);
            req.end_date = RepairReq.EndTime.AddHours(-12);
            req.time_type = RepairReq.SearchType; // 1: 依下注时间查询 2: 依结算日期查询
            SABA_GetBetDetailByTimeframe_Res result = await _gameApiService._SabaAPI.GetBetDetailByTimeframe(req);
            if (result.Data.BetDetails.Count > 0)
            {
                await PostSabaRecord(result.Data);
                IEnumerable<IGrouping<DateTime?, SABA_BetDetails>> linqRes = result.Data.BetDetails.GroupBy(x => x.winlost_datetime);
                foreach (IGrouping<DateTime?, SABA_BetDetails> reportDate in linqRes)
                {
                    await CreateGameReportFromBetRecord(reportDate.Key.GetValueOrDefault());
                    _logger.LogInformation("Renew saba game report by daily record Date : {reportDate}", reportDate.Key.ToString());
                }
            }
            return result.Data.BetDetails.Count;
        }
        public async Task<int> SabaRecordPaser(IEnumerable<dynamic> saba_records)
        {
            var result = await GetBetTypeCache(nameof(Platform.SABA));
            foreach (SABA_BetDetails betRecord in saba_records)
            {
                var bettype = result.FirstOrDefault(x => x.id == betRecord.bet_type);
                if (bettype != null && bettype.bet_info != null)
                {
                    betRecord.bet_type_en = bettype.bet_type;
                    var values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(bettype.bet_info);
                    if (values.ContainsKey(betRecord.bet_team))
                    {

                        if (values[betRecord.bet_team] == "home")
                        {
                            betRecord.bet_team = betRecord.hometeamname_en;
                        }
                        else if (values[betRecord.bet_team] == "away")
                        {
                            betRecord.bet_team = betRecord.awayteamname_en;
                        }
                        else
                        {
                            betRecord.bet_team = values[betRecord.bet_team];
                        }
                    }
                }
                // settlement_time為null時,使用transaction_time
                if (betRecord.settlement_time == null && betRecord.ticket_status != "running")
                {
                    betRecord.settlement_time = betRecord.transaction_time;
                }
            }
            return 0;
        }
        private async Task<IEnumerable<dynamic>> GetBetTypeCache(string Game_id)
        {
            return await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.GameInfo}/BetType/{Game_id}",
            async () =>
            {
                var result = await _sabaDbService.GetSabaBetType();
                return result;
            },
            _BetTypeCacheSeconds);
        }
    }

}
