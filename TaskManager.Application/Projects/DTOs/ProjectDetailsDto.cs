namespace TaskManager.Application.Projects.DTOs
{
    public class ProjectDetailsDto
    {
        //Id, Title, Description, CreatedOn
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
