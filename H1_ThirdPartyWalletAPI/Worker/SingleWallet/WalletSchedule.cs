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
using StackExchange.Redis;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class WalletSchedule : IInvocable
    {
        private readonly ILogger<WalletSchedule> _logger;
        private readonly IDBService _serviceDB;
        private readonly ICacheDataService _cacheDataService;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly string _redisKey = $"{RedisCacheKeys.WalletTransaction}/walletList";

        public WalletSchedule(ILogger<WalletSchedule> logger, IDBService serviceDB, ICacheDataService cacheDataService, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _serviceDB = serviceDB;
            _cacheDataService = cacheDataService;
            _connectionMultiplexer = connectionMultiplexer;
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
                var taskList = new List<Task<int>>();       // 任務清單
                var keyList = new List<string>();           // 紀錄已經處理過的key

                using (var conn = new NpgsqlConnection(Config.OneWalletAPI.DBConnection.Wallet.Master))
                {
                    await conn.OpenAsync();
                    await using (var tran = await conn.BeginTransactionAsync())
                    {
                        try
                        {
                            // 第幾批次
                            for (int i = 0; i < count; i++)
                            {
                                // 每一批次要處裡幾筆資料
                                for (int j = 0; j < batchSize; j++)
                                {
                                    var key = await _cacheDataService.ListPopAsync<string>(_redisKey);
                                    if (!keyList.Contains(key))
                                    {
                                        keyList.Add(key);
                                        var data = await _cacheDataService.StringGetAsync<Wallet>(key);
                                        taskList.Add(_serviceDB.PutWallet(conn, tran, data));
                                    }
                                }

                                var countResult = await Task.WhenAll(taskList);
                                if (countResult.Any(x => x != 1))
                                {
                                    throw new ExceptionMessage((int)ResponseCode.WriteTransactionRecordFail, MessageCode.Message[(int)ResponseCode.WriteTransactionRecordFail]);
                                }

                                taskList.Clear();
                            }

                            // 剩下來的餘數
                            for (int i = 0; i < remainder; i++)
                            {
                                var key = await _cacheDataService.ListPopAsync<string>(_redisKey);
                                if (!keyList.Contains(key))
                                {
                                    keyList.Add(key);
                                    var data = await _cacheDataService.StringGetAsync<Wallet>(key);
                                    taskList.Add(_serviceDB.PutWallet(conn, tran, data));
                                }
                            }

                            var result = await Task.WhenAll(taskList);
                            if (result.Any(x => x != 1))
                            {
                                throw new ExceptionMessage((int)ResponseCode.WriteTransactionRecordFail, MessageCode.Message[(int)ResponseCode.WriteTransactionRecordFail]);
                            }

                            taskList.Clear();
                            keyList.Clear();
                            await tran.CommitAsync();
                        }
                        catch (ExceptionMessage ex)
                        {
                            //DB寫入失敗，將資料push回Redis
                            foreach (string r in keyList)
                            {
                                await _cacheDataService.ListPushAsync(_redisKey, r);
                            }
                            await tran.RollbackAsync();
                            throw new Exception(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            //DB寫入失敗，將資料push回Redis
                            foreach (string r in keyList)
                            {
                                await _cacheDataService.ListPushAsync(_redisKey, r);
                            }
                            await tran.RollbackAsync();
                            throw new Exception(ex.Message);
                        }
                    }
                    await conn.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                await Task.CompletedTask;
                _logger.LogError("Wallet Schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
