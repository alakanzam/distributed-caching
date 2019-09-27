namespace DistributedCacheExercise.Interfaces
{
    public interface ITextKeyValueCacheService<TValue> : IKeyValueCacheService<string, TValue>
    {
        
    }
}