using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistributedCacheExercise.Extensions;
using DistributedCacheExercise.Interfaces;
using DistributedCacheExercise.Models.Entities;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace DistributedCacheExercise.Services
{
    public class InMemoryKeyValueCacheService<TValue> : ITextKeyValueCacheService<TValue>
    {
        #region Constructor

        public InMemoryKeyValueCacheService(IMongoCollection<KeyValueItem> cacheEntries)
        {
            _cacheEntries = cacheEntries;
            _localCachedEntries = new List<KeyValueItem>();
        }

        #endregion

        #region Properties

        private readonly IMongoCollection<KeyValueItem> _cacheEntries;

        private IList<KeyValueItem> _localCachedEntries;

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public Task AddAsync(string key, TValue value, int? seconds)
        {
            // Serialize value.
            var serializedValue = JsonConvert.SerializeObject(value);
            double? expiredTime = null;

            if (seconds != null && seconds > 0)
                expiredTime = DateTime.UtcNow.AddSeconds(seconds.Value).ToUnixTime();

            // Format item key.
            var formattedKey = FormatEntryKey(key);

            var keyFilterBuilder = Builders<KeyValueItem>.Filter;
            var keyFilterDefinition = keyFilterBuilder.Eq(item => item.Key, formattedKey);

            var keyUpdateBuilder = Builders<KeyValueItem>.Update;
            var keyUpdateDefinition = keyUpdateBuilder
                .Set(item => item.Value, serializedValue)
                .Set(item => item.ExpiredTime, expiredTime);

            var findOneAndUpdateOptions = new FindOneAndUpdateOptions<KeyValueItem, KeyValueItem>();
            findOneAndUpdateOptions.ReturnDocument = ReturnDocument.After;
            findOneAndUpdateOptions.IsUpsert = true;

            _cacheEntries.FindOneAndUpdate(keyFilterDefinition, keyUpdateDefinition, findOneAndUpdateOptions);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Normalize the input key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string FormatEntryKey(string key)
        {
            return key.Trim().ToLower();
        }

        /// <summary>
        ///     Get and serialize value by searching its key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<TValue> ReadAsync(string key)
        {
            var formattedKey = FormatEntryKey(key);
            var unixTime = DateTime.UtcNow.ToUnixTime();

            // Read from local cache first.
            var localCachedItem = _localCachedEntries
                .FirstOrDefault(item => item.Key.Equals(formattedKey) && (item.ExpiredTime == null || (item.ExpiredTime != null && unixTime <= item.ExpiredTime)));

            // Cache item is not found.
            if (localCachedItem == null)
                localCachedItem = await BuildCachedItemAsync(formattedKey);

            if (localCachedItem == null)
                return default;

            lock (_localCachedEntries)
            {
                _localCachedEntries.Add(localCachedItem);
            }

            return JsonConvert.DeserializeObject<TValue>(localCachedItem.Value);
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public IList<TValue> ReadValues()
        {
            var values = _localCachedEntries
                .Select(x => JsonConvert.DeserializeObject<List<TValue>>(x.Value))
                .ToList() as IList<TValue>;

            return values;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public Task ReloadAsync()
        {
            var unixTime = DateTime.UtcNow.ToUnixTime();

            var itemFilterDefinition = Builders<KeyValueItem>.Filter
                .Gte(item => item.ExpiredTime, unixTime);

            // Block another threads from using local cached item while it is being reloaded.
            lock (_localCachedEntries)
            {
                // Load all keys from database.
                var entries = _cacheEntries.Find(itemFilterDefinition)
                    .ToList();

                _localCachedEntries = entries;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Remove an item from cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Task<bool> RemoveAsync(string key)
        {
            // Get formatted key.
            var formattedKey = FormatEntryKey(key);

            lock (_localCachedEntries)
            {
                var deletedItem = _localCachedEntries.FirstOrDefault(x => x.Key == formattedKey);
                if (deletedItem != null)
                    _localCachedEntries.Remove(deletedItem);
            }

            var keyFilterDefinition = Builders<KeyValueItem>.Filter
                .Eq(item => item.Key, formattedKey);

            _cacheEntries.DeleteMany(keyFilterDefinition);
            return Task.FromResult(true);
        }

        public void Update(string key, TValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Raised when a key is not found in local cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected async Task<KeyValueItem> BuildCachedItemAsync(string key)
        {
            var formattedKey = FormatEntryKey(key);
            var unixTime = DateTime.UtcNow.ToUnixTime();

            var cachedItemFilterDefinition = Builders<KeyValueItem>.Filter
                .Eq(item => item.Key, formattedKey);

            cachedItemFilterDefinition &= Builders<KeyValueItem>.Filter
                .Gte(x => x.ExpiredTime, unixTime);

            var cachedEntry = await _cacheEntries
                .Find(cachedItemFilterDefinition)
                .FirstOrDefaultAsync();

            if (cachedEntry == null)
                return null;

            try
            {
                lock (_localCachedEntries)
                {
                    var localCachedEntry = _localCachedEntries.FirstOrDefault(x => x.Key == formattedKey);
                    if (localCachedEntry == null)
                    {
                        localCachedEntry = new KeyValueItem(cachedEntry.Key);
                        localCachedEntry.Value = cachedEntry.Value;
                        localCachedEntry.ExpiredTime = cachedEntry.ExpiredTime;
                        _localCachedEntries.Add(localCachedEntry);
                        return localCachedEntry;
                    }

                    localCachedEntry.Value = cachedEntry.Value;
                    localCachedEntry.ExpiredTime = cachedEntry.ExpiredTime;
                    return localCachedEntry;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}