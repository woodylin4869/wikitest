using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.DataModel;
using H1_ThirdPartyWalletAPI.Service.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service
{
    public interface IGamePlatformUserService
    {
        /// <summary>
        /// Get GamePlatformUser
        /// Cache 600s
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        Task<List<GamePlatformUser>> GetGamePlatformUserAsync(string clubId);
        /// <summary>
        /// Get Single GamePlatformUser
        /// Cache 600s
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        Task<GamePlatformUser> GetSingleGamePlatformUserAsync(string clubId, Platform platform);
        /// <summary>
        /// Post GamePlatformUser
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<int> PostGamePlatformUserAsync(GamePlatformUser userData);
        /// <summary>
        /// Post GamePlatformUser With Retry
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> PostGamePlatformUserRetryAsync(GamePlatformUser userData, int retry);
    }

    public class GamePlatformUserService : IGamePlatformUserService
    {
        private readonly ILogger<GamePlatformUserService> _logger;
        private readonly IDBService _dbService;
        private readonly ICacheDataService _cacheService;

        const int _cacheSeconds = 600;

        public GamePlatformUserService(ILogger<GamePlatformUserService> logger, IDBService dbService, ICacheDataService cacheDataService)
        {
            _logger = logger;
            _dbService = dbService;
            _cacheService = cacheDataService;
        }

        /// <summary>
        /// Get GamePlatformUser
        /// Cache 600s
        /// </summary>
        /// <param name="clubId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Task<List<GamePlatformUser>> GetGamePlatformUserAsync(string clubId)
        {
            if (string.IsNullOrWhiteSpace(clubId))
            {
                throw new ArgumentException($"'{nameof(clubId)}' 不得為 Null 或空白字元。", nameof(clubId));
            }

            return GetGamePlatformUserCacheAsync(clubId);
        }

        private async Task<List<GamePlatformUser>> GetGamePlatformUserCacheAsync(string clubId)
        {
            try
            {
                List<GamePlatformUser> gamePlatformUsers = await _cacheService.GetOrSetValueAsync($"{RedisCacheKeys.PlatformUser}/{L2RedisCacheKeys.game_user}/{clubId}",
                async () =>
                {
                    var result = await _dbService.GetGamePlatformUser(clubId);
                    return result;
                },
                _cacheSeconds);
                return gamePlatformUsers;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{action} {level} {message}", nameof(GetGamePlatformUserCacheAsync), LogLevel.Error, ex.Message);
                return new();
            }
        }

        /// <summary>
        /// Get Single GamePlatformUser
        /// Cache 600s
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<GamePlatformUser> GetSingleGamePlatformUserAsync(string clubId, Platform platform)
        {
            if (string.IsNullOrWhiteSpace(clubId))
            {
                throw new ArgumentException($"'{nameof(clubId)}' 不得為 Null 或空白字元。", nameof(clubId));
            }

            var gamePlatformUsers = await GetGamePlatformUserAsync(clubId);
            return gamePlatformUsers.FirstOrDefault(x => x.game_platform == platform.ToString());
        }

        /// <summary>
        /// Post GamePlatformUser
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<int> PostGamePlatformUserAsync(GamePlatformUser userData)
        {
            if (userData is null)
            {
                throw new ArgumentNullException(nameof(userData));
            }
            try
            {
                var result = await _dbService.PostGamePlatformUser(userData);

                if (result == 0)
                    return result;

                _ = _cacheService.KeyDelete($"{RedisCacheKeys.PlatformUser}/{L2RedisCacheKeys.game_user}/{userData.club_id}");

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{action} {level} {message}", nameof(PostGamePlatformUserAsync), LogLevel.Error, ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Post GamePlatformUser With Retry
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> PostGamePlatformUserRetryAsync(GamePlatformUser userData, int retry)
        {
            if (userData is null)
                throw new ArgumentNullException(nameof(userData));

            if (retry == 0) return false;

            var postResult = await PostGamePlatformUserAsync(userData);

            if (postResult == 1) return true;

            return await PostGamePlatformUserRetryAsync(userData, retry - 1);
        }
    }
}
