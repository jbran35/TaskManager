namespace TaskManager.Application.Interfaces
{
    public interface IProjectDetailsCacheInvalidator
    {
        string[] CacheKeys(Guid userId, Guid projectId); 
    }
}
