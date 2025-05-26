using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using H1_ThirdPartyWalletAPI.Service.Game;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model.Config;
using System.Data;
using static H1_ThirdPartyWalletAPI.Model.DataModel.WalletTransferRecord;

namespace H1_ThirdPartyWalletAPI.Service
{
    public interface ITransferService
    {
        public Task<string> DepositGameWalletV2(GamePlatformUser platform_user, Platform Source, Platform Target, decimal? stop_balance, decimal Amount = 0);
        public Task<string> WithDrawGameWalletV2(GamePlatformUser platform_user, Platform Source, Platform Target, decimal Amount = 0);
        public Task<string> CheckTransferRecord(DateTime startTime, DateTime endTime, WalletTransferRecord.TransferStatus status);
        public Task<bool> KickUser(GamePlatformUser platform_user, string gamePlatform);
        public Task<TransferFund> TransferFund(Guid id, string Club_id, Platform Source, Platform Target, decimal amount, bool CashOutAll);
        Task<ResCodeBase> H1TransferFundIn(Guid Session_id, string Club_id, decimal amount);
        Task<ResCodeBase> H1TransferFundOut(Guid Session_id, string Club_id);
        public Task<TransferFund> TransferMemberWallet(NpgsqlConnection conn, IDbTransaction tran, Guid id, string Club_id, Platform Source, Platform Target, decimal amount, bool CashOutAll);
        public Task<string> CheckSingleTransferRecord(WalletTransferRecord transferRecord);
        Task<ResCodeBase> DeleteElectronicDepositRecordCache(string Club_id);
        Task<List<WalletTransferRecord>> GetElectronicDepositRecordCache(string Club_id);
    }
    public class TransferService : ITransferService
    {
        private readonly ILogger<TransferService> _logger;
        private readonly ICommonService _commonService;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly IGameApiService _gameaApiService;
        private readonly ITransferWalletService _transferWalletService;
        private readonly ISingleWalletService _singleWalletService;
        private int _cacheSeconds = 600;
        private readonly ICacheDataService _cacheDataService;
        public TransferService(ILogger<TransferService> logger
            , IGameApiService gameaApiService
            , IGameInterfaceService gameInterfaceService
            , ICommonService commonService
            , ITransferWalletService transferWalletService
            , ISingleWalletService singleWalletService
            , ICacheDataService cacheDataService
          )
        {
            _logger = logger;
            _gameaApiService = gameaApiService;
            _gameInterfaceService = gameInterfaceService;
            _commonService = commonService;
            _transferWalletService = transferWalletService;
            _singleWalletService = singleWalletService;
            _cacheDataService = cacheDataService;

        }
        public async Task<TransferFund> TransferMemberWallet(NpgsqlConnection conn, IDbTransaction tran, Guid id, string Club_id, Platform Source, Platform Target, decimal amount, bool CashOutAll)
        {
            TransferFund res = new TransferFund();
            try
            {
                await _commonService._cacheDataService.LockAsync(
                $"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}",
                async () =>
                {
                    Wallet walletData;
                    // 取餘額
                    if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                    {
                        walletData = await _singleWalletService.GetWallet(Club_id);
                    }
                    else
                    {
                        walletData = await _transferWalletService.GetWallet(conn, tran, Club_id);
                    }
                    if (walletData == null)
                    {
                        throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                    }
                    decimal before_balance = walletData.Credit;
                    decimal lock_balance = walletData.Lock_credit;
                    decimal after_balance = 0;
                    string type = "";
                    string Franchiser_id = walletData.Franchiser_id;

                    if (Source == Platform.H1) //轉入中心錢包
                    {
                        after_balance = before_balance + amount;
                        type = nameof(WalletTransferRecord.TransferType.IN);
                    }
                    else//從中心錢包轉出
                    {
                        type = nameof(WalletTransferRecord.TransferType.OUT);
                        if (CashOutAll)//將所有錢包餘額轉出
                        {
                            if (before_balance == 0)//W1錢包沒餘額
                            {
                                throw new ExceptionMessage((int)ResponseCode.InsufficientBalance, MessageCode.Message[(int)ResponseCode.InsufficientBalance]);
                            }
                            amount = before_balance;
                        }
                        after_balance = before_balance - amount;
                        if (after_balance < 0)
                        {
                            throw new ExceptionMessage((int)ResponseCode.InsufficientBalance, MessageCode.Message[(int)ResponseCode.InsufficientBalance]);
                        }
                    }
                    //insert record
                    WalletTransferRecord RecordData = new WalletTransferRecord();
                    RecordData.id = id;
                    RecordData.source = Source.ToString();
                    RecordData.target = Target.ToString();
                    RecordData.create_datetime = DateTime.Now;
                    RecordData.success_datetime = DateTime.Now;
                    RecordData.amount = amount;
                    RecordData.before_balance = before_balance;
                    RecordData.after_balance = after_balance;
                    RecordData.status = nameof(TransferStatus.init);
                    RecordData.Club_id = Club_id;
                    RecordData.Franchiser_id = Franchiser_id;
                    RecordData.type = type;
                    int insert_record_result = await _commonService._serviceDB.PostTransferRecord(conn, tran, RecordData);
                    if (insert_record_result != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.InsertTransferRecordFail, MessageCode.Message[(int)ResponseCode.InsertTransferRecordFail]);
                    }

                    //update record
                    RecordData.status = "success";
                    RecordData.success_datetime = DateTime.Now;
                    if (await _commonService._serviceDB.PutTransferRecord(conn, tran, RecordData) != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.UpdateTransferRecordFail, MessageCode.Message[(int)ResponseCode.UpdateTransferRecordFail]);
                    }
                    walletData.Credit = after_balance;
                    walletData.Lock_credit = lock_balance;

                    if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                    {
                        await _singleWalletService.SetWallet(walletData);
                    }
                    else
                    {
                        await _transferWalletService.SetWallet(conn, tran, walletData);
                    }
                    res.code = (int)ResponseCode.Success;
                    res.Message = MessageCode.Message[(int)ResponseCode.Success];
                    res.Data = RecordData;
                });
                return res;
            }
            catch (ExceptionMessage ex)
            {
                res.code = ex.MsgId;
                res.Message = ex.Message;
                return res;
            }
        }
        public async Task<string> DepositGameWalletV2(GamePlatformUser platform_user, Platform Source, Platform Target, decimal? stop_balance, decimal Amount = 0)
        {
            try
            {
                Wallet walletData = new Wallet();
                decimal game_balance = 0;
                stop_balance = stop_balance ?? -1;
                //1. 將轉入金額鎖定
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            //處理小數點2位以下不轉入
                            walletData = await _transferWalletService.GetWallet(conn, tran, platform_user.club_id);
                            //判定停利額度
                            if (stop_balance != -1)
                            {
                                if (walletData.Credit >= stop_balance)
                                {
                                    throw new ExceptionMessage((int)ResponseCode.OverStopBalance, MessageCode.Message[(int)ResponseCode.OverStopBalance]);
                                }
                            }
                            decimal transfer_amount = Math.Floor(walletData.Credit * 100) / 100;
                            //不指定轉入金額
                            if (Amount == 0)
                            {
                                walletData.Lock_credit += transfer_amount;
                                game_balance = transfer_amount;
                                walletData.Credit -= transfer_amount;
                            }
                            else
                            {
                                walletData.Lock_credit += Amount;
                                game_balance = Amount;
                                walletData.Credit -= Amount;
                            }
                            await _transferWalletService.SetWallet(conn, tran, walletData);
                            await tran.CommitAsync();
                        }
                        catch (ExceptionMessage ex)
                        {
                            throw new ExceptionMessage((int)ResponseCode.OverStopBalance, MessageCode.Message[(int)ResponseCode.OverStopBalance]);
                        }
                        catch (Exception ex)
                        {
                            await tran.RollbackAsync();
                            var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                            var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                            _logger.LogError("DepositGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                        }
                    }
                    await conn.CloseAsync();
                }

                //2. 建立訂單
                WalletTransferRecord RecordData = new WalletTransferRecord();
                RecordData.id = Guid.NewGuid();
                RecordData.source = Source.ToString();
                RecordData.target = Target.ToString();
                RecordData.create_datetime = DateTime.Now;
                RecordData.success_datetime = DateTime.Now;
                RecordData.amount = game_balance;
                RecordData.status = nameof(TransferStatus.init);
                RecordData.Club_id = platform_user.club_id;
                RecordData.Franchiser_id = walletData.Franchiser_id;
                RecordData.type = Target.ToString();
                if (game_balance > 0)
                {

                    int insert_record_result = await _commonService._serviceDB.PostTransferRecord(RecordData);
                    if (insert_record_result != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.InsertTransferRecordFail, MessageCode.Message[(int)ResponseCode.InsertTransferRecordFail]);
                    }

                    //3. API轉入遊戲
                    var transferResult = await _gameInterfaceService.Deposit(platform_user, walletData, RecordData);
                    using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                    {
                        await conn.OpenAsync();
                        using (var tran = conn.BeginTransaction())
                        {
                            try
                            {
                                //4. 取得錢包並鎖定
                                walletData = await _transferWalletService.GetWallet(conn, tran, platform_user.club_id);
                                if (transferResult == nameof(WalletTransferRecord.TransferStatus.success))
                                {
                                    RecordData.before_balance = walletData.Credit + RecordData.amount;
                                    RecordData.after_balance = walletData.Credit;
                                    RecordData.success_datetime = DateTime.Now;
                                    walletData.Lock_credit = walletData.Lock_credit - RecordData.amount;
                                }
                                else if (transferResult == nameof(WalletTransferRecord.TransferStatus.pending))
                                {
                                    RecordData.before_balance = walletData.Credit + RecordData.amount;
                                    RecordData.after_balance = walletData.Credit;
                                    RecordData.success_datetime = DateTime.Now;
                                }
                                else
                                {
                                    walletData.Lock_credit = walletData.Lock_credit - RecordData.amount;
                                    walletData.Credit = walletData.Credit + RecordData.amount;
                                    RecordData.before_balance = walletData.Credit;
                                    RecordData.after_balance = walletData.Credit;
                                    RecordData.success_datetime = DateTime.Now;
                                }
                                //5. 更新Wallet
                                walletData.last_platform = Target.ToString();
                                await _transferWalletService.SetWallet(conn, tran, walletData);
                                await _commonService._serviceDB.SetWalletLastPlatform(tran, walletData.Club_id, Target.ToString());
                                //6. 更新交易紀錄
                                int update_record_result = await _commonService._serviceDB.PutTransferRecord(conn, tran, RecordData);
                                if (update_record_result != 1)
                                {
                                    throw new ExceptionMessage((int)ResponseCode.UpdateTransferRecordFail, MessageCode.Message[(int)ResponseCode.UpdateTransferRecordFail]);
                                }
                                await tran.CommitAsync();



                            }
                            catch (ExceptionMessage ex)
                            {
                                await tran.RollbackAsync();
                                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                                _logger.LogError("DepositGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                                return ex.Message.ToString();
                            }
                            catch (Exception ex)
                            {
                                await tran.RollbackAsync();
                                _logger.LogError(ex, "DepositGameWalletV2 exception");
                            }
                        }
                        await conn.CloseAsync();
                    }
                }

                //7.紀錄電子遊戲轉入
                var plarformType = _gameInterfaceService.GetPlatformType((Platform)Enum.Parse(typeof(Platform), RecordData.target.ToUpper()));
                if ((int)plarformType == (int)PlatformType.Electronic)//判斷是否為純電子遊戲
                    EnqueueElectronicDepositRecordCache(RecordData);
                return "success";
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogInformation("DepositGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("DepositGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
        }
        public async Task<string> WithDrawGameWalletV2(GamePlatformUser gameUser, Platform Source, Platform Target, decimal Amount = 0)
        {
            try
            {
                WalletTransferRecord RecordData = new WalletTransferRecord();
                //1. 取得遊戲餘額
                MemberBalance memberBalance = await _transferWalletService.GetGameCredit(Source, gameUser);
                //2. 確認餘額足夠(餘額0或餘額不足就直接提款遊戲現在餘額)
                decimal game_balance = 0;
                if (Amount == 0 || Amount > memberBalance.Amount)
                {
                    game_balance = memberBalance.Amount;
                }
                else
                {
                    game_balance = Amount;
                }
                var walletData = await _transferWalletService.GetWalletCache(gameUser.club_id);
                if (game_balance > 0)
                {
                    //3. 建立訂單(餘額大於0才作轉帳)
                    RecordData.id = Guid.NewGuid();
                    RecordData.source = Source.ToString();
                    RecordData.target = Target.ToString();
                    RecordData.create_datetime = DateTime.Now;
                    RecordData.success_datetime = DateTime.Now;
                    RecordData.amount = game_balance;
                    RecordData.status = nameof(TransferStatus.init);
                    RecordData.Club_id = walletData.Club_id;
                    RecordData.Franchiser_id = walletData.Franchiser_id;
                    RecordData.type = Source.ToString();
                    int insert_record_result = await _commonService._serviceDB.PostTransferRecord(RecordData);
                    if (insert_record_result != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.InsertTransferRecordFail, MessageCode.Message[(int)ResponseCode.InsertTransferRecordFail]);
                    }
                    //4. API從遊戲轉出
                    var transferResult = await _gameInterfaceService.Withdraw(gameUser, walletData, RecordData);
                    //5. 更新交易紀錄,更新餘額
                    using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                    {
                        await conn.OpenAsync();
                        using (var tran = conn.BeginTransaction())
                        {
                            try
                            {
                                //6. 取得錢包並鎖定
                                walletData = await _transferWalletService.GetWallet(conn, tran, walletData.Club_id);
                                if (transferResult == nameof(WalletTransferRecord.TransferStatus.success))
                                {
                                    RecordData.before_balance = walletData.Credit;
                                    RecordData.after_balance = RecordData.before_balance + RecordData.amount;
                                    RecordData.success_datetime = DateTime.Now;
                                    walletData.Credit = RecordData.after_balance;
                                }
                                else if (transferResult == nameof(WalletTransferRecord.TransferStatus.pending))
                                {
                                    RecordData.before_balance = walletData.Credit;
                                    RecordData.after_balance = RecordData.before_balance + RecordData.amount;
                                    RecordData.success_datetime = DateTime.Now;
                                    walletData.Lock_credit += RecordData.amount;
                                }
                                else
                                {
                                    RecordData.before_balance = walletData.Credit;
                                    RecordData.after_balance = RecordData.before_balance;
                                    RecordData.success_datetime = DateTime.Now;
                                }
                                //7. 更新Wallet
                                await _transferWalletService.SetWallet(conn, tran, walletData);
                                //8. 更新交易紀錄
                                int update_record_result = await _commonService._serviceDB.PutTransferRecord(conn, tran, RecordData);
                                if (update_record_result != 1)
                                {
                                    throw new ExceptionMessage((int)ResponseCode.UpdateTransferRecordFail, MessageCode.Message[(int)ResponseCode.UpdateTransferRecordFail]);
                                }
                                await tran.CommitAsync();
                            }
                            catch (ExceptionMessage ex)
                            {
                                await tran.RollbackAsync();
                                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                                _logger.LogError("WithDrawGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                                return ex.Message.ToString();
                            }
                            catch (Exception ex)
                            {
                                await tran.RollbackAsync();
                            }
                        }
                        await conn.CloseAsync();
                    }
                }
                else if (game_balance < 0) //遊戲負額度需要沖銷帳
                {
                    await DepositGameWalletV2(gameUser, Target, Source, -1, Math.Abs(game_balance));
                }
                //將last_platform清空
                await UpdateLastPlatform(walletData, Source, Target);
                return "success";
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("WithDrawGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("WithDrawGameWalletV2 exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
        }


        public async Task<string> CheckSingleTransferRecord(WalletTransferRecord transferRecord)
        {
            var expiry = TimeSpan.FromSeconds(15);
            var wait = TimeSpan.FromMilliseconds(10);
            var retry = TimeSpan.FromMilliseconds(10);
            var lockKey = $"{LockRedisCacheKeys.W1WalletLock}:{transferRecord.Club_id}";
            return await _commonService._cacheDataService.LockAsyncRegular(lockKey,
                () => CheckSingleTransferRecordCore(transferRecord)
                , expiry, wait, retry);
        }

        public async Task<string> CheckSingleTransferRecordCore(WalletTransferRecord transferRecord)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var lockRecord = await _commonService._serviceDB.GetTransferRecordByIdLock(conn, tran, transferRecord.id, transferRecord.create_datetime);
                        await handlePendingTransferRecord(conn, tran, lockRecord);
                        await tran.CommitAsync();
                    }
                    catch (ExceptionMessage ex)
                    {
                        await tran.RollbackAsync();
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError("CheckSingleTransferRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                        return ex.Message.ToString();
                    }
                    catch (Exception ex)
                    {
                        await tran.RollbackAsync();
                        var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                        var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                        _logger.LogError("CheckSingleTransferRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                        return ex.Message.ToString();
                    }
                }
                await conn.CloseAsync();
                return "success";
            }
        }
        public async Task<string> CheckTransferRecord(DateTime startTime, DateTime endTime, WalletTransferRecord.TransferStatus TransferStatus)
        {
            try
            {
                GetApiHealthRes res = new GetApiHealthRes();
                res.Data = await _commonService._apiHealthCheck.GetAllHealthInfo();

                var PlatformList = res.Data.Where(x => x.Status == 0).ToList();

                WalletTransferRecord record_filter = new WalletTransferRecord();
                record_filter.status = TransferStatus.ToString();
                IEnumerable<WalletTransferRecord> results = await _commonService._serviceDB.GetTransferRecord(record_filter, startTime, endTime);
                results = results.Where(r => PlatformList.Any(p => p.Platform.ToUpper() == r.type.ToUpper())).ToList();

                foreach (WalletTransferRecord transferRecord in results)
                {
                    var result = await CheckSingleTransferRecord(transferRecord);
                }
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CheckTransferRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("CheckTransferRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return ex.Message.ToString();
            }
            return "success";
        }
        public async Task<bool> KickUser(GamePlatformUser platform_user, string gamePlatform)
        {
            Platform platformid = (Platform)Enum.Parse(typeof(Platform), gamePlatform.ToUpper());
            await _gameInterfaceService.KickUser(platformid, platform_user);
            return true;
        }
        private async Task UpdateLastPlatform(Wallet userWallet, Platform Source, Platform Target)
        {
            //最後遊戲是轉出遊戲,將last_platform清空
            if (Source.ToString() == userWallet.last_platform)
            {
                userWallet.last_platform = null;
                await _commonService._serviceDB.PutWalletLastPlatform(userWallet);
                await _commonService._serviceDB.DeleteWalletLastPlatform(userWallet.Club_id);
                return;
            }
            //轉入遊戲, 將last_platform改為target platform
            else if (Source.ToString() == nameof(Platform.W1))
            {
                userWallet.last_platform = Target.ToString();
                await _commonService._serviceDB.PutWalletLastPlatform(userWallet);
                await _commonService._serviceDB.SetWalletLastPlatform(userWallet.Club_id, Target.ToString());
                return;
            }
            else
            {
                return;
            }
        }
        private async void EnqueueElectronicDepositRecordCache(WalletTransferRecord transferRecord)
        {
            try
            {
                var queueKey = $"{RedisCacheKeys.ElectronicDepositRecord}:{transferRecord.Club_id}";
                var records = await _commonService._cacheDataService.StringGetAsync<List<WalletTransferRecord>>(queueKey);
                records ??= new();
                // 分別處理 amount > 0 和 amount == 0 的記錄
                var recordsWithPositiveAmount = records.Where(r => r.amount > 0).OrderByDescending(r => r.create_datetime).ToList();
                var recordsWithZeroAmount = records.Where(r => r.amount == 0).OrderByDescending(r => r.create_datetime).ToList();

                // 處理當前 transferRecord
                if (transferRecord.amount > 0)
                {
                    // 如果 amount > 0，過濾掉相同 type 的記錄，並加入當前的 transferRecord，保留最多 3 筆
                    recordsWithPositiveAmount = FilterAndAddRecord(recordsWithPositiveAmount, transferRecord, 3);
                }
                else if (transferRecord.amount == 0)
                {
                    if (!recordsWithPositiveAmount.Any(r => r.type == transferRecord.type))
                    {
                        // 如果沒有相同 type 且 amount > 0 的記錄，則保留最新的 amount == 0 記錄
                        recordsWithZeroAmount = FilterAndAddRecord(recordsWithZeroAmount, transferRecord, 1);
                    }
                }

                // 合併並排序，最終保留最新的每個 type 記錄
                var finalRecords = recordsWithPositiveAmount
                .Concat(recordsWithZeroAmount)
                .GroupBy(r => r.type)
                .Select(g => g.OrderByDescending(r => r.create_datetime).FirstOrDefault())  // Get the most recent record per type
                .ToList();

                await _commonService._cacheDataService.StringSetAsync(queueKey, finalRecords, (int)TimeSpan.FromDays(14).TotalSeconds);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "EnqueueElectronicDepositRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
        /// <summary>
        /// 提取邏輯: 過濾掉相同 type 的記錄，加入新的 transferRecord 並按時間排序
        /// </summary>
        /// <param name="recordList"></param>
        /// <param name="newRecord"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        private List<WalletTransferRecord> FilterAndAddRecord(List<WalletTransferRecord> recordList, WalletTransferRecord newRecord, int takeCount)
        {
            return recordList
                .Where(r => r.type != newRecord.type) // 過濾掉相同 type 的記錄
                .Append(newRecord)  // 加入新的 transferRecord
                .OrderByDescending(r => r.create_datetime)  // 按時間排序
                .Take(takeCount)  // 保留最多 N 筆
                .ToList();
        }

        public async Task handlePendingTransferRecord(NpgsqlConnection conn, IDbTransaction tran, WalletTransferRecord r)
        {
            Guid transation_id = r.id;
            decimal CreditChange = 0;
            decimal LockCreditChange = 0;
            var CheckTransferRecordResponse = await _gameInterfaceService.CheckTransferRecord(r);
            CreditChange = CheckTransferRecordResponse.CreditChange;
            LockCreditChange = CheckTransferRecordResponse.LockCreditChange;
            if (CreditChange != 0 || LockCreditChange != 0)
            {
                await _commonService._cacheDataService.LockAsync(
                $"{RedisCacheKeys.WalletTransaction}/wallet/{r.Club_id}",
                async () =>
                {
                    // 取餘額
                    Wallet walletData = await _transferWalletService.GetWallet(conn, tran, r.Club_id);
                    // 更新餘額存進redis
                    walletData.Credit += CreditChange;
                    walletData.Lock_credit += LockCreditChange;

                    if (walletData.Credit < 0)
                    {
                        throw new ExceptionMessage((int)ResponseCode.InsufficientBalance, MessageCode.Message[(int)ResponseCode.InsufficientBalance]);
                    }
                    if (walletData.Lock_credit < 0)
                    {
                        throw new ExceptionMessage((int)ResponseCode.InsufficientLockBalance, MessageCode.Message[(int)ResponseCode.InsufficientLockBalance]);
                    }
                    if (Config.OneWalletAPI.WalletMode == "SingleWallet")
                    {
                        await _singleWalletService.SetWallet(walletData);
                    }
                    else
                    {
                        await _transferWalletService.SetWallet(conn, tran, walletData);
                    }
                    //紀錄最後餘額
                    r.after_balance = walletData.Credit;
                });
            }
            if (await _commonService._serviceDB.PutTransferRecord(conn, tran, CheckTransferRecordResponse.TRecord) != 1)
            {
                throw new ExceptionMessage((int)ResponseCode.UpdateTransferRecordFail, MessageCode.Message[(int)ResponseCode.UpdateTransferRecordFail]);
            }
        }
        public async Task<ResCodeBase> H1TransferFundIn(Guid Session_id, string Club_id, decimal amount)
        {
            var res = new ResCodeBase();
            try
            {
                WalletSessionV2 resData = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{Club_id}",
                async () =>
                {
                    List<short> status = new List<short>{
                              (short)WalletSessionV2.SessionStatus.DEPOSIT
                            , (short)WalletSessionV2.SessionStatus.WITHDRAW
                            , (short)WalletSessionV2.SessionStatus.REFUND
                     };
                    var walletSessionV2 = await _commonService._serviceDB.GetWalletSessionV2(status, Club_id);
                    if (walletSessionV2.Count() == 1)
                    {
                        return walletSessionV2.SingleOrDefault();
                    }
                    else if (walletSessionV2.Count() > 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.SessionMultiFail, MessageCode.Message[(int)ResponseCode.SessionMultiFail]);
                    }
                    else
                    {
                        return null;
                    }
                },
                _cacheSeconds);
                if (resData != null)
                {
                    throw new ExceptionMessage((int)ResponseCode.SessionNotEnd, MessageCode.Message[(int)ResponseCode.SessionNotEnd]);
                }
                //取得User Wallet
                Wallet userWallet = await _transferWalletService.GetWalletCache(Club_id);
                //產生Session Data
                WalletSessionV2 walletSessionV2 = new WalletSessionV2();
                walletSessionV2.session_id = Session_id;
                walletSessionV2.start_time = DateTime.Now;
                walletSessionV2.status = WalletSessionV2.SessionStatus.DEPOSIT;
                walletSessionV2.club_id = Club_id;
                walletSessionV2.start_balance = amount;
                walletSessionV2.franchiser_id = userWallet.Franchiser_id;
                //先將club Session Data寫入Redis
                await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{Club_id}", walletSessionV2, _cacheSeconds);
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            if (!await _commonService._serviceDB.PostWalletSessionV2(conn, tran, walletSessionV2))
                            {
                                throw new Exception("Post wallet session fail");
                            }
                            var transferResult = await TransferMemberWallet(conn, tran, Guid.NewGuid(), Club_id, Platform.H1, Platform.W1, amount, true);
                            if (transferResult.code == (int)ResponseCode.Success)
                            {
                                await tran.CommitAsync();
                            }
                            else
                            {
                                throw new Exception(transferResult.Message);
                            }
                        }
                        catch (ExceptionMessage ex)
                        {
                            await tran.RollbackAsync();
                            await _commonService._cacheDataService.KeyDelete($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{Club_id}");
                            throw new ExceptionMessage(ex.MsgId, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            await tran.RollbackAsync();
                            await _commonService._cacheDataService.KeyDelete($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{Club_id}");
                            throw new Exception(ex.Message);
                        }
                    }
                    await conn.CloseAsync();
                }
                //將Session id 建到 cache
                await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{Session_id}", walletSessionV2, _cacheSeconds);
                return res;
            }
            catch (ExceptionMessage ex)
            {
                _logger.LogInformation("H1 Transfer Fund In exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                res.code = ex.MsgId;
                res.Message = ex.Message;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("H1 Transfer Fund In exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.FundTransferW1Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        public async Task<ResCodeBase> H1TransferFundOut(Guid Session_id, string Club_id)
        {
            var res = new ResCodeBase();
            try
            {
                var session = await _commonService._cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{Session_id}",
                async () =>
                {
                    return await _commonService._serviceDB.GetWalletSessionV2ById(Session_id);
                },
                _cacheSeconds);
                if (session == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.SessionNotFound, MessageCode.Message[(int)ResponseCode.SessionNotFound]);
                }
                if (session.status != WalletSessionV2.SessionStatus.DEPOSIT)
                {
                    throw new ExceptionMessage((int)ResponseCode.SessionWithdrawn, MessageCode.Message[(int)ResponseCode.SessionWithdrawn]);
                }
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var sessionData = await _commonService._serviceDB.GetWalletSessionV2Lock(conn, tran, Session_id, WalletSessionV2.SessionStatus.DEPOSIT);
                            if (sessionData == null)
                            {
                                throw new ExceptionMessage((int)ResponseCode.SessionNotFound, MessageCode.Message[(int)ResponseCode.SessionNotFound]);
                            }
                            sessionData.status = WalletSessionV2.SessionStatus.WITHDRAW;
                            sessionData.update_time = DateTime.Now;
                            if (!await _commonService._serviceDB.PutWalletSessionV2(conn, tran, sessionData))
                            {
                                throw new Exception("Session update fail");
                            }
                            await tran.CommitAsync();
                            await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.club}/{Club_id}", sessionData, _cacheSeconds);
                            await _commonService._cacheDataService.StringSetAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{Session_id}", sessionData, _cacheSeconds);
                            //將待洗分Session加到Redis list
                            await _commonService._cacheDataService.ListPushAsync($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.withdraw_list}", sessionData);
                        }
                        catch (ExceptionMessage ex)
                        {
                            await tran.RollbackAsync();
                            _logger.LogError("H1TransferFund update session exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                            throw new ExceptionMessage(ex.MsgId, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            await tran.RollbackAsync();
                            throw new Exception(ex.Message);
                        }
                    }
                    await conn.CloseAsync();
                }
                return res;

            }
            catch (ExceptionMessage ex)
            {
                _logger.LogInformation("H1TransferFund out exception EX : {ex}  MSG : {Message}", ex.GetType().FullName, ex.Message);
                res.code = ex.MsgId;
                res.Message = ex.Message;
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("H1TransferFund out exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                res.code = (int)ResponseCode.FundTransferW1Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString();
                return res;
            }
        }
        public async Task<TransferFund> TransferFund(Guid id, string Club_id, Platform Source, Platform Target, decimal amount, bool CashOutAll)
        {
            TransferFund res = new TransferFund();
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            res = await TransferMemberWallet(conn, tran, id, Club_id, Source, Target, amount, CashOutAll);
                            if (res.code == (int)ResponseCode.Success)
                            {
                                await tran.CommitAsync();
                            }
                            else
                            {
                                throw new ExceptionMessage(res.code, res.Message);
                            }
                        }
                        catch (ExceptionMessage ex)
                        {
                            await tran.RollbackAsync();
                            throw new ExceptionMessage(ex.MsgId, ex.Message);
                        }
                        catch (Exception ex)
                        {
                            await tran.RollbackAsync();
                            throw new Exception(ex.Message);
                        }
                    }
                    await conn.CloseAsync();
                }
                return res;
            }
            catch (ExceptionMessage ex)
            {
                res.code = (int)ResponseCode.FundTransferW1Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString();
                res.Data = null;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                if (ex.MsgId == (int)ResponseCode.InsufficientBalance)
                {
                    _logger.LogInformation("TransferMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                else
                {
                    _logger.LogError("TransferMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                return res;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                res.code = (int)ResponseCode.FundTransferW1Fail;
                res.Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString();
                res.Data = new WalletTransferRecord();
                _logger.LogError("TransferMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return res;
            }
        }
        public async Task<List<WalletTransferRecord>> GetElectronicDepositRecordCache(string Club_id)
        {
            try
            {
                var records = await _commonService._cacheDataService.StringGetAsync<List<WalletTransferRecord>>($"{RedisCacheKeys.ElectronicDepositRecord}:{Club_id}");
                return records ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} exeception! clubId:{clubId} Message:{message}", "GetElectronicDepositRecordCache", Club_id, ex.Message);
                return new();
            }
        }
        public async Task<ResCodeBase> DeleteElectronicDepositRecordCache(string Club_id)
        {
            try
            {
                await _commonService._cacheDataService.KeyDelete($"{RedisCacheKeys.ElectronicDepositRecord}:{Club_id}");
                return new();
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "DeleteElectronicDepositRecord exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                var res = new ResCodeBase
                {
                    code = (int)ResponseCode.Fail,
                    Message = MessageCode.Message[(int)ResponseCode.FundTransferW1Fail] + " | " + ex.Message.ToString()
                };
                return res;
            }
        }
    }
}