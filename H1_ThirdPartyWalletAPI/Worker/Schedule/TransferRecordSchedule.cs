using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using H1_ThirdPartyWalletAPI.Service;
using System;
using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Worker
{
    public class TransferRecordSchedule : IInvocable
    {
        private readonly ILogger<TransferRecordSchedule> _logger;
        private readonly ITransferService _transferService;
        public TransferRecordSchedule(ILogger<TransferRecordSchedule> logger, ITransferService transferService)
        {
            _logger = logger;
            _transferService = transferService;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
                {
                    { "Schedule", this.GetType().Name },
                    { "ScheduleExecId", Guid.NewGuid().ToString() }
                });

            _logger.LogInformation("Invoke TransferRecordSchedule on time : {time}", DateTime.Now);
            await _transferService.CheckTransferRecord(DateTime.Now.AddHours(-4), DateTime.Now.AddMinutes(-4), Model.DataModel.WalletTransferRecord.TransferStatus.pending);
            await _transferService.CheckTransferRecord(DateTime.Now.AddHours(-4), DateTime.Now.AddMinutes(-4), Model.DataModel.WalletTransferRecord.TransferStatus.init);
            _logger.LogDebug("Transfer Record Schedule Done");
            await Task.CompletedTask;
        }
    }
}
