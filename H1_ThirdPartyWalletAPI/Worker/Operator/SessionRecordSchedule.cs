using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.W1API;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每分鐘找到有更新的帳務資料
    /// </summary>
    public class SessionRecordSchedule : IInvocable
    {
        private readonly ILogger<SessionRecordSchedule> _logger;
        private readonly IWalletSessionService _walletSessionService;

        public SessionRecordSchedule(ILogger<SessionRecordSchedule> logger, IWalletSessionService walletSessionService)
        {
            _logger = logger;
            _walletSessionService = walletSessionService;
        }
        public async Task Invoke()
        {
            try
            {
                await _walletSessionService.GetFreshWalletSession();
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run SessionRecordSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            finally
            {
                await Task.CompletedTask;
            }

        }
    }
}
