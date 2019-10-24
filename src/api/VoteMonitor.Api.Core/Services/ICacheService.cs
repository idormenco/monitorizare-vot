﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace VoteMonitor.Api.Core.Services
{
	/// <summary>
	/// Interface for the caching service to be used.
	/// </summary>
	public interface ICacheService
	{
		Task<T> GetOrSaveDataInCacheAsync<T>(string cacheKey, Func<Task<T>> source, DistributedCacheEntryOptions options = null)
		Task<T> GetOrSaveDataInCacheAsync<T>(CacheObjectsName name, Func<Task<T>> source, DistributedCacheEntryOptions options = null);
		Task<T> GetObjectSafeAsync<T>(string name);
		Task SaveObjectSafeAsync(string name, object value, DistributedCacheEntryOptions options = null);

	}
}
