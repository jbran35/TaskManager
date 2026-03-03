using TaskManager.Application.DTOs;
using TaskManager.Application.Users.DTOs.Requests;
using TaskManager.Application.Users.DTOs.Responses;

namespace TaskManager.Application.Interfaces
{
    public interface ILoginUserHandler
    {

        /// <summary>
        ///     Used to attempt to login a user with the provided credentials. 
        /// </summary>
        /// <param name="request"> 
        ///     Contains the username and password provided.
        /// </param>
        /// 
        /// <returns>
        ///     A response indicating whether the login was successful, and if so: 
        ///     includes a JWT token for authentication in subsequent requests.
        /// </returns>
        public Task<LoginUserResponse> AttemptLogin(LoginUserRequest request);


    }
}
