using TaskManager.Application.Users.DTOs.Requests;
using TaskManager.Application.Users.DTOs.Responses;

namespace TaskManager.Application.Interfaces
{
    public interface IRegisterUserHandler
    {
        /// <summary>
        ///     Used to register a new user in the system.
        /// </summary>
        /// <param name="request">
        ///     A request holding the account details for the new user. 
        /// </param>
        /// <returns>
        ///     A response dto with a success bool and message.
        /// </returns>
        public Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);
    }
}
