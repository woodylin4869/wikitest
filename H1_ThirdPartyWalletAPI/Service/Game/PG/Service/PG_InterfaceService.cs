using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Response;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Concurrent;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Game;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using H1_ThirdPartyWalletAPI.Model.Game.PG.Request;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using Newtonsoft.Json;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Model.Game.OB.Reqserver;

namespace H1_ThirdPartyWalletAPI.Service.Game.PG.Service
{
    public class PG_RecordService: IPGInterfaceService
    {
        private readonly ILogger<PG_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly ConcurrentQueue<long> _PGRecordQueue;
        private readonly IPgDBService _pgDBService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameReportDBService _gameReportDBService;
        private const int MIN_QUEEUE_SIZE = 10000;
        private const int MAX_QUEEUE_SIZE = 99999;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        public PG_RecordService(ILogger<PG_RecordService> logger, 
            ICommonService commonService
            , IGameApiService gameaApiService
            , ISummaryDBService summaryDBService
            , IPgDBService pgDBService
            , IGameReportDBService gameReportDBService)
        {
            _logger = logger;
            _commonService = commonService;
            _PGRecordQueue = new ConcurrentQueue<long>();
            _gameApiService = gameaApiService;
            _pgDBService = pgDBService;
            _gameReportDBService = gameReportDBService;
            _summaryDBService = summaryDBService;
        }
        #region GameInterfaceService
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            var req = new GetPlayerWalletRequest();
            req.operator_token = Config.CompanyToken.PG_Token;
            req.secret_key = Config.CompanyToken.PG_Key;
            req.player_name = platform_user.game_user_id;
            MemberBalance Balance = new MemberBalance();
            try
            {
                var res = await _gameApiService._PgAPI.GetPlayerWalletAsync(req);
                if (res.error != null)
                {
                    throw new Exception(res.error.message);
                }
                Balance.Amount = res.data.cashBalance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Pg餘額取得失敗 Msg: {Message}", ex.Message);
            }
            Balance.Wallet = nameof(Platform.PG);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                Model.Game.PG.Request.KickRequest reqData = new Model.Game.PG.Request.KickRequest();
                reqData.operator_token = Config.CompanyToken.PG_Token;
                reqData.secret_key = Config.CompanyToken.PG_Key;
                reqData.player_name = platform_user.game_user_id;
                var res = await _gameApiService._PgAPI.KickAsync(reqData);
                if (res.error != null)
                {
                    throw new Exception(res.error.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出PG使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }
        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var currency = walletData.Currency;
            try
            {
                var transfer_amount = RecordData.amount;
                Model.Game.PG.Request.TransferInRequest requestData = new Model.Game.PG.Request.TransferInRequest();
                requestData.operator_token = Config.CompanyToken.PG_Token;
                requestData.secret_key = Config.CompanyToken.PG_Key;
                requestData.player_name = platform_user.game_user_id;
                requestData.amount = transfer_amount;
                requestData.transfer_reference = RecordData.id.ToString();
                requestData.currency = Model.Game.PG.PG.Currency[currency];
                var responseData = await _gameApiService._PgAPI.TransferInAsync(requestData);
                if (responseData.error != null)
                {
                    //3001 不能空值
                    //3005 玩家钱包不存在
                    //3100 转账失败
                    //3101 转账请求中，请重试查看最新状态
                    if (responseData.error.code == "3101")
                    {
                        RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    }
                    else if (responseData.error.code == "3001" || responseData.error.code == "3005" || responseData.error.code == "3100")
                    {
                        throw new ExceptionMessage(int.Parse(responseData.error.code), responseData.error.message);
                    }
                    else //收到未知錯誤要改pending
                    {
                        RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    }
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PG TransferIn Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("PG TransferIn Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInPgFail Msg: {Message}", ex.Message);
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
                Model.Game.PG.Request.TransferOutRequest requestData = new Model.Game.PG.Request.TransferOutRequest();
                requestData.operator_token = Config.CompanyToken.PG_Token;
                requestData.secret_key = Config.CompanyToken.PG_Key;
                requestData.player_name = platform_user.game_user_id;
                requestData.amount = game_balance;
                requestData.transfer_reference = RecordData.id.ToString();
                requestData.currency = Model.Game.PG.PG.Currency[currency];
                var responseData = await _gameApiService._PgAPI.TransferOutAsync(requestData);
                if (responseData.error != null)
                {
                    //3001 不能空值
                    //3005 玩家钱包不存在
                    //3100 转账失败
                    //3101 转账请求中，请重试查看最新状态
                    if (responseData.error.code == "3101")
                    {
                        RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    }
                    else if (responseData.error.code == "3001" || responseData.error.code == "3005" || responseData.error.code == "3100")
                    {
                        throw new ExceptionMessage(int.Parse(responseData.error.code), responseData.error.message);
                    }
                    else //收到未知錯誤要改pending
                    {
                        RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                    }
                }
                else
                {
                    RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
                }
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PG TransferOut Timeout ex : {ex}", ex);
            }
            catch (ExceptionMessage ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.fail);
                _logger.LogError("PG TransferOut Fail ex : {ex}", ex.Message);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("PG TransferOut Fail ex : {ex}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.PG.PG.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            ForwardGame res = new ForwardGame();
            //Step 1 Create Member
            var requestData = new CreateRequest();
            requestData.currency = Model.Game.PG.PG.Currency[userData.Currency];
            requestData.nickname = userData.Club_Ename;
            requestData.operator_token = Config.CompanyToken.PG_Token;
            requestData.secret_key = Config.CompanyToken.PG_Key;
            requestData.player_name = (Config.OneWalletAPI.Prefix_Key + userData.Club_id).ToLower();
            try
            {
                var result = await _gameApiService._PgAPI.CreateAsync(requestData);
                if (result.error != null)
                {
                    throw new Exception(result.error.message);
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.CreatePgUserFail, MessageCode.Message[(int)ResponseCode.CreatePgUserFail] + "|" + ex.Message.ToString());
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = requestData.player_name;
            gameUser.game_platform = request.Platform;
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            Model.Game.PG.Request.GetGameUrlRequest UrlRequest = new Model.Game.PG.Request.GetGameUrlRequest();
            UrlRequest.btt = 1;
            UrlRequest.ops = platformUser.game_user_id;
            UrlRequest.ot = Config.CompanyToken.PG_Token;
            if (!request.GameConfig.ContainsKey("gameCode"))
            {
                throw new Exception("game code not found");
            }
            string gameCode = request.GameConfig["gameCode"];
            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.f = request.GameConfig["lobbyURL"];
            }
            if (request.GameConfig.ContainsKey("lang") && request.GameConfig["lang"] != null && Model.Game.PG.PG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.l = Model.Game.PG.PG.lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.l = Model.Game.PG.PG.lang["en-US"];
            }
            try
            {
                var token_res = _gameApiService._PgAPI.GetGameUrl(gameCode, UrlRequest);
                return token_res;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            Model.Game.PG.Request.GetSingleTransactionRequest PgreqData = new Model.Game.PG.Request.GetSingleTransactionRequest();
            PgreqData.operator_token = Config.CompanyToken.PG_Token;
            PgreqData.secret_key = Config.CompanyToken.PG_Key;
            PgreqData.transfer_reference = transfer_record.id.ToString();
            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(transfer_record.Club_id, Platform.PG);
            PgreqData.player_name = gameUser.game_user_id;
            GetSingleTransactionResponse Pgreuslt = await _gameApiService._PgAPI.GetSingleTransactionAsync(PgreqData);
            if (Pgreuslt.error == null)
            {
                if (transfer_record.target == nameof(Platform.PG))//轉入PG直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.PG))
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
                if(Pgreuslt.error.code != "3040")
                {
                    throw new Exception("PG CheckTransferRecord error code" + Pgreuslt.error.code);
                }    
                if (transfer_record.target == nameof(Platform.PG))//轉入PG直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.PG))
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
            IEnumerable<dynamic> pg_results = await _pgDBService.GetPgRecordBySummary(RecordReq);
            pg_results = pg_results.OrderByDescending(e => e.betendtime);
            res.Data = pg_results.ToList();
            return res;
        }
        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            IEnumerable<dynamic> resPgRecord = await _pgDBService.GetPgRecord(RecordDetailReq.record_id, RecordDetailReq.ReportTime);
            if (resPgRecord.Count() != 1)
            {
                throw new Exception("no data");
            }
            Model.Game.PG.Request.RedirectToBetDetailRequest reqDataPG = new Model.Game.PG.Request.RedirectToBetDetailRequest();
            reqDataPG.gid = resPgRecord.SingleOrDefault().gameid.ToString();

            if (Model.Game.PG.PG.lang.ContainsKey(RecordDetailReq.lang))
            {
                reqDataPG.lang = Model.Game.PG.PG.lang[RecordDetailReq.lang];
            }
            else
            {
                reqDataPG.lang = Model.Game.PG.PG.lang["en-US"];
            }
            reqDataPG.psid = resPgRecord.SingleOrDefault().parentbetid.ToString();
            reqDataPG.sid = resPgRecord.SingleOrDefault().betid.ToString();
            reqDataPG.type = "operator";

            string resDataPG = await _gameApiService._PgAPI.RedirectToBetDetail(reqDataPG);
            //res.Data = JsonConvert.SerializeObject(resDataDS);
            return resDataPG;
        }
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            RepairReq.StartTime = RepairReq.StartTime.AddSeconds(-RepairReq.StartTime.Second).AddMilliseconds(-RepairReq.StartTime.Millisecond);
            RepairReq.EndTime = RepairReq.EndTime.AddSeconds(-RepairReq.EndTime.Second).AddMilliseconds(-RepairReq.EndTime.Millisecond);
            DateTime startTime = RepairReq.StartTime;
            DateTime endTime = RepairReq.EndTime;
            var RepairCount = 0;
            while (RepairReq.EndTime.Subtract(startTime).TotalMinutes > 5)
            {
                endTime = startTime.AddMinutes(5);
                _logger.LogDebug("Repair Pg record start Time : {startTime} end Time : {endTime}", startTime, endTime);
                RepairCount += await RepairPg(startTime, endTime);
                startTime = endTime;
                await Task.Delay(1000);
            }
            _logger.LogDebug("Repair Pg record start Time : {startTime} end Time : {endTime}", startTime, RepairReq.EndTime);
            RepairCount += await RepairPg(startTime, RepairReq.EndTime);
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
            return _gameApiService._PgAPI.GetPlayerWalletAsync(new()
            {
                operator_token = Config.CompanyToken.PG_Token,
                secret_key = Config.CompanyToken.PG_Key,
                player_name = "HealthCheck"
            });
        }
        #endregion

        #region GameRecordService
        /// <summary>
        /// 新增 5 分鐘匯總帳
        /// </summary>
        /// <param name="recordData"></param>
        /// <returns></returns>
        public async Task PostPgRecord(List<GetHistoryResponse.Data> recordData)
        {
            var linqRes = recordData.GroupBy(x => x.playerName);
            if (_PGRecordQueue.Count < MIN_QUEEUE_SIZE)
            {
                foreach (var group in linqRes)
                {
                    string club_id = group.Key.Substring(3);
                    Wallet memberWalletData = await GetWalletCache(club_id);
                    if (memberWalletData == null || memberWalletData.Club_id == null)
                    {
                        continue;
                    }

                    var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.PG);
                    if (gameUser == null || gameUser.game_user_id != group.Key)
                    {
                        continue;
                    }

                    var minTimeStamp = recordData.Min(x => x.betEndTime);
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    var startTime = dtDateTime.AddMilliseconds(minTimeStamp).ToLocalTime();
                    var recordHistory = await _pgDBService.GetPgRecordByPlayer(group.Key, startTime.AddSeconds(-1));
                    var recordHistoryList = recordHistory.ToList();
                    foreach (var record in recordHistoryList)
                    {
                        _PGRecordQueue.Enqueue(record.betid);
                    }
                }
            }
            using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
            {
                await conn.OpenAsync();                
                foreach (var group in linqRes)
                {
                    using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            string club_id = group.Key.Substring(3);
                            Wallet memberWalletData = await GetWalletCache(club_id);
                            if (memberWalletData == null || memberWalletData.Club_id == null)
                            {
                                throw new Exception("沒有會員id");
                            }

                            var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.PG);
                            if (gameUser == null || gameUser.game_user_id != group.Key)
                            {
                                throw new Exception("No pg user");
                            }

                            //彙總注單
                            Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();                            
                            //代處理注單
                            List<GetHistoryResponse.Data> betDetailDataTemp = new List<GetHistoryResponse.Data>();
                            //已結算注單
                            List<GetHistoryResponse.Data> betDetailData = new List<GetHistoryResponse.Data>();
                            //檢查重複注單
                            var recordList = group.ToList();
                            foreach (var record in recordList)
                            {
                                var hasRecord = _PGRecordQueue.Contains(record.betId);                                
                                if(hasRecord)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (betDetailDataTemp.Where(x => x.betId == record.betId).ToList().Any())
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        betDetailDataTemp.Add(record);
                                    }                                    
                                }
                            }
                            foreach (GetHistoryResponse.Data r in betDetailDataTemp)//loop club id bet detail
                            {
                                BetRecordSummary sumData = new BetRecordSummary();
                                sumData.Club_id = memberWalletData.Club_id;
                                sumData.Game_id = nameof(Platform.PG);
                                // Unix timestamp is seconds past epoch
                                DateTime tempDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                tempDateTime = tempDateTime.AddMilliseconds(r.betEndTime).ToLocalTime();
                                tempDateTime = tempDateTime.AddMinutes(-tempDateTime.Minute % 5);
                                tempDateTime = tempDateTime.AddSeconds(-tempDateTime.Second);
                                tempDateTime = tempDateTime.AddMilliseconds(-tempDateTime.Millisecond);
                                sumData.ReportDatetime = tempDateTime;
                                //sumData.ReportDatetime = new DateTime(tempDateTime.Year, tempDateTime.Month, tempDateTime.Day, tempDateTime.Hour, (tempDateTime.Minute / 5) * 5, 0);
                                //確認是否已經超過搬帳時間 For H1 only
                                if (Config.OneWalletAPI.RCGMode == "H1")
                                {
                                    if (DateTime.Now.Hour >= 12) //換日線
                                    {
                                        DateTime ReportDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.betId);
                                        }
                                    }
                                    else
                                    {
                                        var lastday = DateTime.Now.AddDays(-1);
                                        DateTime ReportDateTime = new DateTime(lastday.Year, lastday.Month, lastday.Day, 12, 00, 0);
                                        if (sumData.ReportDatetime < ReportDateTime)
                                        {
                                            sumData.ReportDatetime = ReportDateTime; //修改報表時間到當日12:00
                                            _logger.LogWarning("彙總帳跨日更新Bet Record ID:{record_id}", r.betId);
                                        }
                                    }
                                }
                                //先確認有沒有符合的匯總單
                                if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
                                {
                                    sumData = summaryData[sumData.ReportDatetime.ToString()];
                                    //合併處理
                                    sumData = Calculate(sumData, r);
                                    summaryData[sumData.ReportDatetime.ToString()] = sumData;
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
                                        sumData = Calculate(sumData, r);
                                    }
                                    else //有資料就更新
                                    {
                                        sumData = results.SingleOrDefault();
                                        //合併處理
                                        sumData = Calculate(sumData, r);
                                    }
                                    summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
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
                            int PostRecordResult = await _pgDBService.PostPgRecord(conn, tran, betDetailData);
                            await tran.CommitAsync();
                            //將Record id 寫入queue
                            foreach(var PGrecord in betDetailData)
                            {
                                _PGRecordQueue.Enqueue(PGrecord.betId);
                            }

                        }
                        catch (Exception ex)
                        {
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("Run pg record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);
                            await tran.RollbackAsync();
                        }

                    }
                }
                await conn.CloseAsync();
            }
            //清除超過上限的queue
            if (_PGRecordQueue.Count > MAX_QUEEUE_SIZE)
            {
                for (int i = 0; i < 10000; i++)
                {
                    long record_id;
                    _PGRecordQueue.TryDequeue(out record_id);
                }
            }
        }
        public async Task SummaryGameProviderReport(DateTime startDateTime , DateTime endDateTime)
        {
            while(true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if (reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create PG game provider report time {datetime}", reportTime);
                // 每小時投注匯總
                var result = await _gameApiService._PgAPI.GetHandsSummaryHourlyAsync(new GetHandsSummaryHourlyRequest()
                {
                    operator_token = Config.CompanyToken.PG_Token,
                    secret_key = Config.CompanyToken.PG_Key,
                    from_time = ((DateTimeOffset)reportTime).ToUnixTimeMilliseconds(),
                    to_time = ((DateTimeOffset)reportTime.AddHours(1).AddMilliseconds(-1)).ToUnixTimeMilliseconds(),
                });

                if (result.error != null)
                {
                    throw new Exception(result.error.message);
                }

                // 匯總帳寫回 DB (雖然 API 回傳 List，但每次只查詢一小時，理論上只會有一筆資料)
                var target = result.data.FirstOrDefault();
                // 沒有資料寫入空的匯總帳就結束排程
                if (target == null)
                {
                    // 遊戲商的每小時匯總報表(遊戲商的欄位格式)
                    var data = new t_pg_game_report
                    {
                        datetime = reportTime,
                        totalhands = 0,
                        currency = "THB",
                        totalbetamount = 0,
                        totalwinamount = 0,
                        totalplayerwinlossamount = 0,
                        totalcompanywinlossamount = 0,
                        transactiontype = 4,
                        totalcollapsespincount = 0,
                        totalcollapsefreespincount = 0
                    };
                    await _pgDBService.DeletePgReport(data);
                    await _pgDBService.PostPgReport(data);
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.PG),
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
                    // 遊戲商的每小時匯總報表(遊戲商的欄位格式)
                    var data = new t_pg_game_report
                    {
                        datetime = GetDateTimeBySeconds(target.dateTime / 1000), // dateTime 為 UnixTimeMilliseconds
                        totalhands = result.data.Sum(x => x.totalHands),
                        currency = target.currency,
                        totalbetamount = result.data.Sum(x => x.totalBetAmount),
                        totalwinamount = result.data.Sum(x => x.totalWinAmount),
                        totalplayerwinlossamount = result.data.Sum(x => x.totalPlayerWinLossAmount),
                        totalcompanywinlossamount = result.data.Sum(x => x.totalCompanyWinLossAmount),
                        transactiontype = 4,
                        totalcollapsespincount = result.data.Sum(x => x.totalCollapseSpinCount),
                        totalcollapsefreespincount = result.data.Sum(x => x.totalCollapseFreeSpinCount)
                    };
                    await _pgDBService.DeletePgReport(data);
                    await _pgDBService.PostPgReport(data);
                    // 遊戲商的每小時匯總報表(W1欄位格式)
                    GameReport reportData = new GameReport();
                    reportData.platform = nameof(Platform.PG);
                    reportData.report_datetime = reportTime;
                    reportData.report_type = (int)GameReport.e_report_type.FinancalReport;
                    reportData.total_bet = data.totalbetamount;
                    reportData.total_win = data.totalwinamount;
                    reportData.total_netwin = reportData.total_win - reportData.total_bet;
                    reportData.total_count = data.totalhands;
                    await _gameReportDBService.DeleteGameReport(reportData);
                    await _gameReportDBService.PostGameReport(reportData);
                    startDateTime = startDateTime.AddHours(1);
                }
                await Task.Delay(3000);
            }
        }
        public async Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime)
        {
            while(true)
            {
                DateTime reportTime = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0);
                if(reportTime >= endDateTime)
                {
                    //超過結束時間離開迴圈
                    break;
                }
                _logger.LogDebug("Create PG game W1 report time {datetime}", reportTime);
                IEnumerable<dynamic> dailyReport = await _pgDBService.SumPgBetRecordHourly(reportTime);
                var HourlylyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.PG);
                reportData.report_datetime = reportTime;
                reportData.report_type = (int)GameReport.e_report_type.GameBetRecord;
                reportData.total_bet = HourlylyReportData.total_bet == null ? 0 : Math.Abs(HourlylyReportData.total_bet);
                reportData.total_win = HourlylyReportData.total_win == null ? 0 : HourlylyReportData.total_win;
                reportData.total_netwin = reportData.total_win - reportData.total_bet;
                reportData.total_count = HourlylyReportData.total_cont == null ? 0 : HourlylyReportData.total_cont;
                await _gameReportDBService.DeleteGameReport(reportData);
                await _gameReportDBService.PostGameReport(reportData);
                startDateTime = startDateTime.AddHours(1);
                await Task.Delay(3000);            
            }
        }
        #endregion
        private BetRecordSummary Calculate(BetRecordSummary SummaryData, GetHistoryResponse.Data r)
        {
            SummaryData.RecordCount++;
            SummaryData.Bet_amount += r.betAmount;
            SummaryData.Turnover += r.betAmount;
            SummaryData.Netwin += r.winAmount - r.betAmount;
            SummaryData.Win += r.winAmount;
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

        private DateTime GetDateTimeBySeconds(double seconds)
        {
            return DateTime.Parse(DateTime.Now.ToString("1970-01-01 00:00:00")).AddSeconds(seconds).ToLocalTime();  // 時區要 +8
        }

        /// <summary>
        /// PG 帳務比對
        /// 1. 比對轉帳中心與遊戲商的匯總帳是否一致
        /// 2. 如果帳務不一致的話，啟動補單機制
        /// 3. 將最後匯總結果寫回 DB
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private async Task<int> RepairPg(DateTime startTime, DateTime endTime)
        {
            // 遊戲商的歷史下注紀錄
            var unixStartTime = ((DateTimeOffset)startTime).ToUnixTimeMilliseconds();
            var unixEndTime = ((DateTimeOffset)endTime).ToUnixTimeMilliseconds();

            var list = new List<GetHistoryResponse.Data>();
            var stop = false;

            while (!stop)
            {
                var betRecord = await _gameApiService._PgAPI.GetHistoryForSpecificTimeRangeAsync(new GetHistoryForSpecificTimeRangeRequest()
                {
                    operator_token = Config.CompanyToken.PG_Token,
                    secret_key = Config.CompanyToken.PG_Key,
                    count = 5000,
                    bet_type = 1,
                    from_time = unixStartTime,
                    to_time = unixEndTime
                });


                if (betRecord.data.Any())
                {
                    unixStartTime = betRecord.data.Max(x => x.betEndTime) + 1;
                    var pgHistoryList = JsonConvert.DeserializeObject<List<GetHistoryResponse.Data>>(JsonConvert.SerializeObject(betRecord.data));
                    list.AddRange(pgHistoryList);
                    await Task.Delay(5000);
                }
                else
                {
                    stop = true;
                }
            }

            // 轉帳中心的歷史下注紀錄
            var w1CenterList = await _pgDBService.SumPgBetRecord(startTime, endTime);

            var repairList = new List<GetHistoryResponse.Data>();
            foreach (var record in list)
            {
                var hasData = w1CenterList.Where(x => x.betid == record.betId).Any();
                if (hasData == false)
                {
                    repairList.Add(record);
                }
            }
            // 注單明細補單、更新 5 分鐘匯總帳
            await PostPgRecord(repairList);
            return repairList.Count();
        }

    }
}