using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service
{
    public interface IRepairBetRecordService
    {
        public Task<ResCodeBase> RepairGameRecord(RepairBetSummaryReq RepairReq, bool writeLog = false);

        public Task<RepairLogByPageResp> GetRepairLog(RepairLogByPageReq repairLogByPageReq);
    }
    public class RepairBetRecordService : IRepairBetRecordService
    {
        private const string RepairLogTaskID = "RepairLog";
        private readonly ILogger<RepairBetRecordService> _logger;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly ICacheDataService _cacheDataService;
        public RepairBetRecordService(ILogger<RepairBetRecordService> logger
            , IGameInterfaceService gameInterfaceService
            , ICacheDataService cacheDataService)
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
            _cacheDataService = cacheDataService;
        }

        public async Task<ResCodeBase> RepairGameRecord(RepairBetSummaryReq RepairReq, bool writeLog = false)
        {
            #region RepairLog
            string taskId = RepairLogTaskID;
            string executionId1 = Guid.NewGuid().ToString();

            TimeSpan expiry = TimeSpan.FromDays(7);
            var GameId = RepairReq.game_id;
            var startTime1 = DateTime.Now;
            var title1 = $"{GameId}補單_開始時間:{RepairReq.StartTime.ToString("yyyy/MM/dd HH:mm:ss")} 結束時間:{RepairReq.EndTime.ToString("yyyy/MM/dd HH:mm:ss")}";
            RepairLogModel RepairLogModel = new RepairLogModel();
            RepairLogModel.GameId = GameId;
            RepairLogModel.Title = title1;
            RepairLogModel.StartTime = startTime1;

            #endregion

            ResCodeBase res = new ResCodeBase();

            var expire = TimeSpan.FromMinutes(1);
            var wait = TimeSpan.MaxValue;
            var retry = TimeSpan.FromSeconds(5);
            await _cacheDataService.LockAsyncRegular($"{RedisCacheKeys.RepairBetRecord}:{RepairReq.game_id}",
                async () =>
                {
                    try
                    {
                        if (writeLog)
                        {
                            //寫入PENDING
                            RepairLogModel.Status = "PENDING";
                            await _cacheDataService.StringSetAsync($"{taskId}:execution:{executionId1}", RepairLogModel, (int)expiry.TotalSeconds);
                            await _cacheDataService.ListLeftPushAsync($"{taskId}", $"{taskId}:execution:{executionId1}");
                        }
                        res.Message = await _gameInterfaceService.RepairGameRecord(RepairReq);

                        if (writeLog)
                        {
                            //執行成功
                            RepairLogModel.EndTime = DateTime.Now;
                            RepairLogModel.Status = "SUCCESS";
                            RepairLogModel.Message = res.Message;
                            await _cacheDataService.StringSetAsync($"{taskId}:execution:{executionId1}", RepairLogModel, (int)expiry.TotalSeconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (writeLog)
                        {
                            string errorMessage = ex.Message.Length > 300 ? ex.Message.Substring(0, 300) + "..." : ex.Message;
                            //執行失敗
                            RepairLogModel.EndTime = DateTime.Now;
                            RepairLogModel.Status = "FAIL";
                            RepairLogModel.Message = errorMessage;
                            await _cacheDataService.StringSetAsync($"{taskId}:execution:{executionId1}", RepairLogModel, (int)expiry.TotalSeconds);
                        }
                        throw;
                    }
                    finally
                    {
                        using (LogContext.PushProperty("X_Action", "RepairGameRecord"))
                        using (LogContext.PushProperty("GameId", GameId))
                        {
                            _logger.LogInformation("RepairGameRecord 補單開始時間: {starttime} 結束時間: {endtime}  Message :  {Message} ",
                                                    RepairReq.StartTime.ToString("yyyy-MM-dd HH:mm"),
                                                    RepairReq.EndTime.ToString("yyyy-MM-dd HH:mm"),
                                                    res.Message);
                        }
                    }
                }, expire, wait, retry);
            return res;
        }

        /// <summary>
        /// 取得RedisLog
        /// </summary>
        /// <param name="repairLogByPageReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<RepairLogByPageResp> GetRepairLog(RepairLogByPageReq repairLogByPageReq)
        {
            RepairLogByPageResp resp = new RepairLogByPageResp();

            string taskId = RepairLogTaskID;

            int pageNumber = repairLogByPageReq.Page ?? 1;
            int pageSize = repairLogByPageReq.Count ?? 100;

            resp.Data = await GetLogs($"{taskId}", pageNumber, pageSize);
            resp.TotalCount = await _cacheDataService.ListLengthAsync($"{taskId}");
            return resp;
        }
        public async Task<List<RepairLogModel>> GetLogs(string LogListKey, int pageNumber, int pageSize)
        {
            await _cacheDataService.CleanExpiredListKeysAsync(LogListKey);
            int start = (pageNumber - 1) * pageSize;
            int stop = start + pageSize - 1;
            var keys = await _cacheDataService.ListGetByRangeAsync<string>(LogListKey, start, stop);
            var logs = await _cacheDataService.BatchStringGetAsync<RepairLogModel>(keys);
            return logs;
        }
    }
}
