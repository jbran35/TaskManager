using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a secure authentication token for the specified user.
        /// </summary>
        /// 
        /// <remarks>Ensure that the provided user object contains all necessary information required for
        /// token generation. Passing a null or improperly initialized user may result in an exception or invalid
        /// token.</remarks>
        /// 
        /// <param name="user">The user for whom the authentication token is to be created. This parameter must not be null and should
        /// represent a valid, initialized user instance.</param>
        /// 
        /// <returns>A string containing the generated authentication token for the user. The token can be used to authenticate
        /// the user in subsequent requests.</returns>
        public string CreateToken(User user);
    }
}
