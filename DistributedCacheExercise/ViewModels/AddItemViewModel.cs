using System.ComponentModel.DataAnnotations;

namespace DistributedCacheExercise.ViewModels
{
    public class AddItemViewModel
    {
        #region Properties

        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        public int? LifeTimeInSeconds { get; set; }

        #endregion
    }
}