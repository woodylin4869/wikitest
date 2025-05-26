using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.W1API;
using System.Collections.Generic;
using System.Linq;
using H1_ThirdPartyWalletAPI.Model;
using System.Threading;

namespace H1_ThirdPartyWalletAPI.Worker
    {
    /// <summary>
    /// 取得待退款清單
    /// </summary>
    public class SessionRefundQueueSchedule : ICancellableInvocable, IInvocable
    {
        private readonly ILogger<SessionRefundQueueSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IWalletSessionService _walletSessionService;
        private static long task_id = 0;
        private static int MaxRefundCount = 10;//最大一次處理提款數量
        private readonly string _redisKey = $"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.refund_list}";

        public CancellationToken CancellationToken { get ; set ; }

        public SessionRefundQueueSchedule(ILogger<SessionRefundQueueSchedule> logger, ICommonService commonService, IWalletSessionService walletSessionService)
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

            await _walletSessionService.H1HealthCheck();

            while (!CancellationToken.IsCancellationRequested)
            {
                using var execIdLoggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });

                try
                {
                    long ListLength = await _commonService._cacheDataService.ListLengthAsync(_redisKey);
                    var RefundTaskList = new List<Task<WalletSessionV2>>();
                    //限制一次處理數量，避免Connection過多
                    ListLength = ListLength < MaxRefundCount ? ListLength : MaxRefundCount;
                    task_id++;
                    if (ListLength > 0)
                    {
                        _logger.LogDebug("Refund List Length : {length}", ListLength);
                        for (long i = 0; i < ListLength; i++)
                        {
                            var Session = await _commonService._cacheDataService.ListPopAsync<WalletSessionV2>(_redisKey);
                            if (Session != null)
                            {
                                if (DateTime.Now.AddSeconds(-Session.push_times * 5) > Session.update_time)
                                {
                                    RefundTaskList.Add(_walletSessionService.RefundSession(task_id, Session));
                                }
                                else
                                {
                                    //沒有要推送的丟回Queue中
                                    await _commonService._cacheDataService.ListPushAsync(_redisKey, Session);
                                }
                            }
                        }
                        var refundResult = await Task.WhenAll(RefundTaskList);

                        foreach (var walletSession in refundResult)
                        {
                            //退款失敗要將Session加回Queue
                            if (walletSession == null || walletSession.status != WalletSessionV2.SessionStatus.UNSETTLE)
                            {
                                await _commonService._cacheDataService.ListPushAsync(_redisKey, walletSession);
                            }
                        }
                    }
                    else
                    {
                        if (task_id % 100 == 1)
                        {
                            _logger.LogDebug("Refund task id  : {id}", task_id);
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
