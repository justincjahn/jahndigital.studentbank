using System;
using System.Linq;
using System.Threading.Tasks;
using jahndigital.studentbank.dal.Contexts;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.server.Models;
using jahndigital.studentbank.server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace jahndigital.studentbank.server.Controllers
{
    /// <summary>
    /// Handle user authentication and displaying user information.
    /// </summary>
    [Authorize(Roles = Constants.Role.ROLE_SUPERUSER), ApiController, Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService) => _userService = userService;

        /// <summary>
        /// Authenticate a user via email and password and return a JWT token.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("authenticate"), AllowAnonymous]
        public IActionResult Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model, getIp());

            if (response == null) {
                return BadRequest(new { message = "Username or password incorrect." });
            }

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// Refresh a user's token.
        /// </summary>
        /// <returns></returns>
        [HttpPost("refresh-token"), AllowAnonymous]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _userService.RefreshToken(refreshToken, getIp());

            if (response == null) {
                return Unauthorized(new { message = "Invalid Token"});
            }

            setTokenCookie(response.RefreshToken);


            return Ok(response);
        }

        /// <summary>
        /// Revoke a JWT
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(token)) {
                return BadRequest(new { message = "Token is required."});
            }

            var response = _userService.RevokeToken(token, getIp());

            if (!response) {
                return NotFound(new { message = "Token not found." });
            }

            return Ok(new { message = "Token Revoked" });
        }

        /// <summary>
        /// Get a user by ID.
        /// </summary>
        /// <param name="userId">The ID number of the user.</param>
        /// <param name="context">Database context.</param>
        [HttpGet("{userId}"), Authorize(Policy = Constants.AuthPolicy.DataOwner + "<" + Constants.Privilege.PRIVILEGE_MANAGE_USERS + ">")]
        public ActionResult<UserDTO> Get(int userId, [FromServices] AppDbContext context)
        {
            var user = context.Users
                .Include(x => x.Role)
                .SingleOrDefault(x => x.Id == userId);

            if (user == null) {
                return NotFound(new { message = "User not found." });
            }

            return UserDTO.FromEntity(user);
        }

        /// <summary>
        /// Get the refresh tokens of a specific user.
        /// </summary>
        /// <param name="userId">The ID number of the user.</param>
        /// <param name="context"></param>
        [HttpGet("{userId}/refresh-tokens"), Authorize(Policy = Constants.AuthPolicy.DataOwner)]
        public ActionResult<RefreshToken> GetRefreshTokens(int userId, [FromServices] AppDbContext context)
        {
            var user = context.Users.SingleOrDefault(x => x.Id == userId);
            if (user == null) return NotFound(new { message = "User not found."});
            return Ok(user.RefreshTokens);
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<ActionResult<UserDTO>> NewUser([FromBody] UserDTO model, [FromServices] AppDbContext context)
        {
            var user = UserDTO.ToEntity(model);

            // Make sure there's no user with the same email.
            var dbUser = await context.Users.SingleOrDefaultAsync(x => x.Email == user.Email);
            if (dbUser != null) {
                return this.BadRequest(new { message = "User already exists with the same email."});
            }

            var role = await context.Roles.SingleOrDefaultAsync(x => x.Id == model.RoleId);
            if (role == null) {
                return this.BadRequest(new { message = "Role does not exist."});
            }

            context.Add(user);
            await context.SaveChangesAsync();

            return Ok(UserDTO.FromEntity(user));
        }

        /// <summary>
        /// Update a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dtoUser"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpPut("{userId}"), Authorize(Roles = Constants.Role.ROLE_SUPERUSER, Policy = Constants.AuthPolicy.DataOwner)]
        public async Task<ActionResult<UserDTO>> UpdateUser(long userId, [FromBody] UserDTO dtoUser, [FromServices] AppDbContext context)
        {
            var dbUser = await context.Users.SingleOrDefaultAsync(x => x.Id == userId);
            if (dbUser == null) {
                return NotFound(new { message = "User does not exist."});
            }

            Role? role = null;
            if (dtoUser.RoleId != null) {
                role = await context.Roles.SingleOrDefaultAsync(x => x.Id == dtoUser.RoleId.Value);
                if (role == null) {
                    return BadRequest(new { message = "Role does not exist."});
                }
            }

            if (role != null && dbUser.RoleId != role.Id) {
                dbUser.Role = role;
            }

            if (!string.IsNullOrEmpty(dtoUser.GetPassword())) {
                dbUser.Password = dtoUser.GetPassword();
            }

            if (!string.IsNullOrEmpty(dtoUser.Email)) {
                dbUser.Email = dtoUser.Email;
            }
            
            context.Update(dbUser);
            await context.SaveChangesAsync();
            return Ok(UserDTO.FromEntity(dbUser));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string getIp()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For")) {
                return Request.Headers["X-Forwarded-For"];
            } else {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
