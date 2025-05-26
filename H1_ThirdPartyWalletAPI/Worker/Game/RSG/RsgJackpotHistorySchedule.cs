using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.W1API;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker.Game.RSG
{
    /// <summary>
    /// 定期取得皇電中獎清單
    /// 透過headlsess service將彩金清單推到wallet-api的每個pod
    /// </summary>
    public class RsgJackpotHistorySchedule : IInvocable
    {
        private readonly ILogger<RsgJackpotHistorySchedule> _logger;
        private readonly IJackpotHistoryService _jackpotHistoryService;
        private List<JackpotHistory> _histories = new();
        private DateTime _nextTime = DateTime.MinValue;
        private readonly HttpClient _httpClient = new();

        public RsgJackpotHistorySchedule(ILogger<RsgJackpotHistorySchedule> logger, IJackpotHistoryService jackpotHistoryService)
        {
            _logger = logger;
            _jackpotHistoryService = jackpotHistoryService;
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
                //每隔兩分鐘才去RSG取得中獎清單
                if (DateTime.Now.ToLocalTime() > _nextTime)
                {
                    // 取得中獎會員清單
                    var now = DateTime.Now;
                    var start = now.AddDays(-60);
                    var end = now.Hour > 12 ? now : now.AddDays(-1);
                    var key = $"{RedisCacheKeys.JackpotHistory}";
                    _histories = await _jackpotHistoryService.GetJackpotHistoryFromRsgApiAsnyc(start, end);
                    _nextTime = DateTime.Now.ToLocalTime().AddMinutes(2);
                }
                
                if (!_histories.Any()) return;

                //透過headless service取得wallet-api各個pod的IP
                var walletApiPodIps = await Dns.GetHostAddressesAsync(Config.RsgJackpotHistoryConfig.HlDomain);

                //將中獎清單塞到wallet-api各個pod
                var postContent = new StringContent(JsonConvert.SerializeObject(_histories), Encoding.UTF8, "application/json");
                var tasks = new List<Task>();
                foreach (var pod in walletApiPodIps)
                {
                    var url = $"http://{pod}/w1api/JackpotHistory/SetJackpotHistory";
                    tasks.Add(_httpClient.PostAsync(url, postContent));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{action} {level} {message}", nameof(RsgJackpotHistorySchedule), LogLevel.Error, ex.Message);
            }
        }
    }
}

