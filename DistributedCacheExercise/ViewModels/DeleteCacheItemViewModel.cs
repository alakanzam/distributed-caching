using System.ComponentModel.DataAnnotations;

namespace DistributedCacheExercise.ViewModels
{
    public class DeleteCacheItemViewModel
    {
        #region Properties

        [Required]
        public string Key { get; set; }

        #endregion
    }
}