using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedCacheExercise.Interfaces
{
    public interface IKeyValueCacheService<TKey, TValue>
    {
        #region Methods

        /// <summary>
        ///     Add key-value to cache with expiration time (calculate by using life time (seconds))
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds">Life time of cache item (in seconds)</param>
        Task AddAsync(TKey key, TValue value, int? seconds);

        /// <summary>
        ///     Get template by using specific key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TValue> ReadAsync(TKey key);

        /// <summary>
        ///     Remove a value from dictionary.
        /// </summary>
        /// <param name="key"></param>
        Task<bool> RemoveAsync(TKey key);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        IList<TValue> ReadValues();

        /// <summary>
        ///     Find key in dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TKey FormatEntryKey(TKey key);

        /// <summary>
        /// Update the cache item.
        /// </summary>
        void Update(TKey key, TValue value);

        /// <summary>
        /// Reload cache asynchronously.
        /// </summary>
        /// <returns></returns>
        Task ReloadAsync();

        #endregion
    }
}