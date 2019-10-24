﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VoteMonitor.Api.Core.Services
{
    /// <inheritdoc />
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        public CacheService(IDistributedCache cache, ILogger logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T> GetOrSaveDataInCacheAsync<T>(CacheObjectsName name, Func<Task<T>> source, DistributedCacheEntryOptions options = null)
		{
			return await GetOrSaveDataInCacheAsync<T>(name.ToString(), source, options);
		}

		public async Task<T> GetOrSaveDataInCacheAsync<T>(string cacheKey, Func<Task<T>> source, DistributedCacheEntryOptions options = null)
        {
            var obj = await GetObjectSafeAsync<T>(cacheKey);

            if (obj != null)
                return obj;

            var result = await source();

            await SaveObjectSafeAsync(cacheKey, result, options);

            return result;
        }

        public async Task<T> GetObjectSafeAsync<T>(string cacheKey)
        {
            var result = default(T);

            try
            {
                var cache = await _cache.GetAsync(cacheKey.ToString());

                if (cache == null)
                {
                    _logger.LogInformation($"Cache missed for {cacheKey}");
                    return default(T);
                }

                var obj = JsonConvert.DeserializeObject<T>(GetString(cache));

                return obj;

            }
            catch (Exception exception)
            {
                _logger.LogError(GetHashCode(),exception,exception.Message);
            }

            return result;
        }

        public async Task SaveObjectSafeAsync(string cacheKey, object value, DistributedCacheEntryOptions options = null)
        {
            try
            {
                var obj = JsonConvert.SerializeObject(value);

                if (options != null)
                    await _cache.SetAsync(cacheKey, GetBytes(obj), options);
                else
                    await _cache.SetAsync(cacheKey, GetBytes(obj));

            }
            catch (Exception exception)
            {
                _logger.LogError(GetHashCode(), exception, exception.Message);
            }
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        private static string GetString(byte[] bytes)
        {
            var chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

    }
    /// <summary>
    /// Enum for forms' names in cache
    /// </summary>
    public enum CacheObjectsName
    {
        /// <summary>
        /// First form
        /// </summary>
        FormularA,
        /// <summary>
        /// Second form
        /// </summary>
        FormularB,
        /// <summary>
        /// this is becoming redundant
        /// </summary>
        FormularC
    }
}
