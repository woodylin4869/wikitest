using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.W1API;
using System.Collections.Generic;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每分鐘搬移已經洗分Session到History
    /// </summary>
    public class SessionMoveSchedule : IInvocable
    {
        private readonly ILogger<SessionMoveSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IWalletSessionService _walletSessionService;

        public SessionMoveSchedule(ILogger<SessionMoveSchedule> logger, ICommonService commonService, IWalletSessionService walletSessionService)
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
            _logger.LogDebug("Invoke SessionMoveSchedule on time : {time}", DateTime.Now);
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await _walletSessionService.MoveRefundedWalletSessionToHistory();
                stopwatch.Stop();
                _logger.LogDebug("MoveRefundedWalletSessionToHistory elapsed time : {time}", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run session move schedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }
    }
}
