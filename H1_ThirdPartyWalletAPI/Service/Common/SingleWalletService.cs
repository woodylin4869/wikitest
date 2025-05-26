using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Caching.Memory;


namespace H1_ThirdPartyWalletAPI.Service
{
    public interface ISingleWalletService
    {
        public Task<GetMemberBalance> GetMemberWalletBalance(string Club_id);   
        public Task<decimal> WalletTransaction(WalletTransactionBaseMessage<dynamic> TransactionData);
        public Task<Wallet> GetWallet(string Club_id);
        public Task<bool> SetWallet(Wallet walletData);

    }
    public interface ITransaction<T>
    {
        Task<decimal> Transaction(T transData, Wallet walletData);

    }
    public class RCGTransaction : ITransaction<RcgWalletTransaction>
    {
        private readonly ICacheDataService cacheDataService;

        public RCGTransaction(ICacheDataService cacheDataService)
        {
            this.cacheDataService = cacheDataService;
        }

        public async Task<decimal> Transaction(RcgWalletTransaction transData, Wallet walletData)
        {
            //RcgWalletTransaction transData = TransactionData.Data;
            transData.before_balance = walletData.Credit;
            transData.franchiser_id = walletData.Franchiser_id;
            if (transData.tran_type.ToLower() == "debit")
            {
                transData.after_balance = transData.before_balance - transData.amount;
                if (transData.after_balance < 0)
                {
                    throw new ExceptionMessage((int)ResponseCode.InsufficientBalance, MessageCode.Message[(int)ResponseCode.InsufficientBalance]);
                }
            }
            else if (transData.tran_type.ToLower() == "credit" || transData.tran_type.ToLower() == "cancel")
            {
                transData.after_balance = transData.before_balance + transData.amount;
            }
            else
            {
                throw new ExceptionMessage((int)ResponseCode.TransactionTypeFail, MessageCode.Message[(int)ResponseCode.TransactionTypeFail]);
            }
            // 錢包紀錄存進redis
            await cacheDataService.ListPushAsync(
            $"{RedisCacheKeys.WalletTransaction}/RCG/record",
            transData);
            var AfterBalance = transData.after_balance;
            return AfterBalance;
        }


    }
    public class JDBTransaction : ITransaction<JDBWalletTransaction>
    {

        private readonly ICacheDataService cacheDataService;

        public JDBTransaction(ICacheDataService cacheDataService)
        {
            this.cacheDataService = cacheDataService;
        }
        public async Task<decimal> Transaction(JDBWalletTransaction transData, Wallet walletData)
        {
            transData.before_balance = walletData.Credit;
            transData.franchiser_id = walletData.Franchiser_id;
            if (transData.tran_type.ToLower() == "debit")
            {
                transData.after_balance = transData.before_balance - transData.amount;
                if (transData.after_balance < 0)
                {
                    throw new ExceptionMessage((int)ResponseCode.InsufficientBalance, MessageCode.Message[(int)ResponseCode.InsufficientBalance]);
                }
            }
            else if (transData.tran_type.ToLower() == "credit" || transData.tran_type.ToLower() == "cancel")
            {
                transData.after_balance = transData.before_balance + transData.amount;
            }
            else
            {
                throw new ExceptionMessage((int)ResponseCode.TransactionTypeFail, MessageCode.Message[(int)ResponseCode.TransactionTypeFail]);
            }
            // 錢包紀錄存進redis
            await cacheDataService.ListPushAsync(
            $"{RedisCacheKeys.WalletTransaction}/JDB/record",
            transData);
            var AfterBalance = transData.after_balance;
            return AfterBalance;
        }

        public Task<decimal> Transaction<T>(T transData, Wallet walletData)
        {
            var AfterBalance = Transaction(transData, walletData);
            return AfterBalance;
        }
    }
    public class SingleWalletService : ISingleWalletService
    {
        private readonly ILogger<SingleWalletService> _logger;
        private readonly IDBService _serviceDB;
        private readonly ICacheDataService _cacheDataService;
        //private readonly IMemoryCache _memoryCache;
        //private readonly string PGMaster;
        private int _cacheSeconds = 600;

        public SingleWalletService(ILogger<SingleWalletService> logger, IDBService serviceDB
            , ICacheDataService cacheDataService)
        {
            _logger = logger;
            _cacheDataService = cacheDataService;
            //_memoryCache = memoryCache;
            _serviceDB = serviceDB;
            //PGMaster = _configuration.GetValue<string>("OneWallet-API:DBConnection:PGMaster");
            //GetMemberWallet();
        }
        //public async Task<string> GetMemberWallet()
        //{
        //    try
        //    {
        //        IEnumerable<dynamic> results = await _serviceDB.GetAllMemberWallet();
        //        //var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddSeconds(15));
        //        //memoryCache.Set("MemberWallet", results, cacheEntryOptions);
        //        _memoryCache.Set("MemberWallet", results);
        //        return "success";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Get member wallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
        //        return "fail";
        //    }
        //}
        public async Task<GetMemberBalance> GetMemberWalletBalance(string Club_id)
        {
            GetMemberBalance resData = new GetMemberBalance();
            try
            {
                resData.Data = new List<MemberBalance>();
                //取得中心錢包餘額
                Wallet results = await GetWallet(Club_id);
                if(results == null)
                {
                    throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                }
                MemberBalance W1Balance = new MemberBalance();
                W1Balance.Wallet = nameof(Platform.W1);
                W1Balance.Amount = results.Credit;
                resData.Data.Add(W1Balance);
                return resData;
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("TransferMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return resData;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("TransferMemberWallet exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return resData;
            }
        }      
        public async Task<decimal> WalletTransaction(WalletTransactionBaseMessage<dynamic> TransactionData)
        {
            try
            {
                decimal AfterBalance = 0;
                await _cacheDataService.LockAsync(
                    $"{RedisCacheKeys.WalletTransaction}/wallet/{TransactionData.Club_id}",
                    async () =>
                    {
                        // 取餘額
                        Wallet walletData = await GetWallet(TransactionData.Club_id);
                        if (walletData == null)
                        {
                            throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                        }

                        switch (TransactionData.source)
                        {
                            case Platform.RCG:
                                RcgWalletTransaction rcgWalletTransaction = TransactionData.Data;
                                ITransaction<RcgWalletTransaction> RCGtransaction = new RCGTransaction(_cacheDataService);
                                AfterBalance = await RCGtransaction.Transaction(rcgWalletTransaction, walletData);
                                break;
                            case Platform.JDB:
                                JDBWalletTransaction jDBWalletTransaction = TransactionData.Data;
                                ITransaction<JDBWalletTransaction> JDBtransaction = new JDBTransaction(_cacheDataService);
                                AfterBalance = await JDBtransaction.Transaction(jDBWalletTransaction, walletData);
                                break;
                            default:
                                throw new ExceptionMessage((int)ResponseCode.UnknowPlatform, MessageCode.Message[(int)ResponseCode.UnknowPlatform]);
                        }
                        // 更新餘額存進redis
                        walletData.Credit = AfterBalance;
                        await _cacheDataService.StringSetAsync(
                            $"{RedisCacheKeys.WalletTransaction}/wallet/{TransactionData.Club_id}",
                            walletData,
                            _cacheSeconds);
                        // 異動過餘額的key存進redis
                        await _cacheDataService.ListPushAsync(
                            $"{RedisCacheKeys.WalletTransaction}/walletList",
                            $"{RedisCacheKeys.WalletTransaction}/wallet/{TransactionData.Club_id}");
                    });
                return AfterBalance;
            }
            catch (ExceptionMessage ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("UpdateRcgAuthToken exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                if (ex.MsgId == (int)ResponseCode.InsufficientBalance)
                    return -2;
                if (ex.MsgId == (int)ResponseCode.WriteTransactionRecordFail)
                    return -1;
                return -4;
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("UpdateRcgAuthToken exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                return -4;
            }
        }
        public async Task<Wallet> GetWallet(string Club_id)
        {
            Wallet walletData = await _cacheDataService.GetOrSetValueAsync($"{RedisCacheKeys.WalletTransaction}/wallet/{Club_id}",
            async () =>
            {
                Wallet resData = new Wallet();
                try
                {
                    IEnumerable<Wallet> result = await _serviceDB.GetWallet(Club_id);
                    if (result.Count() != 1)
                    {
                        throw new ExceptionMessage((int)ResponseCode.UserNotFound, MessageCode.Message[(int)ResponseCode.UserNotFound]);
                    }
                    return result.SingleOrDefault();
                }
                catch
                {
                    return resData;
                }
            },
            _cacheSeconds);
            return walletData;
        }
        public async Task<bool> SetWallet(Wallet walletData)
        {
            // 更新餘額存進redis
            await _cacheDataService.StringSetAsync(
                $"{RedisCacheKeys.WalletTransaction}/wallet/{walletData.Club_id}",
                walletData,
                _cacheSeconds);
            // 異動過餘額的key存進redis
            await _cacheDataService.ListPushAsync(
                $"{RedisCacheKeys.WalletTransaction}/walletList",
                $"{RedisCacheKeys.WalletTransaction}/wallet/{walletData.Club_id}");

            return true;
        }
    }

    //data modle 暫時先放這
    public class WalletTransactionBaseMessage<T>
    {
        public string Club_id { get; set; }
        public Platform source { get; set; }
        public T Data { get; set; }
    }
}