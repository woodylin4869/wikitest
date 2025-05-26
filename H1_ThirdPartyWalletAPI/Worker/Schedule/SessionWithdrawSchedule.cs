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

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每秒鐘檢查洗分清單
    /// </summary>
    public class SessionWithdrawSchedule : IInvocable
    {
        private readonly ILogger<SessionWithdrawSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IWalletSessionService _walletSessionService;
        private static long task_id = 0;
        private static int MaxWithdrawCount = 50;//最大一次處理提款數量

        public SessionWithdrawSchedule(ILogger<SessionWithdrawSchedule> logger, ICommonService commonService, IWalletSessionService walletSessionService)
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
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });
            try
            {
                task_id++;
                if (task_id % 60 == 1)
                {
                    await _walletSessionService.H1HealthCheck();
                }
                if (task_id % 2 == 1)
                {
                    List<short> status = new List<short>{
                            (short)WalletSessionV2.SessionStatus.WITHDRAW
                         };
                    var WithdrawSession = await _commonService._serviceDB.GetWalletSessionV2(status);
                    _logger.LogDebug("TaskId: {tid} Waiting withdraw total : {count}", task_id, WithdrawSession.Count());
                    int withdarwCounter = 1;
                    var WithdrawTaskList = new List<Task>();
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    foreach (WalletSessionV2 session in WithdrawSession)
                    {
                        //只處理過久都沒有提款的Session 
                        if (session.update_time < DateTime.Now.AddMinutes(-5))
                        {
                            if (withdarwCounter > MaxWithdrawCount)
                            {
                                //限制一次處理數量，避免Connection過多
                                break;
                            }
                            WithdrawTaskList.Add(_walletSessionService.WithdrawSession(task_id, session));
                            withdarwCounter++;
                        }
                    }
                    await Task.WhenAll(WithdrawTaskList);
                    sw.Stop();
                    _logger.LogDebug("TaskId: {tid} withdraw expend : {ElapsedMilliseconds}", task_id, sw.ElapsedMilliseconds);
                }
                else
                {
                    List<short> status = new List<short>{
                        (short)WalletSessionV2.SessionStatus.REFUND
                        };
                    var RefundSession = await _commonService._serviceDB.GetWalletSessionV2(status);
                    _logger.LogDebug("TaskId: {tid} Waiting REFUND total : {count}", task_id, RefundSession.Count());

                    var RefundTaskList = new List<Task<WalletSessionV2>>();
                    int refundCounter = 1;
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    foreach (WalletSessionV2 session in RefundSession)
                    {
                        if (session.update_time < DateTime.Now.AddMinutes(-5))
                        {
                            if (refundCounter > MaxWithdrawCount)
                            {
                                //限制一次處理數量，避免Connection過多
                                break;
                            }
                            //推送次數使用Redis值參考
                            var sesssionCache = await _commonService._cacheDataService.StringGetAsync<WalletSessionV2>($"{RedisCacheKeys.WalletSession}/{L2RedisCacheKeys.session_id}/{session.session_id}");
                            if (sesssionCache == null)
                            {
                                _logger.LogDebug("TaskId: {tid} Run Refund Id : {id}", task_id, session.session_id);
                                sesssionCache = session;
                            }
                            if (DateTime.Now.AddSeconds(-sesssionCache.push_times * 5) > sesssionCache.update_time)
                            {
                                session.push_times = sesssionCache.push_times;
                                RefundTaskList.Add(_walletSessionService.RefundSession(task_id, session));
                                refundCounter++;
                            }
                        }
                    }
                    var refundResult = await Task.WhenAll(RefundTaskList);
                    sw.Stop();
                    _logger.LogDebug("TaskId: {tid} refund expend : {ElapsedMilliseconds}", task_id, sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run SessionWithdrawSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            finally
            {
                _logger.LogDebug("SessionWithdrawSchedule TaskId: {tid} Done...", task_id);
                await Task.CompletedTask;
            }

        }
    }
}
