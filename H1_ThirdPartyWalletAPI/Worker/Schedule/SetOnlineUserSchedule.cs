using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coravel.Invocable;
using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Helpers;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using H1_ThirdPartyWalletAPI.Model.Game;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Enum;
using H1_ThirdPartyWalletAPI.Model.Game.RSG.Request;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Service.Common;
using H1_ThirdPartyWalletAPI.Service.Game;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Worker.Schedule
{
    /// <summary>
    /// 每分鐘更新線上使用者
    /// </summary>
    public class SetOnlineUserSchedule : IInvocable
    {
        private readonly ILogger<SetOnlineUserSchedule> _logger;
        private readonly ICommonService _commonService;
        private readonly IMemoryCache _memoryCache;
        private readonly IGameApiService _gameApiService;

        public SetOnlineUserSchedule(ILogger<SetOnlineUserSchedule> logger,
            ICommonService commonService,
            IMemoryCache memoryCache,
            IGameApiService gameApiService)
        {
            _logger = logger;
            _commonService = commonService;
            _memoryCache = memoryCache;
            _gameApiService = gameApiService;
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
                // 例外集合
                var exceptions = new ConcurrentQueue<Exception>();

                // 取得開放的遊戲館別
                var openGameList = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));

                // 查詢各遊戲館在線人數
                foreach (var game in openGameList)
                {
                    try
                    {
                        var cacheKey = $"{RedisCacheKeys.OnlineUser}/{game}/H1";

                        // 查詢各遊戲館在線人數
                        var onlineList = await OnlineUserHandlerAsync(game);

                        // 存進本機快取
                        var json = JsonConvert.SerializeObject(onlineList);
                        var bytes = Encoding.UTF8.GetBytes(json);
                        _memoryCache.Set(cacheKey, GzipHelper.Compress(bytes),
                            new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(30)));
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                }

                if (!exceptions.IsEmpty)
                {
                    throw new AggregateException(exceptions);
                }
            }
            catch (AggregateException error)
            {
                foreach (Exception item in error.InnerExceptions)
                {
                    var errorLine = new System.Diagnostics.StackTrace(item, true).GetFrame(0).GetFileLineNumber();
                    var errorFile = new System.Diagnostics.StackTrace(item, true).GetFrame(0).GetFileName();
                    _logger.LogError("Run SetOnlineUserSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", item.GetType().FullName, item.Message, errorFile, errorLine);
                }
            }
            catch (Exception ex)
            {
                var errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var errorFile = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileName();
                _logger.LogError("Run SetOnlineUserSchedule exception EX : {ex}  MSG : {Message}  Error Line : {errorFile}.{errorLine}", ex.GetType().FullName, ex.Message, errorFile, errorLine);
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// 在線清單
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> OnlineUserHandlerAsync(string platform)
        {
            switch (platform)
            {
                case "RSG":
                    return await GetRsgOnlineUserAsync();
                case "RCG":
                    return await GetRcgOnlineUserAsync();
                default:
                    var list = await GetLastPlatformAsync(platform);
                    return list.Select(x => x.club_id).ToList();
            }
        }

        /// <summary>
        /// 預設在線清單
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private async Task<IEnumerable<GetOnlineUserData>> GetLastPlatformAsync(string platform)
        {
            return (await _commonService._serviceDB.GetWalletLastPlatformByPlatform(platform))
                .Select(w => new GetOnlineUserData()
                {
                    club_id = w.club_id,
                    last_platform = w.last_platform
                });
        }

        /// <summary>
        /// RSG 在線清單
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ExceptionMessage"></exception>
        private async Task<IEnumerable<string>> GetRsgOnlineUserAsync()
        {
            const string memory_cache_key = "RSG_System_Web_Code";
            const int memory_cache_min = 30; //分鐘

            // 取得SystemCode、WebId
            var agentList = await _memoryCache.GetOrCreateAsync(memory_cache_key, async entry =>
            {
                var AgentList = await _commonService._serviceDB.GetRSgSystemWebCode();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(memory_cache_min));
                entry.SetOptions(cacheEntryOptions);
                return AgentList;
            });

            if (agentList == null || !agentList.Any()) return new List<string>();

            var onlineUsers = new List<string>();

            foreach (var agent in agentList)
            {
                var req = new PlayerOnlineListRequest()
                {
                    SystemCode = agent.system_code,
                    WebId = agent.web_id,
                    Page = 1,
                    Rows = 500,
                    GameId = 0
                };

                while (true)
                {
                    var res = await _gameApiService._RsgAPI.PlayerOnlineListAsync(req);

                    if (res.ErrorCode != (int)ErrorCodeEnum.OK) //回應異常拋出錯誤
                        throw new ExceptionMessage(res.ErrorCode, res.ErrorMessage);

                    if (res.Data == null) break; //無資料跳出

                    if (res.Data.UserList != null)
                        onlineUsers.AddRange(res.Data.UserList.Select(x => x.UserId).ToList());

                    if (res.Data.PageCount <= req.Page) break; //末頁跳出

                    req.Page++;
                }
            }

            return onlineUsers;
        }

        /// <summary>
        /// RCG 在線清單
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetRcgOnlineUserAsync()
        {
            const string memory_cache_key = "RCG_System_Web_Code";
            const int memory_cache_min = 15; //分鐘

            var systemWebCodeList = await _memoryCache.GetOrCreateAsync(memory_cache_key, async entry =>
            {
                IEnumerable<dynamic> systemWebCode = await _commonService._serviceDB.GetSystemWebCode();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(memory_cache_min));
                _memoryCache.Set(memory_cache_key, systemWebCode, cacheEntryOptions);
                return systemWebCode;
            });

            var list = new List<string>();

            foreach (var code in systemWebCodeList)
            {
                var res = await _gameApiService._RcgAPI.GetPlayerOnlineList(new RCG_GetPlayerOnlineList
                {
                    systemCode = code.system_code,
                    webId = code.web_id
                });

                if (res != null && res.data.dataList.Count > 0)
                {
                    list.AddRange(res.data.dataList.Select(x => x.memberAccount).ToList());
                }
            }

            return list;
        }
    }
}
