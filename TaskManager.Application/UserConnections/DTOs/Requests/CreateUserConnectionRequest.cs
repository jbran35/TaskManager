namespace TaskManager.Application.UserConnections.DTOs.Requests
{
    public record CreateUserConnectionRequest
    {
        public Guid AssigneeId { get; set; }
        //public bool FromAddTodoItem { get; set; } = false;

        //public bool FromEditTodoItem { get; set; } = false;

    }

}
