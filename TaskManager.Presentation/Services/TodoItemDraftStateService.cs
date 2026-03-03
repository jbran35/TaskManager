using TaskManager.Application.UserConnections.DTOs;

namespace TaskManager.Presentation.Services
{
    public class TodoItemDraftStateService
    {
        private TodoItemModel _model = new TodoItemModel();

        public TodoItemModel GetModelFromCache()
        {
            return _model;
        }

        public void SetModelInCache(TodoItemModel model)
        {
            if (model is not null)
                _model = model;
        }

        public void SetAssigneeInModel(UserConnectionDto newConnection)
        {
            if (newConnection is not null)
                _model.AssigneeId = newConnection.AssigneeId;
        }

    }


}
