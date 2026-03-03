using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.API.DTOs.Account;
using TaskManager.Application.Interfaces;
using TaskManager.Application.UserConnections.Commands;
using TaskManager.Application.UserConnections.DTOs;
using TaskManager.Application.UserConnections.DTOs.Requests;
using TaskManager.Application.UserConnections.DTOs.Responses;

using TaskManager.Application.UserConnections.Queries;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.DTOs;
using TaskManager.Application.Users.DTOs.Requests;
using TaskManager.Application.Users.DTOs.Responses;
using TaskManager.Application.Users.Queries;



namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        IMediator mediator,
        ITokenService tokenService
        ) : ControllerBase
    {
        private readonly IMediator _mediator = mediator; 
        private readonly ITokenService _tokenService = tokenService;


        //Delete endpoint for removing an assignee connection.
        [HttpDelete("assignees/{connectionId}")]
        public async Task<ActionResult<DeleteAssigneeResponse>> DeleteAssigneeAsync([FromRoute] Guid connectionId)
        {
            //Validate user is authenticated and get user id from claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "Unauthorized" });
            }

            var userId = Guid.Parse(userIdString);

            var command = new DeleteUserConnectionCommand(userId, connectionId);

            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result) : BadRequest(result.ErrorMessage);
        }

        //Endpoint for adding an assignee connection between the authenticated user and another user.
        [HttpPost("assignees")]
        public async Task<ActionResult<UserConnectionDto>> AddAssignee([FromBody] CreateUserConnectionRequest connectionRequest)
        {
            //Validate user is authenticated and get user id from claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "Unauthorized" });
            }

            var userId = Guid.Parse(userIdString);

            var command = new CreateUserConnectionCommand(userId, connectionRequest.AssigneeId);

            var result = await _mediator.Send(command);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);

        }

        //Endpoint for retrieving all assignee connections for the authenticated user.
        [HttpGet("assignees")]
        public async Task<ActionResult<List<UserConnectionDto>>> GetAssignees()
        {
            //Validate user is authenticated and get user id from claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "Unauthorized" });
            }

            var userId = Guid.Parse(userIdString);

            var query = new GetActiveUserConnectionsQuery(userId);

            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }


        //Endpoint for finding a user by email to add as an assignee connection.
        [HttpGet("search/{userEmail}")]

        public async Task<ActionResult<GetUserResponse>> FindUser([FromRoute] string userEmail)
        {
            //Validate User Identity
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { Message = "User ID not found in token" });
            }

            var userId = Guid.Parse(userIdString);

            var query = new GetUserQuery(userEmail);

            var result = await _mediator.Send(query);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
        }

        //Login Endpoint
        [HttpPost("login")]
        public async Task<ActionResult<LoginUserResponse>> Login([FromBody] LoginModel model)
        {
            Console.WriteLine("In Login Endpoint");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model Not Valid. Returning...");
                return BadRequest(ModelState);
            }

            if (model.Username is null || model.Password is null)
            {
                return BadRequest();
            }

            var loginRequest = new LoginUserRequest
            {
                UserName = model.Username,
                Password = model.Password
            };

            var command = new LoginUserCommand(model.Username, model.Password);
            var response = await _mediator.Send(command);

            if (!response.Success || response.Token is null)
            {
                return Unauthorized(new { Message = response.Message });
            }

            Response.Cookies.Append("authToken", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            Console.WriteLine("From API: " + response.FirstName + " " + response.LastName + " " + response.Email + " " + response.UserName);
            return Ok(new
            {
                Token = response.Token,
                Email = response.Email,
                UserName = response.UserName,
                FirstName = response.FirstName,
                LastName = response.LastName,
                Id = response.Id,
                Message = "User logged in successfully"
            });
        }

        //Registration Endpoint
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid || model is null)
            {
                return BadRequest(ModelState);
            }

            if (model.Username is null || model.Password is null || model.Email is null ||
                model.FirstName is null || model.LastName is null)
            {
                return BadRequest();
            }

            var registrationRequest = new RegisterUserRequest
            {
                //UserName, Password, Email, FirstName, LastName
                UserName = model.Username,
                Password = model.Password,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var command = new RegisterUserCommand(model.Username, model.Password, model.Email, model.FirstName, model.LastName);
            var response = await _mediator.Send(command);

            if (!response.Success)
            {
                return BadRequest(new { Message = response.Message });
            }

            return Ok(new { Message = "User registered successfully" });
        }

        //[Authorize]
        //[HttpPost("updateprofile")]
        //public async Task<ActionResult<UpdateProfileResponse>> UpdateUserInfo([FromBody] UpdateProfileRequest request)
        //{   //Validate User Identity
        //    Console.WriteLine("Made it to /updateprofile");
        //    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    Console.WriteLine("Starting Auth");
        //    if (string.IsNullOrEmpty(userIdString))
        //    {
        //        return Unauthorized(new { Message = "User ID not found in token" });
        //    }

        //    var userId = Guid.Parse(userIdString);

        //    request.Id = userId;


        //    if (request is null || request.Id == Guid.Empty || request.Id != userId)
        //    {
        //        return BadRequest(new UpdateProfileResponse { Message = "Bad Request" });
        //    }
        //    Console.WriteLine("Finished Auth");


        //    //Call Service to complete project

        //    Console.WriteLine("Calling Service");
        //    var response = await _updateUserInfoService.UpdateProfileAsync(request);



        //    if (response is null || !response.Success)
        //    {
        //        return new UpdateProfileResponse { Message = response?.Message ?? "Issue Updating Profile" };
        //    }

        //    return Ok(response);
        //}


        //[HttpPost("identity-refresh")]
        //public async Task<ActionResult> RefreshSessionAsync([FromBody] UpdateProfileRequest request)
        //{

        //    Console.WriteLine("Made it to API Endpoint RefreshSession");

        //    //Authenticate User
        //    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    if (string.IsNullOrEmpty(userIdString))
        //    {
        //        return Unauthorized();
        //    }

        //    var userId = Guid.Parse(userIdString);

        //    if (userId == Guid.Empty)
        //    {
        //        return Unauthorized();
        //    }

        //    //Grab Claims Needed
        //    var userName = User.FindFirstValue(ClaimTypes.Name);
        //    var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        //    var lastName = User.FindFirstValue(ClaimTypes.Surname);

        //    Console.WriteLine("Claims found: \n");
        //    Console.WriteLine(userName + " " + lastName + " " + userName + " " + userId);


        //    return Ok();
        //}

        //    var firstName = User.FindFirstValue(ClaimTypes.GivenName);
        //    var lastName = User.FindFirstValue(ClaimTypes.Surname);
        //    var email = User.FindFirstValue(ClaimTypes.Email);
        //    var userName = User.FindFirstValue(ClaimTypes.Name);

        //    if (firstName is not null) identity.RemoveClaim(firstName);
        //    if (lastName is not null) identity.RemoveClaim(lastName);
        //    if (email is not null) identity.RemoveClaim(email);
        //    if (userName is not null) identity.RemoveClaim(userName);

        //    Console.WriteLine();
        //    Console.WriteLine("Refreshing claims");
        //    Console.WriteLine();

        //    if (!string.IsNullOrWhiteSpace(newProfile.FirstName)) identity.AddClaim(new Claim(ClaimTypes.GivenName, newProfile.FirstName));
        //    if (!string.IsNullOrWhiteSpace(newProfile.LastName)) identity.AddClaim(new Claim(ClaimTypes.Surname, newProfile.LastName));
        //    if (!string.IsNullOrWhiteSpace(newProfile.Email)) identity.AddClaim(new Claim(ClaimTypes.Email, newProfile.Email));
        //    if (!string.IsNullOrWhiteSpace(newProfile.UserName)) identity.AddClaim(new Claim(ClaimTypes.Name, newProfile.UserName));

        //    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
        //                              new ClaimsPrincipal(identity));

        //    return Results.Ok();

        //}
    }
}