using TaskManager.Application.Users.DTOs.Requests;

namespace TaskManager.Application.Interfaces
{
    public interface IIdentityService
    {
        /// <summary>
        ///     Used to update the user's claims after the change their profile details - as to display those updated details from their Claims. 
        /// </summary>
        /// <param name="request"></param>
        public Task RefreshUserClaimsAsync(UpdateProfileRequest request);
    }
}
