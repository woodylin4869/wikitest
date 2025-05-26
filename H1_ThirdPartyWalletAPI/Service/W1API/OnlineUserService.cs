using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H1_ThirdPartyWalletAPI.Model.W1API;
using H1_ThirdPartyWalletAPI.Model;
using H1_ThirdPartyWalletAPI.Model.Config;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using H1_ThirdPartyWalletAPI.Helpers;
using Newtonsoft.Json;

namespace H1_ThirdPartyWalletAPI.Service.W1API
{
    public interface IOnlineUserService
    {
        Task<GetOnlineUserListRes> GetOnlineUser(GetOnlineUserListReq getonlineUserReq);
    }

    public class OnlineUserService : IOnlineUserService
    {
        private readonly IMemoryCache _memoryCache;

        public OnlineUserService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<GetOnlineUserListRes> GetOnlineUser(GetOnlineUserListReq getonlineUserReq)
        {
            // 查詢全部館別或是單一館別
            var openGameList = new List<string>();
            if (getonlineUserReq.Platform == null)
            {
                openGameList = new List<string>(Config.OneWalletAPI.OpenGame.Split(','));
            }
            else
            {
                openGameList.Add(getonlineUserReq.Platform);
            }

            var taskResult = await Task.WhenAll(openGameList.Select(game => Task.Run(async () =>
            {
                var cacheKey = $"{RedisCacheKeys.OnlineUser}/{game}/H1";
                var bytes = await _memoryCache.GetOrCreateAsync(cacheKey, entry =>
                {
                    // 沒快取就給預設值
                    var json = JsonConvert.SerializeObject(new List<string>());
                    var bytes = Encoding.UTF8.GetBytes(json);
                    entry.SetOptions(new MemoryCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddMinutes(10)));
                    return Task.FromResult(GzipHelper.Compress(bytes));
                });

                var json = Encoding.UTF8.GetString(GzipHelper.Decompress(bytes));
                var list = JsonConvert.DeserializeObject<List<string>>(json);

                return list.Select(clubId => new OnlineUserData()
                {
                    Club_id = clubId,
                    Platform = game
                }).ToList();
            })));

            return new GetOnlineUserListRes()
            {
                UserList = taskResult.SelectMany(x => x).ToList()
            };
        }
    }
}