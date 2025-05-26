using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Linq;
using Npgsql;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Service;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class RcgTransactionSchedule : IInvocable
    {
        private readonly ILogger<RcgTransactionSchedule> _logger;
        private readonly IDBService _serviceDB;
        private readonly ICacheDataService _cacheDataService;
        private readonly string _redisKey = $"{RedisCacheKeys.WalletTransaction}/RCG/record";

        public RcgTransactionSchedule(ILogger<RcgTransactionSchedule> logger, IDBService serviceDB, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _serviceDB = serviceDB;
            _cacheDataService = cacheDataService;
        }
        public async Task Invoke()
        {
            try
            {
                var length = await _cacheDataService.ListLengthAsync(_redisKey);
                if (length <= 0)
                {
                    await Task.CompletedTask;
                    return;
                }

                const int batchSize = 1000;                   // 一次處裡1000筆
                var count = length / batchSize;         // 批次處裡
                var remainder = length % batchSize;     // 餘數
                //var taskList = new List<Task<int>>();       // 任務清單
                using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    // 第幾批次
                    for (int i = 0; i <= count; i++)
                    {
                        List<RcgWalletTransaction> transactionData = new List<RcgWalletTransaction>();
                        if (i < count)
                        {
                            for (int j = 0; j < batchSize; j++)
                            {
                                var data = await _cacheDataService.ListPopAsync<RcgWalletTransaction>(_redisKey);
                                transactionData.Add(data);
                            }
                        }
                        else //餘數
                        {
                            for (int j = 0; j < remainder; j++)
                            {
                                var data = await _cacheDataService.ListPopAsync<RcgWalletTransaction>(_redisKey);
                                transactionData.Add(data);
                            }
                        }
                        //回寫DB Redis壓測時要先拿掉
                        await using (var tran = await conn.BeginTransactionAsync())
                        {
                            try
                            {
                                await _serviceDB.PostRcgTransaction(tran.Connection, tran, transactionData);
                                await tran.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                //DB寫入失敗，將資料push回Redis
                                foreach (RcgWalletTransaction r in transactionData)
                                {
                                    await _cacheDataService.ListPushAsync(
                                    $"{RedisCacheKeys.WalletTransaction}/RCG/record",
                                    r);
                                }
                                await tran.RollbackAsync();
                                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                                _logger.LogError("Save Redis RCG transaction to DB exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                            }

                        }
                    }
                    await conn.CloseAsync();
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await Task.CompletedTask;
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Rcg Transaction Schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            //using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
            //{
            //    conn.Open();
            //    await using (var tran = await conn.BeginTransactionAsync())
            //    {
            //        try
            //        {
            //            // 第幾批次
            //            for (int i = 0; i < count; i++)
            //            {
            //                // 每一批次要處裡幾筆資料
            //                for (int j = 0; j < batchSize; j++)
            //                {
            //                    var data = await _cacheDataService.ListPopAsync<RcgWalletTransaction>(_redisKey);
            //                    taskList.Add(_serviceDB.PostRcgTransaction(conn, tran, data));
            //                }

            //                var countResult = await Task.WhenAll(taskList);
            //                if (countResult.Any(x => x != 1))
            //                {
            //                    throw new ExceptionMessage((int)ResponseCode.WriteTransactionRecordFail, MessageCode.Message[(int)ResponseCode.WriteTransactionRecordFail]);
            //                }

            //                taskList.Clear();
            //            }

            //            // 剩下來的餘數
            //            for (int i = 0; i < remainder; i++)
            //            {
            //                var data = await _cacheDataService.ListPopAsync<RcgWalletTransaction>(_redisKey);
            //                taskList.Add(_serviceDB.PostRcgTransaction(conn, tran, data));
            //            }

            //            var result = await Task.WhenAll(taskList);
            //            if (result.Any(x => x != 1))
            //            {
            //                throw new ExceptionMessage((int)ResponseCode.WriteTransactionRecordFail, MessageCode.Message[(int)ResponseCode.WriteTransactionRecordFail]);
            //            }

            //            taskList.Clear();
            //            await tran.CommitAsync();
            //        }
            //        catch (Exception ex)
            //        {
            //            await tran.RollbackAsync();
            //            throw;
            //        }
            //    }
            //}
        }
    }
}
