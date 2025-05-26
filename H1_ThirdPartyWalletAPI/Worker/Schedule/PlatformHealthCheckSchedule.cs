using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Worker.Schedule
{
    public class PlatformHealthCheckSchedule : IInvocable
    {
        private readonly ILogger<PlatformHealthCheckSchedule> _logger;
        private readonly IGameInterfaceService _gameInterfaceService;
        private readonly IApiHealthCheckService _apiHealthCheckService;
        private readonly ICacheDataService _cacheDataService;
        private int _cacheSeconds = 60 * 12;

        public PlatformHealthCheckSchedule(ILogger<PlatformHealthCheckSchedule> logger,
            IGameInterfaceService gameInterfaceService,
            IApiHealthCheckService apiHealthCheckService,
            ICacheDataService cacheDataService)
        {
            _logger = logger;
            _gameInterfaceService = gameInterfaceService;
            _apiHealthCheckService = apiHealthCheckService;
            _cacheDataService = cacheDataService;
        }


        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });

            _logger.LogDebug("Invoke PlatformHealthCheckSchedule on time : {time}", DateTime.Now);

            var openGames = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));

            // 假設有一個 List<Platform> 用來存儲需要調整呼叫頻率的 Platform
            List<Platform> specificPlatforms = new List<Platform>
            { Platform.VA};
            await Parallel.ForEachAsync(openGames, async (openGame, cancel) =>
            {
                var platform = (Platform)Enum.Parse(typeof(Platform), openGame.ToUpper());
                for (var i = 0; i < 3; i++)
                {
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        try
                        {
                            await _gameInterfaceService.HealthCheck(platform);
                        }
                        catch (TaskCanceledException) { throw; }
                        catch { }

                        sw.Stop();
                        _apiHealthCheckService.SetResponseData(platform, new()
                        {
                            ElapsedMilliseconds = sw.ElapsedMilliseconds,
                        });
                        _logger.LogDebug("{action} {platform} {time}", nameof(PlatformHealthCheckSchedule), platform, sw.ElapsedMilliseconds);
                    }
                    catch (TaskCanceledException ex)
                    {
                        var GetError = JsonConvert.DeserializeObject<ErrorData>(ex.InnerException?.Message);

                        await _cacheDataService.StringSetAsync(
                            $"{RedisCacheKeys.ApiHealthCheck}/{L2RedisCacheKeys.api_response}/{platform}/LOG",
                            new
                            {
                                url = GetError.Url,
                                requestData = GetError.RequestData
                            }, _cacheSeconds);


                        _apiHealthCheckService.SetResponseData(platform, new()
                        {
                            ElapsedMilliseconds = 99999,
                        });
                    }
                    finally
                    {
                        // 計算 HealthCheck API 的執行時間
                        var elapsedMilliseconds = sw.ElapsedMilliseconds;

                        // 計算剩餘時間，最多延遲 3.3 秒
                        var remainingDelay = Math.Max(0, 3300 - elapsedMilliseconds);

                        // 判斷是否是指定的 platform
                        if (specificPlatforms.Contains(platform)) // 檢查 platform 是否在列表中
                        {
                            // 針對特定 platform 
                            await Task.Delay((int)remainingDelay, cancel);
                        }
                        else
                        {
                            remainingDelay = Math.Max(0, 1000 - elapsedMilliseconds);
                            // 其他 platform 維持 10 毫秒延遲
                            await Task.Delay((int)remainingDelay, cancel);
                        }
                    }
                }
            });
        }
    }
    public class ErrorData
    {
        public string Url { get; set; }
        public string RequestData { get; set; }
    }
}
