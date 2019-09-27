using System;

namespace DistributedCacheExercise.Models.Entities
{
    public class KeyValueItem
    {
        public KeyValueItem(string key)
        {
            Key = key;
        }

        #region Properties

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string Key { get; private set; }

        public string Value { get; set; }

        /// <summary>
        /// Time when item should be expired.
        /// </summary>
        public double? ExpiredTime { get; set; }

        #endregion
    }
}