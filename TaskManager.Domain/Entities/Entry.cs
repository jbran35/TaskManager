using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Events;
using TaskManager.Domain.ValueObjects;


namespace TaskManager.Domain.Entities
{

    //For Projects and TodoItems, as they both need Names and Descriptions
    public abstract class Entry : Entity
    {
        public Title Title { get; protected set; }
        public Description Description { get; protected set; }
        public Guid OwnerId { get; protected set; }
        public virtual User? Owner { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public Status Status { get; protected set; }
        public bool IsOwnerLoaded => Owner != null;


        protected Entry() : base() { }
        protected Entry(Title title, Description description, Guid ownerId) : base()
        {
            if (ownerId == Guid.Empty) throw new ArgumentException("Owner ID cannot be empty.", nameof(ownerId));

            Title = title; 
            Description = description;
            OwnerId = ownerId;
            CreatedOn = DateTime.UtcNow;
            Status = Status.Incomplete;
        }

        public Result UpdateTitle(string newTitle)
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            var titleResult = Title.Create(newTitle);

            if (titleResult.IsFailure) return Result.Failure(titleResult.ErrorMessage!);

            Title = titleResult.Value;
            AddDomainEvent(new EntityTitleUpdatedEvent(this.Id));

            return Result.Success();

        }

        public Result UpdateDescription(string newDescription)
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            var descriptionResult = Description.Create(newDescription);

            if(descriptionResult.IsFailure) return Result.Failure(descriptionResult.ErrorMessage!);


            Description = descriptionResult.Value;
            AddDomainEvent(new EntityDescriptionUpdatedEvent(this.Id));

            return Result.Success();
        }

        public virtual Result MarkAsComplete()
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            if (Status != Status.Complete) Status = Status.Complete;
            AddDomainEvent(new EntityCompletedEvent(this.Id));
            return Result.Success();
        
        }

        public Result MarkAsIncomplete()
        {
            var deletedCheck = CheckIfDeleted();
            if (deletedCheck.IsFailure) return deletedCheck;

            if (Status != Status.Incomplete) Status = Status.Incomplete;
            AddDomainEvent(new EntityMarkedIncompleteEvent(this.Id));
            return Result.Success();

        }

        public Result MarkAsDeleted()
        {
            if (Status != Status.Deleted) Status = Status.Deleted;
            AddDomainEvent(new EntityDeletedEvent(this.Id));
            return Result.Success();
        }

        public Result CheckIfDeleted()
        {
            if (Status == Status.Deleted)
            {
                return Result.Failure("Cannot perform this operation on a deleted entry.");
            }

            return Result.Success();
        }
    }
}
