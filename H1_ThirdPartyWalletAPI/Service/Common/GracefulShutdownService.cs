using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lab.GracefulShutdown.Net6;
internal class GracefulShutdownService : IHostedService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private Task _backgroundTask;
    private bool _stop;
    private readonly ILogger<GracefulShutdownService> _logger;
    private readonly RequestProcessingFlag _processingFlag;

    public GracefulShutdownService(IHostApplicationLifetime appLifetime, ILogger<GracefulShutdownService> logger, RequestProcessingFlag processingFlag)
    {
        this._appLifetime = appLifetime;
        _logger = logger;
        _processingFlag = processingFlag;
    }

    public Task StartAsync(CancellationToken cancel)
    {
        _logger.LogInformation($"{DateTime.Now} Service starting...");

        this._backgroundTask = Task.Run(async () => { await this.ExecuteAsync(cancel); }, cancel);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancel)
    {
        _logger.LogInformation($"{DateTime.Now} Service stopping...");
        this._stop = true;
        await this._backgroundTask;

        while (_processingFlag.Count > 0 && //仍有Request在執行
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() - _processingFlag.UnixSecond < 270) //且最新Request執行不到270秒
        {
            _logger.LogWarning("Request Processing!... Count:{count} UnixSecond:{UnixSecond}", _processingFlag.Count, DateTimeOffset.FromUnixTimeSeconds(_processingFlag.UnixSecond).ToLocalTime());
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
        }

        Console.WriteLine($"{DateTime.Now} Serice done");
    }

    private async Task ExecuteAsync(CancellationToken cancel)
    {
        _logger.LogInformation($"{DateTime.Now} Service run!");

        if (Config.OneWalletAPI.Redis_PreKey != "Local")
        {
            while (!this._stop)
            {
                _logger.LogDebug($"{DateTime.Now} Service running... Processing Request:{_processingFlag.Count}");
                await Task.Delay(TimeSpan.FromSeconds(1), cancel);
            }
        }
        _logger.LogInformation($"{DateTime.Now} Service Graceful Shutdown");
    }
}