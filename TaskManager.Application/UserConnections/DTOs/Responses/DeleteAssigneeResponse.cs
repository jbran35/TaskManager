using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.UserConnections.DTOs.Responses
{
    public record DeleteAssigneeResponse
    {
        public Guid DeletedAssigneeId { get; init; } = Guid.Empty; 
        public bool Success { get; init; }
        public string Message { get; init; }
    }
}
