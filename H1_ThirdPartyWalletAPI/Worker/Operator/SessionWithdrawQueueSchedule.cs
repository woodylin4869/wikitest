using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.W1API;
using System.Collections.Generic;
using H1_ThirdPartyWalletAPI.Model;
using System.Threading;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 取得待洗分清單
    /// </summary>
    public class SessionWithdrawQueueSchedule : ICancellableInvocable, IInvocable
    {
        private readonly ILogger<SessionWithdrawQueueSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IWalletSessionService _walletSessionService;
        private static long task_id = 0;
        private static int MaxWithdrawCount = 10;//最大一次處理提款數量
        private readonly string _redisKey = $"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.withdraw_list}";
        public CancellationToken CancellationToken { get; set; }
        public SessionWithdrawQueueSchedule(ILogger<SessionWithdrawQueueSchedule> logger, ICommonService commonService, IWalletSessionService walletSessionService)
        {
            _logger = logger;
            _commonService = commonService;
            _walletSessionService = walletSessionService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                });

            while (!CancellationToken.IsCancellationRequested)
            {
                using var execIdLoggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });

                try
                {
                    long ListLength = await _commonService._cacheDataService.ListLengthAsync(_redisKey);
                    var WithdrawTaskList = new List<Task>();
                    //限制一次處理數量，避免Connection過多
                    ListLength = ListLength < MaxWithdrawCount ? ListLength : MaxWithdrawCount;
                    task_id++;
                    if (ListLength > 0)
                    {
                        _logger.LogDebug("Withdraw List Length : {length}", ListLength);
                        for (long i = 0; i < ListLength; i++)
                        {

                            var Session = await _commonService._cacheDataService.ListPopAsync<WalletSessionV2>(_redisKey);
                            if (Session != null)
                            {
                                WithdrawTaskList.Add(_walletSessionService.WithdrawSession(task_id, Session));
                            }
                        }
                        
                        await Task.WhenAll(WithdrawTaskList);
                    }
                    else
                    {
                        if (task_id % 100 == 1)
                        {
                            _logger.LogDebug("Withdraw task id  : {id}", task_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                    _logger.LogError("Run SessionWithdrawQueueSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
                }
                await Task.Delay(200, CancellationToken);
            }
        }
    }
}
