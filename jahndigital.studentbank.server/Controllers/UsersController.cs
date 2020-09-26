using System;
using System.Linq;
using jahndigital.studentbank.server.Entities;
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
    [Authorize, ApiController, Route("[controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// The service used to generate and update JWT tokens.
        /// </summary>
        private IUserService _userService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userService"></param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticate a user via email and password and return a JWT token.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous, HttpPost("authenticate")]
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
        [AllowAnonymous, HttpPost("refresh-token")]
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
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<User> GetById(int id, [FromServices] AppDbContext context)
        {
            var user = context.Users
                .Include(x => x.Role)
                .SingleOrDefault(x => x.Id == id);

            if (user == null) {
                return NotFound(new { message = "User not found." });
            }

            return user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [HttpGet("{id}/refresh-tokens")]
        public ActionResult<RefreshToken> GetRefreshTokens(int id, [FromServices] AppDbContext context)
        {
            var user = context.Users.SingleOrDefault(x => x.Id == id);
            if (user == null) return NotFound(new { message = "User not found."});
            return Ok(user.RefreshTokens);
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
