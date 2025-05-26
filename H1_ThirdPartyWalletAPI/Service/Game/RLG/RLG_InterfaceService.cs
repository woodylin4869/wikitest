using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Request;
using H1_ThirdPartyWalletAPI.Model.Game.RLG.Response;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Worker;
using H1_ThirdPartyWalletAPI.Worker.Game.CR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;
using static H1_ThirdPartyWalletAPI.Model.Game.RLG.Response.GetBetRecordResponse;

namespace H1_ThirdPartyWalletAPI.Service.Game.RLG
{
    public interface IRlgInterfaceService : IGameInterfaceService
    {
        Task<int> PostRlgRecord(List<GetBetRecordResponseDataList> recordData);
        Task SummaryGameProviderReport(DateTime startDateTime, DateTime endDateTime);
        Task SummaryW1Report(DateTime startDateTime, DateTime endDateTime);
        Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq);
        Task<List<GetBetRecordResponseDataList>> GetRlgBetRecords(DateTime startTime, DateTime endTime);
    }
    public class RLG_RecordService : IRlgInterfaceService
    {
        private readonly ILogger<RLG_RecordService> _logger;
        private readonly ICommonService _commonService;
        private readonly IRlgDbService _rlgDbService;
        private readonly ISummaryDBService _summaryDBService;
        private readonly IGameApiService _gameApiService;
        private readonly IGameReportDBService _gameReportDBService;
        const int _cacheSeconds = 600;
        const int _cacheFranchiserUser = 1800;
        private readonly ISystemParameterDbService _systemParameterDbService;

        public RLG_RecordService(ILogger<RLG_RecordService> logger,
            ICommonService commonService,
            IRlgDbService rlgDbService,
            ISummaryDBService summaryDBService,
            IGameApiService gameaApiService, 
            IGameReportDBService gameReportDBService,
            ISystemParameterDbService systemParameterDbService
        )
        {
            _logger = logger;
            _commonService = commonService;
            _rlgDbService = rlgDbService;
            _summaryDBService = summaryDBService;
            _gameApiService = gameaApiService;
            _gameReportDBService = gameReportDBService;
            _systemParameterDbService = systemParameterDbService;
        }
        #region GameInterfaceService
        public async Task<MemberBalance> GetGameCredit(Platform platform, GamePlatformUser platform_user)
        {
            MemberBalance Balance = new MemberBalance();
            try
            {
                //var RlgaData = await _commonService._serviceDB.GetRLGTokenAsync(platform_user.club_id);
                //if (RlgaData == null)
                //{
                //    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                //}
                var listdata = new List<BatchBalanceRequest.BatchBalanceRequestData>()
                { new BatchBalanceRequest.BatchBalanceRequestData()
                      {
                          UserId = platform_user.game_user_id,
                          WebId = Config.CompanyToken.RLG_WebID
                      }
                };

                var responseData = await _gameApiService._RlgAPI.BatchBalanceAsync(new BatchBalancepostdata()
                {
                    SystemCode = Config.CompanyToken.RLG_SystemCode,
                    Data = JsonConvert.SerializeObject(listdata)
                });

                if (responseData.errorcode != (int)ErrorCodeEnum.OK)
                {
                    throw new Exception(responseData.errormessage);
                }
                Balance.Amount = responseData.data[0].balance;
            }
            catch (Exception ex)
            {
                Balance.Amount = 0;
                Balance.code = (int)ResponseCode.Fail;
                Balance.Message = MessageCode.Message[(int)ResponseCode.Fail] + " | " + ex.Message;
                _logger.LogError("Rlg餘額取得失敗 Msg: {Message}", ex.Message);
            }

            Balance.Wallet = nameof(Platform.RLG);
            return Balance;
        }
        public async Task<bool> KickUser(Platform platform, GamePlatformUser platform_user)
        {
            try
            {
                //var RlgaData = await _commonService._serviceDB.GetRLGTokenAsync(platform_user.club_id);
                //if (RlgaData == null)
                //{
                //    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                //}
                var responseData = await _gameApiService._RlgAPI.KickoutAsync(new Model.Game.RLG.Request.KickoutRequest()
                {
                    SystemCode = Config.CompanyToken.RLG_SystemCode,
                    WebId = Config.CompanyToken.RLG_WebID,
                    UserId = platform_user.game_user_id,
                });

                if (responseData.errorcode != (int)ErrorCodeEnum.OK)
                {
                    throw new Exception(responseData.errormessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("踢出RLG使用者失敗 id:{account} Msg: {Message}", platform_user.game_user_id, ex.Message);
            }
            return true;
        }
        public Task<bool> KickAllUser(Platform platform)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Deposit(GamePlatformUser platform_user, Wallet walletData, WalletTransferRecord RecordData)
        {
            var transfer_amount = RecordData.amount;
            var currency = walletData.Currency;
            try
            {
                //var rlgdata = await _commonService._serviceDB.GetRLGTokenAsync(platform_user.club_id);
                //if (rlgdata == null)
                //{
                //    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                //}
                var responseData = await _gameApiService._RlgAPI.DepositAsync(new Model.Game.RLG.Request.DepositRequest
                {
                    SystemCode = Config.CompanyToken.RLG_SystemCode,
                    WebId = Config.CompanyToken.RLG_WebID,
                    UserId = platform_user.game_user_id,
                    TransferNo = RecordData.id.ToString().Replace("-", "").Substring(0, 20),
                    Balance = transfer_amount
                });

                if (responseData.errorcode != (int)ErrorCodeEnum.OK)
                {
                    throw new Exception(responseData.errormessage);
                }
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RLG TransferIn Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("FundTransferInRsgFail Msg: {Message}", ex.Message);
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
                //var rlgdata = await _commonService._serviceDB.GetRLGTokenAsync(platform_user.club_id);
                //if (rlgdata == null)
                //{
                //    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                //}
                // RLG 會員在線上的話會無法提款，必須先做踢線
                //await _gameApiService._RlgAPI.KickoutAsync(new Model.Game.RLG.Request.KickoutRequest()
                //{
                //    SystemCode = Config.CompanyToken.RLG_SystemCode,
                //    WebId = Config.CompanyToken.RLG_WebID,
                //    UserId = platform_user.game_user_id,
                //});
                //RLG 先組成DATA 之後序列化
                var listdata = new List<Model.Game.RLG.Request.BatchWithdrawalRequest.BatchWithdrawalRequestData>()
                            {
                                 new Model.Game.RLG.Request.BatchWithdrawalRequest.BatchWithdrawalRequestData()
                                 {
                                     WebId= Config.CompanyToken.RLG_WebID,
                                     TransferNo=RecordData.id.ToString().Replace("-", "").Substring(0, 20),
                                     Balance=game_balance,
                                     UserId=platform_user.game_user_id,
                                 }
                            };

                var responseData = await _gameApiService._RlgAPI.BatchWithdrawalAsync(new Model.Game.RLG.Request.BatchWithdrawalpostdata()
                {
                    SystemCode = Config.CompanyToken.RLG_SystemCode,
                    Data = JsonConvert.SerializeObject(listdata)

                });

                if (responseData.errorcode != (int)ErrorCodeEnum.OK)
                {
                    throw new Exception(responseData.errormessage);
                }

                RecordData.status = nameof(WalletTransferRecord.TransferStatus.success);
            }
            catch (TaskCanceledException ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RLG TransferOut Timeout ex : {ex}", ex);
            }
            catch (Exception ex)
            {
                RecordData.status = nameof(WalletTransferRecord.TransferStatus.pending);
                _logger.LogError("RLG TransferOut Fail ex : {ex}", ex.Message);
            }
            return RecordData.status;
        }
        public async Task<GamePlatformUser> CreateGameUser(ForwardGameReq request, Wallet userData)
        {
            if (!Model.Game.RLG.RLG.Currency.ContainsKey(userData.Currency))
            {
                throw new ExceptionMessage((int)ResponseCode.UnavailableCurrency, MessageCode.Message[(int)ResponseCode.UnavailableCurrency]);
            }
            //DB->新增Token設定黨
            //var results = await _commonService._serviceDB.GetRLGTokenAsync(userData.Club_id);
            //if (results == null)
            //{
            //    RlgToken RlgData = new RlgToken();
            //    RlgData.club_id = userData.Club_id;
            //    RlgData.system_code = request.GameConfig["SystemCode"];
            //    RlgData.web_id = request.GameConfig["webid"];
            //    if (await _commonService._serviceDB.PostRLGToken(RlgData) != 1)
            //    {
            //        throw new ExceptionMessage((int)ResponseCode.CreateRlgUserTokenFail, MessageCode.Message[(int)ResponseCode.CreateRlgUserTokenFail]);
            //    }
            //    results = await _commonService._serviceDB.GetRLGTokenAsync(userData.Club_id);
            //}
            ForwardGame res = new ForwardGame();
            //Step 1 Create Member
            Model.Game.RLG.RLG.lang.TryGetValue(request.GameConfig["lang"], out var lang);
            lang ??= Model.Game.RLG.RLG.lang["en-US"];
            Model.Game.RLG.Request.CreateOrSetUserRequest requestData = new Model.Game.RLG.Request.CreateOrSetUserRequest();
            requestData.SystemCode = Config.CompanyToken.RLG_SystemCode;
            requestData.WebId = Config.CompanyToken.RLG_WebID;
            requestData.UserId = userData.Club_id;
            requestData.UserName = userData.Club_Ename;
            requestData.Language = lang;
            try
            {
                var result = await _gameApiService._RlgAPI.CreateOrSetUserAsync(requestData);
                if (result.errorcode != (int)ErrorCodeEnum.OK && result.errorcode != (int)ErrorCodeEnum.DataExist)
                {
                    throw new Exception(result.errormessage);
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.CreateRsgUserFail, MessageCode.Message[(int)ResponseCode.CreateRsgUserFail] + "|" + ex.Message.ToString());
            }
            var gameUser = new GamePlatformUser();
            gameUser.club_id = userData.Club_id;
            gameUser.game_user_id = requestData.UserId;
            gameUser.game_platform = request.Platform;
            gameUser.agent_id = requestData.WebId;
            return gameUser;
        }
        public async Task<string> Login(ForwardGameReq request, Wallet userData, GamePlatformUser platformUser)
        {
            //Step 3 Get Game URL
            Model.Game.RLG.Request.GetURLTokenRequest UrlRequest = new Model.Game.RLG.Request.GetURLTokenRequest();
            UrlRequest.SystemCode = Config.CompanyToken.RLG_SystemCode;
            UrlRequest.WebId = Config.CompanyToken.RLG_WebID;
            UrlRequest.UserId = platformUser.game_user_id;
            UrlRequest.Language = request.GameConfig["lang"];
            UrlRequest.IsMobile = request.GameConfig["device"] == "DESKTOP" ? "0" : "1";//手機板1:網頁板0

            if (request.GameConfig.ContainsKey("lobbyURL"))
            {
                UrlRequest.ExitAction = request.GameConfig["lobbyURL"];
            }

            if (request.GameConfig.ContainsKey("lang") && Model.Game.RLG.RLG.lang.ContainsKey(request.GameConfig["lang"]))
            {
                UrlRequest.Language = Model.Game.RLG.RLG.lang[request.GameConfig["lang"]];
            }
            else
            {
                UrlRequest.Language = Model.Game.RLG.RLG.lang["en-US"];
            }

            if (request.GameConfig.ContainsKey("enterGame"))
            {
                UrlRequest.EnterGame = request.GameConfig["enterGame"];
            }

            if (request.GameConfig.ContainsKey("enterType"))
            {
                UrlRequest.EnterType = request.GameConfig["enterType"];
            }

            try
            {
                var token_res = await _gameApiService._RlgAPI.GetURLTokenAsync(UrlRequest);
                return token_res.data.url;
            }
            catch (Exception ex)
            {
                throw new ExceptionMessage((int)ResponseCode.GetGameURLFail, MessageCode.Message[(int)ResponseCode.GetGameURLFail] + "|" + ex.Message.ToString());
            }
        }
        public async Task<CheckTransferRecordResponse> CheckTransferRecord(WalletTransferRecord transfer_record)
        {
            var CheckTransferRecordResponse = new CheckTransferRecordResponse();
            TransferRecordRequest RlgReqData = new()
            {
                SystemCode = Config.CompanyToken.RLG_SystemCode,
                WebId = Config.CompanyToken.RLG_WebID,
                StartTime = transfer_record.create_datetime.AddMinutes(-15).AddHours(-1), // +8 => +7
                EndTime = transfer_record.create_datetime.AddMinutes(15).AddHours(-1),  // +8 => +7
                PageIndex = 1,
                PageSize = 1,
                Language = "zhtw",
                TransferNo = transfer_record.id.ToString().Replace("-", "").Substring(0, 20)
            };
            var RlgReuslt = await _gameApiService._RlgAPI.TransferRecordAsync(RlgReqData);

            if (RlgReuslt.errorcode != "000000") throw new ExceptionMessage(ResponseCode.Fail, RlgReuslt.errormessage);
            var checkResult = RlgReuslt.data.datalist.Any(r =>
                r.transferno == transfer_record.id.ToString().Replace("-", "").Substring(0, 20));

            if (checkResult)
            {
                if (transfer_record.target == nameof(Platform.RLG))//轉入RLG直接改訂單狀態為成功
                {
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RLG))
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
                if (transfer_record.target == nameof(Platform.RLG))//轉入RLG直接改訂單狀態為失敗
                {
                    CheckTransferRecordResponse.CreditChange = transfer_record.amount;
                    CheckTransferRecordResponse.LockCreditChange = -transfer_record.amount;

                }
                else if (transfer_record.source == nameof(Platform.RLG))
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
            GetBetRecord res = new();
            var createtimePair = await _summaryDBService.GetPartitionTime(Guid.Parse(RecordReq.summary_id), RecordReq.ReportTime) ?? Array.Empty<DateTime>();
            var batRecords = new List<dynamic>();
            foreach (var createTime in createtimePair)
            {
                var records = (await _rlgDbService.GetRlgV2RecordBySummary(createTime,RecordReq.ReportTime,RecordReq.ClubId)).ToList();
                if(!records.Any())
                    records = (await _rlgDbService.GetRlgRecordBySummary(RecordReq.summary_id, createTime)).ToList();

                records.ForEach(r =>
                {
                    if (r.status == 0)
                        r.drawtime = null;
                });

                batRecords.AddRange(records);
            }
            res.Data = batRecords.OrderByDescending(e => e.createtime).ToList();
            return res;
        }

        public async Task<string> GameDetailURL(GetBetDetailReq RecordDetailReq)
        {
            var summary = await _summaryDBService.GetRecordSummaryById(new()
            {
                summary_id = RecordDetailReq.summary_id.ToString(),
                ReportTime = RecordDetailReq.ReportTime,
            });
           
            //var clubtoken = await _commonService._serviceDB.GetRLGTokenAsync(summary.Club_id);

            Model.Game.RLG.RLG.lang.TryGetValue(RecordDetailReq.lang, out var lang);
            lang ??= Model.Game.RLG.RLG.lang["en-US"];

            var rlgResponseData = await _gameApiService._RlgAPI.BetInfourlAsync(new Model.Game.RLG.Request.BetInfoRequest()
            {
                BetNo = RecordDetailReq.record_id,
                Language = lang,
                SetOption = 2,
                SystemCode = Config.CompanyToken.RLG_SystemCode,
                WebId = Config.CompanyToken.RLG_WebID,
            });

            if (rlgResponseData != null && rlgResponseData.errorcode != "000000")
            {
                throw new Exception("no data");
            }
            return rlgResponseData.data.url;
        }
        
        public async Task<string> RepairGameRecord(RepairBetSummaryReq RepairReq)
        {
            var RepairCount = 0;
            RepairCount += await RepairRlg(RepairReq.StartTime, RepairReq.EndTime);


            #region 重新匯總帳
            var ReportScheduleTime = DateTime.Parse((await _systemParameterDbService.GetSystemParameter(RlgReportSchedule.SYSTEM_PARAMETERS_KEY)).value);

            var start = new DateTime(RepairReq.StartTime.Year, RepairReq.StartTime.Month, RepairReq.StartTime.Day, RepairReq.StartTime.Hour, RepairReq.StartTime.Minute, 0);
            var maxEnd = new DateTime(RepairReq.EndTime.Year, RepairReq.EndTime.Month, RepairReq.EndTime.Day, RepairReq.EndTime.Hour, RepairReq.EndTime.Minute, 0);
            var offSet = TimeSpan.FromHours(1);
            while (start < maxEnd)
            {
                if (start > ReportScheduleTime)
                {
                    break;
                }

                var end = start.Add(offSet);


                await SummaryW1Report(start, end);
                await SummaryGameProviderReport(start, end);

                start = end;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            #endregion

            var returnString = string.Format(" Game: {0} 新增資料筆數: {1}", RepairReq.game_id, RepairCount);
            return returnString;
        }

        public PlatformType GetPlatformType(Platform platform)
        {
            return PlatformType.Chess;
        }

        public Task HealthCheck(Platform platform)
        {
            return _gameApiService._RlgAPI.GetOpenGameAsync(new()
            {
                Language = Model.Game.RLG.RLG.lang["zh-TW"]
            });
        }
        #endregion
        #region GameRecordService
        public async Task<int> PostRlgRecord(List<GetBetRecordResponseDataList> recordData)
        {
            if (recordData is null || !recordData.Any())
                return 0;

            var postResult = 0;

            var start = recordData.Min(r => r.createtime).AddHours(1);// +7 => +8
            var end = recordData.Max(r => r.createtime).AddSeconds(1).AddHours(1);// +7 => +8

            var exsitsPKs = (await _rlgDbService.GetRlgRecordPrimaryKey(start, end))
                .Select(r => new { r.ordernumber, r.createtime, r.drawtime })
                .ToHashSet();

            var sw = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<IGrouping<string, GetBetRecordResponseDataList>> linqRes = recordData.GroupBy(x => x.userid);

            foreach (IGrouping<string, GetBetRecordResponseDataList> group in linqRes)
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
                            // 紀錄 reportTime 跟 playTime 的關聯
                            var dic = new Dictionary<string, HashSet<string>>();
                            var dt = DateTime.Now;
                            var reportTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, (dt.Minute / 5) * 5, 0);
                            club_id = group.Key;
                            Wallet memberWalletData = await GetWalletCache(club_id);
                           
                            //注單
                            List<GetBetRecordResponseDataList> betDetailData = new List<GetBetRecordResponseDataList>();

                            var cancelStatus = new int[] { 2, 3 };
                            var record = group.Select(r => {

                                r.drawtime = r.drawtime.AddHours(1); //+7 => +8
                                r.createtime = r.createtime.AddHours(1); //+7 => +8
                                r.pre_bettingbalance = r.bettingbalance;
                                r.pre_totalamount = r.totalamount;
                                r.club_id = memberWalletData.Club_id;
                                r.franchiser_id = memberWalletData.Franchiser_id;
                                r.report_time = reportTime;
                                r.partition_time = r.createtime;

                                if (cancelStatus.Contains(r.status))
                                    r.totalamount = decimal.Zero;

                                return r;
                            });

                            foreach (var item in record)
                            {
                                if (!exsitsPKs.Add(new { item.ordernumber, item.createtime, item.drawtime })) continue;

                                await Calculate(conn, tran, item);
                                
                                betDetailData.Add(item);
                                var summaryTime = reportTime.ToString("yyyy-MM-dd HH:mm");
                                if (!dic.ContainsKey(summaryTime))
                                {
                                    dic.Add(summaryTime, new HashSet<string>());
                                }
                                dic[summaryTime].Add(item.createtime.ToString("yyyy-MM-dd HH:mm:ss"));
                            }

                            foreach (var item in dic)
                            {
                                foreach (var subItem in item.Value)
                                {
                                    var key = nameof(Platform.RLG) + $"{RedisCacheKeys.BetSummaryTime}:{item.Key}";
                                    await _commonService._cacheDataService.ListPushAsync(key, subItem);
                                }
                            }

                            //寫入明細帳
                            if (betDetailData.Any())
                            {
                                postResult += await _rlgDbService.PostRlgRecord(conn, tran, betDetailData);
                            }

                            //寫入未結算單
                            if (betDetailData.Any(d => d.status == 0))
                            {
                                await _rlgDbService.PostRlgRunningRecord(conn, tran, betDetailData.Where(d => d.status == 0).ToList());
                            }

                            //刪除未結算單
                            foreach (var settleRecord in betDetailData.Where(d => d.status > 0))
                                await _rlgDbService.DeleteRlgRunningRecord(conn, tran, settleRecord);

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

                            _logger.LogError(ex, "Run rlg record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

                        }
                    }
                    await conn.CloseAsync();
                }
            }
            sw.Stop();
            _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
            return postResult;

        }

        //public async Task<int> PostRlgRecordback(List<GetBetRecordResponseDataList> recordData)
        //{
        //    if (recordData is null || !recordData.Any())
        //        return 0;

        //    var postResult = 0;

        //    var start = recordData.Min(r => r.createtime).AddHours(1);// +7 => +8
        //    var end = recordData.Max(r => r.createtime).AddSeconds(1).AddHours(1);// +7 => +8

        //    var exsitsPKs = (await _rlgDbService.GetRlgRecordPrimaryKey(start, end))
        //        .Select(r => new { r.ordernumber, r.createtime, r.drawtime })
        //        .ToHashSet();

        //    var sw = System.Diagnostics.Stopwatch.StartNew();
        //    IEnumerable<IGrouping<string, GetBetRecordResponseDataList>> linqRes = recordData.GroupBy(x => x.userid);

        //    foreach (IGrouping<string, GetBetRecordResponseDataList> group in linqRes)
        //    {
        //        using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.BetLog.Master))
        //        {
        //            await conn.OpenAsync();
        //            using (var tran = conn.BeginTransaction())
        //            {
        //                try
        //                {
        //                    sw.Stop();
        //                    _logger.LogDebug("Begin Transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                    sw.Restart();
        //                    string club_id;
        //                    club_id = group.Key;
        //                    Wallet memberWalletData = await GetWalletCache(club_id);
        //                    if (memberWalletData == null || memberWalletData.Club_id == null)
        //                    {
        //                        throw new Exception("沒有會員id");
        //                    }

        //                    var gameUser = await _commonService._gamePlatformUserService.GetSingleGamePlatformUserAsync(club_id, Platform.RLG);
        //                    if (gameUser == null || gameUser.game_user_id != group.Key)
        //                    {
        //                        throw new Exception("No rlg user");
        //                    }
        //                    //彙總注單
        //                    Dictionary<string, BetRecordSummary> summaryData = new Dictionary<string, BetRecordSummary>();
        //                    //注單
        //                    List<GetBetRecordResponseDataList> betDetailData = new List<GetBetRecordResponseDataList>();

        //                    var summaryBetRecordMappings = new HashSet<t_summary_bet_record_mapping>();

        //                    var cancelStatus = new int[] { 2, 3 };
        //                    var record = group.Select(r => {

        //                        r.drawtime = r.drawtime.AddHours(1); //+7 => +8
        //                        r.createtime = r.createtime.AddHours(1); //+7 => +8
        //                        r.pre_bettingbalance = r.bettingbalance;
        //                        r.pre_totalamount = r.totalamount;
        //                        r.club_id = memberWalletData.Club_id;
        //                        r.franchiser_id = memberWalletData.Franchiser_id;

        //                        if (cancelStatus.Contains(r.status))
        //                            r.totalamount = decimal.Zero;

        //                        return r;
        //                    });

        //                    foreach (var item in record)
        //                    {
        //                        if (!exsitsPKs.Add(new { item.ordernumber, item.createtime, item.drawtime })) continue;

        //                        DateTime tempDateTime = DateTime.Now;
        //                        BetRecordSummary sumData = new()
        //                        {
        //                            Club_id = memberWalletData.Club_id,
        //                            Game_id = nameof(Platform.RLG),
        //                            ReportDatetime = tempDateTime.AddTicks(-(tempDateTime.Ticks % TimeSpan.FromMinutes(5).Ticks))
        //                        };

        //                        //先確認有沒有符合的匯總單
        //                        if (summaryData.ContainsKey(sumData.ReportDatetime.ToString()))
        //                        {
        //                            sumData = summaryData[sumData.ReportDatetime.ToString()];
        //                            //合併處理
        //                            sumData = await Calculate(conn, tran, sumData, item);
        //                            summaryData[sumData.ReportDatetime.ToString()] = sumData;
        //                        }
        //                        else
        //                        {
        //                            //用Club_id與ReportDatetime DB取得彙總注單
        //                            IEnumerable<dynamic> results = await _summaryDBService.GetRecordSummaryLock(conn, tran, sumData);
        //                            sw.Stop();
        //                            _logger.LogDebug("get summary record ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                            sw.Restart();
        //                            if (!results.Any()) //沒資料就建立新的
        //                            {
        //                                //建立新的Summary
        //                                sumData.Currency = memberWalletData.Currency;
        //                                sumData.Franchiser_id = memberWalletData.Franchiser_id;

        //                                //合併處理
        //                                sumData = await Calculate(conn, tran, sumData, item);
        //                            }
        //                            else //有資料就更新
        //                            {
        //                                sumData = results.SingleOrDefault();
        //                                //合併處理
        //                                sumData = await Calculate(conn, tran, sumData, item);
        //                            }
        //                            summaryData.Add(sumData.ReportDatetime.ToString(), sumData);
        //                        }
        //                        item.summary_id = sumData.id;
        //                        betDetailData.Add(item);

        //                        var mapping = new t_summary_bet_record_mapping()
        //                        {
        //                            summary_id = sumData.id,
        //                            report_time = sumData.ReportDatetime.Value,
        //                            partition_time = item.createtime.Date
        //                        };
        //                        summaryBetRecordMappings.Add(mapping);
        //                    }

        //                    List<BetRecordSummary> summaryList = summaryData.Values.ToList();
        //                    await _summaryDBService.PostRecordSummary(conn, tran, summaryList);

        //                    //寫入匯總帳對應
        //                    await _summaryDBService.PostSummaryBetRecordMapping(tran, summaryBetRecordMappings);

        //                    //寫入明細帳
        //                    if (betDetailData.Any())
        //                    {
        //                        postResult += await _rlgDbService.PostRlgRecord(conn, tran, betDetailData);
        //                    }

        //                    //寫入未結算單
        //                    if (betDetailData.Any(d => d.status == 0))
        //                    {
        //                        await _rlgDbService.PostRlgRunningRecord(conn, tran, betDetailData.Where(d => d.status == 0).ToList());
        //                    }

        //                    //刪除未結算單
        //                    foreach (var settleRecord in betDetailData.Where(d => d.status > 0))
        //                        await _rlgDbService.DeleteRlgRunningRecord(conn, tran, settleRecord);

        //                    tran.Commit();
        //                    sw.Stop();
        //                    _logger.LogDebug("commit transaction ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //                    sw.Restart();
        //                }
        //                catch (Exception ex)
        //                {
        //                    tran.Rollback();
        //                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();

        //                    _logger.LogError(ex, "Run rlg record group: {key} exception EX : {ex}  MSG : {Message} Error Line : {errorFile}.{errorLine} ", group.Key, ex.GetType().FullName.ToString(), ex.Message.ToString(), errorFile, errorLine);

        //                }
        //            }
        //            await conn.CloseAsync();
        //        }
        //    }
        //    sw.Stop();
        //    _logger.LogDebug("end ElapsedMilliseconds: {ElapsedMilliseconds}", sw.ElapsedMilliseconds);
        //    return postResult;

        //}
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
                _logger.LogDebug("Create RLG game provider report time {datetime}", reportTime);
                // 每小時投注匯總
                // 泰國時區所以要減一小時  +7
                GetBetTotalListserverRequest req = new GetBetTotalListserverRequest()
                {
                    StartTime = reportTime.AddHours(-1).ToString("yyyy-MM-dd HH:00:00"),
                    SystemCode = Config.CompanyToken.RLG_SystemCode,
                    WebId = Config.CompanyToken.RLG_WebID,
                    GameId = "",
                    EndTime = reportTime.ToString("yyyy-MM-dd HH:00:00")
                };

                //取得這小時
                GetBetTotalListResponse rlgCenterList = await _gameApiService._RlgAPI.GetBetTotalListAsync(req);
                if (rlgCenterList.data.datalist.Length == 0)
                {
                    // 遊戲商(轉帳中心的欄位格式)
                    var gameEmptyReport = new GameReport
                    {
                        platform = nameof(Platform.RLG),
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
                    var rlgSummaryReport = new GameReport
                    {
                        platform = nameof(Platform.RLG),
                        report_datetime = reportTime,
                        report_type = (int)GameReport.e_report_type.FinancalReport,
                        total_bet = rlgCenterList.data.datalist.Sum(x => x.bettotalmoney),
                        total_win = rlgCenterList.data.datalist.Sum(x => x.wintotalmoney),
                        total_netwin = rlgCenterList.data.datalist.Sum(x => x.winlosemoney),
                        total_count = rlgCenterList.data.datalist.Sum(x => x.totalbet),
                    };

                    await _gameReportDBService.DeleteGameReport(rlgSummaryReport);
                    await _gameReportDBService.PostGameReport(rlgSummaryReport);
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
                _logger.LogDebug("Create RLG game W1 report time {datetime}", reportTime);
                IEnumerable<dynamic> dailyReport = await _rlgDbService.SumRlgBetRecordHourly(reportTime);
                var HourlylyReportData = dailyReport.SingleOrDefault();
                GameReport reportData = new GameReport();
                reportData.platform = nameof(Platform.RLG);
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
        public async Task<dynamic> GetBetRecordUnsettle(GetBetRecordUnsettleReq RecordReq)
        {
            GetBetRecordUnsettleRes res = new GetBetRecordUnsettleRes();
            IEnumerable<dynamic> rlg_results = await _rlgDbService.GetRlgRunningRecord(RecordReq);
            rlg_results = rlg_results.OrderByDescending(e => e.createtime);
            res.Data = rlg_results.ToList();
            return res;

        }
        #endregion
        private async Task Calculate(NpgsqlConnection conn, IDbTransaction tran, GetBetRecordResponseDataList r)
        {
            var Records = await _rlgDbService.GetRlgRecordV2ByOrder(conn, tran, r);
            if (!Records.Any())
                Records = await _rlgDbService.GetRlgRecordByOrder(conn, tran, r);
            if (Records.Any())
            {
                var lastRecord = Records.OrderByDescending(r => r.drawtime).First();
                r.totalamount -= lastRecord.pre_totalamount;
                r.bettingbalance -= lastRecord.pre_bettingbalance;
            }

        }

        //private async Task<BetRecordSummary> Settle(NpgsqlConnection conn, IDbTransaction tran, BetRecordSummary SummaryData, GetBetRecordResponseDataList r)
        //{
        //    var Refoundresults = await _commonService._serviceDB.GetRlgRecordByOrder(conn, tran, r);
        //    Refoundresults = Refoundresults.ToList();
        //    var settlement = Refoundresults.Where(x => x.status == 1).OrderByDescending(x => x.drawtime).FirstOrDefault();
        //    switch (r.status)
        //    {
        //        //0 未結算
        //        case 0:
        //            r.pre_bettingbalance = -1 * (r.totalamount);
        //            r.pre_totalamount = r.totalamount;

        //            SummaryData.RecordCount++;
        //            SummaryData.Bet_amount += r.totalamount;
        //            SummaryData.Turnover += 0;
        //            SummaryData.Netwin += (r.bettingbalance - r.totalamount);
        //            SummaryData.Win += 0;
        //            if (r.drawtime > DateTime.Now.AddDays(29))
        //            {
        //                r.drawtime = DateTime.Now.Date.AddDays(1);
        //            }
        //            r.bettingbalance = -1 * (r.totalamount);
        //            break;
        //        //不處理 重複單
        //        case 4:
        //            //waiting單暫不處理
        //            break;
        //        case 1:
        //            if (Refoundresults.Where(x => x.status == 0).ToList().Count == 0 && Refoundresults.Where(x => x.status == 1).ToList().Count == 0) //沒有未結算單，也沒有已結算單
        //            {
        //                r.pre_bettingbalance = r.bettingbalance;
        //                r.pre_totalamount = r.totalamount;
        //                SummaryData.RecordCount++;
        //                SummaryData.Bet_amount += r.totalamount;
        //                SummaryData.Turnover += r.totalamount;
        //                SummaryData.Netwin += (r.bettingbalance - r.totalamount);
        //                SummaryData.Win += r.bettingbalance != 0 ? r.bettingbalance : (r.bettingbalance - r.totalamount);
        //            }
        //            else
        //            {
        //                if (Refoundresults.Where(x => x.status == 1).ToList().Count == 0) //有未結算單，沒有已結算
        //                {
        //                    SummaryData.RecordCount++;
        //                    SummaryData.Bet_amount += 0;
        //                    SummaryData.Turnover += r.totalamount;
        //                    SummaryData.Netwin += r.bettingbalance != 0 ? r.bettingbalance : 0;
        //                    SummaryData.Win += r.bettingbalance;

        //                    //明細也要將未結算單預扣金額加回去
        //                    r.pre_bettingbalance = r.bettingbalance;
        //                    r.pre_totalamount = r.totalamount;
        //                    r.totalamount = 0;

        //                }
        //                else //有已結算單
        //                {
        //                    if (Refoundresults.Where(x => x.status == 0).ToList().Count == 0) //沒有未結算，有已結算
        //                    {
        //                        r.totalamount = 0;
        //                        r.pre_bettingbalance = r.bettingbalance != 0 ? r.bettingbalance : -1 * (settlement.bettingbalance);
        //                        r.pre_totalamount = 0;
        //                        r.bettingbalance = (r.bettingbalance - settlement.bettingbalance);
        //                        SummaryData.RecordCount++;
        //                        SummaryData.Bet_amount += 0;
        //                        SummaryData.Turnover += 0;
        //                        SummaryData.Netwin += r.bettingbalance;
        //                        SummaryData.Win += r.bettingbalance;

        //                        //明細也要將未結算單預扣金額加回去

        //                    }
        //                    else //有未結算，也有已結算
        //                    {
        //                        //明細也要將未結算單預扣金額加回去
        //                        r.pre_bettingbalance = r.bettingbalance != 0 ? r.bettingbalance : -1 * (settlement.bettingbalance);
        //                        r.bettingbalance = (r.bettingbalance - settlement.bettingbalance);
        //                        r.pre_totalamount = 0;
        //                        r.totalamount = 0;

        //                        SummaryData.RecordCount++;
        //                        SummaryData.Bet_amount += 0;
        //                        SummaryData.Turnover += 0;
        //                        SummaryData.Netwin += r.bettingbalance != 0 ? r.bettingbalance : 0;
        //                        SummaryData.Win += r.bettingbalance;
        //                    }
        //                }
        //            }
        //            await _commonService._serviceDB.DeleteRlgRunningRecord(conn, tran, r);
        //            break;
        //        case 2://取消單           
        //        case 3://刪除單
        //            //無未結算 無結算
        //            if (Refoundresults.Where(x => x.status == 0).ToList().Count == 0 && Refoundresults.Where(x => x.status == 1).ToList().Count == 0)
        //            {
        //                r.totalamount = 0;
        //                r.pre_bettingbalance = 0;
        //                r.pre_totalamount = 0;
        //                SummaryData.RecordCount++;
        //                SummaryData.Bet_amount += r.totalamount;
        //                SummaryData.Turnover += 0;
        //                SummaryData.Netwin += 0;
        //                SummaryData.Win += r.bettingbalance;
        //            }
        //            //有未結算單 無結算單
        //            if (Refoundresults.Where(x => x.status == 0).ToList().Count == 1 && Refoundresults.Where(x => x.status == 1).ToList().Count == 0)
        //            {
        //                r.bettingbalance = r.totalamount;
        //                SummaryData.RecordCount++;
        //                SummaryData.Bet_amount += 0;
        //                SummaryData.Turnover += 0;
        //                SummaryData.Netwin += r.bettingbalance;
        //                SummaryData.Win += r.bettingbalance;

        //                //明細也要將未結算單預扣金額加回去
        //                r.pre_bettingbalance = 0;
        //                r.pre_totalamount = 0;
        //                r.totalamount = 0;
        //            }
        //            //無未結算單 有結算
        //            if (Refoundresults.Where(x => x.status == 0).ToList().Count == 0 && Refoundresults.Where(x => x.status == 1).ToList().Count >= 1)
        //            {
        //                r.bettingbalance = (-1 * (settlement.bettingbalance)) + r.totalamount;//取出已結帳單 將金錢扣回去
        //                SummaryData.RecordCount++;
        //                SummaryData.Bet_amount += r.totalamount;
        //                SummaryData.Turnover += 0;
        //                SummaryData.Netwin += r.bettingbalance;
        //                SummaryData.Win += r.bettingbalance;

        //                //明細也要將未結算單預扣金額加回去

        //                r.pre_bettingbalance = (-1 * (settlement.bettingbalance));
        //                r.pre_totalamount = -r.totalamount;
        //            }
        //            //有未結算  有結算
        //            if (Refoundresults.Where(x => x.status == 0).ToList().Count == 1 && Refoundresults.Where(x => x.status == 1).ToList().Count >= 1)
        //            {
        //                r.bettingbalance = settlement.bettingbalance < 0 ? r.totalamount : (-1 * (settlement.bettingbalance) + r.totalamount);//取出已結帳單 將金錢扣回去
        //                SummaryData.RecordCount++;
        //                SummaryData.Bet_amount += 0;
        //                SummaryData.Turnover += 0;
        //                SummaryData.Netwin += r.bettingbalance != 0 ? r.bettingbalance : 0;
        //                SummaryData.Win += r.bettingbalance;

        //                //明細也要將未結算單預扣金額加回去

        //                r.pre_bettingbalance = settlement.bettingbalance < 0 ? 0 : -1 * (settlement.bettingbalance);
        //                r.pre_totalamount = -r.totalamount;
        //                r.totalamount = 0;
        //            }
        //            await _commonService._serviceDB.DeleteRlgRunningRecord(conn, tran, r);
        //            break;
        //    }
        //    SummaryData.updatedatetime = DateTime.Now;
        //    SummaryData.JackpotWin = 0;
        //    return SummaryData;
        //}

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
        public async Task<List<GetBetRecordResponseDataList>> GetRlgBetRecords(DateTime startTime, DateTime endTime)
        {
            var result = new List<GetBetRecordResponseDataList>();
            GetBetRecordRequest req = new GetBetRecordRequest();
            req.StartTime = startTime.AddHours(-1); // +8 => +7
            req.SystemCode = Config.CompanyToken.RLG_SystemCode;
            req.WebId = Config.CompanyToken.RLG_WebID;
            req.UserId = "";
            req.GameId = "";
            req.EndTime = endTime.AddHours(-1); // +8 => +7
            req.PageSize = 1000;
            req.Language = "zhcn";

            var setOptions = new int[] { 1, 2 }; //1:未結算(投注時間)、2:已結算(實際開獎時間)
            foreach(var setOption in setOptions)
            {
                req.SetOption = setOption;
                req.PageIndex = 1;
                while (true)
                {
                    var response = await _gameApiService._RlgAPI.GetBetRecordAsync(req);

                    if (response.errorcode != "000000")
                        throw new ExceptionMessage(ResponseCode.Fail, $"RLG GetBetRecord Fail! Message:{response.errormessage}");

                    if(response.data.datalist.Any())
                        result.AddRange(response.data.datalist);

                    if (response.data.totalPage <= response.data.currentPage)
                        break;

                    req.PageIndex++;
                }
            }

            return result;
        }

        private async Task<int> RepairRlg(DateTime startTime, DateTime endTime)
        {
            var response = await GetRlgBetRecords(startTime, endTime);
            if (!response.Any()) return 0;

            var postResult = 0;
            foreach (var group in response.GroupBy(b => b.createtime.Ticks / (long)TimeSpan.FromHours(3).Ticks))
            {
                postResult += await PostRlgRecord(group.ToList());
            }
            return postResult;
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
            var summaryRecords = await _rlgDbService.SummaryGameRecord(reportDatetime, startTime, endTime);
            var Groupsummary = summaryRecords.GroupBy(x => x.userid);
            sw1.Stop();
            _logger.LogInformation("SummaryGameRecord count:{count}, cost:{cost}", summaryRecords.Count(), sw1.ElapsedMilliseconds);

            var userlist = summaryRecords.Select(x => x.userid).Distinct().ToList();
            var userWalletList = (await _commonService._serviceDB.GetWallet(userlist)).ToDictionary(r => r.Club_id, r => r);
            var summaryRecordList = new List<BetRecordSummary>();
            var summaryBetRecordMappings = new List<t_summary_bet_record_mapping>();


            foreach (var summaryRecord in Groupsummary)
            {
                if (!userWalletList.TryGetValue(summaryRecord.Key, out var userWallet)) continue;

                var summaryData = new BetRecordSummary();
                summaryData.Turnover = summaryRecord.Sum(x => x.bet);
                summaryData.ReportDatetime = reportDatetime;
                summaryData.Currency = userWallet.Currency;
                summaryData.Club_id = userWallet.Club_id;
                summaryData.Franchiser_id = userWallet.Franchiser_id;
                summaryData.RecordCount = summaryRecord.Sum(x => x.count);
                summaryData.Game_id = nameof(Platform.RLG);
                summaryData.Game_type = summaryRecord.Sum(x =>x.game_type);
                summaryData.JackpotWin = summaryRecord.Sum(x => x.jackpot);
                summaryData.Bet_amount = summaryRecord.Sum(x => x.bet);
                summaryData.Win = summaryRecord.Sum(x => x.win);
                summaryData.Netwin = summaryRecord.Sum(x => x.win) - summaryRecord.Sum(x => x.bet);
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
