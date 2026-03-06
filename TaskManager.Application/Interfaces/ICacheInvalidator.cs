namespace TaskManager.Application.Interfaces
{
    public interface ICacheInvalidator
    {
        Guid UserId { get; }
        string[] Keys { get; } 
    }
}
