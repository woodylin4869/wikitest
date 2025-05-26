using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Model.H1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Common.DB.Betlogs;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Worker
{
    /// <summary>
    /// 每分鐘帳務檢查
    /// </summary>
    public class KickIdleUserSchedule : IInvocable
    {
        private readonly ILogger<KickIdleUserSchedule> _logger;
        private readonly ISummaryDBService _summaryDbService;
        private readonly IGameApiService _gameApiService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDBService _serviceDB;

        private DateTime _nexTime = DateTime.MinValue;

        public KickIdleUserSchedule(ILogger<KickIdleUserSchedule> logger,
            ISummaryDBService summaryDbService,
            IGameApiService gameApiService,
            IMemoryCache memoryCache,
            IDBService serviceDB)
        {
            _logger = logger;
            _summaryDbService = summaryDbService;
            _gameApiService = gameApiService;
            _memoryCache = memoryCache;
            _serviceDB = serviceDB;
        }
        public async Task Invoke()
        {
            using var loggerScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                { "Schedule", this.GetType().Name },
                { "ScheduleExecId", Guid.NewGuid().ToString() }
            });

            try
            { // 如果是周一并且时间在 11 点到 13 点之间，不执行
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour >= 11 && DateTime.Now.Hour < 13)
                {
                    //_logger.LogInformation("跳过排程执行，因为当前时间是每周一的 11 点到 13 点之间");
                    return;
                }

                if (DateTime.Now < _nexTime)
                    return;

                // 閒置會員 Id
                var idleIds = await GetIdleClubIds(600);

                if (!idleIds.Any())
                {
                    //無閒置人員則等一分鐘後再執行踢線排程
                    _nexTime = DateTime.Now.AddMinutes(1);
                    return;
                }

                _logger.LogInformation("閒置人數: {count}", idleIds.Count());

                foreach (var clubId in idleIds)
                {
                    //不在線上做踢線動作
                    await _gameApiService._h1API.Kick(new KickUserReq
                    {
                        club_id = clubId
                    });

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError(ex, "Run KickIdleUserSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
        }

        /// <summary>
        /// 取得閒置會員 clubId
        /// </summary>
        public async Task<IEnumerable<string>> GetIdleClubIds(int limitLength)
        {
            var now = DateTime.Now;

            // 閒置會員
            var idleIds = await _summaryDbService.GetInactiveClubIdByRecordSummary(now, limitLength * 50);

            if (now.Hour >= 6 && now.Hour < 8)
            {
                // LastPlatform 閒置會員
                var LastPlatformidleIds = await _serviceDB.GetWalletLastPlatformByCreateTime(now.AddDays(-1), 200);
                idleIds.UnionWith(LastPlatformidleIds);

                _logger.LogInformation("最後館別人數: {LastPlatformCount}", LastPlatformidleIds.Count());
            }

            var kickIds = new List<string>();

            foreach (var clubId in idleIds)
            {
                var cacheKey = $"kick/{clubId}";

                // 過濾掉 30 分鐘內踢過的會員
                if (_memoryCache.TryGetValue(cacheKey, out _)) continue;

                // 紀錄踢過的會員
                _memoryCache.Set(cacheKey, clubId,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(30)));

                kickIds.Add(clubId);
                if (kickIds.Count >= limitLength)
                    break;
            }

            return kickIds;
        }
    }
}
