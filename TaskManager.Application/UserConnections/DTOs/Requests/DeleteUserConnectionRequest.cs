namespace TaskManager.Application.UserConnections.DTOs.Requests
{
    public record DeleteUserConnectionRequest
    {
        //ID For the Connection Itself
        public Guid Id { get; set; }
    }
  
}
