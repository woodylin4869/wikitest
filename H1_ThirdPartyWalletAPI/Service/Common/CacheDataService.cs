using H1_ThirdPartyWalletAPI.Code;
using H1_ThirdPartyWalletAPI.Helpers;
using H1_ThirdPartyWalletAPI.Model.Config;
using Newtonsoft.Json;
using RedLockNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H1_ThirdPartyWalletAPI.Service.Common
{
    public interface ICacheDataService
    {
        Task<TObject> GetOrSetValueAsync<TObject>(string key, Func<Task<TObject>> fn, int cacheSeconds, bool refresh = false) where TObject : class;

        Task<TObject> LockAsync<TObject>(string lockKey, Func<Task<TObject>> fn);

        Task LockAsync(string lockKey, Func<Task> fn);
        Task LockAsyncRegular(string lockKey, Func<Task> fn);
        Task LockAsyncRegular(string lockKey, Func<Task> fn, TimeSpan expiry, TimeSpan wait, TimeSpan retry);

        Task<TObject> StringGetAsync<TObject>(string key) where TObject : class;

        Task StringSetAsync<TObject>(string key, TObject value, int cacheSeconds) where TObject : class;

        Task StringSetAsync<TObject>(string key, TObject value, int cacheSeconds, CommandFlags commandFlags) where TObject : class;

        Task<bool> KeyDelete(string key);

        Task<long> ListPushAsync<TObject>(string key, TObject value) where TObject : class;

        Task<TObject> ListPopAsync<TObject>(string key) where TObject : class;

        Task<long> ListLengthAsync(string key);
        Task<List<T>> ListGetAsync<T>(string key);
        Task<T> ListGetByIndexAsync<T>(string key, long index) where T : class;
        List<Task<bool>> BatchStringSetAsync<T>(IDictionary<string, T> pairs, TimeSpan expire) where T : class;
        Task<List<T>> BatchStringGetAsync<T>(IEnumerable<string> keys) where T : class;
        List<Task<long>> BatchListPushAsync<T>(string key, IEnumerable<T> values) where T : class;
        Task<List<T>> BatchListPopAsync<T>(string key, long count) where T : class;
        Task<T> LockAsyncRegular<T>(string lockKey, Func<Task<T>> fn, TimeSpan expiry, TimeSpan wait, TimeSpan retry);
        Task<bool> HashSetAsync<T>(string key, string hashKey, T hashValue);
        Task HashSetAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> keyValuePairs);
        Task<T> HashGetAsync<T>(string key, string hashKey);
        Task<List<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys);
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);
        Task GZipStringSetAsync<TObject>(string key, TObject value, int cacheSeconds) where TObject : class;
        Task<TObject> GZipStringGetAsync<TObject>(string key) where TObject : class;
        Task<TObject> GetOrSetGZipValueAsync<TObject>(string key, Func<Task<TObject>> fn, int cacheSeconds, bool refresh = false) where TObject : class;
        Task<long> SortedSetAddAsync(string key, IDictionary<string, double> values);
        Task<(string element, double score)> SortedSetPopMaxAsync(string key);
        Task<(string element, double score)> SortedSetPopMinAsync(string key);
        Task<List<T>> ListGetByRangeAsync<T>(string key, int start, int stop);
        Task CleanExpiredListKeysAsync(string key, int chunkSize = 100, int maxListSize = 10000);
        Task<long> ListLeftPushAsync<TObject>(string key, TObject value) where TObject : class;
    }
    public class CacheDataService : ICacheDataService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDistributedLockFactory _distributedLockFactory;
        private readonly IDatabase _redis;
        private readonly string _prefix = $"H1_ThirdPartyWalletAPI:{Config.OneWalletAPI.Redis_PreKey}:";

        public CacheDataService(IConnectionMultiplexer connectionMultiplexer, IDistributedLockFactory distributedLockFactory)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _distributedLockFactory = distributedLockFactory;
            _redis = _connectionMultiplexer.GetDatabase(Config.Redis.DefaultDb);
        }

        public async Task<TObject> GetOrSetValueAsync<TObject>(string key, Func<Task<TObject>> fn, int cacheSeconds, bool refresh = false) where TObject : class
        {
            var result = await StringGetAsync<TObject>(key);
            if (result != null && refresh)
            {
                await KeyDelete(key);
                result = default;
            }

            if (result == null)
            {
                result = await fn();
                if (result != null)
                {
                    await StringSetAsync(key, result, cacheSeconds);
                }
            }

            return result;
        }

        public async Task<TObject> GetOrSetGZipValueAsync<TObject>(string key, Func<Task<TObject>> fn, int cacheSeconds, bool refresh = false) where TObject : class
        {
            var result = await GZipStringGetAsync<TObject>(key);
            if (result != null && refresh)
            {
                await KeyDelete(key);
                result = default;
            }

            if (result == null)
            {
                result = await fn();
                if (result != null)
                {
                    await GZipStringSetAsync(key, result, cacheSeconds);
                }
            }

            return result;
        }

        public async Task<TObject> StringGetAsync<TObject>(string key) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = await _redis.StringGetAsync(redisKey);
            if (!data.HasValue)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<TObject>(data);
        }

        public async Task StringSetAsync<TObject>(string key, TObject value, int cacheSeconds) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = JsonConvert.SerializeObject(value);
            await _redis.StringSetAsync(redisKey, data, TimeSpan.FromSeconds(cacheSeconds));
        }

        public async Task StringSetAsync<TObject>(string key, TObject value, int cacheSeconds, CommandFlags commandFlags) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = JsonConvert.SerializeObject(value);
            await _redis.StringSetAsync(redisKey, data, TimeSpan.FromSeconds(cacheSeconds), flags: commandFlags);
        }

        public Task GZipStringSetAsync<TObject>(string key, TObject value, int cacheSeconds) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = JsonConvert.SerializeObject(value);
            return _redis.StringSetAsync(redisKey, GzipHelper.Compress(Encoding.UTF8.GetBytes(data)), TimeSpan.FromSeconds(cacheSeconds));
        }

        public async Task<TObject> GZipStringGetAsync<TObject>(string key) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = await _redis.StringGetAsync(redisKey);
            if (!data.HasValue) return default;
            var jsonStr = Encoding.UTF8.GetString(GzipHelper.Decompress((byte[])data));
            return JsonConvert.DeserializeObject<TObject>(jsonStr);
        }

        public async Task<TObject> LockAsync<TObject>(string lockKey, Func<Task<TObject>> fn)
        {
            var resource = _prefix + "redLock:" + lockKey;
            var expiry = TimeSpan.FromSeconds(30);
            var wait = TimeSpan.FromSeconds(1);
            var retry = TimeSpan.FromMilliseconds(500);

            using (var redLock = await _distributedLockFactory.CreateLockAsync(resource, expiry, wait, retry))
            {
                if (redLock.IsAcquired)
                {
                    return await fn();
                }

                throw new CacheLockingException();
            }
        }
        public async Task LockAsync(string lockKey, Func<Task> fn)
        {
            if (Config.OneWalletAPI.WalletMode == "SingleWallet")
            {
                var resource = _prefix + "redLock:" + lockKey;
                var expiry = TimeSpan.FromSeconds(30);
                var wait = TimeSpan.FromSeconds(1);
                var retry = TimeSpan.FromMilliseconds(500);

                using (var redLock = await _distributedLockFactory.CreateLockAsync(resource, expiry, wait, retry))
                {
                    if (redLock.IsAcquired)
                    {
                        await fn();
                    }
                    else
                    {
                        throw new CacheLockingException();
                    }
                }
            }
            else
            {
                await fn();
            }
        }
        public Task LockAsyncRegular(string lockKey, Func<Task> fn)
        {
            var expiry = TimeSpan.FromSeconds(10);
            var wait = TimeSpan.FromSeconds(1);
            var retry = TimeSpan.FromMilliseconds(500);

            return LockAsyncRegular(lockKey, fn, expiry, wait, retry);
        }
        public async Task LockAsyncRegular(string lockKey, Func<Task> fn, TimeSpan expiry, TimeSpan wait, TimeSpan retry)
        {
            var resource = _prefix + "redLock:" + lockKey;
            await using var redLock = await _distributedLockFactory.CreateLockAsync(resource, expiry, wait, retry);
            if (redLock.IsAcquired)
                await fn();
            else
                throw new CacheLockingException();
        }
        public async Task<T> LockAsyncRegular<T>(string lockKey, Func<Task<T>> fn, TimeSpan expiry, TimeSpan wait, TimeSpan retry)
        {
            var resource = _prefix + "redLock:" + lockKey;
            await using var redLock = await _distributedLockFactory.CreateLockAsync(resource, expiry, wait, retry);
            if (redLock.IsAcquired)
                return await fn();
            else
                throw new CacheLockingException();
        }
        public async Task<bool> KeyDelete(string key)
        {
            var redisKey = _prefix + key;
            return await _redis.KeyDeleteAsync(redisKey);
        }

        public async Task<long> ListPushAsync<TObject>(string key, TObject value) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = JsonConvert.SerializeObject(value);
            return await _redis.ListRightPushAsync(redisKey, data);
        }
        public async Task<long> ListLeftPushAsync<TObject>(string key, TObject value) where TObject : class
        {
            var redisKey = _prefix + key;
            var data = JsonConvert.SerializeObject(value);
            return await _redis.ListLeftPushAsync(redisKey, data);
        }
        public Task<bool> HashSetAsync<T>(string key, string hashKey, T hashValue)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' 不得為 Null 或空白字元。", nameof(key));
            }

            if (string.IsNullOrWhiteSpace(hashKey))
            {
                throw new ArgumentException($"'{nameof(hashKey)}' 不得為 Null 或空白字元。", nameof(hashKey));
            }

            var redisKey = _prefix + key;
            var data = JsonConvert.SerializeObject(hashValue);
            return _redis.HashSetAsync(redisKey, hashKey, data);
        }

        public Task HashSetAsync<T>(string key, IEnumerable<KeyValuePair<string, T>> keyValuePairs)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' 不得為 Null 或空白字元。", nameof(key));
            }

            var redisKey = _prefix + key;
            var entrys = keyValuePairs
                .Select(pairs => new HashEntry(pairs.Key, JsonConvert.SerializeObject(pairs.Value)))
                .ToArray();

            return _redis.HashSetAsync(redisKey, entrys);
        }


        public async Task<T> HashGetAsync<T>(string key, string hashKey)
        {
            var redisKey = _prefix + key;
            var val = await _redis.HashGetAsync(redisKey, hashKey);
            if (!val.HasValue) return default(T);
            return JsonConvert.DeserializeObject<T>(val);
        }

        public async Task<List<T>> HashGetAsync<T>(string key, IEnumerable<string> hashKeys)
        {
            var redisKey = _prefix + key;
            var hashKeysAry = hashKeys.Select(k => (RedisValue)k).ToArray();
            var vals = await _redis.HashGetAsync(redisKey, hashKeysAry);
            vals ??= Array.Empty<RedisValue>();
            if (!vals.Any(v => v.HasValue)) return new();

            return vals
                .Where(v => v.HasValue)
                .Select(v => JsonConvert.DeserializeObject<T>(v))
                .ToList() ?? new();
        }

        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            var redisKey = _prefix + key;
            var entris = await _redis.HashGetAllAsync(redisKey);
            entris ??= Array.Empty<HashEntry>();
            if (!entris.Any(e => e != default)) return new();
            return entris.Where(e => e != default).ToDictionary(e => (string)e.Name, e => JsonConvert.DeserializeObject<T>(e.Value));
        }

        public async Task<TObject> ListPopAsync<TObject>(string key) where TObject : class
        {
            var redisKey = _prefix + key;
            var result = await _redis.ListLeftPopAsync(redisKey);
            if (result.HasValue)
            {
                return JsonConvert.DeserializeObject<TObject>(result);
            }
            else
            {
                return null;
            }
        }

        public async Task<long> ListLengthAsync(string key)
        {
            var redisKey = _prefix + key;
            return await _redis.ListLengthAsync(redisKey);
        }

        public async Task<List<T>> ListGetAsync<T>(string key)
        {
            var redisKey = _prefix + key;
            var list = await _redis.ListRangeAsync(redisKey);
            return list.Select(item => JsonConvert.DeserializeObject<T>(item)).ToList();
        }

        public async Task<T> ListGetByIndexAsync<T>(string key, long index) where T : class
        {
            var redisKey = _prefix + key;
            var result = await _redis.ListGetByIndexAsync(redisKey, index);
            if (result.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
            else
            {
                return null;
            }
        }

        public async Task<long> SortedSetAddAsync(string key, IDictionary<string, double> values)
        {
            var redisKey = _prefix + key;
            var entries = values.Select(p => new SortedSetEntry(new RedisValue(p.Key), p.Value)).ToArray();
            return await _redis.SortedSetAddAsync(redisKey, entries);
        }

        public async Task<(string element, double score)> SortedSetPopMaxAsync(string key)
        {
            var redisKey = _prefix + key;
            var entry = await _redis.SortedSetPopAsync(redisKey, Order.Descending);
            return entry.HasValue ? (element: entry.Value.Element, score: entry.Value.Score) : default;
        }

        public async Task<(string element, double score)> SortedSetPopMinAsync(string key)
        {
            var redisKey = _prefix + key;
            var entry = await _redis.SortedSetPopAsync(redisKey, Order.Ascending);
            return entry.HasValue ? (element: entry.Value.Element, score: entry.Value.Score) : default;
        }

        public List<Task<long>> BatchListPushAsync<T>(string key, IEnumerable<T> values) where T : class
        {
            var db = _redis.CreateBatch();

            var tasks = new List<Task<long>>();

            var redisKey = _prefix + key;
            foreach (var value in values)
            {
                var data = JsonConvert.SerializeObject(value);
                tasks.Add(db.ListRightPushAsync(redisKey, data));
            }

            db.Execute();

            return tasks;
        }

        public async Task<List<T>> BatchListPopAsync<T>(string key, long count) where T : class
        {
            var db = _redis.CreateBatch();

            var tasks = new List<Task<RedisValue>>();

            var redisKey = _prefix + key;
            for (var i = 0; i < count; i++)
            {
                tasks.Add(db.ListLeftPopAsync(redisKey));
            }

            db.Execute();

            var results = new List<T>();
            foreach (var task in tasks)
            {
                var value = await task;
                if (value.HasValue)
                    results.Add(JsonConvert.DeserializeObject<T>(value));
            }

            return results;
        }

        public List<Task<bool>> BatchStringSetAsync<T>(IDictionary<string, T> pairs, TimeSpan expire) where T : class
        {
            var db = _redis.CreateBatch();

            var tasks = new List<Task<bool>>();

            foreach (var pair in pairs)
            {
                var key = _prefix + pair.Key;
                var data = JsonConvert.SerializeObject(pair.Value);
                tasks.Add(db.StringSetAsync(key, data, expire));
            }

            db.Execute();

            return tasks;
        }

        public async Task<List<T>> BatchStringGetAsync<T>(IEnumerable<string> keys) where T : class
        {
            var db = _redis.CreateBatch();

            var tasks = new List<Task<RedisValue>>();

            foreach (var key in keys)
            {
                tasks.Add(db.StringGetAsync(_prefix + key));
            }

            db.Execute();

            var results = new List<T>();
            foreach (var task in tasks)
            {
                var value = await task;
                if (value.HasValue)
                    results.Add(JsonConvert.DeserializeObject<T>(value));
            }

            return results;
        }

        public async Task<List<T>> ListGetByRangeAsync<T>(string key, int start, int stop)
        {
            var redisKey = _prefix + key;
            var list = await _redis.ListRangeAsync(redisKey, start, stop);
            return list.Select(item => JsonConvert.DeserializeObject<T>(item)).ToList();
        }

        public async Task CleanExpiredListKeysAsync(string key, int chunkSize = 100, int maxListSize = 10000)
        {
            try
            {
                var redisKey = _prefix + key;
                var keys = await _redis.ListRangeAsync(redisKey);

                // Trim the list to the most recent 10,000 items
                if (keys.Length > maxListSize)
                {
                    await _redis.ListTrimAsync(redisKey, -maxListSize, -1);
                    keys = await _redis.ListRangeAsync(redisKey); // Retrieve the trimmed list
                }

                // Process keys in chunks
                for (int i = 0; i < keys.Length; i += chunkSize)
                {
                    var chunk = keys.Skip(i).Take(chunkSize);

                    var tasks = chunk.Select(async listKey =>
                    {
                        try
                        {
                            var redisKey2 = _prefix + listKey.ToString().Trim('\"');
                            if (!await _redis.KeyExistsAsync(redisKey2))
                            {
                                await _redis.ListRemoveAsync(redisKey, listKey);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the exception for the specific listKey
                            Console.WriteLine($"Error processing key '{listKey}': {ex.Message}");
                        }
                    });

                    await Task.WhenAll(tasks);

                    // Delay between chunks
                    if (i + chunkSize < keys.Length)
                    {
                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for the overall process
                Console.WriteLine($"Error cleaning expired list keys for '{key}': {ex.Message}");
            }
        }

    }

    public class CacheLockingException : ExceptionMessage
    {
        public CacheLockingException() : base(ResponseCode.Fail, "locking") { }

        public CacheLockingException(string key) : base(ResponseCode.Fail, $"{key} locking") { }
    }
}