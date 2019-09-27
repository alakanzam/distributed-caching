using System.Threading.Tasks;
using DistributedCacheExercise.Interfaces;
using DistributedCacheExercise.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DistributedCacheExercise.Controllers
{
    [Route("api/cache")]
    public class CacheController : Controller
    {
        #region Properties

        private readonly ITextKeyValueCacheService<string> _textKeyValueCacheService;

        #endregion

        #region Constructor

        public CacheController(ITextKeyValueCacheService<string> textKeyValueCacheService)
        {
            _textKeyValueCacheService = textKeyValueCacheService;
        }

        #endregion

        #region Methods

        [HttpGet("")]
        public async Task<ActionResult<string>> GetAsync([FromQuery] FindItemViewModel model)
        {
            if (model == null)
            {
                model = new FindItemViewModel();
                TryValidateModel(model);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var value = await _textKeyValueCacheService.ReadAsync(model.Key);
            return Ok(value);
        }

        [HttpPost("")]
        public async Task<ActionResult> AddAsync([FromBody] AddItemViewModel model)
        {
            if (model == null)
            {
                model = new AddItemViewModel();
                TryValidateModel(model);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _textKeyValueCacheService.AddAsync(model.Key, model.Value, model.LifeTimeInSeconds);
            return Ok();
        }

        [HttpDelete("")]
        public async Task<ActionResult> DeleteAsync([FromQuery] DeleteCacheItemViewModel model)
        {
            if (model == null)
            {
                model = new DeleteCacheItemViewModel();
                TryValidateModel(model);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _textKeyValueCacheService.RemoveAsync(model.Key);
            return Ok();
        }

        #endregion
    }
}