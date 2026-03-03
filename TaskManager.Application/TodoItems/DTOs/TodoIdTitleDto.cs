namespace TaskManager.Application.TodoItems.DTOs
{
    //A DTO that contains only the ID and Title of a Todo item
    //Used in Presentation to pass a todo item's ID and Title from ProjectDetailedViews to modals.
    public record TodoIdTitleDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
    }
}
