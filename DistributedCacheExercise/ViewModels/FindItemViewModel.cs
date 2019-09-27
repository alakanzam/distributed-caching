using System.ComponentModel.DataAnnotations;

namespace DistributedCacheExercise.ViewModels
{
    public class FindItemViewModel
    {
        #region Properties

        [Required]
        public string Key { get; set; }

        #endregion
    }
}