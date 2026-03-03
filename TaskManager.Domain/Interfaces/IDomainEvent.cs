namespace TaskManager.Domain.Interfaces
{
    public interface IDomainEvent
    {
        /// <summary>
        /// Gets the date and time at which the event occurred.
        /// </summary>
        /// <remarks>This property provides the timestamp of the event, which can be used for logging,
        /// auditing, or tracking purposes.</remarks>
        DateTime OccurredOn { get; }
    }
}
