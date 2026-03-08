namespace TaskManager.Application.Common
{
    public static class CacheKeys
    {
        public static string ProjectTiles(Guid userId)
            => $"project_tiles:{userId}";

        public static string ProjectDetailedViews(Guid userId, Guid projectId)
            => $"project_details:{userId}:{projectId}";

        public static string UserConnections(Guid userId)
          => $"user_connections:{userId}";

        public static string AssignedTodoItems(Guid userId)
         => $"assigned_todo_items:{userId}";

        public static string Connections(Guid userId)
            => $"connections:{userId}";
    }
}
